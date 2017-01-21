using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


// Gate opens at least 45 minutes before departure
// Gate closes at most 15 minutes before departure

// Check-in opens at least 2 hours before departure
// Check-in closes 40 minutes before departure




[System.Serializable]
public class Schedule
{
	// PUBLIC VARS
	public int numOfFlightsVisible = 5;

	[Range(10, 120)] public int gateOpenDurationMinutes = 45;
	[Range(10, 60)]  public int gateCloseTimeMinutes = 15;		// Minutes before departure
	[Range(30, 180)] public int checkInOpenDurationMinutes = 120;
	[Range(30, 120)] public int checkInCloseTimeMinutes = 40;   // Minutes before departure

	public PassengerSpawning passengerSpawning;
	public int departureSpawnEndMinutes = 60;
	public int departureSpawnDurationMinutes = 120;
	public int arrivalSpawnDurationMinutes = 15;
	public int minPassengerOnPlane = 30;
	public int maxPassengersOnPlane = 50;

	public AnimationCurve departureDensity;

	// HIDDEN PUBLIC VARS
	[HideInInspector] public int[] checkInStartByFlightNum;
	[HideInInspector] public int[] checkInEndByFlightNum;
	[HideInInspector] public int[] boardingStartByFlightNum;
	[HideInInspector] public int[] boardingEndByFlightNum;

	// PRIVATE VARS
	private List<Combo> departures;
	private List<Combo> arrivals;
	private Combo[] departuresByFlightNum;
	private Combo[] arrivalsByFlightNum;
	private static int flightNumberCounter = 0;
	private PlaneSpot[] parkingSpots;

	private Rand rand;

	private GameObject hud;
	private Text scheduleText;
	private Canvas canvas;



	private class Combo
	{
		public Flight flight;
		public Plane plane;
	}



	public Flight GetFlight(int flightNum)
	{
		return departuresByFlightNum[flightNum].flight;
	}


// ============================================================================
//	INIT
//
	public void Init(Clock clock)
	{
		rand = new Rand();

		departures = new List<Combo>();
		arrivals = new List<Combo>();

		PlaneSpot[] tmp = GameObject.FindObjectsOfType<PlaneSpot>();
		parkingSpots = new PlaneSpot[tmp.Length];
		foreach(PlaneSpot ps in tmp)
		{
			parkingSpots[ps.spotNum] = ps;
			ps.parkPos = ps.transform.position;
			ps.Hide();
		}

		RegenerateSchedule(clock);

		passengerSpawning.Init();


		// HUD
		hud = new GameObject("FlightScheduleUI");

		hud.AddComponent<Canvas>();
		canvas = hud.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;

		hud.AddComponent<Text>();
		scheduleText = hud.GetComponent<Text>();
		Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		scheduleText.font = font;
		scheduleText.color = Color.red;
		scheduleText.material = font.material;
	}
//
// ============================================================================


// ============================================================================
//	UPDATE
//
	public void Update(Clock clock, Map map)
	{
		// Remove ALL arrivals and departures that are done
		while(	arrivals.Count > 0 &&
				(clock.hour > arrivals[0].flight.hour ||
				(clock.hour >= arrivals[0].flight.hour &&
				clock.minute >= arrivals[0].flight.minute)))
		{
			// Start spawning passengers. Do this here, to ensure spawning starts before the Flight is removed from 'arrivals'!
			Flight arrival = arrivals[0].flight;
			int dt = (clock.hour - arrival.hour) * 60 + clock.minute - arrival.minute;
			if(dt >= 0 && dt <= arrivalSpawnDurationMinutes)
			{
				int num = minPassengerOnPlane + (int)(rand.NextNormalNormal() * (maxPassengersOnPlane - minPassengerOnPlane));
				passengerSpawning.Spawn(arrival.flightNumber, num, clock.Now(), 60.0f * arrivalSpawnDurationMinutes, arrival);
				arrival.spawning = true;
			}

			// "Spawn" the arriving plane
			arrivals[0].plane.Spawn();
			arrivals[0].flight.planeSpawned = true;

			arrivals.RemoveAt(0);
		}
		while(	departures.Count > 0 &&
				(clock.hour > departures[0].flight.hour ||
				(clock.hour >= departures[0].flight.hour &&
				clock.minute >= departures[0].flight.minute)))
		{
			// Make the plane leave
			departures[0].plane.parkingSpot.currentPlane = null;
			departures[0].plane.Despawn();

			departures.RemoveAt(0);
		}

		// Open/close locations
		UpdateLocations(clock, map);

		// Check if departing passengers need to be spawned
		foreach(Combo c in departures)
		{
			Flight d = c.flight;
			int dt = (d.hour - clock.hour) * 60 + d.minute - clock.minute;
			if(!d.spawning && d.spawned == 0)
			{
				if(dt > departureSpawnEndMinutes && dt <= departureSpawnEndMinutes + departureSpawnDurationMinutes)
				{
					int num = minPassengerOnPlane + (int)(rand.NextNormalNormal() * (maxPassengersOnPlane - minPassengerOnPlane));
					passengerSpawning.Spawn(d.flightNumber, num, clock.Now(), 60.0f * departureSpawnDurationMinutes, d);
					d.baggage = (int)(3.0f * num * rand.NextUniform());
					d.spawning = true;
				}
			}
			if((clock.hour * 60 + clock.minute) >= boardingStartByFlightNum[d.flightNumber] &&
				!d.planeSpawned  &&
				c.plane.parkingSpot.currentPlane == null)
			{
				// Spawn plane
				d.planeSpawned = true;
				c.plane.Spawn();
			}
		}

		// Update passenger Spawners
		passengerSpawning.Update(clock);

		PrepareUI(clock, map);
	}

