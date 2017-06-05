/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace OpenHardwareMonitor.Common {
  public class Settings /*: ISettings */ {
    protected readonly ISettings settings;

    public Settings(ISettings settings) {
      this.settings = settings;
    }

    public ISettings InnerSettings {
      get { return settings; }
    }

    public bool Contains(string name) {
      return settings.Contains(name);
    }

    public void SetValue(string name, string value) {
      settings.SetValue(name, value);
    }

    public string GetValue(string name, string value) {
      return settings.GetValue(name, value);
    }

    public void Remove(string name) {
      settings.Remove(name);
    }

    public bool Contains(Identifier identifier) {
      return settings.Contains(identifier.ToString());
    }

    public void SetValue(Identifier identifier, string value) {
      settings.SetValue(identifier.ToString(), value);
    }

    public string GetValue(Identifier identifier, string value) {
      return settings.GetValue(identifier.ToString(), value);
    }

    public void Remove(Identifier identifier) {
      settings.Remove(identifier.ToString());
    }

    public void SetValue(string name, int value) {
      settings.SetValue(name, value.ToString(CultureInfo.InvariantCulture));
    }

    public int GetValue(string name, int value) {
      int result;
      return int.TryParse(
        settings.GetValue(name, value.ToString(CultureInfo.InvariantCulture)),
        NumberStyles.Integer, CultureInfo.InvariantCulture,
        out result) ? result : value;
    }

    public void SetValue(Identifier identifier, int value) {
      SetValue(identifier.ToString(), value);
    }

    public int GetValue(Identifier identifier, int value) {
      return GetValue(identifier.ToString(), value);
    }

    public void SetValue(string name, float value) {
      settings.SetValue(name, value.ToString(CultureInfo.InvariantCulture));
    }

    public float GetValue(string name, float value) {
      float result;
      return float.TryParse(
        settings.GetValue(name, value.ToString(CultureInfo.InvariantCulture)),
        NumberStyles.Float, CultureInfo.InvariantCulture,
        out result) ? result : value;
    }

    public void SetValue(Identifier identifier, float value) {
      SetValue(identifier.ToString(), value);
    }

    public float GetValue(Identifier identifier, float value) {
      return GetValue(identifier.ToString(), value);
    }

    public void SetValue(string name, bool value) {
      settings.SetValue(name, value ? "true"  : "false");
    }

    public bool GetValue(string name, bool value) {
      return settings.GetValue(name, value ? "true"  : "false") == "true";
    }

    public void SetValue(Identifier identifier, bool value) {
      SetValue(identifier.ToString(), value);
    }

    public bool GetValue(Identifier identifier, bool value) {
      return GetValue(identifier.ToString(), value);
    }

    public void SetValue(string name, byte[] value) {
      settings.SetValue(name, Convert.ToBase64String(value));
    }

    public byte[] GetValue(string name) {
      return Convert.FromBase64String(settings.GetValue(name, string.Empty));
    }

    public void SetValue(Identifier identifier, byte[] value) {
      SetValue(identifier.ToString(), value);
    }

    public byte[] GetValue(Identifier identifier) {
      return GetValue(identifier.ToString());
    }

    public void SetValueCompressed(string name, byte[] value) {
      using(MemoryStream bufferStream = new MemoryStream()) {
        using(DeflateStream compressedStream = new DeflateStream(bufferStream, CompressionLevel.Optimal, true)) {
          compressedStream.Write(value, 0, value.Length);
        }
        settings.SetValue(name, Convert.ToBase64String(bufferStream.ToArray()));
      }
    }

    public byte[] GetValueCompressed(string name) {
      string str = settings.GetValue(name, null);
      try {
        byte[] data = Convert.FromBase64String(str);
        using(MemoryStream bufferStream = new MemoryStream()) {
          using(DeflateStream compressedStream =
                new DeflateStream(new MemoryStream(data), CompressionMode.Decompress)) {
            compressedStream.CopyTo(bufferStream);
          }
          return bufferStream.ToArray();
        }
      }
      catch {
      }

      return new byte[0];
    }

    public void SetValueCompressed(Identifier identifier, byte[] value) {
      SetValueCompressed(identifier.ToString(), value);
    }

    public byte[] GetValueCompressed(Identifier identifier) {
      return GetValueCompressed(identifier.ToString());
    }
  }
}
