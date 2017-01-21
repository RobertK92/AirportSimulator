using UnityEngine;
using System.Collections;


[System.Serializable]
public class Clock
{
	[HideInInspector] public int hour;
	[HideInInspector] public int minute;
	[HideInInspector] public int second;
	[Range(0.0f, 100.0f)]   public float timeScale = 1.0f;
	[Range(0.0f, 10000.0f)] public float extraSpeed = 1.0f;

	private float inGameTime;

	public void Init(int startHour)
	{
		hour = startHour;
		minute = 0;
		second = 0;

		inGameTime = (hour * 60.0f + minute) * 60.0f + second;
	}

	public void Update()
	{
		Time.timeScale = timeScale;

		// Using dt is not as accurate as keeping absolute measurements and
		//  finding differences manually, but it is much easier, and accurate
		//  enough for the in-game time.
		inGameTime += extraSpeed * Time.deltaTime;

		int t = (int)inGameTime;
		hour = (t / 3600) % 24;
		minute = (t / 60) % 60;
		second = t % 60;
	}

	public void SetTime(int h, int m, int s)
	{
		hour = h;
		minute = m;
		second = s;

		inGameTime = (hour * 60.0f + minute) * 60.0f + second;
	}

	public float Now()
	{
		return inGameTime;
	}

	public float TotalSpeed()
	{
		return timeScale * extraSpeed;
	}

	public override string ToString()
	{
		return ""	+ (hour / 10) + (hour % 10) + ':'
					+ (minute / 10) + (minute % 10) + ':'
					+ (second / 10) + (second % 10) + "::"
					+ (Now() - (int)Now());
	}

	public string ToStringHMS()
	{
		return ""	+ (hour / 10) + (hour % 10) + ':'
					+ (minute / 10) + (minute % 10) + ':'
					+ (second / 10) + (second % 10);
	}

	public string ToStringHM()
	{
		return ""	+ (hour / 10) + (hour % 10) + ':'
					+ (minute / 10) + (minute % 10);
	}
}

public class Rand
{
	// Keep uniform and normal index separate. This was chosen to prevent
	//  skipping of values. But no testing was done, and either approach may be
	//  similarly good.
	private uint lastUniformIndex = 0;
	private uint lastNormalIndex = 0;

	// Keep these static. This has the advantage that multiple FastRands will
	//  not take a lot of space. This has the disadvantage that multiple
	//  FastRands all have the same set of random values.
	private static uint capacity = 1000;
	private static float[] uniform = null;
	private static float[] normal = null;

	public Rand()
	{
		if(uniform == null)
		{
			uniform = new float[capacity];
			normal = new float[capacity];

			Generate();
		}
	}

	public Rand(uint size)
	{
		if(uniform == null)
		{
			capacity = size;
			uniform = new float[capacity];
			normal = new float[capacity];

			Generate();
		}
	}

	/// <summary>
	/// "seed" the random engine. Since this class uses a pre-generated array,
	/// Seed() simply changes the current index into the array.
	/// </summary>
	/// <param name="seed"></param>
	public void Seed(uint seed)
	{
		lastUniformIndex = seed % capacity;
		lastNormalIndex = lastUniformIndex;
	}

	/// <summary>
	/// Return the next uniform random float from the pre-generated array using
	/// a UNIFORM distribution.
	/// </summary>
	/// <returns></returns>
	public float NextUniform()
	{
		lastUniformIndex = (lastUniformIndex + 2 > capacity ? 0 : lastUniformIndex + 1);
		return uniform[lastUniformIndex];
	}

	/// <summary>
	/// Return the next normal random float from the pre-generated array using
	/// a NORMAL distribution.
	/// </summary>
	/// <returns></returns>
	public float NextNormal()
	{
		lastNormalIndex = (lastNormalIndex + 2 > capacity ? 0 : lastNormalIndex + 1);
		return normal[lastNormalIndex];
	}

	/// <summary>
	/// Return the next normal random float from the pre-generated array using
	/// a NORMAL distribution. AND map it to [0, 1]
	/// </summary>
	/// <returns></returns>
	public float NextNormalNormal()
	{
		lastNormalIndex = (lastNormalIndex + 2 > capacity ? 0 : lastNormalIndex + 1);
		float val = 0.166666666f * (3.0f + normal[lastNormalIndex]);
		return (val < 0 ? 0.0f : (val > 1 ? 1.0f : val));
	}

