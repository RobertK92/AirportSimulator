namespace Deprecated
{
	using UnityEngine;

	namespace AI
	{
		[DisallowMultipleComponent]
		public class CheckInAction : GoapAction
		{
			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasCheckedIn
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			protected override void Awake()
			{
				base.Awake();
				_target = GameObject.FindGameObjectWithTag(UnityConstants.Tags.Check_In);
			}

			protected override void OnExecute(GoapState state)
			{
				state.HasCheckedIn = true;
			}

			protected override float Duration
			{
				get
				{
					return 5.0f;
				}
			}
		}
	}
}
