﻿using System;
using System.Globalization;

namespace MissionControl.Data.Components
{
    public class PressureComponent : SensorComponent, ILoggable
    {
        private int _rawPressure = 373;

        private const float _minADC = 372.36f;
        private const float _maxADC = 1861.81f;
        private int _maxPressure = 50;
        
        private readonly Calibrator _calibrator;
        private readonly Uncalibrator _uncalibrator;
        private float Calibrated => _calibrator(_rawPressure);
        
        public override string TypeName => "Pressure";
        public override int ByteSize => 2;
        public override bool Signed => false;
        public override int Raw => _rawPressure;

        public PressureComponent(byte boardID, string graphicID, string name, int maxPressure) : base(boardID, graphicID, name)
        {
            _maxPressure = maxPressure;
            _calibrator = x => { return _maxPressure * (x - _minADC) / (_maxADC - _minADC); };
           _uncalibrator = x => { return ((x * (_maxADC - _minADC) / _maxPressure) + _minADC); };
        }
        
        public PressureComponent(byte boardID, string graphicID, string name, int maxPressure, Calibrator calibrator, Uncalibrator uncalibrator) : base(boardID, graphicID, name)
        {
            _maxPressure = maxPressure;
            _calibrator = calibrator;
            _uncalibrator = uncalibrator;
        }

        public float Relative()
        {
            return Calibrated;
        }

        public float Absolute(float atmosphere) 
        { 
            return Calibrated + atmosphere;
        }

        public override void Set(int val)
        {
            _rawPressure = val;
        }

        public string ToLog()
        {
            return ToRounded(Relative(), 4);
        }

        public override string ToDisplay()
        {
            return ToRounded(Relative(), 2);
        }

        public string LogHeader()
        {
            return Name + " [barg]";
        }

        public float UncalibratedValue(float value)
        {
            return _uncalibrator(value - PreferenceManager.Manager.Preferences.Fluid.TodaysPressure);
        }
    }
}
