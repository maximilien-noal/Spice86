namespace Spice86.Logging;

using Serilog.Core;
using Serilog.Events;

using Spice86.Shared.Interfaces;

/// <summary>
/// Represents logger property bag enricher.
/// </summary>
internal sealed class LoggerPropertyBagEnricher(ILoggerPropertyBag propertyBag) : ILogEventEnricher {
    /// <summary>
    /// Performs the enrich operation.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="propertyFactory">The property factory.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("IP", propertyBag.CsIp));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ContextIndex", propertyBag.ContextIndex));
    }
}