namespace DropletsInMotion.Domain
{
    /// <summary>
    /// Naming was decided by Joel whom you can complain to if you do not agree
    /// </summary>
    public class Electrode
    {
        public int Id { get; }
        public int CoordinateX { get; set; }
        public int CoordinateY { get; set; }

        public Electrode(int id, int coordinateX, int coordinateY)
        {
            Id = id;
            CoordinateX = coordinateX;
            CoordinateY = coordinateY;
        }

        public override string ToString()
        {
            return $"Electrode(Id: {Id}, CoordinateX: {CoordinateX}, CoordinateY: {CoordinateY})";
        }
    }
}
