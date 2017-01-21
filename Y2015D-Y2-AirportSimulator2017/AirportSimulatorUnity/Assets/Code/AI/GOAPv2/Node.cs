namespace GOAP
{
	using System.Collections.Generic;

	public class Node : IDeepCloneable<Node>
	{
		public Node parent;
		public Dictionary<GoapState, bool> state;
		public IGoapAction action;
		public int cost;

		public Node(Node parent, Dictionary<GoapState, bool> state, IGoapAction action, int cost)
		{
			this.parent = parent;
			this.state = state;
			this.action = action;
			this.cost = cost;
		}

		public bool Equals(Node node)
		{
			return action == node.action;
		}

		public Node DeepClone()
		{
			return new Node(parent, state, action, cost);
		}
	}
}