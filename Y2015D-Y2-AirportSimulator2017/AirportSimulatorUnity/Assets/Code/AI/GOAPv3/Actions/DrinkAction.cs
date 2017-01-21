namespace AI
{
	namespace GOAPv3
	{
		using System;
		using UnityEngine;

		public class DrinkAction : GoapAction
		{
			public override int Cost { get { return 5; } }

			public override float Duration { get { return 5f; } }

			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasDrunk,

			state => state.HasLiquid
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			public override bool Init(GoapAgent agent)
			{
				_currentDuration = Duration;
				return true;
			}

			public override void ChangeState(GoapState state)
			{
				state.HasDrunk = true;
				state.HasLiquid = false;
			}

			public override void NotifyAirport() { }

			public override void Reset()
			{
				_currentDuration = 0f;
			}
		}
	}
}
