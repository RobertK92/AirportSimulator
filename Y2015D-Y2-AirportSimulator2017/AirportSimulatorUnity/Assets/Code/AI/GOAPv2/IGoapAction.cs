namespace GOAP
{
	using System.Collections.Generic;

	public interface IGoapAction
	{
		Dictionary<GoapState, bool> Preconditions { get; }
		Dictionary<GoapState, bool> Effects { get; }

		int Cost { get; }
		bool IsInRange { get; }

		//bool IsAbleToRun();
		void Execute(GoapAgent agent);
	}
}