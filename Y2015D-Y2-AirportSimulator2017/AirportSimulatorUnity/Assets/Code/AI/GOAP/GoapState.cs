namespace Deprecated
{
	using System.Reflection;

	public class GoapState
	{
		public bool HasBoardingPass;
		public bool HasCheckedIn;
		public bool HasBoardedPlane;

		public bool HasEaten;
		public bool HasFood;

		public bool HasDrunk;
		public bool HasDrinkableLiquid;

		public bool HasUsedToilet;

		public bool HasLookedAtMap;

		/// <summary>
		/// Copies the data of another GoapState object.
		/// </summary>
		/// <param name="target">The other GoapState object.</param>
		public void CopyTo(GoapState target)
		{
			foreach (FieldInfo field in typeof(GoapState).GetFields())
			{
				field.SetValue(target, field.GetValue(this));
			}
		}
	}
}
