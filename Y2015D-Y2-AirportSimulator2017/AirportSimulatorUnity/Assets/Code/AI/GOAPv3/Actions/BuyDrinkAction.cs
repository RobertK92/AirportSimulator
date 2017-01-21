namespace AI
{
	namespace GOAPv3
	{
        using System.Collections.Generic;
        using System.Linq;
        using UnityEngine;

        public class BuyDrinkAction : GoapAction
		{
			public override int Cost { get { return 10; } }

			public override float Duration { get { return 20f; } }

			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasLiquid,
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			public override bool Init(GoapAgent agent)
			{
				if (IsInitialized) return true;

				var targets = Airport.Instance.GetTargets<CafeImpl>(LocationType.CAFE, agent.State.CurrentZone);
				var position = agent.transform.position;
                IEnumerable<CafeImpl> potentialTargets = targets.Where(x => x.isOpen).Where(x => x.queue.CanQueue);
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
				state.HasLiquid = true;
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
