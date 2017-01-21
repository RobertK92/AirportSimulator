namespace GOAP
{
	using UnityEngine;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;

	public class GoapAgent : MonoBehaviour
	{
		private enum FSMState
		{
			Plan,
			Move,
			Action
		};
		private FSMState _fsmState;

		private UnityEngine.AI.NavMeshAgent _agent;

		private GoapPlanner _planner;
		private Stack<IGoapAction> _actions;
		private Dictionary<GoapState, bool> _goals;

		public Dictionary<GoapState, bool> State { get { return _state; } }
		private Dictionary<GoapState, bool> _state;


		// Use this for initialization
		void Awake()
		{
			Debug.Assert(GetComponent<UnityEngine.AI.NavMeshAgent>(), "This action requires a NavMeshAgent component on the object.");
			_agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
			_fsmState = FSMState.Plan;
			_actions = new Stack<IGoapAction>();
			_planner = new GoapPlanner(GetComponents<IGoapAction>(), this);

			_planner.MakePlan(_state, _actions, _goals);



		}

		// Update is called once per frame
		void Update()
		{
			switch (_fsmState)
			{
				case FSMState.Plan:
					{
						if (_goals == null) break;
						if (_goals.Count == 0) break;

						// make plan based on utility theory   
						if (_planner.MakePlan(_state, _actions, _goals))
						{
							_goals.Clear();
							_fsmState = FSMState.Action;
						}
						else
						{
							Debug.LogError("Plan could not be found.");
						}
						break;
					}

				case FSMState.Move:
					{
						if (_actions.Peek().IsInRange)
							_fsmState = FSMState.Action;
						break;
					}

				case FSMState.Action:
					{
						if (_actions.Count == 0)
						{
							_fsmState = FSMState.Plan;
							break;
						}

						//_currentAction = _actions.Peek().GetType().Name;
						//                    Debug.Log(_actions.Peek());

						if (_actions.Peek().IsInRange)
						{
							_actions.Peek().Execute(this);
							//_actions.Peek().OnExecuted.Invoke(_actions.Peek().GetType());
							_actions.Pop();
						}
						else
						{
							//MoveTo(_actions.Peek().Target.transform.position);
							_fsmState = FSMState.Move;
						}
						break;
					}
			}
		}

		private void MoveTo(Vector3 destination)
		{
			_agent.SetDestination(destination);
		}

		public void AssignGoals(Dictionary<GoapState, bool> goals)
		{
			_goals = goals;
		}
	}
}