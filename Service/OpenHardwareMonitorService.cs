/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.Threading;
using System.ServiceProcess;

using OpenHardwareMonitor.Common;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.WMI;

namespace OpenHardwareMonitor.Service {
  public class OpenHardwareMonitorService : ServiceBase {
    public const string MyServiceName = "OpenHardwareMonitorService";
    private const int UpdateInterval = 1000;

    private Computer computer;
    private WmiProvider wmiProvider;
    private IVisitor updateVisitor = new UpdateVisitor();
    private Timer timer;

    public OpenHardwareMonitorService() {
      InitializeComponent();
    }

    private void InitializeComponent() {
      this.ServiceName = MyServiceName;
      this.CanPauseAndContinue = true;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing) {
      // cleanup if OnStart failed
      if (timer != null) {
        try {
          StopTimer();
        } catch(Exception) {
        }
        timer = null;
      }
      if (computer != null) {
        try {
          computer.Close();
        } catch(Exception) {
        }
        computer = null;
      }
      if (wmiProvider != null) {
        try {
          wmiProvider.Dispose();
        } catch(Exception) {
        }
        wmiProvider = null;
      }
      base.Dispose(disposing);
    }

    /// <summary>
    /// Start this service.
    /// </summary>
    protected override void OnStart(string[] args) {
      StartService();
    }

    /// <summary>
    /// Stop this service.
    /// </summary>
    protected override void OnStop() {
      StopService();
    }

    protected override void OnPause() {
      timer.Change(0, Timeout.Infinite);
    }

    protected override void OnContinue() {
      timer.Change(0, UpdateInterval);
    }

    internal void StartService() {
      computer = new Computer(new ServiceSettings());
      computer.MainboardEnabled = true;
      computer.CPUEnabled = true;
      computer.RAMEnabled = true;
      computer.GPUEnabled = true;
      computer.FanControllerEnabled = true;
      computer.HDDEnabled = true;

      wmiProvider = new WmiProvider(computer);

      computer.Open();

      timer = new Timer(new TimerCallback(TimerFunc), null, 0, UpdateInterval);
    }

    internal void StopService() {
      StopTimer();
      timer = null;

      computer.Close();
      computer = null;
      wmiProvider.Dispose();
      wmiProvider = null;
    }

    private void TimerFunc(object state) {
      computer.Accept(updateVisitor);
    }

    private void StopTimer() {
      using(WaitHandle wait = new AutoResetEvent(false)) {
        timer.Dispose(wait);
        wait.WaitOne(2 * UpdateInterval, false);
      }
    }
  }
}
