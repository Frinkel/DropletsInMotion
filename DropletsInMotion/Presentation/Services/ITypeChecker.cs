using DropletsInMotion.Infrastructure.Models.Commands;

public interface ITypeChecker
{
    void typeCheck(List<ICommand> commands);
    public void resetTypeEnviroments();
}