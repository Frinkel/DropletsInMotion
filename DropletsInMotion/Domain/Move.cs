namespace DropletsInMotion.Domain
{
    public class Move : ICommand
    {
        public string DropletName { get; }
        public int PositionX { get; }
        public int PositionY { get; }

        public Move(string dropletName, int positionX, int positionY)
        {
            DropletName = dropletName;
            PositionX = positionX;
            PositionY = positionY;
        }

        public override string ToString()
        {
            return $"Move(DropletName: {DropletName}, PositionX: {PositionX}, PositionY: {PositionY})";
        }
    }
}
