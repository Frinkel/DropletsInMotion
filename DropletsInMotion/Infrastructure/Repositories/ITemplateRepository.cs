using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Infrastructure.Repositories;

public interface ITemplateRepository
{ 
    void AddSplit(SplitTemplate splitTemplate, string template);
    void AddRavel(RavelTemplate ravelTemplate, string template);
    void AddUnravel(UnravelTemplate unravelTemplate, string template);


    void Initialize();
    List<SplitTemplate>? SplitTemplates { get; }
    List<RavelTemplate>? RavelTemplates { get; }
    List<UnravelTemplate>? UnravelTemplates { get; }
    List<MergeTemplate> MergeTemplates { get; }
    void AddMerge(MergeTemplate mergeTemplate, string template);
}