/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.Management;
using OpenHardwareMonitor.Common;

namespace OpenHardwareMonitor.Client {
  using WMIParameter = WMI.Parameter;

  internal class Parameter : Element, IParameter {
    private readonly ISensor sensor;
    private readonly string name;
    private readonly string description;
    private readonly float defaultValue;
    private bool isDefault;
    private float value;

    public Parameter(ManagementScope scope, ManagementPath path, ISensor sensor, ISettings settings)
      : this(new WMIParameter(scope, path), sensor, settings) {
    }

    private Parameter(WMIParameter instance, ISensor sensor, ISettings settings)
      : base(instance, settings) {

      this.sensor = sensor;
      name = instance.Name;
      description = instance.Description;
      defaultValue = instance.DefaultValue;

      if (Settings.Contains(Identifier))
        instance.SetDefault();
      else
        instance.SetValue(Settings.GetValue(Identifier, defaultValue));

      isDefault = instance.IsDefault;
      value = instance.Value;
    }

    #region IParameter implementation
    public ISensor Sensor {
      get {
        return sensor;
      }
    }

    public string Name {
      get {
        return name;
      }
    }

    public string Description {
      get {
        return description;
      }
    }

    public float Value {
      get {
        try {
          WMIParameter instance = new WMIParameter(Path);
          value = instance.Value;
        } catch(ManagementException) {
        }
        return value;
      }
    }

    public float DefaultValue {
      get {
        return defaultValue;
      }
    }

    public bool IsDefault {
      get {
        try {
          WMIParameter instance = new WMIParameter(Path);
          isDefault = instance.IsDefault;
        } catch(ManagementException) {
        }
        return isDefault;
      }
    }

    public void SetValue(float value) {
      try {
        WMIParameter instance = new WMIParameter(Path);
        instance.SetValue(value);
        Settings.SetValue(Identifier, value);
      } catch(ManagementException) {
      }
    }

    public void SetDefault() {
      try {
        WMIParameter instance = new WMIParameter(Path);
        instance.SetDefault();
        Settings.Remove(Identifier);
      } catch(ManagementException) {
      }
    }
    #endregion

    #region IVisitable implementation
    public void Accept(IVisitor visitor)
    {
      if (visitor == null)
        throw new ArgumentNullException("visitor");
      visitor.VisitParameter(this);
    }

    public void Traverse(IVisitor visitor)
    {
    }
    #endregion
  }
}
