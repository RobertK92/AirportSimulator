namespace AI
{
	namespace GOAPv3
	{
		using UnityEngine;

		public class EatAction : GoapAction
		{
			public override int Cost { get { return 5; } }

			public override float Duration { get { return 5f; } }

			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasEaten,

			state => state.HasFood
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			public override bool Init(GoapAgent agent)
			{
				_currentDuration = Duration;
				return true;
			}

			public override void ChangeState(GoapState state)
			{
				state.HasEaten = true;
				state.HasFood = false;
			}

			public override void NotifyAirport() { }

			public override void Reset()
			{
				_currentDuration = 0f;
			}
		}
	}
}
