namespace Deprecated
{
	using UnityEngine;
	using System.Collections;
	using System;

	public class Node : IDeepCloneable<Node>
	{
		public Node parent;
		public GoapAction action;
		public GoapState state;

		public Node(Node parent, GoapState state, GoapAction action)
		{
			this.parent = parent;
			this.state = state;
			this.action = action;
		}

		public bool Equals(Node node)
		{
			return action == node.action;
		}

		public Node DeepClone()
		{
			return new Node(parent, state, action);
		}
	}
}