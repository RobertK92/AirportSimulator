namespace Deprecated
{
	using UnityEngine;
	using System.Collections.Generic;
	using System.Linq;


	public delegate bool GoapGoal(GoapState state);
	public abstract class GoapAction : MonoBehaviour
	{
		protected int _cost = 1;
		protected int _range = 1;
		protected GameObject _target;

		private UnityEngine.AI.NavMeshAgent _navAgent;
		//private AI.QueueMember _queueMember;


		/// <summary>
		/// The total time required to complete this action, taken into account walking time and queues.
		/// </summary>
		//public float TimeRemaining
		//{
		//	get
		//	{
		//		float result = 0.0f;
		//		float vel = _navAgent.velocity.magnitude == 0.0f ? 0.00001f : _navAgent.velocity.magnitude;
		//		float timeToMove = (_navAgent.remainingDistance / vel);
		//		result += timeToMove;
		//		if (_queueMember.InQueue)
		//		{
		//			result += Duration * _queueMember.PositionInQueue;
		//		}
		//		result += Duration;
		//		return result;
		//	}
		//}

		public System.Action<System.Type> OnExecuted = delegate { };

		protected virtual void Awake()
		{
			_navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
			//_queueMember = GetComponent<AI.QueueMember>();
		}

		public void Execute(GoapState state)
		{
			OnExecute(state);
			OnExecuted.Invoke(GetType());
		}

		/// <summary>
		/// Modifies the state with the effect of the action.
		/// Calling base will find the closest target in the list.
		/// </summary>
		/// <param name="state">The current state of the agent.</param>
		protected abstract void OnExecute(GoapState state);

		/// <summary>
		/// The execution cost of the action.
		/// </summary>
		public int Cost { get { return _cost; } }

		/// <summary>
		/// True if the action is in range to be executed.
		/// </summary>
		public bool IsInRange { get { return (_target == null) ? true : (transform.position - _target.transform.position).magnitude < _range; } }

		/// <summary>
		/// The target of the action.
		/// </summary>
		public GameObject Target { get { return _target; } }

		/// <summary>
		/// The necessary preconditions for the action to execute.
		/// </summary>
		public abstract GoapGoal[] Preconditions { get; }

		/// <summary>
		/// The time it takes to complete the action in seconds.
		/// </summary>
		protected abstract float Duration { get; }
	}
}