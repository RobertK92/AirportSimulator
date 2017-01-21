using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AI
{
	using GOAPv3;

    [RequireComponent(typeof(GoapAgent), typeof(UnityEngine.AI.NavMeshAgent))]
    public class UtilityAgent : MonoBehaviour
    {
        private float _hunger;
        public float Hunger
        {
            get { return _hunger; }
            set
            {
                _hunger = Mathf.Clamp(value, 0.0f, 1.0f);
            }
        }

        private float _thirst;
        public float Thirst
        {
            get { return _thirst; }
            set
            {
                _thirst = Mathf.Clamp(value, 0.0f, 1.0f);
            }
        }

        private float _toilet;
        public float Toilet
        {
            get { return _toilet; }
            set
            {
                _toilet = Mathf.Clamp(value, 0.0f, 1.0f);
            }
        }

        private float _boardPlane;
        public float BoardPlane
        {
            get { return _boardPlane; }
            set
            {
                _boardPlane = Mathf.Clamp(value, 0.0f, 1.0f);
            }
        }

        private float _takeCover;
        public float takeCover
        {
            get { return _takeCover; }
            set
            {
                _takeCover = Mathf.Clamp(value, 0.0f, 1.0f);
            }
        }

        
        public float hungerRate = 1.0f;
        public float thirstRate = 1.0f;

        public LongTermGoal initialGoal;
        public List<Utility> utilities = new List<Utility>();
        
        public LongTermGoal CurrentGoal { get; private set; }

        private GoapAgent goapAgent;
        private LongTermGoal oldGoal;
        private UnityEngine.AI.NavMeshAgent navAgent;

		private float _boardPlaneIncrement;

        public System.Action<LongTermGoal> OnLongTermGoalChanged = delegate { };
        
        private void Awake()
        {
            goapAgent = GetComponent<GoapAgent>();
            navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            OnLongTermGoalChanged += OnLongTermGoalHasChanged;
            _hunger = Random.Range(0.0f, 1.0f);
            _thirst = Random.Range(0.0f, 1.0f);
            _toilet = Random.Range(0.0f, 1.0f);
        }

        private void Start()
        {
            OnLongTermGoalChanged(initialGoal);

			var timeUntilBoard = GetComponent<Agent>().flight.boardingStart - Airport.Instance.clock.Now();
			_boardPlaneIncrement = (1 - _boardPlane) / timeUntilBoard;
        }


        private void OnLongTermGoalHasChanged(LongTermGoal goal)
        {
            switch (goal)
            {
                case LongTermGoal.None:
                    goapAgent.AssignGoals(null, LongTermGoal.None);
                    navAgent.SetDestination(transform.position);
                    break;
                case LongTermGoal.Eat:
					goapAgent.State.HasEaten = false;
                    goapAgent.AssignGoals(new GoapGoal[]
                    {
                        state => state.HasEaten
                    }, goal);
                    break;
                case LongTermGoal.Drink:
					goapAgent.State.HasDrunk = false;
					goapAgent.AssignGoals(new GoapGoal[]
                    {
                        state => state.HasDrunk
                    }, goal);
                    break;
                case LongTermGoal.Toilet:
					goapAgent.State.HasUsedToilet = false;
					goapAgent.AssignGoals(new GoapGoal[]
                    {
                        state => state.HasUsedToilet
                    }, goal);
                    break;
                case LongTermGoal.TakeCover:
                    break;
                case LongTermGoal.BoardPlane:
					goapAgent.State.HasBoardedPlane = false;
					goapAgent.AssignGoals(new GoapGoal[]
                    {
                        state => state.HasBoardedPlane
                    }, goal);
                    break;
                default:
                    break;
            }
        }

        private void Update()
        {
            Thirst += Time.deltaTime / 100 * thirstRate;
            Hunger += Time.deltaTime / 100 * hungerRate;
            BoardPlane += Time.deltaTime * _boardPlaneIncrement;

            CurrentGoal = UpdateLongTermGoal();
            if (CurrentGoal != oldGoal)
                OnLongTermGoalChanged.Invoke(CurrentGoal);
            oldGoal = CurrentGoal;
        }

        public void OnLongTermGoalCompleted(bool successful)
        {
            CurrentGoal = UpdateLongTermGoal();
            OnLongTermGoalChanged.Invoke(CurrentGoal);
        }

        public LongTermGoal UpdateLongTermGoal()
        {
            List<Utility> sorted = utilities
                .OrderBy(x => x.UtilityValue)
                .ThenBy(x => x.priority)
                .ToList();
            sorted.Reverse();
            if (sorted[0].UtilityValue <= 0.0f)
                return LongTermGoal.None;
            return sorted[0].Action;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            //Gizmos.DrawWireCube(transform.position, Vector3.one);
            Gizmos.color = Color.white;
        }
    }
}