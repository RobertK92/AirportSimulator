
using System;
using System.Collections.Generic;
using AI;
using UnityEngine;


// New locations CANNOT be added at runtime!
// A location CANNOT change its LocationType at runtime!
// A location CANNOT change its Transform at runtime!
// A location CAN open and close at runtime.

[System.Serializable]
public class Map
{
	// PUBLIC VARS
	public bool destroyLocationGameObjects = true;
	public int maxLocationsPerType = 25;


	// HIDDEN VARS
	// [LocationType][Zone][index]
	[HideInInspector] public LocationImpl[][][] locations;


	private GameObject queues;




// ============================================================================
//	INIT
//
	public void Init(Schedule sched)
	{
		locations = new LocationImpl[(int)LocationType.NUM][][];
		queues = new GameObject();
		queues.name = "Queues";

		

		// Store all locations temporarily in lists
		List<List<List<Location>>> locs = new List<List<List<Location>>>();
		for(int i = 0; i < (int)LocationType.NUM; ++i)
		{
			locs.Add(new List<List<Location>>());
			for(int j = 0; j < (int)Zone.NUM; ++j)
			{
				locs[i].Add(new List<Location>());
			}
		}

		Location[] allLocs = GameObject.FindObjectsOfType<Location>();
		foreach(Location l in allLocs)
		{
			locs[(int)l.locationType][(int)l.zone].Add(l);
		}
		

		// Create location implementations.
		InitLocations(locs, sched);



		//  TODO: HACK: Open up everything, and then close off some locations
		foreach(LocationImpl[][] lll in locations)
		{
			foreach(LocationImpl[] ll in lll)
			{
				foreach(LocationImpl l in ll)
				{
					Airport.Instance.OpenLocation(l, 0);
				}
			}
		}
		// TODO: HACK: Since all locations are open for game review, close the gates and desks, so they start closed
		foreach(GateImpl g in locations[(int)LocationType.GATE][(int)Zone.GATES])
		{
			Airport.Instance.CloseLocation(g);
		}
		foreach(CheckInImpl c in locations[(int)LocationType.CHECKIN][(int)Zone.MAIN_HALL])
		{
			Airport.Instance.CloseLocation(c);
		}
		// TODO: open up all locations that are always open

		// TODO: employees are not spent on gates and check-in desks, unless
		//            more than a single one is used for a flight. It would be
		//            silly to allow the Airport to be so understaffed that
		//            some flights cannot be staffed even for the bare basics.
	}

//
// ============================================================================


// ============================================================================
//	UPDATE
//
	public void Update(Clock clock)
	{
		for(int i = 0; i < (int)LocationType.NUM; ++i)
		{
			for(int j = 0; j < locations[i].Length; ++j)
			{
				for(int k = 0; k < locations[i][j].Length; ++k)
				{
					locations[i][j][k].Update(clock);
				}
			}
		}
	}

//
// ============================================================================


	public LocationImpl GetLocation(LocationType type, Zone zone, int num)
	{
		return locations[(int)type][(int)zone][num];
	}

	public LocationImpl GetCheckInDesk(int deskNum)
	{
		return locations[(int)LocationType.CHECKIN][(int)Zone.MAIN_HALL][deskNum];
	}

	public SecurityImpl GetSecurityPort(int portNum)
	{
		return locations[(int)LocationType.SECURITY][(int)Zone.MAIN_HALL][portNum] as SecurityImpl;
	}

	public GateImpl GetGate(int gateNum)
	{
		return locations[(int)LocationType.GATE][(int)Zone.GATES][gateNum] as GateImpl;
	}


