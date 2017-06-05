/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.Service {
  [RunInstaller(true)]
  public class ProjectInstaller : Installer {
    private readonly ServiceProcessInstaller serviceProcessInstaller;
    private readonly ServiceInstaller serviceInstaller;
    private readonly AssemblyInstaller wmiInstaller;
    private readonly AssemblyInstaller driverInstaller;

    public ProjectInstaller() {
      serviceProcessInstaller = new ServiceProcessInstaller();
      serviceProcessInstaller.Account = ServiceAccount.LocalSystem;

      serviceInstaller = new ServiceInstaller();
      serviceInstaller.ServiceName = OpenHardwareMonitorService.MyServiceName;
      serviceInstaller.StartType = ServiceStartMode.Automatic;
      serviceInstaller.ServicesDependedOn = new string[] { DriverInstaller.DriverName, "Winmgmt" };

      wmiInstaller = new AssemblyInstaller(typeof(OpenHardwareMonitor.WMI.WmiProvider).Assembly, new string[] { });
      driverInstaller = new AssemblyInstaller(typeof(OpenHardwareMonitor.Hardware.Computer).Assembly, new string[] { });

      this.Installers.AddRange(new Installer[] { serviceProcessInstaller, serviceInstaller, wmiInstaller, driverInstaller });
    }
  }
}
