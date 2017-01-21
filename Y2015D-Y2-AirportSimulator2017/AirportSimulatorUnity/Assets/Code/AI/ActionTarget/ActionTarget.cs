using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AI
{
	public class ActionTarget : MonoBehaviour
	{
		private bool _isOpen;
		public bool CanQueue
		{
			get
			{
				return (Queue.Count < _maxQueueLength) && _isOpen;
			}
		}

		private List<QueueMember> _queue = new List<QueueMember>();
		public List<QueueMember> Queue { get { return _queue; } }

		private List<Vector3> _queuePositions = new List<Vector3>();
		public List<Vector3> QueuePositions { get { return _queuePositions; } }

		private Vector3 _lastPositionInQueue;
		public Vector3 LastPositionInQueue { get { return _lastPositionInQueue; } }

		private Quaternion _lastRotation;
		public Quaternion LastRotation { get { return _lastRotation; } }


		public float _inRangeThreshold = 1.0f;
		public float _queueSpacing = 2.0f;
		public uint _maxQueueLength = 20;

		private void Start()
		{
			_isOpen = true;
			FindNextPos();
		}

		public void FixedUpdate()
		{
			FindNextPos();
		}


		private void FindNextPos()
		{
			if (Queue.Count < QueuePositions.Count) return;

			QueueMember lastMember = Queue.LastOrDefault();
			if (lastMember == null)
			{
				_lastPositionInQueue = transform.position;
				_lastRotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
				_queuePositions.Add(_lastPositionInQueue);
				return;
			}

			Vector3? samplePosition = SampleLastPos(lastMember, -lastMember.transform.forward);
			if (samplePosition == null)
			{
				samplePosition = SampleLastPos(lastMember, -lastMember.transform.right);
				if (samplePosition == null)
				{
					samplePosition = SampleLastPos(lastMember, lastMember.transform.right);
					if (samplePosition == null)
					{
						Debug.Log(string.Format(
							"Unable to join the queue for the '{0}', theres no space behind or next to the last QueueMember.",
							GetType().Name));
					}
				}
			}

			if (samplePosition != null)
			{
				_queuePositions.Add(samplePosition.Value);
				_lastPositionInQueue = samplePosition.Value;

			}
		}

		public bool IsInRange(Vector3 agentPosition)
		{
			return Vector3.Distance(agentPosition, transform.position) <= _inRangeThreshold;
		}

		public void JoinQueue(QueueMember member)
		{
			if (CanQueue)
			{
				FindNextPos();
				Queue.Add(member);
				member.ReadyToExecute();
			}
			else
			{
				Debug.Log(string.Format("The queue for the '{0}' has reached its limit, I need to do something else.", GetType().Name));
			}
		}

		public void LeaveQueue(QueueMember member)
		{
			if (Queue.Contains(member))
			{
				Queue.Remove(member);
				for (int i = 0; i < Queue.Count; i++)
				{
					Queue[i].PositionInQueue = i;
					Vector3 desiredPosition = QueuePositions[i];
					Queue[i].ShiftPosition(desiredPosition);
				}
				QueuePositions.Remove(QueuePositions.Last());
				_lastPositionInQueue = QueuePositions.LastOrDefault();
			}
		}

		private Vector3? SampleLastPos(QueueMember member, Vector3 direction)
		{
			RaycastHit hit;
			Vector3 samplePosition = member.transform.position + direction * _queueSpacing;
			Vector3 left = LastPositionInQueue - member.transform.right * member._navAgent.radius * 2;
			Vector3 right = LastPositionInQueue + member.transform.right * member._navAgent.radius * 2;

			if (Physics.Raycast(left, direction, out hit, _queueSpacing + member._navAgent.radius))
			{
				if (hit.collider != null)
				{
					if (hit.collider.gameObject.layer == UnityConstants.Layers.Wall)
					{
						return null;
					}
				}
			}

			if (Physics.Raycast(right, direction, out hit, _queueSpacing + member._navAgent.radius))
			{
				if (hit.collider != null)
				{
					if (hit.collider.gameObject.layer == UnityConstants.Layers.Wall)
					{
						return null;
					}
				}
			}
			_lastRotation = member.transform.rotation;
			return samplePosition;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(0.0f, 0.0f, 1.0f, 0.5f);
			Gizmos.DrawSphere(transform.position, 1.0f);
			Gizmos.DrawWireSphere(LastPositionInQueue, 1.0f);
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(transform.position, 1.0f);
		}
	}

}

