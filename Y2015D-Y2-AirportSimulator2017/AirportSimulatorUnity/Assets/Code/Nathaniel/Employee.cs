using UnityEngine;
using System.Collections;

public class Employee : MonoBehaviour
{
	private enum State
	{
		HOME,
		GOING_TO_JOB,
		AT_JOB,
		GOING_HOME
	}

	public float targetOffset = 2.0f;

	private State state = State.HOME;
	private UnityEngine.AI.NavMeshAgent navAgent;
	private Vector3 homePos;
	private Vector3 targetPos;
	private LocationImpl target;
	private int flightNumber;
	private EmployeesManager pool;
	private static Rand rand = null;

	public void Init(Vector3 home, EmployeesManager emps)
	{
		if(rand == null)
		{
			rand = new Rand();
		}
		navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		state = State.HOME;
		homePos = home;
		pool = emps;
	}

	void Update()
	{
		switch(state)
		{
		case State.HOME:
			{
				break;
			}
		case State.GOING_TO_JOB:
			{
				Vector3 nextPos = transform.position + transform.forward * Time.deltaTime;
				float dist2 = (targetPos - transform.position).sqrMagnitude;
				float nextDist2 = (nextPos - transform.position).sqrMagnitude;
				if(dist2 < 1.0f || dist2 < nextDist2)
				{
					navAgent.Stop();

					target.Open(flightNumber);
					state = State.AT_JOB;
				}
				break;
			}
		case State.AT_JOB:
			{
				transform.rotation = target.go.transform.rotation;
				break;
			}
		case State.GOING_HOME:
			{
				Vector3 nextPos = transform.position + transform.forward * Time.deltaTime;
				float dist2 = (homePos - transform.position).sqrMagnitude;
				float nextDist2 = (nextPos - transform.position).sqrMagnitude;
				if(dist2 < 1.0f || dist2 < nextDist2)
				{
					navAgent.Stop();

					pool.Return(this);
					state = State.HOME;
				}
				break;
			}
		}
	}

	public void GoToJob(LocationImpl targ, int flightNum)
	{
		flightNumber = flightNum;
		targ.SendEmployee(this);
		target = targ;
		targetPos = target.position - target.go.transform.forward * targetOffset;
		navAgent.SetDestination(targetPos);
		navAgent.Resume();
		state = State.GOING_TO_JOB;
	}

	public void GoHome()
	{
		navAgent.SetDestination(homePos);
		navAgent.Resume();
		state = State.GOING_HOME;
	}
}

