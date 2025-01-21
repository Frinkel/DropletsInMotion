namespace DropletsInMotion.Application.Services.Routers.Models;

public class Frontier
{
    private readonly PriorityQueue<State, (int, int)> _queue;

    public Frontier()
    {
        _queue = new PriorityQueue<State, (int, int)>();
    }

    public void Add(State state)
    {
        int fScore = state.GetFScore();
        int gScore = state.G;

        // Lexicographical priority (F, G)
        _queue.Enqueue(state, (fScore, -gScore));
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

    public bool Contains(State state)
    {
        var stateSet = new HashSet<State>(_queue.UnorderedItems.Select(i => i.Element));
        var contains = stateSet.Contains(state);

        return contains;
    }

}
