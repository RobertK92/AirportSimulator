using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class Airport : Singleton<Airport>
{
	[Header("Tweakable Variables")]
	public int dayStart = 4;

	[Header("Objects")]
	public Clock clock;
	public Schedule schedule;
	public Map map;
	public EmployeesManager employees;


	// Keep the location that want to open, but can't for some reason
	private List<LocationImpl> pendingOpen;
	private List<int> pendingOpenFlights;
	

// ============================================================================
//	INIT
//
	void Awake()
	{
		pendingOpen = new List<LocationImpl>();
		pendingOpenFlights = new List<int>();
		// TODO: for game review
		schedule.hackMap = map;

		employees.Init();

		clock.Init(dayStart);
		map.Init(schedule);
		schedule.Init(clock);
	}

	void Start()
	{}

//
// ============================================================================


// ============================================================================
//	UPDATE
//
	void Update()
	{
		clock.Update();
		schedule.Update(clock, map);
		map.Update(clock);

		CheckPendingOpens();

		if(clock.hour < dayStart)
		{
			clock.SetTime(dayStart, 0, 0);
			schedule.RegenerateSchedule(clock);
		}

		if(Input.GetKey(KeyCode.LeftShift))
		{
			clock.extraSpeed += 10.0f * Input.GetAxis("Mouse ScrollWheel");
			clock.extraSpeed = Mathf.Clamp(clock.extraSpeed, 0.0f, 10000.0f);
		}
		else
		{
			clock.timeScale += 5.0f * Input.GetAxis("Mouse ScrollWheel");
			clock.timeScale = Mathf.Clamp(clock.timeScale, 0.0f, 100.0f);
		}
	}

	private void CheckPendingOpens()
	{
		if(pendingOpen.Count > 0)
		{
			List<LocationImpl> notOpened = new List<LocationImpl>();
			List<int> notOpenedFlights = new List<int>();
			int i = 0;
			foreach(LocationImpl l in pendingOpen)
			{
				if(!l.isOpening && !l.isOpen)
				{
					if(!employees.SendEmployee(l, pendingOpenFlights[i]))
					{
						notOpened.Add(l);
						notOpenedFlights.Add(pendingOpenFlights[i]);
					}
				}
			}

			pendingOpen = notOpened;
			pendingOpenFlights = notOpenedFlights;
		}
	}

//
// ============================================================================


	public T[] GetTargets<T>(LocationType type, Zone zone) where T : LocationImpl
	{
		return map.locations[(int)type][(int)zone] as T[];
	}

	public GateImpl GetGate(Flight flight)
	{
		int zone = (flight.type == FlightType.ARRIVAL ? (int)Zone.ARRIVAL : (int)Zone.GATES);
		return map.locations[(int)LocationType.GATE][zone][flight.gate] as GateImpl;
	}

	public CheckInImpl GetCheckInDesk(Flight flight)
	{
		return map.locations[(int)LocationType.CHECKIN][(int)Zone.MAIN_HALL][flight.checkInDesk] as CheckInImpl;
	}

	// ============================================================================
	//	PASSENGER FUNCTIONS
	//
	public void Register(LocationImpl location, Agent agent)
	{
		if(location != null && location.isOpen)
		{
			location.Register(agent, clock);
		}
		else
		{
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.CLOSED);
		}
	}

	public void Unregister(LocationImpl location, Agent agent)
	{
		location.Unregister(agent);
	}
//
// ============================================================================


	public bool OpenLocation(LocationImpl loc, int flightNum)
	{
		if(!loc.isOpening && !loc.isOpen)
		{
			if(!employees.SendEmployee(loc, flightNum))
			{
				pendingOpen.Add(loc);
				pendingOpenFlights.Add(flightNum);
				return false;
			}

			return true;
		}
		return false;
	}

	public void CloseLocation(LocationImpl loc)
	{
		loc.Close();
		int i = -1;
		i = pendingOpen.IndexOf(loc);
		if(i >= 0)
		{
			pendingOpen.RemoveAt(i);
			pendingOpenFlights.RemoveAt(i);
		}
	}

	public void LockAirport()
	{
		map.LockAirport();
	}
}
