namespace AI
{
	namespace GOAPv3
	{
		using System;
		using System.Linq;
		using UnityEngine;

		public class CollectLuggageAction : GoapAction
		{
			public override int Cost { get { return 10; } }

			public override float Duration { get { return 5f; } }

			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasCollectedLuggage,

			state => state.HasArrivalSecurityCheck
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			public override bool Init(GoapAgent agent)
			{
				if (IsInitialized) return true;

				var targets = Airport.Instance.GetTargets<LuggageImpl>(LocationType.LUGGAGE_CAROUSSEL, agent.State.CurrentZone);
				var position = transform.position;
				_target = targets.Where(x => x.isOpen).ToArray().Where(x => x.queue.CanQueue).Aggregate((LuggageImpl)null, (acc, item) => acc == null || (acc.position - position).magnitude > (item.position - position).magnitude ? item : acc);

				if (_target == null)
					return false;

				IsInitialized = true;
				_currentDuration = Duration;

				return true;
			}

			public override void ChangeState(GoapState state)
			{
				state.HasCollectedLuggage = true;
			}

			public override void NotifyAirport() { }

			public override void Reset()
			{
				_currentDuration = 0f;
				IsInitialized = false;
			}
		}
	}
}
