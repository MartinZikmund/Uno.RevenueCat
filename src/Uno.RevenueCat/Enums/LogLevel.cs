namespace Uno.RevenueCat.Enums;

/// <summary>
/// Verbosity of the RevenueCat SDK's internal logging, mapped to the native Android and iOS
/// SDK log levels.
/// </summary>
public enum LogLevel
{
    /// <summary>Most detailed logging, including internal SDK diagnostics.</summary>
    Verbose,

    /// <summary>Diagnostic logging useful while developing the integration.</summary>
    Debug,

    /// <summary>General informational messages about SDK activity.</summary>
    Information,

    /// <summary>Recoverable problems that do not stop the SDK from functioning.</summary>
    Warning,

    /// <summary>Failures that prevent an operation from completing.</summary>
    Error
}
