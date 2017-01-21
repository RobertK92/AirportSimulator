using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AI
{
    public abstract class Utility : MonoBehaviour
    {
        public UtilityAgent stats;
        public AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        public int priority;

        public abstract float UtilityValue { get; }
        public abstract LongTermGoal Action { get; }
    }

}