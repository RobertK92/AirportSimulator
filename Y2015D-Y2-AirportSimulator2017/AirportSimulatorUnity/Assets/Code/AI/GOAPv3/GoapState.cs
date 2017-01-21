namespace AI
{
	using System;

	namespace GOAPv3
	{
		public class GoapState
		{
			public bool HasCheckedIn;
			public bool HasSecurityCheck;
			public bool HasArrivalSecurityCheck;
			public bool HasDepositedLuggage;
			public bool HasCollectedLuggage;
			public bool HasExitedAirport;
			public bool HasBoardedPlane;

			public bool HasEaten;
			public bool HasDrunk;

			public bool HasFood;
			public bool HasLiquid;

			public bool HasUsedToilet;

			public bool HasLookedAtMap;

			public Zone CurrentZone { get; set; }

			/// <summary>
			/// Copies the data of another GoapState object.
			/// </summary>
			/// <param name="target">The other GoapState object.</param>
			public void CopyTo(GoapState target)
			{
				foreach (var field in typeof(GoapState).GetFields())
				{
					field.SetValue(target, field.GetValue(this));
				}
			}

			public static GoapState operator |(GoapState lhs, GoapState rhs)
			{
				var state = new GoapState();
				foreach (var field in typeof(GoapState).GetFields())
				{
					var current = (bool)field.GetValue(lhs) | (bool)field.GetValue(rhs);
					field.SetValue(state, current);
				}
				return state;
			}

			public GoapState MergeState(GoapState state)
			{
                return new GoapState
                {
                    HasCheckedIn = HasCheckedIn | state.HasCheckedIn,
                    HasSecurityCheck = HasSecurityCheck | state.HasSecurityCheck,
                    HasArrivalSecurityCheck = HasArrivalSecurityCheck | state.HasArrivalSecurityCheck,
                    HasDepositedLuggage = HasDepositedLuggage | state.HasDepositedLuggage,
                    HasCollectedLuggage = HasCollectedLuggage | state.HasCollectedLuggage,
                    HasExitedAirport = HasExitedAirport | state.HasExitedAirport,
                    HasBoardedPlane = HasBoardedPlane | state.HasBoardedPlane,

                    HasEaten = HasEaten | state.HasEaten,
                    HasDrunk = HasDrunk | state.HasDrunk,

                    HasFood = HasFood | state.HasFood,
                    HasLiquid = HasLiquid | state.HasLiquid,

                    HasUsedToilet = HasUsedToilet | state.HasUsedToilet,

                    HasLookedAtMap = HasLookedAtMap | state.HasLookedAtMap,
                    CurrentZone = CurrentZone
				};
			}
		}
	}
}
