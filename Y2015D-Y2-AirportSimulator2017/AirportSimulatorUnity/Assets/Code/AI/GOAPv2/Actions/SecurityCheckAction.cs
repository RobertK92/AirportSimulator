namespace GOAP
{
	using UnityEngine;
	using System.Collections.Generic;
	using System;

	public class SecurityCheckAction : MonoBehaviour, IGoapAction
	{
		private Dictionary<GoapState, bool> _effects = new Dictionary<GoapState, bool>();
		private Dictionary<GoapState, bool> _preconditions = new Dictionary<GoapState, bool>();

		public Dictionary<GoapState, bool> Effects { get { return _effects; } }
		public Dictionary<GoapState, bool> Preconditions { get { return _preconditions; } }

		public int Cost { get { return 10; } }

		public bool IsInRange { get { return false; } }

		public SecurityCheckAction()
		{
			_preconditions.Add(GoapState.HasSecurityCheck, false);
			_preconditions.Add(GoapState.HasCheckedIn, true);

			_effects.Add(GoapState.HasSecurityCheck, true);
		}

		public void Execute(GoapAgent agent)
		{
			var agentState = agent.State;

			bool match = false;
			foreach (var change in _effects)
			{
				foreach (var state in agentState)
				{
					if (state.Key == change.Key)
					{
						match = true;
						break;
					}
				}

				if (match)
				{
					agentState.Remove(change.Key);
					agentState.Add(change.Key, change.Value);
				}
				else
				{
					agentState.Add(change.Key, change.Value);
				}
			}
		}
	}
}