	private void UpdateLocations(Clock clock, Map map)
	{
		// Open/close any staffed check-in desks and gates
		int now = clock.hour * 60 + clock.minute;
		for(int i = 0; i < map.locations[(int)LocationType.CHECKIN].Length; ++i)
		{
			CheckInImpl ci = map.GetCheckInDesk(i) as CheckInImpl;
			if(ci.isOpen)
			{
				// Desk is open. Close it if check-in time is up
				if(checkInEndByFlightNum[ci.flightNumber] < now)
				{
					Airport.Instance.CloseLocation(ci);
				}
			}
			else
			{
				// Desk is closed. Open it if a departure needs it opened.
				foreach(Combo c in departures)
				{
					Flight dep = c.flight;
					if(dep.checkInDesk == i &&
					   now >= checkInStartByFlightNum[dep.flightNumber] &&
					   now < checkInEndByFlightNum[dep.flightNumber])
					{
						Airport.Instance.OpenLocation(ci, dep.flightNumber);
						break;
					}
				}
			}
		}

		for(int i = 0; i < map.locations[(int)LocationType.GATE].Length - 1; ++i)
		{
			GateImpl gate = map.GetGate(i);
			if(gate.isOpen)
			{
				// Gate is open. Close it if boarding time is up
				if(boardingEndByFlightNum[gate.flightNumber] < now)
				{
					Airport.Instance.CloseLocation(gate);
				}
			}
			else
			{
				// Gate is closed. Open it if a departure needs it opened.
				foreach(Combo c in departures)
				{
					Flight dep = c.flight;
					if(dep.gate == i &&
					   now >= boardingStartByFlightNum[dep.flightNumber] &&
					   now < boardingEndByFlightNum[dep.flightNumber])
					{
						Airport.Instance.OpenLocation(gate, dep.flightNumber);
						break;
					}
				}
			}
		}
	}

//
// ============================================================================

	
	// Genereate a new schedule, starting from NOW
	public void RegenerateSchedule(Clock clock)
	{
		// Reset
		flightNumberCounter = 0;
		Plane.planeCounter = 0;
		departures.Clear();
		arrivals.Clear();


		// Random number of planes are already waiting [2, 5]
		int startPlanes = 1;//2 + (int)Mathf.Round(rand.NextUniform() * 3.0f);
		int numDepartures = 40 + (int)Mathf.Round(rand.NextUniform() * 10.0f) + startPlanes;

		// Create the airplanes
		Plane[] planes = new Plane[numDepartures];
		for(int i = 0; i < numDepartures; ++i)
		{
			planes[i] = new Plane();
		}

		// Each departure needs an arrival
		// Use curve in editor to specify departure "Density" over the day
		float scheduleStart = clock.Now();
		float scheduleEnd = 3600.0f * 23.0f;	// stop at 23:00

		float curveArea = Tools.Integrate(departureDensity, 0.0f, 1.0f, 1000);
		float deltaArea = curveArea / (float)numDepartures;
		float lower = 0.0f;
		for(int i = 0; i < numDepartures; ++i)
		{
			// Find the time at which the next flight should be
			float t = Tools.FindTimeForArea(departureDensity, lower, deltaArea, 100);
			lower = 0.995f * t;

			float time = lower * (scheduleEnd - scheduleStart) + scheduleStart;
			time /= 60.0f;
			int h = (int)(time / 60.0f);
			int m = (int)(time - h * 60.0f);

			// Create the new departing Flight
			Flight d = new Flight
			{
				flightNumber = flightNumberCounter++,
				hour = h,
				minute = m,
				minutesDelay = 0,
				gate = 0,
				type = FlightType.DEPARTURE,
				spawning = false,
				spawned = 0,
				toSpawn = 0,
				checkInDesk = 0,
				baggage = 0,
				planeSpawned = false
			};

			Flight a = null;
			if(startPlanes <= 0)
			{
				time -= planes[i].GetTimeOffset();
				h = (int)(time / 60.0f);
				m = (int)(time - h * 60.0f);

				// Create the new arriving flight
				a = new Flight
				{
					flightNumber = flightNumberCounter++,
					hour = h,
					minute = m,
					minutesDelay = 0,
					gate = 0,
					type = FlightType.ARRIVAL,
					spawning = false,
					spawned = 0,
					toSpawn = 0,
					checkInDesk = 0,
					baggage = 0,
					planeSpawned = false
				};

				arrivals.Add(new Combo()
				{
					flight = a,
					plane = planes[i]
				});
			}
			else
			{
				--startPlanes;
			}
			

			planes[i].Init(a, d);
			departures.Add(new Combo()
			{
				flight = d,
				plane = planes[i]
			});
		}



		DEMO_PlanGates();
		DEMO_PlanDesks();

		ResetArrays();
	}

