namespace AI
{
	namespace GOAPv3
	{
		public class Node
		{
			public Node parent;
			public GoapState state;
			public GoapAction action;
			public int cost;

			/// <summary>
			/// Creates a node for the GOAP Planner.
			/// </summary>
			/// <param name="parent">The parent of the node. Can be null.</param>
			/// <param name="state">The affected state of the node.</param>
			/// <param name="action">The action that modified the state. Can be null.</param>
			/// <param name="cost">The cost of the plan so far.</param>
			public Node(Node parent, GoapState state, GoapAction action, int cost)
			{
				this.parent = parent;
				this.state = state;
				this.action = action;
				this.cost = cost;
			}
		}
	}
}