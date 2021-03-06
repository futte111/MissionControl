﻿using System;
namespace MissionControl.Data.Components
{
    public abstract class SensorComponent : MeasuredComponent, IWarningLimits
    {
        public string PrefMinName { get { return BoardID + "_MIN_LIMIT"; } }
        public string PrefMaxName { get { return BoardID + "_MAX_LIMIT"; } }

        public float MaxLimit { get; set; } = float.NaN;
        public float MinLimit { get; set; } = float.NaN;

        protected SensorComponent(byte boardID, string graphicID, string name) : base(boardID, graphicID, name)
        {
        }

        public bool IsNominal(float value)
        {
            bool nonNominal = false;
            if (!float.IsNaN(MinLimit)) { nonNominal |= value < MinLimit; }
            if (!float.IsNaN(MaxLimit)) { nonNominal |= value > MaxLimit; }
            return !nonNominal;
        }
    }
}
