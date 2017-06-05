/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System.Management.Instrumentation;
using OpenHardwareMonitor.Common;

namespace OpenHardwareMonitor.WMI
{
  [ManagementEntity]
  public class Control : Element {
    private readonly IControl control;

    #region WMI Exposed
    [ManagementProbe]
    public string ControlMode {
      get { return control.ControlMode.ToString(); }
    }
    [ManagementProbe]
    public float SoftwareValue {
      get { return control.SoftwareValue; }
    }
    [ManagementProbe]
    public float MinSoftwareValue { get; private set; }
    [ManagementProbe]
    public float MaxSoftwareValue { get; private set; }
    [ManagementTask]
    public void SetDefault() {
      control.SetDefault();
    }
    [ManagementTask]
    public void SetSoftware(float value) {
      control.SetSoftware(value);
    }
    #endregion

    internal Control(IControl control) : base(control.Identifier) {
      this.control = control;
      MinSoftwareValue = control.MinSoftwareValue;
      MaxSoftwareValue = control.MaxSoftwareValue;
    }
  }
}
