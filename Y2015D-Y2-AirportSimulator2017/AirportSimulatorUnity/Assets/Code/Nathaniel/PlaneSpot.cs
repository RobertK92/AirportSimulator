using UnityEngine;
using System.Collections;

public class PlaneSpot : MonoBehaviour
{
	public int spotNum = 0;
	public float spawnDist = 25.0f;
	public float planeSpeed = 8.0f;

	private State state;
	public Vector3 parkPos;

	public Plane currentPlane = null;

	private enum State
	{
		SPAWNING,
		PARKED,
		DESPAWNING,
		INACTIVE
	}

	void Update()
	{
		if(currentPlane == null)
		{
			if(gameObject.activeInHierarchy)
			{
				state = State.INACTIVE;
				Hide();
			}

			return;
		}

		switch(state)
		{
		case State.SPAWNING:
			{
				if(parkPos.x > transform.position.x)
				{
					// Move forward
					float dx = -Time.deltaTime * planeSpeed;
					transform.position += new Vector3(dx, 0.0f, 0.0f);
				}
				else
				{
					transform.position = parkPos;
					state = State.PARKED;
				}
				break;
			}
		case State.PARKED:
			{
				break;
			}
		case State.DESPAWNING:
			{
				if(transform.position.x < parkPos.x + spawnDist)
				{
					// Move forward
					float dx = Time.deltaTime * planeSpeed;
					transform.position += new Vector3(dx, 0.0f, 0.0f);
				}
				else
				{
					state = State.INACTIVE;
				}
				break;
			}
		case State.INACTIVE:
			{
				Hide();
				break;
			}
		}
	}

	public void Hide()
	{
		currentPlane = null;
		gameObject.SetActive(false);
	}

	public void Show()
	{
		gameObject.SetActive(true);
	}

	public void Leave()
	{
		state = State.DESPAWNING;
	}
}
