/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.ComponentModel;
using System.Configuration.Install;

namespace OpenHardwareMonitor.Hardware
{
  [RunInstaller(true)]
  public class DriverInstaller : Installer {
    public static string DriverName {
      get { return Ring0.DriverName; }
    }

    public override void Install(System.Collections.IDictionary stateSaver)
    {
      base.Install(stateSaver);

      string installError;
      if(!Ring0.InstallDriver(out installError))
        throw new InstallException(installError);
    }

    public override void Rollback(System.Collections.IDictionary savedState)
    {
      base.Rollback(savedState);

      Ring0.UninstallDriver();
    }

    public override void Uninstall(System.Collections.IDictionary savedState)
    {
      base.Uninstall(savedState);

      if(!Ring0.UninstallDriver())
        throw new InstallException("Failed to uninstall driver.");
    }
  }
}
