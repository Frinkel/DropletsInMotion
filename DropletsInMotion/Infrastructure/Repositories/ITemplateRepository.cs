using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Infrastructure.Repositories;

public interface ITemplateRepository
{ 
    void AddSplit(SplitTemplate splitTemplate, string template);
    void Initialize();
    List<SplitTemplate>? SplitTemplates { get; }
}