	public void LockAirport()
	{
		for(int i = 0; i < locations.Length; ++i)
		{
			for(int j = 0; j < locations[i].Length; ++j)
			{
				for(int k = 0; k < locations[i][j].Length; ++k)
				{
					Airport.Instance.CloseLocation(locations[i][j][k]);
				}
			}
		}
	}


// ============================================================================
//	INIT
//
	private void InitLocations(List<List<List<Location>>> locs, Schedule sched)
	{
		int id = (int)LocationType.TOILET;
		CreateArray<ToiletImpl>(id, locs[id], sched);
		id = (int)LocationType.CHECKIN;
		CreateArray<CheckInImpl>(id, locs[id], sched);
		id = (int)LocationType.SELF_CHECKIN;
		CreateArray<SelfCheckInImpl>(id, locs[id], sched);
		id = (int)LocationType.SECURITY;
		CreateArray<SecurityImpl>(id, locs[id], sched);
		id = (int)LocationType.CUSTOMS;
		CreateArray<CustomsImpl>(id, locs[id], sched);
		id = (int)LocationType.GATE;
		CreateArray<GateImpl>(id, locs[id], sched);
		id = (int)LocationType.INFODESK;
		CreateArray<InfoDeskImpl>(id, locs[id], sched);
		id = (int)LocationType.ENTRANCE;
		CreateArray<EntranceImpl>(id, locs[id], sched);
		id = (int)LocationType.EXIT;
		CreateArray<ExitImpl>(id, locs[id], sched);
		id = (int)LocationType.RESTAURANT;
		CreateArray<RestaurantImpl>(id, locs[id], sched);
		id = (int)LocationType.AH;
		CreateArray<AHImpl>(id, locs[id], sched);
		id = (int)LocationType.CAFE;
		CreateArray<CafeImpl>(id, locs[id], sched);
		id = (int)LocationType.TAXFREE_STORE;
		CreateArray<TaxfreeImpl>(id, locs[id], sched);
		id = (int)LocationType.LUGGAGE_CAROUSSEL;
		CreateArray<LuggageImpl>(id, locs[id], sched);
		id = (int)LocationType.ARRIVAL_SECURITY;
		CreateArray<ArrivalSecurityImpl>(id, locs[id], sched);
	}

	private void CreateArray<T>(int id, List<List<Location>> ll, Schedule sched) where T : LocationImpl, new()
	{
		int num = ll.Count;
		locations[id] = new T[num][];
		for(int i = 0; i < locations[id].Length; ++i)
		{
			locations[id][i] = new T[ll[i].Count];
		}

		GameObject queueGroup = new GameObject();
		queueGroup.name = (LocationType)id + "_QueueGroup";
		queueGroup.transform.parent = queues.transform;

		bool usePrefIndex = typeof(T) == typeof(GateImpl) || typeof(T) == typeof(CheckInImpl);

		foreach(List<Location> l in ll)
		{
			int j = 0;
			foreach(Location loc in l)
			{
				GameObject theQueue = new GameObject();
				theQueue.AddComponent<ActionTarget>();
				theQueue.name = loc.gameObject.name + "_Queue";
				theQueue.transform.position = loc.gameObject.transform.position;
				theQueue.transform.rotation = loc.gameObject.transform.rotation;
				theQueue.transform.parent = queueGroup.transform;


				// PUT TEXT IN THE SCENE
				GameObject text = new GameObject();
				text.name = "Text";
				text.transform.position = loc.gameObject.transform.position + new Vector3(0.0f, 5.0f, 0.0f);
				TextMesh textMesh = text.AddComponent<TextMesh>();
				Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
				textMesh.font = font;
				textMesh.color = Color.black;
				text.transform.rotation = loc.transform.rotation;
				text.transform.Rotate(0.0f, -90.0f, 0.0f);
				textMesh.text = loc.zone.ToString() + "::" + loc.locationType.ToString();
				// END


				ActionTarget q = theQueue.GetComponent<ActionTarget>();

				int index = (usePrefIndex ? loc.preferredNum : j);
				locations[id][(int)loc.zone][index] = new T
				{
					schedule = sched,
					zone = loc.zone,
					type = loc.locationType,
					position = loc.gameObject.transform.position,
					queue = q,
					goRender = loc.gameObject.GetComponent<Renderer>(),
					isOpen = false,
					isOpening = false,
					go = loc.gameObject,
					needsStaff = (	   loc.locationType == LocationType.CHECKIN
									|| loc.locationType == LocationType.ARRIVAL_SECURITY
									|| loc.locationType == LocationType.CUSTOMS
									|| loc.locationType == LocationType.GATE
									|| loc.locationType == LocationType.INFODESK
									|| loc.locationType == LocationType.LUGGAGE_CAROUSSEL
									|| loc.locationType == LocationType.SECURITY)
				};
				text.transform.parent = q.transform;
				++j;
			}
		}
		if(destroyLocationGameObjects)
		{
			foreach(List<Location> l in ll)
			{
				foreach(Location loc in l)
				{
					GameObject.Destroy(loc.gameObject);
				}
			}
		}
	}

//
// ============================================================================
}
