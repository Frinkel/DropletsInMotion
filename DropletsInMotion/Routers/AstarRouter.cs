using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Compilers.Models;
using DropletsInMotion.Domain;
using DropletsInMotion.Routers.Functions;
using DropletsInMotion.Routers.Models;

namespace DropletsInMotion.Routers
{
    public class AstarRouter
    {

        public AstarRouter()
        {

        }

        public Tuple<byte[,], Dictionary<string, Agent>, List<BoardAction>> Search(State initialState, Frontier frontier, double time)
        {
            frontier.Add(initialState);
            HashSet<State> explored = new HashSet<State>();

            while (true)
            {
                if (frontier.IsEmpty()) return null; // TODO: Do we want to return null? 
                State state = frontier.Pop();

                // Reached goal state
                if (state.IsGoalState() || state.G >= 10)
                {
                    Console.WriteLine($"We reached a goal state at depth {state.G}");
                    var actions = state.ExtractActions(time);
                    var agents = state.Agents;
                    var contaminationMap = state.ContaminationMap;

                    //foreach (var action in actions)
                    //{
                    //    Console.WriteLine(action);
                    //}

                    foreach (var agent in agents)
                    {
                        Console.WriteLine(agent.Value);
                    }

                    ApplicableFunctions.PrintContaminationState(state.ContaminationMap);

                    //List<State> chosenStates = new List<State>();
                    //State currentState = state;
                    //while (currentState.Parent != null)
                    //{
                    //    chosenStates.Add(currentState);
                    //    currentState = currentState.Parent;
                    //}

                    //chosenStates = chosenStates.OrderBy(s => s.G).ToList();

                    //foreach (var cstate in chosenStates)
                    //{
                    //    Console.WriteLine($"State Depth: {cstate.G}");
                    //    Console.WriteLine(state.GetHeuristic());
                    //    //foreach (var action in cstate.JointAction)
                    //    //{
                    //    //    Console.WriteLine();
                    //    //    //Console.WriteLine($"Agent {action.Key} - {action.Value.Name}");
                    //    //}
                    //}

                    return new Tuple<byte[,], Dictionary<string, Agent>, List<BoardAction>>(contaminationMap, agents, actions);
                }

                // Add the current state to explored states
                explored.Add(state);

                // Expand states
                var expandedStates = state.GetExpandedStates();
                foreach (var expandedState in expandedStates)
                {
                    if (!frontier.Contains(expandedState) && !explored.Contains(expandedState))
                    {
                        frontier.Add(expandedState);
                    }
                }

            }
        }
    }
}
