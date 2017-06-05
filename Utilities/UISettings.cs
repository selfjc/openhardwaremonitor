/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.Globalization;
using System.Drawing;
using OpenHardwareMonitor.Common;

namespace OpenHardwareMonitor {
  public class UISettings : Settings {
    public UISettings(ISettings settings) : base(settings) {
    }

    public void SetValue(string name, Color value) {
      settings.SetValue(name, value.ToArgb().ToString("X8"));
    }

    public Color GetValue(string name, Color value) {
      int result;
      return int.TryParse(
        settings.GetValue(name, value.ToArgb().ToString("X8")),
        NumberStyles.HexNumber, CultureInfo.InvariantCulture,
        out result) ? Color.FromArgb(result) : value;
    }

    public void SetValue(Identifier identifier, Color value) {
      SetValue(identifier.ToString(), value);
    }

    public Color GetValue(Identifier identifier, Color value) {
      return GetValue(identifier.ToString(), value);
    }
  }
}
