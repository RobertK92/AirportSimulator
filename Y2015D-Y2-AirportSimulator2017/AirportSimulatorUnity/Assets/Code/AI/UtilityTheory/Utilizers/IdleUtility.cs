using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AI
{
    public class IdleUtility : Utility
    {
        [Range(0.0f, 1.0f)]
        public float threshold = 0.3f;

        public override LongTermGoal Action
        {
            get
            {
                return LongTermGoal.None;
            }
        }

        public override float UtilityValue
        {
            get
            {
                return threshold;
            }
        }
    }
}
