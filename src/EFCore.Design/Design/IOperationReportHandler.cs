// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Used to handle reported design-time activity.
/// </summary>
public interface IOperationReportHandler
{
    /// <summary>
    ///     Gets the contract version of this handler.
    /// </summary>
    /// <value> The contract version of this handler. </value>
    int Version { get; }

    /// <summary>
    ///     Invoked when an error is reported.
    /// </summary>
    /// <param name="message">The message.</param>
    void OnError(string message);

    /// <summary>
    ///     Invoked when a warning is reported.
    /// </summary>
    /// <param name="message">The message.</param>
    void OnWarning(string message);

    /// <summary>
    ///     Invoked when information is reported.
    /// </summary>
    /// <param name="message">The message.</param>
    void OnInformation(string message);

    /// <summary>
    ///     Invoked when verbose information is reported.
    /// </summary>
    /// <param name="message">The message.</param>
    void OnVerbose(string message);
}