	/// <summary>
	/// Generate the arrays for the different distributions, using Random.value
	/// to generate uniformly distributed values, and Box–Muller method for the
	/// normal distribution (with two Random.value values).
	/// </summary>
	private void Generate()
	{
		for(uint i = 0; i < capacity; i += 2)
		{
			float u = Random.value;
			float v = Random.value;
			float a = Mathf.Sqrt(-2.0f * Mathf.Log(u));
			float b = 2.0f * Mathf.PI * v;
			normal[i] = a * Mathf.Cos(b);
			normal[i + 1] = a * Mathf.Sin(b);

			uniform[i] = Random.value;
			uniform[i + 1] = Random.value;
		}
	}
}


// ENUMS
public enum LocationType
{
	TOILET = 0,
	CHECKIN = 1,
	SELF_CHECKIN = 2,
	SECURITY = 3,
	CUSTOMS = 4,
	GATE = 5,
	INFODESK = 6,
	ENTRANCE = 7,
	EXIT = 8,
	RESTAURANT = 9,
	AH = 10,
	CAFE = 11,
	TAXFREE_STORE = 12,
	LUGGAGE_CAROUSSEL = 13,
	ARRIVAL_SECURITY = 14,
	NUM
}

public enum Zone
{
	MAIN_HALL = 0,
	GATES = 1,
	ARRIVAL = 2,
	NUM
}

public enum Message
{
	CURRENT_STEP_FINISHED,
	CURRENT_STEP_CANCELLED
}

public enum Status
{
	SUCCES,
	FAILURE,
	WRONG_LOCATION,
	TOO_EARLY,
	TOO_LATE,
	MISSING_PREREQUISITES,
	CANCELLED,
	LOCATION_BUSY,
	CLOSED
}


public enum FlightType
{
	ARRIVAL,
	DEPARTURE
}

public class Flight
{
	public int flightNumber;
	public int hour;
	public int minute;
	public int minutesDelay;
	public int gate;
	public FlightType type;
	public bool spawning;
	public int spawned;
	public int toSpawn;
	public int checkInDesk;
	public int baggage;
	public bool planeSpawned;
	public float boardingStart;
	public float boardingEnd;
	public float checkinStart;
	public float checkinEnd;
}


public static class Tools
{
	public static string TimeToString(int hour, int minutes)
	{
		return "" + (hour / 10) + (hour % 10) + ':' +
		       (minutes / 10) + (minutes % 10);
	}

	public static string TimeToString(int hour, int minutes, int seconds)
	{
		return "" + (hour / 10) + (hour % 10) + ':' +
		       (minutes / 10) + (minutes % 10) + ':' +
		       (seconds / 10) + (seconds % 10);
	}

	public static string TimeToString(Clock c)
	{
		return "" + (c.hour / 10) + (c.hour % 10) + ':' +
		       (c.minute / 10) + (c.minute % 10) + ':' +
		       (c.second / 10) + (c.second % 10);
	}

	public static float Integrate(AnimationCurve curve, float lower, float upper, uint steps)
	{
		lower = Mathf.Clamp(lower, 0.0f, 1.0f);
		upper = Mathf.Clamp(upper, 0.0f, 1.0f);
		if(upper < lower)
		{
			float tmp = lower;
			lower = upper;
			upper = tmp;
		}

		float dt = (upper - lower) / (float)steps;
		float t = 0.0f;
		float A = 0.0f;
		for(uint i = 0; i + 1 < steps; ++i)
		{
			float left = curve.Evaluate(t);
			t += dt;
			float right = curve.Evaluate(t);
			
			A += 0.5f * (left + right) * dt;
		}
		
		return A;
	}

	public static float FindTimeForArea(AnimationCurve curve, float lower, float area, uint steps)
	{
		lower = Mathf.Clamp(lower, 0.0f, 2.0f);
		float upper = 1.0f;
		if(lower >= 1.0f)
		{
			return 1.0f;
		}

		for(int i = 0; i < 5; ++i)
		{
			float dt = (upper - lower) / (float)steps;
			float A = 0.0f;
			float t = lower;
			while(A < area)
			{
				float left = curve.Evaluate(t);
				t += dt;
				float right = curve.Evaluate(t);

				A += 0.5f * (left + right) * dt;
			}

			upper = t;

			if(Mathf.Abs(A - area) < 0.01f)
			{
				break;
			}
		}

		return upper;
	}
}