/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2009-2010 Paul Werelds <paul@werelds.net>
  Copyright (C) 2012 Michael Möller <mmoeller@openhardwaremonitor.org>
  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Instrumentation;
using OpenHardwareMonitor.Common;
using OpenHardwareMonitor.WMI;

[assembly: WmiConfiguration("root\\OpenHardwareMonitor",
                            HostingModel = ManagementHostingModel.Decoupled)]

[RunInstaller(true)]
public class InstanceInstaller : DefaultManagementInstaller {
}

namespace OpenHardwareMonitor.WMI {
  /// <summary>
  /// The WMI Provider.
  /// This class is not exposed to WMI itself.
  /// </summary>
  public class WmiProvider : IDisposable {
    private bool disposed = false;
    private readonly Dictionary<Identifier, Element> activeInstances = new Dictionary<Identifier, Element>();

    public WmiProvider(IComputer computer) {
      foreach (IHardware hardware in computer.Hardware)
        ComputerHardwareAdded(hardware);

      computer.HardwareAdded += ComputerHardwareAdded;
      computer.HardwareRemoved += ComputerHardwareRemoved;
    }

    #region Eventhandlers
    private void ComputerHardwareAdded(IHardware data) {
      if (!activeInstances.ContainsKey(data.Identifier)) {
        data.SensorAdded += HardwareSensorAdded;
        data.SensorRemoved += HardwareSensorRemoved;

        Hardware hardware = new Hardware(data);
        if (data.Parent != null)
          AddChild(data.Parent.Identifier, hardware);
        activeInstances.Add(data.Identifier, hardware);

        foreach (IHardware subHardware in data.SubHardware)
          ComputerHardwareAdded(subHardware);

        try {
          InstrumentationManager.Publish(hardware);
        } catch (Exception) {
        }

        foreach (ISensor sensor in data.Sensors)
          HardwareSensorAdded(sensor);
      }
    }

    private void HardwareSensorAdded(ISensor data) {
      if (!activeInstances.ContainsKey(data.Identifier)) {
        Sensor sensor = new Sensor(data);

        foreach (IParameter param in data.Parameters) {
          if(!activeInstances.ContainsKey(param.Identifier)) {
            Parameter parameter = new Parameter(param);
            sensor.AddChild(parameter);
            activeInstances.Add(param.Identifier, parameter);

            try {
              InstrumentationManager.Publish(parameter);
            } catch (Exception) { }
          }
        }

        if (data.Control != null) {
          Control control = new Control(data.Control);
          sensor.AddChild(control);
          activeInstances.Add(data.Control.Identifier, control);

          try {
            InstrumentationManager.Publish(control);
          } catch (Exception) { }
        }

        AddChild(data.Hardware.Identifier, sensor);
        activeInstances.Add(data.Identifier, sensor);

        try {
          InstrumentationManager.Publish(sensor);
        } catch (Exception) { }
      }
    }

    private void ComputerHardwareRemoved(IHardware hardware) {
      hardware.SensorAdded -= HardwareSensorAdded;
      hardware.SensorRemoved -= HardwareSensorRemoved;

      foreach (ISensor sensor in hardware.Sensors)
        HardwareSensorRemoved(sensor);

      foreach (IHardware subHardware in hardware.SubHardware)
        ComputerHardwareRemoved(subHardware);

      if (hardware.Parent != null)
        RemoveChild(hardware.Parent.Identifier, hardware);

      RevokeInstance(hardware.Identifier);
    }

    private void HardwareSensorRemoved(ISensor sensor) {
      foreach (IParameter parameter in sensor.Parameters) {
        RemoveChild(sensor.Identifier, parameter);
        RevokeInstance(parameter.Identifier);
      }

      if(sensor.Control != null) {
        RemoveChild(sensor.Identifier, sensor.Control);
        RevokeInstance(sensor.Control.Identifier);
      }

      RemoveChild(sensor.Hardware.Identifier, sensor);

      RevokeInstance(sensor.Identifier);
    }
    #endregion

    #region Helpers
    private void AddChild(Identifier parent, Element child) {
      Element element;
      if (activeInstances.TryGetValue(parent, out element))
        element.AddChild(child);
    }

    private void RemoveChild(Identifier parent, IElement child) {
      Element parentElement, childElement;
      if (activeInstances.TryGetValue(parent, out parentElement) &&
          activeInstances.TryGetValue(child.Identifier, out childElement))
        parentElement.RemoveChild(childElement);
    }

    private void RevokeInstance(Identifier identifier) {
      Element element;
      if (!activeInstances.TryGetValue(identifier, out element))
        return;

      try {
        InstrumentationManager.Revoke(element);
      } catch (Exception) { }

      activeInstances.Remove(identifier);
    }
    #endregion

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing) {
      if (!disposed) {
        if (disposing) {
          foreach (Element instance in activeInstances.Values) {
            try {
              InstrumentationManager.Revoke(instance);
            } catch (Exception) { }
          }
        }

        disposed = true;
      }
    }
  }
}
