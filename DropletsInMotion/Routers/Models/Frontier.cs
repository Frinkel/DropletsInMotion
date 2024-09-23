namespace DropletsInMotion.Routers.Models;

public class Frontier
{
    private readonly PriorityQueue<State, int> _queue;

    public Frontier()
    {
        _queue = new PriorityQueue<State, int>();
    }

    public void Add(State state)
    {
        _queue.Enqueue(state, state.GetHeuristic());
    }

    public State Pop()
    {
        if (_queue.TryDequeue(out State state, out _))
        {
            return state;
        }
        throw new InvalidOperationException();
    }

    public bool IsEmpty()
    {
        return _queue.Count == 0;
    }

    public int Size()
    {
        return _queue.Count;
    }

    //public bool Contains(State state)
    //{
    //    return new List<State>(_queue.UnorderedItems.Select(i => i.Element)).Contains(state);
    //}
    public bool Contains(State state)
    {
        var stateSet = new HashSet<State>(_queue.UnorderedItems.Select(i => i.Element));
        //Console.WriteLine($"We check {state.GetHashCode()}");
        var contains = stateSet.Contains(state);
        //foreach (var otherState in stateSet)
        //{
        //    Console.WriteLine(otherState.GetHashCode());
        //    Console.WriteLine(stateSet.Contains(otherState));
        //}
        //Console.WriteLine(contains);

        return contains; // This will now check by hash first, then by Equals if necessary
    }

}
