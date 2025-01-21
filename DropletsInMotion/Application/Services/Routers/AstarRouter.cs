using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure;

namespace DropletsInMotion.Application.Services.Routers
{
    public class AstarRouter
    {

        public AstarRouter() { }

        public State? Search(State initialState, Frontier frontier)
        {
            initialState.IsGoalStateReachable();

            frontier.Add(initialState);
            HashSet<State> explored = new HashSet<State>();

            while (true)
            {
                if (frontier.IsEmpty()) return null;

                State state = frontier.Pop();

                if (state.IsGoalState())
                {

                    return state;
                    
                }

                explored.Add(state);
                Debugger.ExploredStates += 1;

                var expandedStates = state.GetExpandedStates();
                foreach (var expandedState in expandedStates)
                {
                    if (!frontier.Contains(expandedState) && !explored.Contains(expandedState))
                    {
                        frontier.Add(expandedState);
                    }
                    else
                    {
                        Debugger.ExistingStates += 1;
                    }
                }
            }
        }
    }
}
