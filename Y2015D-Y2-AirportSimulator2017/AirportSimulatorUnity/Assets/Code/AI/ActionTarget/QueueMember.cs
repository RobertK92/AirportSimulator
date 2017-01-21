namespace AI
{
	using UnityEngine;
	using GOAPv3;
	using System.Linq;
	using System.Collections.Generic;

	public struct RandomDestination
	{
		public readonly Vector3 Location;
		public readonly int Population;

		public RandomDestination(Vector3 location, int population)
		{
			this.Location = location;
			this.Population = population;
		}
	}

	[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
	public class QueueMember : MonoBehaviour
	{
		public System.Action OnExecute = delegate { };

		public enum QueueState
		{
			None,
			GoingToQueue,
			InQueue,
			Shift
		}

		private ActionTarget _target;
		public ActionTarget Target { get { return _target; } }

		private QueueState _state;
		public QueueState State { get { return _state; } }

		private bool _inQueue;
		public bool InQueue { get { return _inQueue; } }

		public int PositionInQueue { get; set; }

		public Vector3 DesiredPosition { get; set; }

		private QueueState _oldState;
		public UnityEngine.AI.NavMeshAgent _navAgent;

		public event System.Action<QueueMember> OnQueueMemberMoved = delegate { };

		public QueueState currentQueueState;
		public int PosInQueue;
        
		private float _distance = 10f;

		private void Start()
		{
			_navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
			_state = QueueState.None;
		}

		private void Update()
		{
            currentQueueState = _state;
			PosInQueue = PositionInQueue;



			if (State == QueueState.Shift)
			{
				if (Vector3.Distance(DesiredPosition, transform.position) < _navAgent.radius)
				{
					_state = QueueState.InQueue;
                    ReadyToExecute();
				}
			}

			if (State == QueueState.GoingToQueue)
			{
				if (!Target.CanQueue)
				{
					LeaveQueue();
				}
				else
				{
					_navAgent.SetDestination(Target.LastPositionInQueue);
					if (Vector3.Distance(transform.position, Target.LastPositionInQueue) < _navAgent.radius)
					{
						Target.JoinQueue(this);
						_inQueue = true;
						PositionInQueue = Target.Queue.Count - 1;
						_navAgent.transform.rotation = Target.LastRotation;
						_navAgent.updateRotation = false;
						_navAgent.SetDestination(Target.LastPositionInQueue);
						_state = QueueState.InQueue;
					}
				}
			}

			if (_oldState != State)
			{
				if (State != QueueState.InQueue)
				{
					_navAgent.updateRotation = true;
				}
			}

			_oldState = State;
		}

		public void GoToQueue(ActionTarget target)
		{
			_target = target;
			_state = QueueState.GoingToQueue;
		}

		public void LeaveQueue()
		{
			if (Target == null) return;
			Target.LeaveQueue(this);
			_inQueue = false;

			bool posFound = false;
			List<RandomDestination> possibleDestinations = new List<RandomDestination>();
			while (!posFound)
			{

				Vector3 randomPos;

				do
				{
					randomPos = Random.insideUnitSphere * _distance;
					randomPos += transform.position;
				} while ((randomPos - transform.position).magnitude < _navAgent.radius * 3.0f);

				UnityEngine.AI.NavMeshHit hit;
				if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 1, 1))
				{
					UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
					_navAgent.CalculatePath(randomPos, path);
					if (path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
					{
						GoapAgent[] allPeople = FindObjectsOfType<GoapAgent>();
						if (allPeople.Length > 0)
						{
							IEnumerable<GoapAgent> closePeople = allPeople.Where(
								x => Vector3.Distance(x.transform.position, transform.position) < _navAgent.radius * 6.0f);
							int count = closePeople.Count();
							possibleDestinations.Add(new RandomDestination(randomPos, count));
							if (count < 3 || possibleDestinations.Count > 5)
							{
								posFound = true;
							}
						}
					}
				}
			}

			if (possibleDestinations.Count == 1)
			{
				_navAgent.SetDestination(possibleDestinations[0].Location);
			}
			else
			{
				_navAgent.SetDestination(possibleDestinations.OrderBy(x => x.Population).ToList()[0].Location);
			}

            
			_state = QueueState.None;
		}

		public void ReadyToExecute()
		{
			if (PositionInQueue == 0)
				OnExecute();
		}

		public void ShiftPosition(Vector3 shiftedPosition)
		{
			_state = QueueState.Shift;
			DesiredPosition = shiftedPosition;
			_navAgent.SetDestination(shiftedPosition);
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			//Gizmos.DrawLine(transform.position, _navAgent.destination);
			if (_navAgent != null)
				Gizmos.DrawSphere(_navAgent.destination, 0.5f);
			Gizmos.color = Color.white;
		}

        
	}
}