	private void ResetArrays()
	{
		departuresByFlightNum = new Combo[flightNumberCounter + 1];
		arrivalsByFlightNum = new Combo[flightNumberCounter + 1];
		checkInStartByFlightNum = new int[flightNumberCounter + 1];
		checkInEndByFlightNum = new int[flightNumberCounter + 1];
		boardingStartByFlightNum = new int[flightNumberCounter + 1];
		boardingEndByFlightNum = new int[flightNumberCounter + 1];
		for(int i = 0; i <= flightNumberCounter; ++i)
		{
			departuresByFlightNum[i] = new Combo();
			arrivalsByFlightNum[i] = new Combo();
		}
		foreach(Combo c in departures)
		{
			Flight d = c.flight;
			departuresByFlightNum[d.flightNumber].flight = d;

			int t = d.hour * 60 + d.minute;
			checkInStartByFlightNum[d.flightNumber] = t - checkInCloseTimeMinutes - checkInOpenDurationMinutes;
			checkInEndByFlightNum[d.flightNumber] = t - checkInCloseTimeMinutes;

			boardingStartByFlightNum[d.flightNumber] = t - gateCloseTimeMinutes - gateOpenDurationMinutes;
			boardingEndByFlightNum[d.flightNumber] = t - gateCloseTimeMinutes;
		}
		foreach(Combo c in arrivals)
		{
			arrivalsByFlightNum[c.flight.flightNumber].flight = c.flight;
		}
	}

