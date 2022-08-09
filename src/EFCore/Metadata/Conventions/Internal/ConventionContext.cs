// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ConventionContext<TMetadata> : IConventionContext<TMetadata>, IReadableConventionContext
{
    private bool _stopProcessing;
    private readonly ConventionDispatcher _dispatcher;
    private TMetadata? _result;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ConventionContext(ConventionDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TMetadata? Result
        => _result;

    /// <summary>
    ///     Calling this will prevent further processing of the associated event by other conventions.
    /// </summary>
    public virtual void StopProcessing()
    {
        _stopProcessing = true;
        _result = default;
    }

    /// <summary>
    ///     Calling this will prevent further processing of the associated event by other conventions.
    /// </summary>
    /// <remarks>
    ///     The common use case is when the metadata object was removed or replaced by the convention.
    /// </remarks>
    /// <param name="result">The new metadata object or <see langword="null" />.</param>
    public virtual void StopProcessing(TMetadata? result)
    {
        _stopProcessing = true;
        _result = result;
    }

    /// <summary>
    ///     Calling this will prevent further processing of the associated event by other conventions
    ///     if the given objects are different.
    /// </summary>
    /// <remarks>
    ///     The common use case is when the metadata object was replaced by the convention.
    /// </remarks>
    /// <param name="result">The new metadata object or <see langword="null" />.</param>
    public virtual void StopProcessingIfChanged(TMetadata? result)
    {
        if (!Equals(Result, result))
        {
            StopProcessing(result);
        }
    }

    /// <summary>
    ///     Prevents conventions from being executed immediately when a metadata aspect is modified. All the delayed conventions
    ///     will be executed after the returned object is disposed.
    /// </summary>
    /// <remarks>
    ///     This is useful when performing multiple operations that depend on each other.
    /// </remarks>
    /// <returns>An object that should be disposed to execute the delayed conventions.</returns>
    public virtual IConventionBatch DelayConventions()
        => _dispatcher.DelayConventions();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ShouldStopProcessing()
        => _stopProcessing;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ResetState(TMetadata? input)
    {
        _stopProcessing = false;
        _result = input;
    }
}
