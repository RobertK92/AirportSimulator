using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AI;
using System;


// ============================================================================
//	BASE CLASS
//
public class LocationImpl
{
	public bool isOpen = false;
	public LocationType type = LocationType.ENTRANCE;
	public Vector3 position = Vector3.zero;
	public ActionTarget queue;

	public Zone zone = Zone.MAIN_HALL;
	public Schedule schedule;

	public Renderer goRender;
	public bool isOpening = false;
	public bool needsStaff = false;
	protected Employee currentEmployee = null;

	public GameObject go;

	public virtual void Update(Clock clock) { }
	public virtual void Register(Agent agent, Clock clock)
	{
		agent.TakeMessage(Message.CURRENT_STEP_FINISHED, Status.SUCCES);
	}
	public virtual void Unregister(Agent agent)
	{
		agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.SUCCES);
	}

	public void SendEmployee(Employee emp)
	{
		if(needsStaff)
		{
			currentEmployee = emp;
			isOpen = false;
			isOpening = true;
		}
		else
		{
			isOpen = true;
			goRender.material.color = Color.green;
		}
	}
	public virtual void Open(int flightNum)
	{
		isOpen = true;
		isOpening = false;
		goRender.material.color = Color.green;
	}
	public virtual void Close()
	{
		if(currentEmployee != null)
		{
			currentEmployee.GoHome();
		}
		currentEmployee = null;
		isOpening = false;
		isOpen = false;
		goRender.material.color = Color.red;
	}
}


// ============================================================================
//	DERIVED CLASSES
//
public class ToiletImpl : LocationImpl
{}

public class CheckInImpl : LocationImpl
{
	public float checkInDuration = 10.0f;	// seconds

	public int flightNumber = 0;
	private Agent passenger = null;
	private float startTime;


	public override void Update(Clock clock)
	{
		if(passenger != null && clock.Now() - startTime >= checkInDuration)
		{
			// TODO: take baggae from passenger, if needed give him a boarding pass etc.
			passenger.flight.baggage += passenger.baggage;
			passenger.baggage = 0;

			passenger.TakeMessage(Message.CURRENT_STEP_FINISHED, Status.SUCCES);
			passenger = null;
		}
	}

	public override void Register(Agent agent, Clock clock)
	{
		if(passenger != null)
		{
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.LOCATION_BUSY);
			return;
		}
		Flight flight = agent.flight;

		// Find out start and end-time for the specified flightNum
		int start = schedule.checkInStartByFlightNum[flight.flightNumber];
		int end = schedule.checkInEndByFlightNum[flight.flightNumber];
		int now = 60 * clock.hour + clock.minute;

		if(now < start)
		{
			// Passenger is too early
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.TOO_EARLY);
		}
		else if(now > end)
		{
			// Passenger is too late. Tell him/her
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.TOO_LATE);
		}
		else if(flight.flightNumber == flightNumber)
		{
			// Prepare for check-in
			passenger = agent;
			startTime = clock.Now();
		}
		else
		{
			// Inform passenger that he/she is at the wrong desk
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.WRONG_LOCATION);
		}
	}

	public override void Unregister(Agent agent)
	{
		if(passenger == agent)
		{
			passenger.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.SUCCES);
			passenger = null;
		}
	}

	public override void Open(int flightNum)
	{
		flightNumber = flightNum;
		
		// Reset
		passenger = null;
		isOpen = true;
		isOpening = false;

		goRender.material.color = Color.green;
	}

	public override void Close()
	{
		// Tell passenger that its checkin was cancelled
		if(passenger != null)
		{
			passenger.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.CANCELLED);
		}

		// Reset
		passenger = null;
		isOpen = false;
		isOpening = false;
		if(currentEmployee != null)
		{
			currentEmployee.GoHome();
		}
		currentEmployee = null;

		goRender.material.color = Color.red;
	}
}

public class SelfCheckInImpl : LocationImpl
{
	public float checkInDuration = 10.0f;   // seconds

	private Agent passenger = null;
	private float startTime;


	public override void Update(Clock clock)
	{
		if(passenger != null && clock.Now() - startTime >= checkInDuration)
		{
			passenger.flight.baggage += passenger.baggage;
			passenger.baggage = 0;

			passenger.TakeMessage(Message.CURRENT_STEP_FINISHED, Status.SUCCES);
			passenger = null;
		}
	}

	public override void Register(Agent agent, Clock clock)
	{
		if(passenger != null)
		{
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.LOCATION_BUSY);
			return;
		}
		Flight flight = agent.flight;

		// Find out start and end-time for the specified flightNum
		int start = schedule.checkInStartByFlightNum[flight.flightNumber];
		int end = schedule.checkInEndByFlightNum[flight.flightNumber];
		int now = 60 * clock.hour + clock.minute;

