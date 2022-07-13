// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Represents the options to use while reverse engineering a model from the database.
/// </summary>
public class ModelReverseEngineerOptions
{
    /// <summary>
    ///     Gets or sets a value indicating whether to use the database schema names directly.
    /// </summary>
    /// <value> A value indicating whether to use the database schema names directly. </value>
    public virtual bool UseDatabaseNames { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to use the pluralizer.
    /// </summary>
    /// <value> A value indicating whether to use the pluralizer. </value>
    public virtual bool NoPluralize { get; set; }
}
