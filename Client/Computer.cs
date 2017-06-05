/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using OpenHardwareMonitor.Common;

namespace OpenHardwareMonitor.Client {
  using WMIElement = WMI.Element;
  using WMIHardware = WMI.Hardware;
  using WMIInstanceOptionEvent = WMI.InstanceOperationEvent;

  internal enum ElementState {
    New,
    Hidden,
    Visible,
  };

  public class Computer : IComputer {
    private class UpdateArgs {
      public string Identifier { get; private set; }
      public string Parent { get; private set; }

      public UpdateArgs(WMIElement element) {
        Identifier = element.Identifier;
        Parent = element.Parent;
      }
    }

    private readonly SynchronizationContext syncContext; // let everything run in the creator (UI) thread
    private readonly List<Hardware> allHardware = new List<Hardware>();
    private readonly ISettings settings;

    private bool open;

    private bool mainboardEnabled;
    private bool cpuEnabled;
    private bool ramEnabled;
    private bool gpuEnabled;
    private bool fanControllerEnabled;
    private bool hddEnabled;

    private const string DefaultScope = "root\\OpenHardwareMonitor";
    private ManagementScope scope;
    private static WqlEventQuery query = new WqlEventQuery(
      "__InstanceOperationEvent",
     new TimeSpan(0, 0, 5),
     "(__CLASS=\"__InstanceCreationEvent\" OR __CLASS=\"__InstanceDeletionEvent\") AND" +
     "(TargetInstance ISA \"Hardware\" OR TargetInstance ISA \"Sensor\")");
    private readonly ManagementEventWatcher watcher;

    public Computer()
      : this(DefaultScope, null, SynchronizationContext.Current) {
    }

    public Computer(ISettings settings)
      : this(DefaultScope, settings, SynchronizationContext.Current) {
    }

    public Computer(string scope)
      : this(scope, null, SynchronizationContext.Current) {
    }

    public Computer(string scope, ISettings settings)
      : this(scope, settings, SynchronizationContext.Current) {
    }

    public Computer(string scope, ISettings settings, SynchronizationContext syncContext) {
      this.scope = new ManagementScope(scope);
      this.syncContext = syncContext ?? new SynchronizationContext();
      this.settings = settings ?? new EmptySettings();
      watcher = new ManagementEventWatcher(this.scope, query);
      watcher.EventArrived += watcher_EventArrived;
    }

    private void Show(HardwareType type) {
      foreach(Hardware hardware in allHardware) {
        if (hardware.HardwareType == type && hardware.State != ElementState.Visible) {
          hardware.State = ElementState.Visible;
          if (HardwareAdded != null)
            HardwareAdded(hardware);
        }
      }
    }

    private void Hide(HardwareType type) {
      foreach(Hardware hardware in allHardware) {
        if (hardware.HardwareType == type && hardware.State == ElementState.Visible) {
          hardware.State = ElementState.Hidden;
          if (HardwareRemoved != null)
            HardwareRemoved(hardware);
        }
      }
    }

    private Hardware FindHardware(string identifier) {
      foreach(Hardware hardware in allHardware) {
        if (hardware.Identifier == identifier)
          return hardware;
      }
      return null;
    }

    private void AddHardware(object state) {
      UpdateArgs args = (UpdateArgs)state;
      if (!string.IsNullOrEmpty(args.Parent))
        return;

      if (FindHardware(args.Identifier) == null) {
        allHardware.Add(new Hardware(scope, args.Identifier, settings));
        ShowHardware();
      }
    }

    private void RemoveHardware(object state) {
      UpdateArgs args = (UpdateArgs)state;
      if (!string.IsNullOrEmpty(args.Parent))
        return;

      Hardware toRemove = FindHardware(args.Identifier);
      if (toRemove != null) {
        allHardware.Remove(toRemove);
        if (HardwareRemoved != null)
          HardwareRemoved(toRemove);
      }
    }

    private Hardware FindSensorParent(string parent) {
      foreach(Hardware hardware in allHardware) {
        if (hardware.Identifier == parent)
          return hardware;
        foreach(Hardware subHardware in hardware.SubHardware) {
          if (subHardware.Identifier == parent)
            return subHardware;
        }
      }
      return null;
    }

    private void AddSensor(object state) {
      UpdateArgs args = (UpdateArgs)state;
      Hardware parent = FindSensorParent(args.Parent);
      if (parent != null)
        parent.AddSensor(args.Identifier);
    }

    private void RemoveSensor(object state) {
      UpdateArgs args = (UpdateArgs)state;
      Hardware parent = FindSensorParent(args.Parent);
      if (parent != null)
        parent.RemoveSensor(args.Identifier);
    }

