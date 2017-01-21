namespace GOAP
{
	using System.Collections.Generic;
	using System.Linq;

	public class GoapPlanner
	{

		private IGoapAction[] _availableActions;

		private List<List<Node>> _availablePlans;

		public GoapPlanner(IGoapAction[] actions, GoapAgent agent)
		{
			_availableActions = actions;
			_availablePlans = new List<List<Node>>();
		}


		public bool MakePlan(Dictionary<GoapState, bool> currentState, Stack<IGoapAction> actions, Dictionary<GoapState, bool> goals)
		{
			Dictionary<GoapState, bool> testState = new Dictionary<GoapState, bool>();
			testState.Add(GoapState.HasBoardedPlane, false);
			testState.Add(GoapState.HasCheckedIn, false);
			testState.Add(GoapState.HasSecurityCheck, false);
			testState.Add(GoapState.HasUsedToilet, false);

			Dictionary<GoapState, bool> testGoal = new Dictionary<GoapState, bool>();
			testGoal.Add(GoapState.HasBoardedPlane, true);



			var node = new Node(null, testState, null, 0);
			List<Node> leaves = new List<Node>();

			var foundPlan = BuildGraph(node, leaves, _availableActions.ToList(), testGoal);
			if (!foundPlan) return false;


			List<Stack<IGoapAction>> plans = new List<Stack<IGoapAction>>();

			foreach (var leaf in leaves)
			{
				Stack<IGoapAction> stack = new Stack<IGoapAction>();
				var n = leaf;
				while (n != null)
				{
					if (n.action != null)
						stack.Push(n.action);

					n = n.parent;
				}

				plans.Add(stack);
			}



			//var n = leaves.Aggregate((Node)null, (acc, item) => acc == null || acc.cost > item.cost ? item : acc);
			//while (n != null)
			//{
			//	if (n.action != null)
			//		actions.Push(n.action);

			//	n = n.parent;
			//}

			return true;
		}

		public bool BuildGraph(Node parent, List<Node> leaves, List<IGoapAction> availableActions, Dictionary<GoapState, bool> goals)
		{
			bool success = false;

			foreach (var action in availableActions)
			{
				if (IsValid(parent.state, action.Preconditions))
				{
					var affectedState = GetAffectedState(parent.state, action.Effects);
					Node node = new Node(parent, affectedState, action, parent.cost + action.Cost);

					if (IsValid(affectedState, goals))
					{
						leaves.Add(node);
						success = true;
					}
					else
					{
						var actions = new List<IGoapAction>(availableActions.Where(x => x != action));
						if (BuildGraph(node, leaves, actions, goals))
							success = true;
					}
				}
			}

			return success;
		}

		private bool IsValid(Dictionary<GoapState, bool> currentState, Dictionary<GoapState, bool> preconditions)
		{
			bool valid = true;
			foreach (var condition in preconditions)
			{
				bool match = false;
				foreach (var state in currentState)
				{
					if (state.Equals(condition))
					{
						match = true;
						break;
					}
				}
				if (!match)
					valid = false;
			}
			return valid;
		}

		private Dictionary<GoapState, bool> GetAffectedState(Dictionary<GoapState, bool> currentState, Dictionary<GoapState, bool> actionEffect)
		{
			var copiedState = new Dictionary<GoapState, bool>();
			foreach (var state in currentState)
				copiedState.Add(state.Key, state.Value);

			bool match = false;
			foreach (var change in actionEffect)
			{
				foreach (var state in copiedState)
				{
					if (state.Key == change.Key)
					{
						match = true;
						break;
					}
				}

				if (match)
				{
					copiedState.Remove(change.Key);
					copiedState.Add(change.Key, change.Value);
				}
				else
				{
					copiedState.Add(change.Key, change.Value);
				}
			}

			return copiedState;
		}

	}
}