/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.Management.Instrumentation;
using OpenHardwareMonitor.Common;

namespace OpenHardwareMonitor.WMI
{
  /// <summary>
  /// base class, no WMI instances published
  /// </summary>
  [ManagementEntity]
  public abstract class Element {
    #region WMI Exposed
    [ManagementKey]
    public string Identifier { get; protected set; }
    [ManagementProbe]
    public string Parent { get; internal set; }
    #endregion

    internal string ManagementPath { get; private set; }

    protected Element(Identifier identifier) {
      Identifier = identifier.ToString();
      Parent = "";

      object[] customAttributes = this.GetType().GetCustomAttributes(typeof(ManagementEntityAttribute), false);
      if (customAttributes.Length != 0 && ((ManagementEntityAttribute)customAttributes[0]).Name != null)
        ManagementPath = ((ManagementEntityAttribute)customAttributes[0]).Name + ".Identifier=\"" + Identifier + "\"";
      else
        ManagementPath = this.GetType().Name + ".Identifier=\"" + Identifier + "\"";
    }

    internal virtual void AddChild(Element child) {
      throw new NotSupportedException();
    }

    internal virtual void RemoveChild(Element child) {
      throw new NotSupportedException();
    }
  }
}
