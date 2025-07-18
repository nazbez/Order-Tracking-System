using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Serilog.Core;
using Serilog.Events;

namespace OrderTrackingSystem.Logging.Enrichers;

/// <summary>
/// Enriches log events with activity-related properties such as SpanId, TraceId, and ParentId.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ActivityEnricher : ILogEventEnricher
{
    /// <summary>
    /// Enriches the specified log event with activity-related properties if an activity is present.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">The factory used to create log event properties.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;

        if (activity is null)
        {
            return;
        }
        
        logEvent.AddPropertyIfAbsent(new LogEventProperty("SpanId", new ScalarValue(activity.GetSpanId())));
        logEvent.AddPropertyIfAbsent(new LogEventProperty("TraceId", new ScalarValue(activity.GetTraceId())));
        logEvent.AddPropertyIfAbsent(new LogEventProperty("ParentId", new ScalarValue(activity.GetParentId())));
    }
}

/// <summary>
/// Provides extension methods for the <see cref="Activity"/> class to retrieve SpanId, TraceId, and ParentId.
/// </summary>
internal static class ActivityExtensions
{
    /// <summary>
    /// Retrieves the SpanId of the activity based on its ID format.
    /// </summary>
    /// <param name="activity">The activity instance.</param>
    /// <returns>The SpanId as a string.</returns>
    public static string GetSpanId(this Activity activity)
    {
        return activity.IdFormat switch
        {
            ActivityIdFormat.Hierarchical => activity.Id,
            ActivityIdFormat.W3C => activity.SpanId.ToHexString(),
            _ => null,
        } ?? string.Empty;
    }

    /// <summary>
    /// Retrieves the TraceId of the activity based on its ID format.
    /// </summary>
    /// <param name="activity">The activity instance.</param>
    /// <returns>The TraceId as a string.</returns>
    public static string GetTraceId(this Activity activity)
    {
        return activity.IdFormat switch
        {
            ActivityIdFormat.Hierarchical => activity.RootId,
            ActivityIdFormat.W3C => activity.TraceId.ToHexString(),
            _ => null,
        } ?? string.Empty;
    }

    /// <summary>
    /// Retrieves the ParentId of the activity based on its ID format.
    /// </summary>
    /// <param name="activity">The activity instance.</param>
    /// <returns>The ParentId as a string.</returns>
    public static string GetParentId(this Activity activity)
    {
        return activity.IdFormat switch
        {
            ActivityIdFormat.Hierarchical => activity.ParentId,
            ActivityIdFormat.W3C => activity.ParentSpanId.ToHexString(),
            _ => null,
        } ?? string.Empty;
    }
}
