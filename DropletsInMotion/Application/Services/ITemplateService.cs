using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Infrastructure.Models.Domain;
using System.Collections.Generic;

namespace DropletsInMotion.Application.Services
{
    public interface ITemplateService
    {
        List<(string, List<BoardAction>)> Templates { get; }
        Electrode[][] Board { get; set; }

        void LoadTemplatesFromFiles(string folderPath);
        void Initialize(Electrode[][] board);
        List<BoardAction> ApplyTemplate(string templateName, Droplet droplet, double time);
        void PrintAllTemplates();
        List<BoardAction> SpinTemplate(List<BoardAction> template);
    }
}