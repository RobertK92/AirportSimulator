namespace Deprecated
{
	using UnityEngine;

	namespace AI
	{
		[DisallowMultipleComponent]
		public class LookAtMapAction : GoapAction
		{
			public override GoapGoal[] Preconditions
			{
				get { return null; }
			}

			protected override void Awake()
			{
				base.Awake();
				_target = GameObject.FindGameObjectWithTag(UnityConstants.Tags.Map);
			}

			protected override void OnExecute(GoapState state)
			{
				state.HasLookedAtMap = true;
			}

			protected override float Duration
			{
				get
				{
					return 3.0f;
				}
			}
		}
	}
}