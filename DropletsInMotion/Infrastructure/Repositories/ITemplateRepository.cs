using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Infrastructure.Repositories;

public interface ITemplateRepository
{ 
    void AddSplit(SplitTemplate splitTemplate, string template);
    void AddRavel(RavelTemplate ravelTemplate, string template);
    void AddUnravel(UnravelTemplate unravelTemplate, string template);
    void AddDeclare(DeclareTemplate declareTemplate, string template);


    void Initialize();
    List<SplitTemplate>? SplitTemplates { get; }
    List<RavelTemplate>? RavelTemplates { get; }
    List<UnravelTemplate>? UnravelTemplates { get; }
    List<MergeTemplate> MergeTemplates { get; }
    List<DeclareTemplate> DeclareTemplates { get; }
    void AddMerge(MergeTemplate mergeTemplate, string template);
}