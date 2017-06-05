﻿/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2014 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using OpenHardwareMonitor.Common;

namespace OpenHardwareMonitor {
  public class PersistentSettings : ISettings {

    private IDictionary<string, string> settings =
      new Dictionary<string, string>();

    public void Load(string fileName) {
      XmlDocument doc = new XmlDocument();
      try {
        doc.Load(fileName);
      } catch {
        try {
          File.Delete(fileName);
        } catch { }

        string backupFileName = fileName + ".backup";
        try {
          doc.Load(backupFileName);
        } catch {
          try {
            File.Delete(backupFileName);
          } catch { }

          return;
        }
      }

      XmlNodeList list = doc.GetElementsByTagName("appSettings");
      foreach (XmlNode node in list) {
        XmlNode parent = node.ParentNode;
        if (parent != null && parent.Name == "configuration" &&
          parent.ParentNode is XmlDocument) {
          foreach (XmlNode child in node.ChildNodes) {
            if (child.Name == "add") {
              XmlAttributeCollection attributes = child.Attributes;
              XmlAttribute keyAttribute = attributes["key"];
              XmlAttribute valueAttribute = attributes["value"];
              if (keyAttribute != null && valueAttribute != null &&
                keyAttribute.Value != null) {
                settings.Add(keyAttribute.Value, valueAttribute.Value);
              }
            }
          }
        }
      }
    }

    public void Save(string fileName) {

      XmlDocument doc = new XmlDocument();
      doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
      XmlElement configuration = doc.CreateElement("configuration");
      doc.AppendChild(configuration);
      XmlElement appSettings = doc.CreateElement("appSettings");
      configuration.AppendChild(appSettings);
      foreach (KeyValuePair<string, string> keyValuePair in settings) {
        XmlElement add = doc.CreateElement("add");
        add.SetAttribute("key", keyValuePair.Key);
        add.SetAttribute("value", keyValuePair.Value);
        appSettings.AppendChild(add);
      }

      byte[] file;
      using (var memory = new MemoryStream()) {
        using (var writer = new StreamWriter(memory, Encoding.UTF8)) {
          doc.Save(writer);
        }
        file = memory.ToArray();
      }

      string backupFileName = fileName + ".backup";
      if (File.Exists(fileName)) {
        try {
          File.Delete(backupFileName);
        } catch { }
        try {
          File.Move(fileName, backupFileName);
        } catch { }
      }

      using (var stream = new FileStream(fileName,
        FileMode.Create, FileAccess.Write))
      {
        stream.Write(file, 0, file.Length);
      }

      try {
        File.Delete(backupFileName);
      } catch { }
    }

    public bool Contains(string name) {
      return settings.ContainsKey(name);
    }

    public void SetValue(string name, string value) {
      settings[name] = value;
    }

    public string GetValue(string name, string value) {
      string result;
      if (settings.TryGetValue(name, out result))
        return result;
      else
        return value;
    }

    public void Remove(string name) {
      settings.Remove(name);
    }
  }
}
