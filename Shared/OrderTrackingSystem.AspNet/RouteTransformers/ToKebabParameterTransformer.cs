using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;

namespace OrderTrackingSystem.AspNet.RouteTransformers;

/// <summary>
/// Transforms route parameters to kebab-case format for outbound routing.
/// </summary>
public sealed partial class ToKebabParameterTransformer : IOutboundParameterTransformer
{
    /// <summary>
    /// Transforms the given value to kebab-case format for outbound routing.
    /// </summary>
    /// <param name="value">The value to transform.</param>
    /// <returns>
    /// The transformed value in kebab-case format, or <c>null</c> if the input value is <c>null</c>.
    /// </returns>
    public string TransformOutbound(object? value) => value is not null
        ? MyRegex().Replace(value.ToString()!, "$1-$2").ToLower()
        : null;
    
    /// <summary>
    /// Compiled regular expression used to identify camel-case patterns for transformation.
    /// </summary>
    /// <returns>A compiled <see cref="Regex"/> instance.</returns>
    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex MyRegex();
}
