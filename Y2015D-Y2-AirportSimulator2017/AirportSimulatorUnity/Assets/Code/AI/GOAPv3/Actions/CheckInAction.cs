namespace AI
{
	namespace GOAPv3
	{
		using System;
		using UnityEngine;

		public class CheckInAction : GoapAction
		{
			public override int Cost { get { return 10; } }

			public override float Duration { get { return 20f; } }

			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasCheckedIn,
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			public override bool Init(GoapAgent agent)
			{
				if (IsInitialized) return true; 

				var flight = GetComponent<Agent>().flight;
				_target = Airport.Instance.GetCheckInDesk(flight);

				if (_target == null)
					return false;

				IsInitialized = true;
				_currentDuration = Duration;

				return true;
			}

			public override void ChangeState(GoapState state)
			{
				state.HasCheckedIn = true;
			}

			public override void NotifyAirport()
			{
				Airport.Instance.Register(_target, GetComponent<Agent>());
			}

			public override void Reset()
			{
				_currentDuration = 0f;
				IsInitialized = false;
			}
		}
	}
}
