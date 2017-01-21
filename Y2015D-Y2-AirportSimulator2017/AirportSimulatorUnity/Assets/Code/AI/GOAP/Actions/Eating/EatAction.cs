namespace Deprecated
{
	using System;
	using UnityEngine;

	namespace AI
	{
		[DisallowMultipleComponent]
		public class EatAction : GoapAction
		{
			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasEaten,

			state => state.HasFood
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			protected override void Awake()
			{
				base.Awake();
			}

			protected override void OnExecute(GoapState state)
			{
				state.HasEaten = true;
				state.HasFood = false;
			}

			protected override float Duration
			{
				get
				{
					return 0.0f;
				}
			}
		}
	}
}
