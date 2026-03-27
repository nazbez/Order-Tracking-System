using System.Text.Json;
using IntelligentGateway.Application;
using IntelligentGateway.Domain;
using Json.Schema;

namespace IntelligentGateway.Infrastructure;

public sealed class JsonSchemaToolArgumentValidator : IToolArgumentValidator
{
    public ValidationResult Validate(ToolDefinition tool, string argumentsJson)
    {
        try
        {
            var schema = JsonSchema.FromText(tool.InputSchema.RootElement.GetRawText());
            using var argsDocument = JsonDocument.Parse(argumentsJson);
            var result = schema.Evaluate(argsDocument.RootElement);
            if (result.IsValid)
            {
                return ValidationResult.Success();
            }

            var errors = result.Details?.Where(d => !d.IsValid).Select(d => d.Message ?? "Invalid argument").Distinct().ToArray()
                         ?? ["Invalid arguments"];
            return new ValidationResult(false, errors);
        }
        catch (Exception ex)
        {
            return ValidationResult.Failed($"Schema validation error: {ex.Message}");
        }
    }
}
