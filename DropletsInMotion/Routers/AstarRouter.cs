using DropletsInMotion.Routers.Functions;
using DropletsInMotion.Routers.Models;

namespace DropletsInMotion.Routers
{
    public class AstarRouter
    {

        public AstarRouter()
        {

        }

        public State Search(State initialState, Frontier frontier, double time)
        {
            frontier.Add(initialState);
            HashSet<State> explored = new HashSet<State>();

            while (true)
            {
                if (frontier.IsEmpty()) return null; // TODO: Do we want to return null? 
                State state = frontier.Pop();

                if (state.IsGoalState() || state.G >= 60)
                {
                    Console.WriteLine($"We reached a goal state at depth {state.G}");
                    
                    ApplicableFunctions.PrintContaminationState(state.ContaminationMap); // TODO: this can be removed after debug

                    return state;
                }

                explored.Add(state);

                var expandedStates = state.GetExpandedStates();
                foreach (var expandedState in expandedStates)
                {
                    if (!frontier.Contains(expandedState) && !explored.Contains(expandedState))
                    {
                        ApplicableFunctions.IncrementStateAmount(1);
                        frontier.Add(expandedState);
                    }
                    else
                    {
                        ApplicableFunctions.StateAmountExists += 1;
                        //Console.WriteLine($"State already exists {expandedState.GetHashCode()}");
                    }
                }

            }
        }
    }
}
