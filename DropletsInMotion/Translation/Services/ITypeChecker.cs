using DropletsInMotion.Infrastructure.Models.Commands;

public interface ITypeChecker
{
    void TypeCheck(List<ICommand> commands);
    public void ResetTypeEnviroments();
}