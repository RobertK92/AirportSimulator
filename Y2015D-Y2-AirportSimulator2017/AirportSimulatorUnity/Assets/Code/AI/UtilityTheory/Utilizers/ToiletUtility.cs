using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AI
{
    public class ToiletUtility : Utility
    {
        public override LongTermGoal Action
        {
            get
            {
                return LongTermGoal.Toilet;
            }
        }

        public override float UtilityValue
        {
            get
            {
                return curve.Evaluate(stats.Toilet);
            }
        }
    }
}
