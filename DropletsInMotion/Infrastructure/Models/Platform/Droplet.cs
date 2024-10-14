namespace DropletsInMotion.Infrastructure.Models.Platform
{
    public class Droplet
    {
        public string DropletName { get; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public double Volume { get; set; }

        public Droplet(string dropletName, int positionX, int positionY, double volume)
        {
            DropletName = dropletName;
            PositionX = positionX;
            PositionY = positionY;
            Volume = volume;
        }

        public override string ToString()
        {
            return $"Droplet(DropletName: {DropletName}, PositionX: {PositionX}, PositionY: {PositionY}, Volume: {Volume})";
        }
    }
}
