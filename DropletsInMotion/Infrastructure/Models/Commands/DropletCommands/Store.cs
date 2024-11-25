using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class Store : IDropletCommand
    {
        public string DropletName { get; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public double Time { get; set; }
        public ArithmeticExpression PositionXExpression { get; }
        public ArithmeticExpression PositionYExpression { get; }
        public ArithmeticExpression TimeExpression { get; }
        public bool StoreInPosition { get; }

        public Store(string dropletName, int newPositionX, int newPositionY, double time)
        {
            DropletName = dropletName;
            PositionX = newPositionX;
            PositionY = newPositionY;
            Time = time;
            StoreInPosition = false;
        }

        public Store(string dropletName, ArithmeticExpression newPositionXExpression, ArithmeticExpression newPositionYExpression, ArithmeticExpression timeExpression)
        {
            DropletName = dropletName;
            PositionXExpression = newPositionXExpression;
            PositionYExpression = newPositionYExpression;
            TimeExpression = timeExpression;
            StoreInPosition = false;
        }

        public Store(string dropletName, ArithmeticExpression timeExpression)
        {
            DropletName = dropletName;
            TimeExpression = timeExpression;
            StoreInPosition = true;
        }

        public override string ToString()
        {
            return StoreInPosition
                ? $"Store(DropletName: {DropletName}, Position: [Deferred], Time: {TimeExpression})"
                : $"Store(DropletName: {DropletName}, PositionX: {PositionXExpression}, PositionY: {PositionYExpression}, Time: {TimeExpression})";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { DropletName };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { DropletName };
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
            if (StoreInPosition)
            {

                PositionX = -1;
                PositionY = -1;
            }
            else if (PositionXExpression != null && PositionYExpression != null)
            {
                PositionX = (int)PositionXExpression.Evaluate(variableValues);
                PositionY = (int)PositionYExpression.Evaluate(variableValues);
            }

            Time = TimeExpression.Evaluate(variableValues);
        }

        public List<string> GetInputVariables()
        {
            var res = new List<string>();
            if (PositionXExpression != null) res.AddRange(PositionXExpression.GetVariables());
            if (PositionYExpression != null) res.AddRange(PositionYExpression.GetVariables());
            if (TimeExpression != null) res.AddRange(TimeExpression.GetVariables());
            res.AddRange(GetInputDroplets());
            return res;
        }

        public List<string> GetOutputVariables()
        {
            return new List<string> { DropletName };
        }
    }

}
