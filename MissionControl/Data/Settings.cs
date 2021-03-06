﻿using System;
using System.Collections.Generic;
using System.Globalization;

namespace MissionControl.Data
{

    public class Property
    {
        public string PreferenceKey { get; }

        public Property(string prefKey)
        {
            PreferenceKey = prefKey;
        }

    }

    public class StringProperty : Property {

        public string Value { get; set; }

        public StringProperty (string prefKey) : base(prefKey) { }

        public override string ToString()
        {
            return Value;
        }
    }

    public class IntegerProperty : Property
    {

        public int Value { get; set; }

        public IntegerProperty (string prefKey) : base(prefKey) { }

        public override string ToString()
        {
            return Convert.ToString(Value);
        }
    }
    
    public class FloatProperty : Property
    {

        public float Value { get; set; }

        public FloatProperty(string prefKey) : base(prefKey) { }

        public override string ToString()
        {
            return Convert.ToString(Value, CultureInfo.InvariantCulture);
        }
    }

    public class BoolProperty : Property
    {

        public bool Value { get; set; }

        public BoolProperty(string prefKey) : base(prefKey) { }

        public override string ToString()
        {
            return Convert.ToString(Value);
        }
    }

    public class Settings
    {
        // Fluid
        public FloatProperty OxidCV { get; } = new FloatProperty("OxidFluidCV");
        public FloatProperty OxidDensity { get; } = new FloatProperty("OxidFluidDensity");
        public FloatProperty FuelCV { get; } = new FloatProperty("FuelFluidCV");
        public FloatProperty FuelDensity { get; } = new FloatProperty("FuelFluidDensity");
        public FloatProperty TodayPressure { get; } = new FloatProperty("TodayPressure");

        // System
        public StringProperty LogFilePath { get; } = new StringProperty("LogFolder");
        public StringProperty PortName { get; } = new StringProperty("PortName");
        public IntegerProperty BaudRate { get; } = new IntegerProperty("BaudRate");

        public BoolProperty ShowSettingsOnStartup { get; } = new BoolProperty("ShowSettingsOnStartup");
        
        // Visual
        public BoolProperty ShowAbsolutePressure { get; } = new BoolProperty("ShowRelativePressure");

        // Auto Sequence Parameters
        public StringProperty AutoParameters { get; } = new StringProperty("AutoParameters");
        
        public List<Property> Properties()
        {
            return new List<Property> { LogFilePath, PortName, BaudRate, OxidCV, OxidDensity, FuelCV, FuelDensity, TodayPressure, ShowAbsolutePressure, ShowSettingsOnStartup, AutoParameters };
        }
        
        public Dictionary<string, Property> PropertiesByID()
        {
            Dictionary<string, Property> dict = new Dictionary<string, Property>();
            foreach (Property c in Properties())
            {
                dict.Add(c.PreferenceKey, c);
            }

            return dict;
        }


    }
    
   
}
