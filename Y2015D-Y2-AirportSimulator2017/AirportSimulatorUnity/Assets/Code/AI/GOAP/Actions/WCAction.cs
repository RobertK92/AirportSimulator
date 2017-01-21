namespace Deprecated
{
	using UnityEngine;

	namespace AI
	{
		[DisallowMultipleComponent]
		public class WCAction : GoapAction
		{
			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasUsedToilet
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			protected override void Awake()
			{
				base.Awake();
				_target = GameObject.FindGameObjectWithTag(UnityConstants.Tags.Toilet);
			}

			protected override void OnExecute(GoapState state)
			{
				state.HasUsedToilet = true;
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
