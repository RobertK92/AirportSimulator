namespace AI
{
	namespace GOAPv3
	{

		public delegate bool GoapGoal(GoapState state);
		public delegate bool AirportGoal(AirportState state);
		public interface IGoapAction
		{
			/// <summary>
			/// The conditions that need to be met in order for the action to execute.
			/// </summary>
			GoapGoal[] Preconditions { get; }

			/// <summary>
			/// A stack of available targets 
			/// </summary>
			LocationImpl Target { get; }

			/// <summary>
			/// The cost of the action, which the planner makes use of.
			/// </summary>
			int Cost { get; }

			/// <summary>
			/// The duration of the action.
			/// </summary>
			float Duration { get; }

			/// <summary>
			/// Initializes the action and returns true if it's able to be executed.
			/// </summary>
			/// <param name="state">The current airport state.</param>
			/// <returns>True if the action is able to be executed.</returns>
			bool Init(GoapAgent agent);

			/// <summary>
			/// Applies the action's effects on a state.
			/// </summary>
			/// <param name="state"></param>
			void ChangeState(GoapState state);

			/// <summary>
			/// Notifies the airport when an action has been performed.
			/// </summary>
			void NotifyAirport();

			/// <summary>
			/// Returns true if action is ready to be executed.
			/// </summary>
			/// <returns>True if the action is ready to be executed.</returns>
			bool Countdown();

			/// <summary>
			/// 
			/// </summary>
			/// <param name="state"></param>
			bool Execute(GoapState state);
		}
	}
}