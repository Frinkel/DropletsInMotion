namespace DropletsInMotion.Infrastructure.Models.Platform
{
    public class Electrode
    {
        public int Id { get; }
        public int CoordinateX { get; set; }
        public int CoordinateY { get; set; }

        public int ElectrodeId { get; set; }
        public int DriverId { get; set; }

        public Electrode(int id, int coordinateX, int coordinateY, int electrodeId, int driverId)
        {
            Id = id;
            CoordinateX = coordinateX;
            CoordinateY = coordinateY;
            ElectrodeId = electrodeId;
            DriverId = driverId;
        }

        public override string ToString()
        {
            return $"Electrode(Id: {Id}, CoordinateX: {CoordinateX}, CoordinateY: {CoordinateY})";
        }
    }
}
