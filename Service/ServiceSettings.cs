/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using OpenHardwareMonitor.Common;

namespace OpenHardwareMonitor.Service {
  public class ServiceSettings : ISettings {
    public bool Contains(string name) {
      return false;
    }

    public void SetValue(string name, string value) {
    }

    public string GetValue(string name, string value) {
      if (name == "DisableSensorHistory")
        return "true";
      return value;
    }

    public void Remove(string name) {
    }
  }
}
