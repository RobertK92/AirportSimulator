namespace AI
{
	namespace GOAPv3
	{
		using System.Linq;
		using UnityEngine;

		public class LookAtMapAction : GoapAction
		{
			public override int Cost { get { return 10; } }

			public override float Duration { get { return 20f; } }

			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasLookedAtMap
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			public override bool Init(GoapAgent agent)
			{
				if (IsInitialized) return true;

				var targets = Airport.Instance.GetTargets<InfoDeskImpl>(LocationType.INFODESK, agent.State.CurrentZone);
				var position = agent.transform.position;
				_target = targets.Where(x => x.isOpen).Where(x => x.queue.CanQueue).Aggregate((InfoDeskImpl)null, (acc, item) => acc == null || (acc.position - position).magnitude > (item.position - position).magnitude ? item : acc);

				if (_target == null)
					return false;

				IsInitialized = true;
				_currentDuration = Duration;

				return true;
			}

			public override void ChangeState(GoapState state)
			{
				state.HasLookedAtMap = true;
			}

			public override void NotifyAirport() { }

			public override void Reset()
			{
				IsInitialized = false;
				_currentDuration = 0f;
			}
		}
	}
}
