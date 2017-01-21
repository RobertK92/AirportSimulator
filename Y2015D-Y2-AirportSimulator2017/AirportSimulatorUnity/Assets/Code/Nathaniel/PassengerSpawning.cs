using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AI.GOAPv3;
using Random = UnityEngine.Random;

[System.Serializable]
public class PassengerSpawning
{
	public GameObject passengerPrefab = null;
	public GameObject[] entrances;
	public GameObject arrivalGate;

	public float minPassengerLength = 1.3f;
	public float maxPassengerLength = 2.1f;

	public/*private*/ List<Spawner> activeSpawners;
	private int spawnSamples = 10;
	private Rand rand;
	private GameObject agentGroup;

	private Grid grid;


	public/*private*/ class Spawner
	{
		public int flightNum;
		public float startTime;
		public int spawned;
		public int[] spawnCDF;
		public float dt;
		public Flight flight;
	}

	public void Init()
	{
		rand = new Rand();
		agentGroup = new GameObject();
		agentGroup.name = "Agents";
		activeSpawners = new List<Spawner>();
		grid = GameObject.FindObjectOfType<Grid>();
	}

	/// <summary>
	/// Spawn the specified number of passengers during the specified
	/// time period, for the given Flight number of a departure.
	/// </summary>
	/// <param name="flightNumber"> The Flight Number of the passengers </param>
	/// <param name="numOfPassengers"> Number of passengers to spawn </param>
	/// <param name="spawnStartTime"> Start of the spawning period, in [seconds] from the start of the day </param>
	/// <param name="spawnDuration"> Duration of the spawning period, in [seconds] from spawnStartTime </param>
	/// <param name="theFlight"> Flight used for referencing internally. Spawner will modify the Flight ocasionally </param>
	public void Spawn(int flightNumber, int numOfPassengers,
		float spawnStartTime, float spawnDuration, Flight theFlight)
	{
		// First check if a spawner for this flight already exists
		foreach(Spawner s in activeSpawners)
		{
			if(s.flightNum == flightNumber)
			{
				return;
			}
		}

		// Create new Spawner
		Spawner newSpawn = new Spawner
		{
			flightNum = flightNumber,
			startTime = spawnStartTime,
			spawned = 0,
			spawnCDF = new int[spawnSamples],
			dt = spawnDuration / spawnSamples,
			flight = theFlight
		};

		// Determine when each passenger should be spawned
		for(int i = 0; i < numOfPassengers; ++i)
		{
			// For each passenger, determine at what time (normal distr) they
			//  spawn and put them in the appropriate spawRate slot.
			float t = rand.NextNormalNormal() * spawnDuration / newSpawn.dt;
			if(t < spawnSamples - 1)
			{
				newSpawn.spawnCDF[(int)t]++;
			}
		}
		// Change PDF to CDF, for more convenience when updating
		for(int i = 1; i < spawnSamples; ++i)
		{
			newSpawn.spawnCDF[i] += newSpawn.spawnCDF[i - 1];
		}
		newSpawn.flight.toSpawn = newSpawn.spawnCDF[spawnSamples - 1];

		// Don't forget to add the spawner to the list
		activeSpawners.Add(newSpawn);
	}


	public void Update(Clock clock)
	{
		float now = clock.Now();

		List<Spawner> toRemove = new List<Spawner>();
		foreach(Spawner s in activeSpawners)
		{
			// Stop spawning when all passengers are spawned
			if(s.spawned >= s.spawnCDF[spawnSamples - 1])
			{
				toRemove.Add(s);
				continue;
			}

			
			// Determine number of passengers to spawn in this Update
			float time = now - s.startTime;
			int toSpawn = 0;
			if(time >= s.dt * (spawnSamples - 1))
			{
				toSpawn = s.spawnCDF[spawnSamples - 1] - s.spawned;
				s.spawned = s.spawnCDF[spawnSamples - 1];
			}
			else
			{
				int index = (int)(time / s.dt);
				time = time / s.dt - index;
				int num = (int)((1.0f - time) * s.spawnCDF[index] + time * s.spawnCDF[index + 1]);
				toSpawn = num - s.spawned;
				s.spawned = num;
			}
			s.flight.spawned = s.spawned;

			// Spawn the people
			for(int i = 0; i < toSpawn; ++i)
			{
				Transform trans;
				bool departure = s.flight.type == FlightType.DEPARTURE;
				if(departure)
				{
					trans = entrances[(int)Mathf.Round((entrances.Length - 1) * rand.NextUniform())].transform;
				}
				else
				{
					trans = arrivalGate.transform;
				}

				GameObject passenger = GameObject.Instantiate(passengerPrefab, trans.position, trans.rotation) as GameObject;
				// Scale passenger. scale of 0.65 corresponds to ~2 meter. scale of 1.0 corresponds to ~3 meter.
				//  => for length, use 1 meter = 0.3333333333
				// Length = minLength + rand.NextNormalNormal() * (maxLength - minLength)
				// Scale = 0.33333333f * Length
				float scale = 0.33333333f * (minPassengerLength + rand.NextNormalNormal() * (maxPassengerLength - minPassengerLength));
				passenger.transform.localScale = new Vector3(scale, scale, scale);
				grid.AddObject(passenger.GetComponent<Avoidance>());
				passenger.transform.parent = agentGroup.transform;

				passenger.GetComponent<GoapAgent>().State.CurrentZone = (departure ? Zone.MAIN_HALL : Zone.ARRIVAL);

				// Assign flightNumber, Flight info and random number of bags
				Agent agent = passenger.GetComponent<Agent>();
				agent.flight = s.flight;
				agent.flightNumber = s.flightNum;
				agent.baggage = (int)Mathf.Round(rand.NextUniform() * 3.0f);
				agent.grid = grid;
			}
		}

		// Remove finished Spawners
		foreach(Spawner s in toRemove)
		{
			s.flight.spawning = false;
			activeSpawners.Remove(s);
		}
	}
}
