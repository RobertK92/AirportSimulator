namespace Deprecated
{
	using UnityEngine;

	namespace AI
	{
		[DisallowMultipleComponent]
		public class BoardPlaneAction : GoapAction
		{
			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasBoardedPlane,

			state => state.HasCheckedIn,
			state => state.HasBoardingPass
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			protected override void Awake()
			{
				base.Awake();
				_target = GameObject.FindGameObjectWithTag(UnityConstants.Tags.Gate_6);
			}

			protected override void OnExecute(GoapState state)
			{
				state.HasBoardedPlane = true;
			}

			protected override float Duration
			{
				get
				{
					return 0.0f;
				}
			}
		}
	}
}
