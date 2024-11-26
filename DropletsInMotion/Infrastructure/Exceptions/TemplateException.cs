using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Infrastructure.Exceptions
{
    public class TemplateException : Exception
    {
        public ITemplate Template { get; }

        public TemplateException(string message, ITemplate template) : base(message)
        {
            Template = template;
        }

        public TemplateException(string message, ITemplate template, Exception innerException) : base(message, innerException)
        {
            Template = template;
        }
    }
}
