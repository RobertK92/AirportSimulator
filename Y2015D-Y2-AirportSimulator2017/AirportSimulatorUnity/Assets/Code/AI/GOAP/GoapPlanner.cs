namespace Deprecated
{
	using System.Linq;
	using System.Collections.Generic;

	namespace AI
	{
		public class GoapPlanner
		{
			private GoapAction[] _availableActions;
			private GoapAgent _agent;

			private List<List<Node>> _availablePlans;

			public GoapPlanner(GoapAction[] actions, GoapAgent agent)
			{
				_availableActions = actions;
				_agent = agent;
				_availablePlans = new List<List<Node>>();
			}

			/// <summary>
			/// Creates a plan of available actions and pushes them into the action stack.
			/// </summary>
			/// <param name="currentState">The current state of the agent.</param>
			/// <param name="actionStack">The list of populated actions. Should be empty when passed.</param>
			/// <param name="goals">The current goal(s) of the agent.</param>
			/// <param name="cost">The cost of the action. Should be 0 when passed.</param>
			/// <returns></returns>
			public bool MakePlan(GoapState currentState, ref Stack<GoapAction> actionStack, GoapGoal[] goals, int cost)
			{
				List<Node> leaves = new List<Node>();
				var node = new Node(null, currentState, null);

				GoapGoal[] test = new GoapGoal[]
				{
				state => state.HasBoardedPlane
				};

				var actions = _availableActions.GetMatching(currentState, test);

				BuildTree(node, leaves, actions, test);

				leaves.Reverse();
				foreach (var leaf in leaves)
				{
					actionStack.Push(leaf.action);
				}

				return true;
			}

			private bool BuildTree(Node parent, List<Node> leaves, List<GoapAction> availableActions, GoapGoal[] goals)
			{
				var copiedState = new GoapState();

				foreach (var action in availableActions)
				{
					parent.state.CopyTo(copiedState);
					action.Execute(copiedState);

					if (!goals.Any(g => g(copiedState)))
						continue;

					var node = new Node(parent, copiedState, action);

					if (action.Preconditions.All(g => g(parent.state)))
					{
						if (leaves.Exists(x => x.action.GetType() == node.action.GetType()))
							continue;

						leaves.Add(node);
						var actions = availableActions.Where(x => x != action).ToList();
						BuildTree(parent, leaves, actions, goals);

						continue;
					}
					else
					{
						var actions = _availableActions.GetMatching(node.state, action.Preconditions);
						BuildTree(node, leaves, actions, action.Preconditions);

						if (goals.All(g => g(copiedState)))
						{
							_availablePlans.Add(leaves);

							return true;
						}
					}
				}

				return false;
			}



			private void BuildTree(Node parent, ref List<Node> leaves, List<GoapAction> availableActions, GoapGoal[] goals)
			{
				var copiedState = new GoapState();

				foreach (var action in availableActions)
				{
					parent.state.CopyTo(copiedState);
					action.Execute(copiedState);

					if (!goals.Any(g => g(copiedState)))
						continue;

					var node = new Node(parent, copiedState, action);

					if (action.Preconditions == null || action.Preconditions.All(g => g(copiedState)))
					{
						if (leaves.Exists(x => x.action.GetType() == node.action.GetType()))
							continue;
						leaves.Add(node);
						availableActions.Remove(node.action);
						action.Execute(parent.state);
						BuildTree(parent, ref leaves, availableActions, goals);
						return;
					}
					else
					{
						var actions = _availableActions.GetMatching(parent.state, action.Preconditions);
						BuildTree(node, ref leaves, actions, action.Preconditions);

						if (action.Preconditions.All(g => g(node.state)))
						{
							leaves.Add(node);
							action.Execute(parent.state);
							return;
						}
						else
						{
							UnityEngine.Debug.LogError("Could not create plan. Failed action: " + action.GetType().Name);
						}
					}
				}
				if (leaves.Count == 0)
				{
				//	UnityEngine.Debug.LogError(string.Format(
				//		"Could not create plan. No available actions for long term goal: <color=green>{0}</color>",
				//		_agent.GoalManager.CurrentGoal.ToString()));
				}

				return;
			}
		}

	}
}