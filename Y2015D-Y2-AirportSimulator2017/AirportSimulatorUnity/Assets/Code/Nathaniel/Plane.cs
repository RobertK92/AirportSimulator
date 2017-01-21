using UnityEngine;
using System.Collections;

public class Plane
{
	public Flight arrival;
	public Flight departure;

	// Times in minutes
	public float unboardTime = 10.0f;
	public float unloadTime = 15.0f;
	public float cleanTime = 20.0f;
	public float refuelTime = 10.0f;

	private uint state = 0;
	public int planeNum = 0;
	public static int planeCounter = 0;

	public PlaneSpot parkingSpot;

	private enum State
	{
		PARKING = (1 << 0),
		UNBOARDING = (1 << 1),
		UNLOADING = (1 << 2),
		CLEANING = (1 << 3),
		FUELING = (1 << 4),
		BOARDING = (1 << 5),
		LOADING = (1 << 6),
		LEAVING = (1 << 7),
		ARRIVING = (1 << 8),
		DEPARTING = (1 << 9)
	}

	public void Init(Flight arr, Flight dep)
	{
		arrival = arr;
		departure = dep;

		if(arr != null)
		{
			state |= (uint)State.ARRIVING | (uint)State.PARKING;
		}
		else
		{
			state |= (uint)State.DEPARTING | (uint)State.PARKING;
		}

		planeNum = planeCounter;
		planeCounter++;
	}

	public float GetTimeOffset()
	{
		return unboardTime + unloadTime + cleanTime + refuelTime;
	}

	public void Spawn()
	{
		parkingSpot.currentPlane = this;
		parkingSpot.transform.position = parkingSpot.parkPos + new Vector3(parkingSpot.spawnDist, 0.0f, 0.0f);

		parkingSpot.Show();
	}

	public void Despawn()
	{
		parkingSpot.currentPlane = null;
		parkingSpot.Leave();
	}
}
