/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.Management;
using OpenHardwareMonitor.Collections;
using OpenHardwareMonitor.Common;

namespace OpenHardwareMonitor.Client {
  using WMIHardware = WMI.Hardware;

  internal class Hardware : Element, IHardware {
    private readonly string name;
    private string customName;
    private readonly HardwareType hardwareType;
    private readonly ListSet<Hardware> subHardware = new ListSet<Hardware>();
    private IHardware parent;
    private readonly ListSet<Sensor> sensors = new ListSet<Sensor>();

    public ElementState State { get; set; }

    public Hardware(WMIHardware hardware, ISettings settings)
      : this(hardware, null, settings, ElementState.New) {
    }

    public Hardware(ManagementScope scope, string identifier, ISettings settings)
      : this(new WMIHardware(scope, identifier), null, settings, ElementState.Hidden) {
    }

    private Hardware(ManagementScope scope, ManagementPath path, IHardware parent, ISettings settings)
      : this(new WMIHardware(scope, path), parent, settings, ElementState.Visible) {
    }

    private Hardware(WMIHardware instance, IHardware parent, ISettings settings, ElementState state)
      : base(instance, settings) {

      name = instance.Name;
      customName = Settings.GetValue(Identifier + "name", name);
      hardwareType = (HardwareType)Enum.Parse(typeof(HardwareType), instance.HardwareType);
      foreach(ManagementPath subHardware in instance.SubHardware)
        this.subHardware.Add(new Hardware(instance.Scope, subHardware, this, settings));
      this.parent = parent;
      foreach(ManagementPath sensor in instance.Sensors)
        sensors.Add(new Sensor(instance.Scope, sensor, this, settings));
      State = state;
    }

    public void Close() {
      foreach (Hardware subHardware in this.subHardware)
        subHardware.Close();
      foreach (Sensor sensor in sensors)
        sensor.Close();
    }

    public void AddSensor(string identifier) {
      foreach(Sensor sensor in sensors) {
        if(sensor.Identifier == identifier)
          return;
      }
      Sensor newSensor = new Sensor(Scope, identifier, this, Settings.InnerSettings);
      sensors.Add(newSensor);
      if (SensorAdded != null)
        SensorAdded(newSensor);
    }

    public void RemoveSensor(string identifier) {
      Sensor toRemove = null;
      foreach (Sensor sensor in sensors) {
        if (sensor.Identifier == identifier) {
          toRemove = sensor;
          break;
        }
      }
      if (toRemove != null) {
        sensors.Remove(toRemove);
        if (SensorRemoved != null)
          SensorRemoved(toRemove);
      }
    }

    #region IHardware implementation
    public event SensorEventHandler SensorAdded;
    public event SensorEventHandler SensorRemoved;

    public string GetReport() {
      try {
        WMIHardware hardware = new WMIHardware(Path);
        return hardware.GetReport();
      } catch(ManagementException) {
      }
      return null;
    }

    public void Update() {
      if (State == ElementState.Visible) {
        foreach(Sensor sensor in sensors)
          sensor.Update();
      }
    }

    public string Name {
      get { return customName; }
      set {
        if (!string.IsNullOrEmpty(value)) {
          customName = value;
          Settings.SetValue(Identifier + "name", customName);
        } else {
          customName = name;
          Settings.Remove(Identifier + "name");
        }
      }
    }

    public HardwareType HardwareType {
      get { return hardwareType; }
    }

    public IHardware[] SubHardware {
      get { return subHardware.ToArray(); }
    }

    public IHardware Parent {
      get { return parent; }
    }

    public ISensor[] Sensors {
      get { return sensors.ToArray(); }
    }
    #endregion

    #region IVisitable implementation
    public void Accept(IVisitor visitor) {
      if (visitor == null)
        throw new ArgumentNullException("visitor");
      visitor.VisitHardware(this);
    }

    public void Traverse(IVisitor visitor) {
      foreach (ISensor sensor in sensors)
        sensor.Accept(visitor);
    }
    #endregion
  }
}
