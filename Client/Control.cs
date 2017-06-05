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
  using WMIControl = WMI.Control;

  internal class Control : Element, IControl {
    private ControlMode mode;
    private float softwareValue;
    private float minSoftwareValue;
    private float maxSoftwareValue;

    public Control(ManagementScope scope, ManagementPath path, ISettings settings)
      : this(new WMIControl(scope, path), settings) {
    }

    private Control(WMIControl instance, ISettings settings)
      : base(instance, settings) {

      softwareValue = Settings.GetValue(Identifier + "value", 0f);
      mode = (ControlMode)Settings.GetValue(Identifier + "mode", (int)ControlMode.Undefined);

      if (mode == ControlMode.Default)
        instance.SetDefault();
      else if (mode == ControlMode.Software)
        instance.SetSoftware(softwareValue);

      mode = (ControlMode)Enum.Parse(typeof(ControlMode), instance.ControlMode);
      softwareValue = instance.SoftwareValue;
      minSoftwareValue = instance.MinSoftwareValue;
      maxSoftwareValue = instance.MaxSoftwareValue;
    }

    #region IControl implementation
    public void SetDefault() {
      ControlMode = ControlMode.Default;
    }

    public void SetSoftware(float value) {
      SoftwareValue = value;
    }

    public ControlMode ControlMode {
      get {
        try {
          WMIControl instance = new WMIControl(Path);
          mode =(ControlMode)Enum.Parse(typeof(ControlMode), instance.ControlMode);
        } catch(ManagementException) {
        }
        return mode;
      }
      private set {
        try {
          if (value == ControlMode.Default) {
            WMIControl instance = new WMIControl(Path);
            instance.SetDefault();
          }
          Settings.SetValue(Identifier + "mode", (int)value);
        } catch(ManagementException) {
        }
      }
    }

    public float SoftwareValue {
      get {
        try {
          WMIControl instance = new WMIControl(Path);
          softwareValue = instance.SoftwareValue;
        } catch(ManagementException) {
        }
        return softwareValue;
      }
      private set {
        try {
          WMIControl instance = new WMIControl(Path);
          instance.SetSoftware(value);
          Settings.SetValue(Identifier + "value", value);
        } catch(ManagementException) {
        }
      }
    }

    public float MinSoftwareValue {
      get {
        try {
          WMIControl instance = new WMIControl(Path);
          minSoftwareValue = instance.MinSoftwareValue;
        } catch(ManagementException) {
        }
        return minSoftwareValue;
      }
    }

    public float MaxSoftwareValue {
      get {
        try {
          WMIControl instance = new WMIControl(Path);
          maxSoftwareValue = instance.MaxSoftwareValue;
        } catch(ManagementException) {
        }
        return maxSoftwareValue;
      }
    }
    #endregion
  }
}
