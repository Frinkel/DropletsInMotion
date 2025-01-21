using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Infrastructure.Repositories;

public interface ITemplateRepository
{ 
    void AddSplit(SplitTemplate splitTemplate, string template);
    void AddRavel(RavelTemplate ravelTemplate, string template);
    void AddUnravel(UnravelTemplate unravelTemplate, string template);
    void AddDeclare(DeclareTemplate declareTemplate, string template);
    void AddGrow(GrowTemplate growTemplate, string template);
    void AddShrink(ShrinkTemplate shrinkTemplate, string template);
    void AddMerge(MergeTemplate mergeTemplate, string template);


    void Initialize();
    List<SplitTemplate>? SplitTemplates { get; }
    List<RavelTemplate>? RavelTemplates { get; }
    List<UnravelTemplate>? UnravelTemplates { get; }
    List<MergeTemplate> MergeTemplates { get; }
    List<DeclareTemplate> DeclareTemplates { get; }
    List<GrowTemplate> GrowTemplates { get; }
    List<ShrinkTemplate> ShrinkTemplates { get; }
}