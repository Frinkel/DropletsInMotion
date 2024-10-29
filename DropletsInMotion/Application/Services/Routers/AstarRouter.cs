using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure;

namespace DropletsInMotion.Application.Services.Routers
{
    public class AstarRouter
    {

        public AstarRouter()
        {

        }

        public State Search(State initialState, Frontier frontier)
        {
            initialState.IsGoalStateReachable();

            frontier.Add(initialState);
            HashSet<State> explored = new HashSet<State>();

            while (true)
            {
                if (frontier.IsEmpty()) return initialState;
                State state = frontier.Pop();

                if (state.IsGoalState())
                {
                    Console.WriteLine($"We reached a goal state at depth {state.G}");

                    List<State> chosenStates = new List<State>();
                    State currentState = state;
                    while (currentState.Parent != null)
                    {
                        chosenStates.Add(currentState);
                        currentState = currentState.Parent;
                    }
                    chosenStates = chosenStates.OrderBy(s => s.G).ToList();

                    foreach (var returnState in chosenStates)
                    {
                        if (returnState.IsPossibleEndState())
                        {
                            return returnState;
                        }
                    }

                    return state;
                }
                else if (state.G >= 60)
                {
                    Console.WriteLine("This");
                    return state;
                }

                explored.Add(state);

                var expandedStates = state.GetExpandedStates();
                foreach (var expandedState in expandedStates)
                {
                    if (!frontier.Contains(expandedState) && !explored.Contains(expandedState))
                    {
                        Debugger.ExploredStates += 1;
                        frontier.Add(expandedState);
                    }
                    else
                    {
                        Debugger.ExistingStates += 1;
                        //Console.WriteLine($"State already exists {expandedState.GetHashCode()}");
                    }
                }

            }
        }
    }
}
