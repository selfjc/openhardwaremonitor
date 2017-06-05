/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.ServiceProcess;

namespace OpenHardwareMonitor.Service {
  static class Program {
    /// <summary>
    /// This method starts the service.
    /// </summary>
    static void Main() {
#if true
        // To run more than one service you have to add them here
        ServiceBase.Run(new ServiceBase[] { new OpenHardwareMonitorService() });
#else
        try {
          OpenHardwareMonitorService service = new OpenHardwareMonitorService();
          service.StartService();
          Console.WriteLine("Hit any key to stop");
          Console.ReadKey(true);
          service.StopService();
        } catch(Exception ex) {
          Console.WriteLine(ex);
        }
#endif
    }
  }
}
