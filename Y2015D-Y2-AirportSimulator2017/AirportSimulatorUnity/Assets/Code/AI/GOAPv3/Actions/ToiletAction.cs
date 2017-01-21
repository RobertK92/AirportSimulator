namespace AI
{
	namespace GOAPv3
	{
        using UnityEngine;
        using System.Linq;
        using System.Collections.Generic;
        public class ToiletAction : GoapAction
		{
			public override int Cost { get { return 10; } }

			public override float Duration { get { return 20f; } }

			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasUsedToilet
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			public override bool Init(GoapAgent agent)
			{
				if (IsInitialized) return true;

				var targets = Airport.Instance.GetTargets<ToiletImpl>(LocationType.TOILET, agent.State.CurrentZone);
				var position = agent.transform.position;
                IEnumerable<ToiletImpl> potentialTargets = targets.Where(x => x.isOpen).Where(x => x.queue.CanQueue);
                if (potentialTargets != null)
                {
                    _target = potentialTargets.OrderBy(x => x.queue.Queue.Count).ToList()[0];
                }
                
				if (_target == null)
					return false;
				
				IsInitialized = true;
				_currentDuration = Duration;

				return true;
			}

			public override void ChangeState(GoapState state)
			{
				state.HasUsedToilet = true;
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