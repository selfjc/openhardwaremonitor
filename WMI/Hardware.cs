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
  public class Hardware : Element {
    private readonly IHardware hardware;
    
    #region WMI Exposed    
    [ManagementProbe]
    public string Name { get; private set; }
    [ManagementProbe]
    public string HardwareType { get; private set; }
    [ManagementProbe]
    [ManagementReference(Type="Hardware")]
    public string[] SubHardware { 
      get { return subHardware.ToArray(); }
    }
    [ManagementProbe]
    [ManagementReference(Type="Sensor")]
    public string[] Sensors { 
      get { return sensors.ToArray(); }
    }
    [ManagementTask]
    public string GetReport() {
      return hardware.GetReport();
    }
    #endregion

    private readonly List<string> subHardware = new List<string>();
    private readonly List<string> sensors = new List<string>();
    
    internal Hardware(IHardware hardware) : base(hardware.Identifier) {
      this.hardware = hardware;
      Name = hardware.Name;
      HardwareType = hardware.HardwareType.ToString();
    }
    
    internal override void AddChild(Element child) {
      if (child is Hardware) {
        child.Parent = Identifier;
        subHardware.Add(child.ManagementPath);
      } else if (child is Sensor) {
        child.Parent = Identifier;
        sensors.Add(child.ManagementPath);
      } else
        base.AddChild(child);
    } 
    
    internal override void RemoveChild(Element child) {
      if (child is Hardware)
        subHardware.Remove(child.ManagementPath);
      else if (child is Sensor)
        sensors.Remove(child.ManagementPath);
      else
        base.RemoveChild(child);
    }   
  }
}