    private void ShowHardware() {
      if (mainboardEnabled)
        Show(HardwareType.Mainboard);

      if (cpuEnabled)
        Show(HardwareType.CPU);

      if (ramEnabled)
        Show(HardwareType.RAM);

      if (gpuEnabled) {
        Show(HardwareType.GpuAti);
        Show(HardwareType.GpuNvidia);
      }

      if (fanControllerEnabled) {
        Show(HardwareType.TBalancer);
        Show(HardwareType.Heatmaster);
        Show(HardwareType.Aquaero);
      }

      if (hddEnabled)
        Show(HardwareType.HDD);
    }

    private void watcher_EventArrived(object sender, EventArrivedEventArgs e) {
      WMIInstanceOptionEvent instanceEvent = new WMIInstanceOptionEvent(e.NewEvent);
      string instanceClass = instanceEvent.ManagementClassName;
      WMIElement element = instanceEvent.TargetInstance;
      string elementClass = element.ManagementClassName;
      try {
        if (instanceClass == "__InstanceCreationEvent" && elementClass == "Hardware")
          syncContext.Send(AddHardware, new UpdateArgs(element));
        else if (instanceClass == "__InstanceCreationEvent" && elementClass == "Sensor")
          syncContext.Send(AddSensor, new UpdateArgs(element));
        else if (instanceClass == "__InstanceDeletionEvent" && elementClass == "Hardware")
          syncContext.Send(RemoveHardware, new UpdateArgs(element));
        else if (instanceClass == "__InstanceDeletionEvent" && elementClass == "Sensor")
          syncContext.Send(RemoveSensor, new UpdateArgs(element));
      } catch(InvalidAsynchronousStateException) {
      }
    }

    public void Open() {
      if (open)
        return;

      watcher.Start();

      foreach(WMIHardware hardware in WMIHardware.GetInstances(scope, "Parent=\"\""))
        allHardware.Add(new Hardware(hardware, settings));

      ShowHardware();

      open = true;
    }

    public void Close() {
      if (!open)
        return;

      watcher.Stop();

      foreach(HardwareType type in Enum.GetValues(typeof(HardwareType)))
        Hide(type);
      foreach(Hardware hardware in allHardware)
        hardware.Close();
      allHardware.Clear();

      open = false;
    }

    private static void NewSection(TextWriter writer) {
      for (int i = 0; i < 8; i++)
        writer.Write("----------");
      writer.WriteLine();
      writer.WriteLine();
    }

    private static int CompareSensor(ISensor a, ISensor b) {
      int c = a.SensorType.CompareTo(b.SensorType);
      if (c == 0)
        return a.Index.CompareTo(b.Index);
      else
        return c;
    }

    private static void ReportHardwareSensorTree(
      IHardware hardware, TextWriter w, string space)
    {
      w.WriteLine("{0}|", space);
      w.WriteLine("{0}+- {1} ({2})",
        space, hardware.Name, hardware.Identifier);
      ISensor[] sensors = hardware.Sensors;
      Array.Sort(sensors, CompareSensor);
      foreach (ISensor sensor in sensors) {
        w.WriteLine("{0}|  +- {1,-14} : {2,8:G6} {3,8:G6} {4,8:G6} ({5})",
          space, sensor.Name, sensor.Value, sensor.Min, sensor.Max,
          sensor.Identifier);
      }
      foreach (IHardware subHardware in hardware.SubHardware)
        ReportHardwareSensorTree(subHardware, w, "|  ");
    }

    private static void ReportHardwareParameterTree(
      IHardware hardware, TextWriter w, string space) {
      w.WriteLine("{0}|", space);
      w.WriteLine("{0}+- {1} ({2})",
        space, hardware.Name, hardware.Identifier);
      ISensor[] sensors = hardware.Sensors;
      Array.Sort(sensors, CompareSensor);
      foreach (ISensor sensor in sensors) {
        string innerSpace = space + "|  ";
        if (sensor.Parameters.Length > 0) {
          w.WriteLine("{0}|", innerSpace);
          w.WriteLine("{0}+- {1} ({2})",
            innerSpace, sensor.Name, sensor.Identifier);
          foreach (IParameter parameter in sensor.Parameters) {
            string innerInnerSpace = innerSpace + "|  ";
            w.WriteLine("{0}+- {1} : {2}",
              innerInnerSpace, parameter.Name,
              string.Format(CultureInfo.InvariantCulture, "{0} : {1}",
                parameter.DefaultValue, parameter.Value));
          }
        }
      }
      foreach (IHardware subHardware in hardware.SubHardware)
        ReportHardwareParameterTree(subHardware, w, "|  ");
    }

