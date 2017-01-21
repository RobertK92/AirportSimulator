namespace AI
{
	namespace GOAPv3
	{
		using UnityEngine;
		using System.Collections.Generic;
		using System.Reflection;
		using System.Linq;
		using System;

		public class GoapAgent : MonoBehaviour
		{

			public delegate void FSMState();
			private Stack<FSMState> _stateStack = new Stack<FSMState>();

			private FSMState _planState;
			private FSMState _moveState;
			private FSMState _actionState;
			private FSMState _executeState;

			public Zone StartingZone;

			public UnityEngine.AI.NavMeshAgent NavAgent { get; private set; }

			private GoapPlanner _planner;
			private Stack<GoapAction> _actions;
			private GoapGoal[] _goals;

			public GoapState State { get { return GetAgentState(); } }
			private GoapState _state;

			private QueueMember _qmember;
			private Inventory _inventory;
			private UtilityAgent _utilityAgent;

			public GoapAction CurrentAction;
			public LongTermGoal CurrentGoal;
			public string StackState;
            
			// Use this for initialization
			void Awake()
			{
				Debug.Assert(GetComponent<UnityEngine.AI.NavMeshAgent>(), "This action requires a NavMeshAgent component on the object.");
				NavAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

				Debug.Assert(GetComponent<QueueMember>(), "This action requires a QueueMember component on the object.");
				_qmember = GetComponent<QueueMember>();

				Debug.Assert(GetComponent<UtilityAgent>(), "This action requires a UtilityAgent component on the object.");
				_utilityAgent = GetComponent<UtilityAgent>();

				_state = new GoapState();
				_inventory = GetComponent<Inventory>();

				AddActions();
				_actions = new Stack<GoapAction>();
				_planner = new GoapPlanner(GetComponents<GoapAction>());

				_state.CurrentZone = StartingZone;
				InitStates();
				_stateStack.Push(_planState);
			}

			// Update is called once per frame
			void Update()
			{
				if (_stateStack.Peek() != null)
					_stateStack.Peek()();
			}

			private GoapState GetAgentState()
			{
				var updatedState = new GoapState
				{
					HasCheckedIn = _state.HasCheckedIn,
					HasSecurityCheck = _inventory.BoardingPass,
					HasArrivalSecurityCheck = _state.HasArrivalSecurityCheck,
					HasDepositedLuggage = _state.HasDepositedLuggage,
					HasCollectedLuggage = _state.HasCollectedLuggage,
					HasExitedAirport = _state.HasExitedAirport,
					HasBoardedPlane = _state.HasBoardedPlane,
					HasEaten = _state.HasEaten,
					HasDrunk = _state.HasDrunk,
					HasFood = _inventory.Food > 0f,
					HasLiquid = _inventory.Liquid > 0f,
					HasUsedToilet = _state.HasUsedToilet,
					HasLookedAtMap = _state.HasLookedAtMap,
					CurrentZone = _state.CurrentZone
				};

				_state = updatedState;
				return updatedState;
			}

			private void InitStates()
			{
				_qmember.OnExecute += OnTargetReached;

				_planState = () =>
				{
					StackState = "plan";

					if (_goals == null) return;
					if (_goals.Length == 0) return;

					if (_planner.MakePlan(this, _actions, _goals))
					{
						_goals = new GoapGoal[0];
						_stateStack.Pop();
						_stateStack.Push(_actionState);
					}
				};

				_moveState = () =>
				{

				};

				_actionState = () =>
				{
					StackState = "action";

					var action = _actions.Peek();
					CurrentAction = _actions.Peek();

					if (!action.Init(this))
					{
						_stateStack.Pop();
						_stateStack.Push(_planState);
						_utilityAgent.OnLongTermGoalCompleted(false);
						return;
					}

					if (action.Target == null)
					{
						_stateStack.Push(_executeState);
						return;
					}

					if (_qmember.State != QueueMember.QueueState.GoingToQueue)
					{
						_qmember.GoToQueue(action.Target.queue);
						_stateStack.Push(_moveState);
					}
				};

				_executeState = () =>
				{
					StackState = "execute";

					var action = _actions.Peek();
					CurrentAction = _actions.Peek();

					if (action.Execute(GetAgentState()))
					{
                        action.Reset();
                        _actions.Pop();
						if (_qmember.InQueue)
							_qmember.LeaveQueue();
						_stateStack.Pop();

						if (_actions.Count == 0)
						{   
							_stateStack.Pop();
							_stateStack.Push(_planState);
							_utilityAgent.OnLongTermGoalCompleted(true);
						}
					}
                    
				};
			}

			private void OnTargetReached()
			{
				var action = _actions.Peek();

				_stateStack.Pop();

				if (!action.Init(this))
				{
					_stateStack.Pop();
					_stateStack.Push(_planState);
					_utilityAgent.OnLongTermGoalCompleted(false);
					return;
				}

				_stateStack.Push(_executeState);
			}

			public void AssignGoals(GoapGoal[] goals, LongTermGoal goal)
			{
				_goals = goals;
				goal = CurrentGoal;
			}

			private void AddActions()
			{
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
			}
		}
	}
}