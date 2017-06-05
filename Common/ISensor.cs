/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
using System.IO;
using OpenHardwareMonitor.Collections;

namespace OpenHardwareMonitor.Common {

  public enum SensorType {
    Voltage, // V
    Clock, // MHz
    Temperature, // °C
    Load, // %
    Fan, // RPM
    Flow, // L/h
    Control, // %
    Level, // %
    Factor, // 1
    Power, // W
    Data, // GB = 2^30 Bytes    
    SmallData, // MB = 2^20 Bytes
  }

  public struct SensorValue {
    private readonly float value;
    private readonly DateTime time;

    public SensorValue(float value, DateTime time) {
      this.value = value;
      this.time = time;
    }

    public float Value { get { return value; } }
    public DateTime Time { get { return time; } }
    
    public static byte[] Pack(IEnumerable<SensorValue> values) {
      using(var writerTime = new BinaryWriter(new ShuffleStream(sizeof(Int64)))) {
        using(var writerValue = new BinaryWriter(new ShuffleStream(sizeof(Single)))) {
          Int32 count = 0;
          Int64 lastt = 0;
          foreach (SensorValue sensorValue in values) {
            Int64 t = sensorValue.Time.ToBinary();
            writerTime.Write(t - lastt);
            lastt = t;
            writerValue.Write((Single)sensorValue.Value);
            count++;
          }
          byte[] result = new byte[sizeof(Int32) + count * sizeof(Int64) + count * sizeof(Single)];
          BitConverter.GetBytes(count).CopyTo(result, 0);          
          ((ShuffleStream)writerTime.BaseStream).ToArray().CopyTo(result, sizeof(Int32));
          ((ShuffleStream)writerValue.BaseStream).ToArray().CopyTo(result, sizeof(Int32) + count * sizeof(Int64));
          return result;
        }
      }
    }
    
    public static IEnumerable<SensorValue> Unpack(byte[] data) {
      Int32 count = BitConverter.ToInt32(data, 0);
      var result = new List<SensorValue>();
      
      using(var readerTime = new BinaryReader(new UnshuffleStream(data, sizeof(Int32), count * sizeof(Int64), sizeof(Int64)))) {
        using(var readerValue = new BinaryReader(new UnshuffleStream(data, sizeof(Int32) + count * sizeof(Int64), count * sizeof(Single), sizeof(Single)))) {
          Int64 t = 0;
          for (int i = 0; i < count; i++) {
            t += readerTime.ReadInt64();
            var time = DateTime.FromBinary(t);
            var value = readerValue.ReadSingle();
            result.Add(new SensorValue(value, time));
          }
        }
      }
      
      return result;      
    }
  }

  public interface ISensor : IElement, IVisitable {

    IHardware Hardware { get; }

    SensorType SensorType { get; }

    string Name { get; set; }
    int Index { get; }

    bool IsDefaultHidden { get; }

    IReadOnlyArray<IParameter> Parameters { get; }

    float? Value { get; }
    float? Min { get; }
    float? Max { get; }

    void ResetMin();
    void ResetMax();

    IEnumerable<SensorValue> Values { get; }

    IControl Control { get; }
  }

}
