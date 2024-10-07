using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Infrastructure.Models;

public interface IDependencyNode
{
    int NodeId { get; }
    ICommand Command { get; }
    bool IsExecuted { get; set; }
    List<IDependencyNode> Dependencies { get; }
    void MarkAsExecuted();
    void AddDependency(IDependencyNode dependency);
    List<IDependencyNode> getExecutableNodes();
    bool CanExecute();
    string ToString();
}