namespace Deprecated
{
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using System.Linq;
	using System;

	namespace AI
	{
		[DisallowMultipleComponent]
		public class DrinkAction : GoapAction
		{
			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasDrunk,

			state => state.HasDrinkableLiquid
			};

			public override GoapGoal[] Preconditions
			{
				get { return _preconditions; }
			}

			protected override void Awake()
			{
				base.Awake();
			}

			protected override void OnExecute(GoapState state)
			{
				state.HasDrunk = true;
				state.HasDrinkableLiquid = false;
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