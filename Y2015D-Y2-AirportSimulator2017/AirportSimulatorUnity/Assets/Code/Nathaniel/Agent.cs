using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour
{
	public int flightNumber;
	public Flight flight;
	public int baggage;		// num of pieces of baggage

	[HideInInspector] public Grid grid;

	public void TakeMessage(Message mess, Status stat)
	{
		// TODO: do stuff with the message
	}
}
