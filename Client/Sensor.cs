/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.Collections.Generic;
using System.Management;
using OpenHardwareMonitor.Collections;
using OpenHardwareMonitor.Common;

namespace OpenHardwareMonitor.Client {
  using WMISensor = WMI.Sensor;
  using WMIParameter = WMI.Parameter;
  using WMIControl = WMI.Control;

  internal class Sensor : Element, ISensor {
    private readonly IHardware hardware;
    private readonly SensorType sensorType;
    private readonly string defaultName;
    private string name;
    private readonly int index;
    private readonly bool isDefaultHidden;
    private readonly ReadOnlyArray<IParameter> parameters;
    private readonly Control control;
    private float? currentValue;
    private float? minValue;
    private float? maxValue;
    private readonly RingCollection<SensorValue> values = new RingCollection<SensorValue>();

    private float sum;
    private int count;

    public Sensor(ManagementScope scope, ManagementPath path, Hardware hardware, ISettings settings)
      : this(new WMISensor(scope, path), hardware, settings) {
    }

    public Sensor(ManagementScope scope, string identifer, Hardware hardware, ISettings settings)
      : this(new WMISensor(scope, identifer), hardware, settings) {
    }

    private Sensor(WMISensor instance, Hardware hardware, ISettings settings)
      : base(instance, settings) {

      this.hardware = hardware;
      sensorType = (SensorType)Enum.Parse(typeof(SensorType), instance.SensorType);
      defaultName = instance.Name;
      name = defaultName;
      index = instance.Index;
      isDefaultHidden = instance.IsDefaultHidden;
      List<IParameter> parameterList = new List<IParameter>();
      foreach(ManagementPath parameter in instance.Parameters)
        parameterList.Add(new Parameter(instance.Scope, parameter, this, settings));
      this.parameters = new ReadOnlyArray<IParameter>(parameterList.ToArray());
      if (instance.Control != null)
        this.control = new Control(instance.Scope, instance.Control, settings);

      GetSensorValuesFromSettings();
    }

    private void SetSensorValuesToSettings() {
      Settings.SetValueCompressed(Identifier + "values", SensorValue.Pack(values));
    }

    private void GetSensorValuesFromSettings() {
      string name = (Identifier + "values").ToString();
      try {
        IEnumerable<SensorValue> settingsValues = SensorValue.Unpack(Settings.GetValueCompressed(name));
        foreach(SensorValue value in settingsValues)
          AppendValue(value);
      }
      catch {
      }
      if (values.Count > 0)
        AppendValue(float.NaN, DateTime.UtcNow);

      // remove the value string from the settings to reduce memory usage
      Settings.Remove(name);
    }

    public void Close() {
      SetSensorValuesToSettings();
    }

    private void AppendValue(float value, DateTime time) {
      AppendValue(new SensorValue(value, time));
    }

    private void AppendValue(SensorValue value) {
      if (values.Count >= 2 && Math.Abs(values.Last.Value - value.Value) < float.Epsilon &&
          Math.Abs(values[values.Count - 2].Value - value.Value) < float.Epsilon) {
        values.Last = value;
        return;
      }

      values.Append(value);
    }

    public void Update() {
      try {
        WMISensor instance = new WMISensor(Path);
        this.Value = instance.Value;
      } catch(ManagementException) {
      }
    }

    #region ISensor implementation
    public void ResetMin() {
      minValue = null;
    }

    public void ResetMax() {
      maxValue = null;
    }

    public IHardware Hardware {
      get { return hardware; }
    }

    public SensorType SensorType {
      get { return sensorType; }
    }

    public string Name {
      get {
        return name;
      }
      set {
        if (!string.IsNullOrEmpty(value)) {
          name = value;
          Settings.SetValue(Identifier + "name", name);
        } else {
          name = defaultName;
          Settings.Remove(Identifier + "name");
        }
      }
    }

    public int Index {
      get { return index; }
    }

    public bool IsDefaultHidden {
      get { return isDefaultHidden; }
    }

    public IReadOnlyArray<IParameter> Parameters {
      get { return parameters; }
    }

    public float? Value {
      get { return currentValue; }
      set {
        DateTime now = DateTime.UtcNow;
        while (values.Count > 0 && (now - values.First.Time).TotalDays > 1)
          values.Remove();

        if (value.HasValue) {
          sum += value.Value;
          count++;
          if (count == 4) {
            AppendValue(sum / count, now);
            sum = 0;
            count = 0;
          }
        }

        currentValue = value;
        if (minValue > value || !minValue.HasValue)
          minValue = value;
        if (maxValue < value || !maxValue.HasValue)
          maxValue = value;
      }
    }

    public float? Min {
      get { return minValue; }
    }

    public float? Max {
      get { return maxValue; }
    }

    public IEnumerable<SensorValue> Values {
      get { return values; }
    }

    public IControl Control {
      get { return control; }
    }
    #endregion

    #region IVisitable implementation
    public void Accept(IVisitor visitor)
    {
      if (visitor == null)
        throw new ArgumentNullException("visitor");
      visitor.VisitSensor(this);
    }

    public void Traverse(IVisitor visitor)
    {
      foreach (IParameter parameter in parameters)
        parameter.Accept(visitor);
    }
    #endregion
  }
}
