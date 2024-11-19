public class Constraint : IEquatable<Constraint>
{
    public readonly string Agent;
    public readonly int? Time;
    public readonly (int x, int y) Position;

    public Constraint(string agent, (int x, int y) position, int? time = null)
    {
        Agent = agent;
        Time = time;
        Position = position;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Constraint);
    }

    public bool Equals(Constraint other)
    {
        return other != null &&
               Agent == other.Agent &&
               Position.Equals(other.Position) &&
               Time == other.Time;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Agent, Position.x, Position.y, Time);
    }
}