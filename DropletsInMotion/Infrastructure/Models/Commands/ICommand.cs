namespace DropletsInMotion.Infrastructure.Models.Commands
{
    public interface ICommand
    {
        void Evaluate(Dictionary<string, double> variableValues);
    }
}