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
  public class Parameter : Element {
    private readonly IParameter parameter;

    #region WMI Exposed
    [ManagementProbe]
    public string Name { get; private set; }
    [ManagementProbe]
    public string Description { get; private set; }
    [ManagementProbe]
    public float Value {
      get { return parameter.Value; }
    }
    [ManagementProbe]
    public float DefaultValue { get; private set; }
    [ManagementProbe]
    public bool IsDefault {
      get { return parameter.IsDefault; }
    }
    [ManagementTask]
    public void SetValue(float value) {
      parameter.SetValue(value);
    }
    [ManagementTask]
    public void SetDefault() {
      parameter.SetDefault();
    }
    #endregion

    internal Parameter(IParameter parameter) : base(parameter.Identifier) {
      this.parameter = parameter;
      Name = parameter.Name;
      Description = parameter.Description;
      DefaultValue = parameter.DefaultValue;
    }
  }
}
