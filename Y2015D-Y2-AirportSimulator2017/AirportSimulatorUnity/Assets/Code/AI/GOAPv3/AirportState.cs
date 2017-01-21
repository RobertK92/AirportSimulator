namespace AI
{
	namespace GOAPv3
	{
		public class AirportState
		{
			// Gates
			public bool Gate1;
			public bool Gate2;
			public bool Gate3;
			public bool Gate4;
			public bool Gate5;
			public bool Gate6;
			public bool Gate7;
			public bool Gate8;
			public bool GateArrivals;

			// Luggage carrousels
			public bool LuggageCarrousel1;
			public bool LuggageCarrousel2;

			// Security gates
			public bool SecurityGate1;
			public bool SecurityGate2;
			public bool SecurityGateArrivals;

			// Check-in desks
			public bool CheckInDesk1;
			public bool CheckInDesk2;
			public bool CheckInDesk3;
			public bool CheckInDesk4;
			public bool CheckInDesk5;

			// Stores & restaurants
			public bool AlbertHeijn;
			public bool Cafe1;
			public bool Cafe2;
			public bool Restaurant1;

			/// <summary>
			/// Copies the data of another GoapState object.
			/// </summary>
			/// <param name="target">The other GoapState object.</param>
			public void CopyTo(AirportState target)
			{
				foreach (var field in typeof(AirportState).GetFields())
				{
					field.SetValue(target, field.GetValue(this));
				}
			}
		}
	}
}