/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>
  
*/

using System.Management;

namespace OpenHardwareMonitor.Client.WMI {
  internal interface IElement {
    ManagementScope Scope { get; }
    ManagementPath Path { get; }
    
    string Identifier { get; }
    string Parent { get; }
  }
}