		if(now < start)
		{
			// Passenger is too early
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.TOO_EARLY);
		}
		else if(now > end)
		{
			// Passenger is too late. Tell him/her
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.TOO_LATE);
		}
		else
		{
			// Prepare for check-in
			passenger = agent;
			startTime = clock.Now();
		}
	}

	public override void Unregister(Agent agent)
	{
		if(passenger == agent)
		{
			passenger.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.SUCCES);
			passenger = null;
		}
	}

	public override void Open(int flightNum)
	{
		// Reset
		passenger = null;
		isOpen = true;
		isOpening = false;

		goRender.material.color = Color.green;
	}

	public override void Close()
	{
		// Tell passenger that its checkin was cancelled
		if(passenger != null)
		{
			passenger.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.CANCELLED);
		}

		// Reset
		passenger = null;
		isOpen = false;
		isOpening = false;
		if(currentEmployee != null)
		{
			currentEmployee.GoHome();
		}
		currentEmployee = null;

		goRender.material.color = Color.red;
	}
}

public class SecurityImpl : LocationImpl
{
	public float processTime = 5;

	private Agent passenger = null;
	private float startTime;

	public override void Update(Clock clock)
	{
		if(passenger != null && clock.Now() - startTime >= processTime)
		{
			// TODO: change passenger state (or make Agent do that)

			passenger.TakeMessage(Message.CURRENT_STEP_FINISHED, Status.SUCCES);
			passenger = null;
		}
	}

	public override void Register(Agent agent, Clock clock)
	{
		if(passenger != null)
		{
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.LOCATION_BUSY);
			return;
		}
		Flight flight = agent.flight;

		// You can pass security only if boarding for your flight hasn't ended
		int end = schedule.boardingEndByFlightNum[flight.flightNumber];
		int now = 60 * clock.hour + clock.minute;

		if(now > end)
		{
			// Passenger is too late. Tell him/her
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.TOO_LATE);
		}
		else
		{
			// Prepare for security
			passenger = agent;
			startTime = clock.Now();
		}
	}

	public override void Unregister(Agent agent)
	{
		if(passenger == agent)
		{
			passenger.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.SUCCES);
			passenger = null;
		}
	}

	public override void Open(int flightNum)
	{
		// Reset
		passenger = null;
		isOpen = true;
		isOpening = false;

		goRender.material.color = Color.green;
	}

	public override void Close()
	{
		if(passenger != null)
		{
			passenger.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.CANCELLED);
		}

		// Reset
		passenger = null;
		isOpen = false;
		isOpening = false;
		if(currentEmployee != null)
		{
			currentEmployee.GoHome();
		}
		currentEmployee = null;

		goRender.material.color = Color.red;
	}
}

public class CustomsImpl : LocationImpl
{}

public class GateImpl : LocationImpl
{
	public int flightNumber = 0;


	public override void Register(Agent agent, Clock clock)
	{
		Flight flight = agent.flight;

		// Find out start and end-time for the specified flightNum
		int start = schedule.boardingStartByFlightNum[flight.flightNumber];
		int end = schedule.boardingEndByFlightNum[flight.flightNumber];
		int now = 60 * clock.hour + clock.minute;

		if(now < start)
		{
			// Passenger is too early
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.TOO_EARLY);
		}
		else if(now > end)
		{
			// Passenger is too late. Tell him/her
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.TOO_LATE);
		}
		else if(flight.flightNumber != flightNumber)
		{
			// Passenger is at wrong gate
			agent.TakeMessage(Message.CURRENT_STEP_CANCELLED, Status.WRONG_LOCATION);
		}
		else
		{
			// Passenger can pass
			agent.TakeMessage(Message.CURRENT_STEP_FINISHED, Status.SUCCES);

			agent.grid.RemoveObject(agent.gameObject.GetComponent<Avoidance>());
			agent.gameObject.GetComponent<QueueMember>().LeaveQueue();
			agent.gameObject.SetActive(false);
			GameObject.Destroy(agent.gameObject);
		}
	}

	public override void Open(int flightNum)
	{
		flightNumber = flightNum;
		isOpen = true;
		isOpening = false;

		goRender.material.color = Color.green;
	}
}

public class InfoDeskImpl : LocationImpl
{}

public class EntranceImpl : LocationImpl
{}

public class ExitImpl : LocationImpl
{
	public override void Register(Agent agent, Clock clock)
	{
		agent.TakeMessage(Message.CURRENT_STEP_FINISHED, Status.SUCCES);

		agent.grid.RemoveObject(agent.gameObject.GetComponent<Avoidance>());
		agent.gameObject.GetComponent<QueueMember>().LeaveQueue();
		agent.gameObject.SetActive(false);
		GameObject.Destroy(agent.gameObject);
	}
}

public class RestaurantImpl : LocationImpl
{}

public class AHImpl : LocationImpl
{}

public class CafeImpl : LocationImpl
{}

public class TaxfreeImpl : LocationImpl
{}

public class LuggageImpl : LocationImpl
{}

public class ArrivalSecurityImpl : LocationImpl
{
	
}