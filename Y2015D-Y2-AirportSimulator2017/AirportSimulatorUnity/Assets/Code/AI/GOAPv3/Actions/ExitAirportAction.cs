namespace AI
{
	namespace GOAPv3
	{
		using System.Linq;
		using UnityEngine;

		public class ExitAirportAction : GoapAction
		{
			public override int Cost { get { return 10; } }

			public override float Duration { get { return 0f; } }

			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasExitedAirport,

			state => state.HasArrivalSecurityCheck,
			state => state.HasCollectedLuggage
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			public override bool Init(GoapAgent agent)
			{
				if (IsInitialized) return true;

				var targets = Airport.Instance.GetTargets<ExitImpl>(LocationType.EXIT, Zone.MAIN_HALL);
				var position = transform.position;
				_target = targets.Where(x => x.isOpen).Where(x => x.queue.CanQueue).Aggregate((ExitImpl)null, (acc, item) => acc == null || (acc.position - position).magnitude > (item.position - position).magnitude ? item : acc);

				if (_target == null)
					return false;

				IsInitialized = true;
				_currentDuration = Duration;

				return true;
			}

			public override void ChangeState(GoapState state)
			{
				state.HasExitedAirport = true;
				state.CurrentZone = Zone.MAIN_HALL;
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
