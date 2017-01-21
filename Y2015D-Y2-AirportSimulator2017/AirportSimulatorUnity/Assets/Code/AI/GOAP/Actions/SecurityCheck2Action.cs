namespace Deprecated
{
	using System;
	using UnityEngine;

	namespace AI
	{
		[DisallowMultipleComponent]
		public class SecurityCheck2Action : GoapAction
		{
			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasBoardingPass,

			state => state.HasCheckedIn,
			state => state.HasFood
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }


			protected override void Awake()
			{
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
					return 0f;
				}
			}
		}
	}
}