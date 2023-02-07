// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides an API point for provider-specific extensions for configuring a <see cref="ITrigger" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
/// </remarks>
public class TableTriggerBuilder : TriggerBuilder
{
    /// <summary>
    ///     Creates a new builder for the given trigger.
    /// </summary>
    /// <param name="trigger">The trigger to configure.</param>
    public TableTriggerBuilder(IMutableTrigger trigger)
        : base(trigger)
    {
    }

    /// <summary>
    ///     Sets the database name of the trigger.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The database name of the trigger.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual TableTriggerBuilder HasDatabaseName(string? name)
    {
        Metadata.SetDatabaseName(name);

        return this;
    }

    /// <summary>
    ///     Adds or updates an annotation on the trigger. If an annotation with the key specified in <paramref name="annotation" />
    ///     already exists, its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual TableTriggerBuilder HasAnnotation(string annotation, object? value)
        => (TableTriggerBuilder)base.HasAnnotation(annotation, value);

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