	[HideInInspector] public Map hackMap;
	private void DEMO_PlanGates()
	{
		int num = hackMap.locations[(int)LocationType.GATE][(int)Zone.GATES].Length;
		int[] gatesClose = new int[num];
		for(int i = 0; i < num; ++i)
		{
			gatesClose[i] = 0;
		}

		foreach(Combo d in departures)
		{
			int gateClose = d.flight.hour * 60 + d.flight.minute - gateCloseTimeMinutes;
			int gateOpen = gateClose - gateOpenDurationMinutes;

			// If no gate is found, at least it will be a valid number
			d.flight.gate = 0;

			// Find first available gate
			for(int i = 0; i < 8; ++i)
			{
				if(gatesClose[i] < gateOpen)
				{
					d.flight.gate = i;
					d.flight.boardingStart = gateOpen * 60;
					d.flight.boardingEnd = gateClose * 60;
					d.plane.parkingSpot = parkingSpots[i];
					gatesClose[i] = gateClose;
					break;
				}
			}
		}
	}

	private void DEMO_PlanDesks()
	{
		int num = hackMap.locations[(int)LocationType.CHECKIN][(int)Zone.MAIN_HALL].Length;
		int[] desksClose = new int[num];
		for(int i = 0; i < num; ++i)
		{
			desksClose[i] = 0;
		}

		foreach(Combo d in departures)
		{
			int deskOpen = d.flight.hour * 60 + d.flight.minute -
				(checkInOpenDurationMinutes + checkInCloseTimeMinutes);

			// If no desk is found, at least it will be a valid number
			d.flight.checkInDesk = 0;

			// Find first available desk
			for(int i = 0; i < num; ++i)
			{
				if(desksClose[i] < deskOpen)
				{
					d.flight.checkInDesk = i;
					desksClose[i] = deskOpen + checkInOpenDurationMinutes;
					d.flight.checkinStart = deskOpen * 60;
					d.flight.checkinEnd = desksClose[i] * 60;
					break;
				}
			}
		}

	}



	private void PrepareUI(Clock clock, Map map)
	{
		int ai = 0;
		string str = "Time:  " + clock.ToStringHMS() +
			"       Time.timeScale = " + Time.timeScale +
			"       Extra clock speed = " + clock.extraSpeed + "\n\nArrivals:\n";

		foreach(Combo c in arrivals)
		{
			Flight a = c.flight;
			str += "Time:  " + Tools.TimeToString(a.hour, a.minute) +
				"    Delay:  " + Tools.TimeToString(0, a.minutesDelay) +
				"    - Spawning: " + a.spawning +
				"   Spawned: " + a.spawned +
				"\\" + a.toSpawn +
				"   Plane: " + c.plane.planeNum + "\n";
			if(++ai >= numOfFlightsVisible)
			{
				break;
			}
		}

		foreach(PassengerSpawning.Spawner a in passengerSpawning.activeSpawners)
		{
			if(a.flight.type == FlightType.ARRIVAL)
			{
				str += " ARRIVAL spawning. FlightNum: " + a.flight.flightNumber + "   Spawned: " + a.spawned + "\\" +
					   a.flight.toSpawn + "\n";
			}
		}

		str += "\nDepartures:\n";
		ai = 0;
		foreach(Combo c in departures)
		{
			Flight d = c.flight;
			str += "Time:  " + Tools.TimeToString(d.hour, d.minute) +
				"    Delay:  " + Tools.TimeToString(0, d.minutesDelay) +
				"    Desk:  " + d.checkInDesk +
				"    Gate:  " + (int)(d.gate) + 
				"    - Spawning: " + d.spawning +
				"   Spawned: " + d.spawned +
				"\\" + d.toSpawn +
				"   Plane: " + c.plane.planeNum + "\n";
			if(++ai >= numOfFlightsVisible)
			{
				break;
			}
		}



		scheduleText.text = str;
	}
}
