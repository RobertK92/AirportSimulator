namespace AI
{
	namespace GOAPv3
	{
		using System.Collections.Generic;
		using System.Linq;

		public class GoapPlanner
		{
			private GoapAction[] _availableActions;

			/// <summary>
			/// Creates a goal-oriented action planner.
			/// </summary>
			/// <param name="actions">A collection of all actions.</param>
			public GoapPlanner(GoapAction[] actions)
			{
				_availableActions = actions;
			}

			/// <summary>
			/// Creates a plan of actions for a specified goal. Returns true if a plan was found.
			/// </summary>
			/// <param name="currentState">The current state of the agent.</param>
			/// <param name="airportState">The current state of the airport.</param>
			/// <param name="actionOutput">Ouputs the plan of actions for the specified goal. Should be empty</param>
			/// <param name="goals">The end goals of the plan.</param>
			/// <returns>Returns true if a plan was found.</returns>
			public bool MakePlan(GoapAgent agent, Stack<GoapAction> actionOutput, GoapGoal[] goals)
			{
				var node = new Node(null, agent.State, null, 0);
				var leaves = new List<Node>();
				var actions = _availableActions.ToList();
				foreach (var action in actions) action.Reset();

				var foundPlan = BuildGraph(node, leaves, actions, goals);
				if (!foundPlan) return false;

				//List<Stack<GoapAction>> plans = new List<Stack<GoapAction>>();

				//foreach (var leaf in leaves)
				//{
				//	Stack<GoapAction> nodeList = new Stack<GoapAction>();
				//	Node l = leaf;
				//	while (l != null)
				//	{
				//		if (l.action != null)
				//			nodeList.Push(l.action);

				//		l = l.parent;
				//	}
				//	plans.Add(nodeList);
				//}


				var n = leaves.Aggregate((Node)null, (acc, item) => acc == null || acc.cost > item.cost ? item : acc);
				while (n != null)
				{
					if (n.action != null)
						actionOutput.Push(n.action);

					n = n.parent;
				}

				return true;
			}

			/// <summary>
			/// Creates all available plans for the specified goals.
			/// </summary>
			/// <param name="parent">The parent node.</param>
			/// <param name="leaves">A list with all possible plans. Should be empty when passed.</param>
			/// <param name="availableActions">All the available actions.</param>
			/// <param name="goals">The goals of the plan.</param>
			/// <returns></returns>
			private bool BuildGraph(Node parent, List<Node> leaves, List<GoapAction> availableActions, GoapGoal[] goals)
			{
				bool success = false;

				foreach (var action in availableActions)
				{
					if (action.Preconditions.All(x => x(parent.state)))
					{
						var actionState = new GoapState();
						action.ChangeState(actionState);
						var affectedState = actionState.MergeState(parent.state);
						var node = new Node(parent, affectedState, action, parent.cost + action.Cost);

						if (goals.All(x => x(affectedState)))
						{
							leaves.Add(node);
							success = true;
						}
						else
						{
							var actions = new List<GoapAction>(availableActions.Where(x => x != action));
							if (BuildGraph(node, leaves, actions, goals))
								success = true;
						}
					}
				}

				return success;
			}
		}
	}
}