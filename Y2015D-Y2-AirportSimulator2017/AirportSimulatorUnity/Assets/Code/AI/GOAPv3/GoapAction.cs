namespace AI
{
	namespace GOAPv3
	{
		using System;
		using UnityEngine;

		public abstract class GoapAction : MonoBehaviour, IGoapAction
		{
			public System.Action<System.Type, GoapState> OnCompleted = delegate { };

			protected float _currentDuration;

			/// <summary>
			/// True if the action has been initialized;
			/// </summary>
			public bool IsInitialized { get; set; }

			public abstract int Cost { get; }

			public abstract float Duration { get; }

			public abstract GoapGoal[] Preconditions { get; }

			protected LocationImpl _target;

			public LocationImpl Target { get { return _target; } }

			public abstract bool Init(GoapAgent agent);

			public abstract void ChangeState(GoapState state);

			public virtual bool Countdown()
			{
				_currentDuration -= Time.deltaTime;
				if (_currentDuration > 0)
					return false;

				return true;
			}

			public abstract void NotifyAirport();

			public virtual bool Execute(GoapState state)
			{
				if (!Countdown()) return false;

				ChangeState(state);
				NotifyAirport();
				OnCompleted(GetType(), state);
				return true;
			}

			public abstract void Reset();
		}
	}
}