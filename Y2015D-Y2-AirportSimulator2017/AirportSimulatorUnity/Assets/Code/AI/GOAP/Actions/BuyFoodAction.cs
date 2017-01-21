namespace Deprecated
{
	using System;
	using UnityEngine;

	namespace AI
	{
		[DisallowMultipleComponent]
		public class BuyFoodAction : GoapAction
		{
			private GoapGoal[] _preconditions = new GoapGoal[]
			{
			state => !state.HasFood
			};

			public override GoapGoal[] Preconditions { get { return _preconditions; } }

			protected override void Awake()
			{
				base.Awake();


				//_target = FindObjectOfType<AlbertHeijn>().gameObject;
			}

			protected override void OnExecute(GoapState state)
			{
				state.HasFood = true;
				state.HasDrinkableLiquid = true;
			}

			protected override float Duration
			{
				get
				{
					return 2.0f;
				}
			}
		}
	}
}
