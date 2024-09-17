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
        _queue.Enqueue(state, state.Heuristic);
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
        return new List<State>(_queue.UnorderedItems.Select(i => i.Element)).Contains(state);
    }
}