    private static void ReportHardware(IHardware hardware, TextWriter w) {
      string hardwareReport = hardware.GetReport();
      if (!string.IsNullOrEmpty(hardwareReport)) {
        NewSection(w);
        w.Write(hardwareReport);
      }
      foreach (IHardware subHardware in hardware.SubHardware)
        ReportHardware(subHardware, w);
    }

    #region IComputer implementation
    public event HardwareEventHandler HardwareAdded;
    public event HardwareEventHandler HardwareRemoved;

    public string GetReport() {
      using (StringWriter w = new StringWriter(CultureInfo.InvariantCulture)) {
        w.WriteLine();
        w.WriteLine("Open Hardware Monitor Service Report");
        w.WriteLine();

        NewSection(w);
        w.Write("Version: "); w.WriteLine(Application.ProductVersion);
        w.WriteLine();

        NewSection(w);
        w.Write("Common Language Runtime: ");
        w.WriteLine(Environment.Version.ToString());
        w.Write("Operating System: ");
        w.WriteLine(Environment.OSVersion.ToString());
        w.Write("Process Type: ");
        w.WriteLine(IntPtr.Size == 4 ? "32-Bit" : "64-Bit");
        w.WriteLine();

        NewSection(w);
        w.WriteLine("Sensors");
        w.WriteLine();
        foreach (Hardware hardware in allHardware) {
          if (hardware.State == ElementState.Visible)
            ReportHardwareSensorTree(hardware, w, "");
        }
        w.WriteLine();

        NewSection(w);
        w.WriteLine("Parameters");
        w.WriteLine();
        foreach (Hardware hardware in allHardware) {
          if (hardware.State == ElementState.Visible)
            ReportHardwareParameterTree(hardware, w, "");
        }
        w.WriteLine();

        foreach (Hardware hardware in allHardware) {
          if (hardware.State == ElementState.Visible)
            ReportHardware(hardware, w);
        }

        return w.ToString();
      }
    }

    public IHardware[] Hardware {
      get {
        List<IHardware> visibleHardware = new List<IHardware>();
        foreach(Hardware hardware in allHardware) {
          if (hardware.State == ElementState.Visible)
            visibleHardware.Add(hardware);
        }
        return visibleHardware.ToArray();
      }
    }

    public bool MainboardEnabled {
      get { return mainboardEnabled; }
      set {
        if (open && value != mainboardEnabled) {
          if (value)
            Show(HardwareType.Mainboard);
          else
            Hide(HardwareType.Mainboard);
        }
        mainboardEnabled = value;
      }
    }

    public bool CPUEnabled {
      get { return cpuEnabled; }
      set {
        if (open && value != cpuEnabled) {
          if (value)
            Show(HardwareType.CPU);
          else
            Hide(HardwareType.CPU);
        }
        cpuEnabled = value;
      }
    }

    public bool RAMEnabled {
      get { return ramEnabled; }
      set {
        if (open && value != ramEnabled) {
          if (value)
            Show(HardwareType.RAM);
          else
            Hide(HardwareType.RAM);
        }
        ramEnabled = value;
      }
    }

    public bool GPUEnabled {
      get { return gpuEnabled; }
      set {
        if (open && value != gpuEnabled) {
          if (value) {
            Show(HardwareType.GpuAti);
            Show(HardwareType.GpuNvidia);
          } else {
            Hide(HardwareType.GpuAti);
            Hide(HardwareType.GpuNvidia);
          }
        }
        gpuEnabled = value;
      }
    }

    public bool FanControllerEnabled {
      get { return fanControllerEnabled; }
      set {
        if (open && value != fanControllerEnabled) {
          if (value) {
            Show(HardwareType.TBalancer);
            Show(HardwareType.Heatmaster);
            Show(HardwareType.Aquaero);
          } else {
            Hide(HardwareType.TBalancer);
            Hide(HardwareType.Heatmaster);
            Hide(HardwareType.Aquaero);
          }
        }
        fanControllerEnabled = value;
      }
    }

    public bool HDDEnabled {
      get { return hddEnabled; }
      set {
        if (open && value != hddEnabled) {
          if (value)
            Show(HardwareType.HDD);
          else
            Hide(HardwareType.HDD);
        }
        hddEnabled = value;
      }
    }
    #endregion

    #region IVisitable implementation
    public void Accept(IVisitor visitor) {
      if (visitor == null)
        throw new ArgumentNullException("visitor");
      visitor.VisitComputer(this);
    }

    public void Traverse(IVisitor visitor) {
      foreach (Hardware hardware in allHardware)
        hardware.Accept(visitor);
    }
    #endregion
  }
}
