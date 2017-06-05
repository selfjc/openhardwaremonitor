/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2009-2010 Paul Werelds <paul@werelds.net>
  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System.Collections.Generic;
using System.Management.Instrumentation;
using OpenHardwareMonitor.Common;

namespace OpenHardwareMonitor.WMI {
  [ManagementEntity]
  public class Sensor : Element {
    private readonly ISensor sensor;

    #region WMI Exposed
    [ManagementProbe]
    public string Name { get; private set; }
    [ManagementProbe]
    public int Index { get; private set; }
    [ManagementProbe]
    public string SensorType { get; private set; }
    [ManagementProbe]
    public float Value {
      get { return sensor.Value ?? 0f; }
    }
    [ManagementProbe]
    public float Min {
      get { return sensor.Min ?? 0f; }
    }
    [ManagementProbe]
    public float Max {
      get { return sensor.Max ?? 0f; }
    }
    [ManagementProbe]
    public bool IsDefaultHidden { get; private set; }
    [ManagementProbe]
    [ManagementReference(Type="Parameter")]
    public string[] Parameters {
      get { return parameters.ToArray(); }
    }
    [ManagementProbe]
    [ManagementReference(Type="Control")]
    public string Control {
      get { return control; }
    }
    #endregion

    private readonly List<string> parameters = new List<string>();
    private string control;

    internal Sensor(ISensor sensor) : base(sensor.Identifier) {
      this.sensor = sensor;
      Name = sensor.Name;
      Index = sensor.Index;
      SensorType = sensor.SensorType.ToString();
      IsDefaultHidden = sensor.IsDefaultHidden;
    }

    internal override void AddChild(Element child) {
      if (child is Parameter) {
        child.Parent = Identifier;
        parameters.Add(child.ManagementPath);
      } else if (child is Control) {
        child.Parent = Identifier;
        control = child.ManagementPath;
      } else
        base.AddChild(child);
    }

    internal override void RemoveChild(Element child) {
      if (child is Parameter)
        parameters.Remove(child.ManagementPath);
      else if (child is Control)
        control = null;
      else
        base.RemoveChild(child);
    }
  }
}
