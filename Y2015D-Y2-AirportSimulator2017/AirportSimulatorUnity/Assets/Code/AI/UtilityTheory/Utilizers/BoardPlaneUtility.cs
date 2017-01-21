using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AI
{
    public class BoardPlaneUtility : Utility
    {
        public override LongTermGoal Action
        {
            get
            {
                return LongTermGoal.BoardPlane;
            }
        }

        public override float UtilityValue
        {
            get
            {
                return curve.Evaluate(stats.BoardPlane);
            }
        }
    }
}
