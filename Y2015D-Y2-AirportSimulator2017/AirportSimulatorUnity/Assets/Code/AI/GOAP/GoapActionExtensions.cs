namespace Deprecated
{
	using System.Collections.Generic;
	using System.Linq;

	public static class GoapActionExtensions
	{
		public static List<GoapAction> GetMatching(this IEnumerable<GoapAction> actions, GoapState state, GoapGoal[] goals)
		{
			var matchingActions = new List<GoapAction>();
			var copiedState = new GoapState();

			foreach (var action in actions)
			{
				state.CopyTo(copiedState);
				action.Execute(copiedState);

				if (goals.Any(g => g(copiedState)))
					matchingActions.Add(action);
			}

			return (matchingActions.Count > 0) ? matchingActions : new List<GoapAction>(0);
		}
	}
}