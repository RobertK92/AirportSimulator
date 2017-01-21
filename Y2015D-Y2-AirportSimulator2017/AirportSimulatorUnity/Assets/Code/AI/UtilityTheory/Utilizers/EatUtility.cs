using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AI
{
    public class EatUtility : Utility
    {
        public override float UtilityValue
        {
            get
            {
                return curve.Evaluate(stats.Hunger);
            }
        }

        public override LongTermGoal Action
        {
            get
            {
                return LongTermGoal.Eat;
            }
        }
    }
}
