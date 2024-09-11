namespace DropletsInMotion.Domain
{
    public class Droplet
    {
        public string Name { get; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public double Volume { get; set; }

        public Droplet(string name, int positionX, int positionY, double volume)
        {
            Name = name;
            PositionX = positionX;
            PositionY = positionY;
            Volume = volume;
        }

        public override string ToString()
        {
            return $"Droplet(Name: {Name}, PositionX: {PositionX}, PositionY: {PositionY}, Volume: {Volume})";
        }
    }
}
