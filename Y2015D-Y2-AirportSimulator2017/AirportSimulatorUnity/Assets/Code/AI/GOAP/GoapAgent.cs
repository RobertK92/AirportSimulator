namespace Deprecated
{
	using UnityEngine;
	using System.Collections.Generic;
	using System.Reflection;
	using System;
	using System.Linq;

	namespace AI
	{
		//[RequireComponent(typeof(UtilityAgent))]
		public class GoapAgent : MonoBehaviour
		{

			private enum FSMState
			{
				Plan,
				Move,
				Action
			};

			//public UtilityAgent GoalManager { get; private set; }

			private UnityEngine.AI.NavMeshAgent _agent;

			private FSMState _state;

			private Stack<GoapAction> _actions;
			private GoapState _agentState;
			private GoapPlanner _planner;
			private GoapGoal[] _goals;

			// For debugging purposes
			[SerializeField]
			private string _currentAction;

			void Awake()
			{
				Debug.Assert(GetComponent<UnityEngine.AI.NavMeshAgent>(), "This action requires a NavMeshAgent component on the object.");
				_agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
				_state = FSMState.Plan;
				_agentState = new GoapState();
				_actions = new Stack<GoapAction>();

				//GoalManager = GetComponent<UtilityAgent>();

				// Destroy actions that have been added (they will all be added on runtime after this)
				GoapAction[] currentActions = gameObject.GetComponents<GoapAction>();
				for (int i = 0; i < currentActions.Length; i++)
				{
					Destroy(currentActions[i]);
				}

				// Add all component dynamically on runtime
				foreach (Type type in
					Assembly.GetAssembly(typeof(GoapAgent)).GetTypes().Where(
						x => x.IsClass &&
						x.IsSubclassOf(typeof(GoapAction)) &&
						!x.IsAbstract))
				{
					gameObject.AddComponent(type);
				}

				_planner = new GoapPlanner(GetComponents<GoapAction>(), this);
				_goals = new GoapGoal[0];
			}


			void Update()
			{

				switch (_state)
				{
					case FSMState.Plan:
						{
							if (_goals == null) break;
							if (_goals.Length == 0) break;
							// make plan based on utility theory   
							if (_planner.MakePlan(_agentState, ref _actions, _goals, 0))
							{
								_goals = new GoapGoal[0];
								_state = FSMState.Action;
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
								_state = FSMState.Action;
							break;
						}

					case FSMState.Action:
						{
							if (_actions.Count == 0)
							{
								_state = FSMState.Plan;
								break;
							}

							_currentAction = _actions.Peek().GetType().Name;
							//                    Debug.Log(_actions.Peek());

							if (_actions.Peek().IsInRange)
							{
								_actions.Peek().Execute(_agentState);
								//_actions.Peek().OnExecuted.Invoke(_actions.Peek().GetType());
								_actions.Pop();
							}
							else
							{
								MoveTo(_actions.Peek().Target.transform.position);
								_state = FSMState.Move;
							}
							break;
						}
				}
			}

			private void MoveTo(Vector3 destination)
			{
				_agent.SetDestination(destination);
			}

			public void SetGoals(GoapGoal[] goals)
			{
				_goals = goals;
				_state = FSMState.Plan;
			}
		}
	}
}