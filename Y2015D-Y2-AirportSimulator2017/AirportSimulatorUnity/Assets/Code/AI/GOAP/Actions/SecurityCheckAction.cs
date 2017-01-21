namespace Deprecated
{
	using UnityEngine;

	namespace AI
	{
		[DisallowMultipleComponent]
		public class SecurityCheckAction : GoapAction
		{
			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasBoardingPass,


			state => state.HasCheckedIn
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			protected override void Awake()
			{
				base.Awake();
				_target = GameObject.FindGameObjectWithTag(UnityConstants.Tags.Customs);
			}

			protected override void OnExecute(GoapState state)
			{
				state.HasBoardingPass = true;
			}

			protected override float Duration
			{
				get
				{
					return 8.0f;
				}
			}
		}
	}
}
