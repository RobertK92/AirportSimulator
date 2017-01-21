namespace AI
{
	namespace GOAPv3
	{
		using System.Linq;
		using UnityEngine;

		public class SecurityCheckAction : GoapAction
		{
			public override int Cost { get { return 10; } }

			public override float Duration { get { return 20f; } }

			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasSecurityCheck,

			state => state.HasCheckedIn,
			state => state.CurrentZone == Zone.MAIN_HALL
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			public override bool Init(GoapAgent agent)
			{
				if (IsInitialized) return true;

				var targets = Airport.Instance.GetTargets<SecurityImpl>(LocationType.SECURITY, agent.State.CurrentZone);
				var position = agent.transform.position;
				_target = targets.Where(x => x.isOpen).Where(x => x.queue.CanQueue).Aggregate((SecurityImpl)null, (acc, item) => acc == null || (acc.position - position).magnitude > (item.position - position).magnitude ? item : acc);

				if (_target == null)
					return false;

				IsInitialized = true;
				_currentDuration = Duration;

				return true;
			}

			public override void ChangeState(GoapState state)
			{
				state.HasSecurityCheck = true;
				state.CurrentZone = Zone.GATES;
			}

			public override void NotifyAirport()
			{
				Airport.Instance.Register(_target, GetComponent<Agent>());
			}


			public override void Reset()
			{
				IsInitialized = false;
				_currentDuration = 0f;
			}
		}
	}
}
