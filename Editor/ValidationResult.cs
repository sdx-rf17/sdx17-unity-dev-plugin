using System.Collections.Generic;

namespace Sdx17.DevPlugin
{
    /// <summary>Severity of a single validation finding.</summary>
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    /// <summary>One finding from the asset validator.</summary>
    public readonly struct ValidationResult
    {
        public ValidationResult(ValidationSeverity severity, string target, string message)
        {
            Severity = severity;
            Target = target ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public ValidationSeverity Severity { get; }
        public string Target { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"[{Severity}] {Target}: {Message}";
        }
    }

    /// <summary>Summary of a validation run, kept separate from the logging.</summary>
    public sealed class ValidationReport
    {
        private readonly List<ValidationResult> results = new List<ValidationResult>();

        public IReadOnlyList<ValidationResult> Results => results;

        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public int InfoCount { get; private set; }
        public int CheckedObjectCount { get; set; }

        public bool HasErrors => ErrorCount > 0;

        public void Add(ValidationResult result)
        {
            results.Add(result);
            switch (result.Severity)
            {
                case ValidationSeverity.Error: ErrorCount++; break;
                case ValidationSeverity.Warning: WarningCount++; break;
                default: InfoCount++; break;
            }
        }

        public void Add(ValidationSeverity severity, string target, string message)
        {
            Add(new ValidationResult(severity, target, message));
        }

        public string SummaryLine()
        {
            return $"Checked {CheckedObjectCount} object(s): {ErrorCount} error(s), {WarningCount} warning(s), {InfoCount} info note(s).";
        }
    }
}
