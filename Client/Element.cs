/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System.Management;
using OpenHardwareMonitor.Common;

namespace OpenHardwareMonitor.Client {
  using WMIIElement = WMI.IElement;

  internal class Element : IElement {
    protected ManagementScope Scope { get; private set; }
    protected ManagementPath Path { get; private set; }
    protected Settings Settings { get; private set; }

    #region IElement implementation
    public Identifier Identifier { get; private set; }
    #endregion

    public Element(WMIIElement instance, ISettings settings) {
      Scope = instance.Scope.Clone();
      Path = instance.Path.Clone();
      Settings = new Settings(settings);

      Identifier = Identifier.FromString(instance.Identifier);
    }
  }
}
