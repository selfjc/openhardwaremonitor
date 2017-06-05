/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Text;
using OpenHardwareMonitor.Common;

namespace OpenHardwareMonitor.Hardware {

  internal struct ParameterDescription {
    private readonly string name;
    private readonly string description;
    private readonly float defaultValue;    

    public ParameterDescription(string name, string description, 
      float defaultValue) {
      this.name = name;
      this.description = description;
      this.defaultValue = defaultValue;
    }

    public string Name { get { return name; } }

    public string Description { get { return description; } }

    public float DefaultValue { get { return defaultValue; } }
    
    public string IdentifierName {
      get {
        StringBuilder b = new StringBuilder();
        foreach(char c in name) {
          if((c <= '\u007f') && char.IsLetterOrDigit(c))
            b.Append(char.ToLowerInvariant(c));
        }
        return b.ToString();
      }
    }
  }

  internal class Parameter : IParameter {
    private readonly Identifier identifier;
    private readonly ISensor sensor;
    private ParameterDescription description;
    private float value;
    private bool isDefault;
    private readonly Settings settings;

    public Parameter(ParameterDescription description, ISensor sensor, 
      ISettings settings) 
    {
      this.identifier = sensor.Identifier + "parameter" + description.IdentifierName;
      this.sensor = sensor;
      this.description = description;
      this.settings = new Settings(settings);
      this.isDefault = !this.settings.Contains(Identifier);
      this.value = this.isDefault ? description.DefaultValue : 
        this.settings.GetValue(Identifier, description.DefaultValue);
    }

    public ISensor Sensor {
      get {
        return sensor;
      }
    }

    public Identifier Identifier {
      get { return identifier; }
    }

    public string Name { get { return description.Name; } }

    public string Description { get { return description.Description; } }

    public float Value {
      get {
        return value;
      }
    }

    public float DefaultValue { 
      get { return description.DefaultValue; } 
    }

    public bool IsDefault {
      get { return isDefault; }
    }

    public void SetValue(float value) {
      isDefault = false;
      this.value = value;
      settings.SetValue(Identifier, value);      
    }
    
    public void SetDefault() {
      isDefault = true;
      value = description.DefaultValue;
      settings.Remove(Identifier);
    }
    
    public void Accept(IVisitor visitor) {
      if (visitor == null)
        throw new ArgumentNullException("visitor");
      visitor.VisitParameter(this);
    }

    public void Traverse(IVisitor visitor) { }
  }
}
