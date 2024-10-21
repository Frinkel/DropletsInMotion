using DropletsInMotion.Application.ExecutionEngine.Models;
using System.Collections.Generic;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Application.Services
{
    public interface ITemplateService
    {
        List<(string, List<BoardAction>)> Templates { get; }
        Electrode[][] Board { get; set; }

        void LoadTemplatesFromFiles(string folderPath);
        void Initialize(Electrode[][] board);
        List<BoardAction> ApplyTemplate(string templateName, Droplet droplet, double time);
        public List<BoardAction> ApplyTemplateScaled(string templateName, Droplet droplet, double time, double scale);

        void PrintAllTemplates();
        List<BoardAction> SpinTemplate(List<BoardAction> template);
    }
}