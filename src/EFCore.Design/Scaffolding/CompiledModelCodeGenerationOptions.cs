// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Represents the options to use while generating code for compiled model metadata.
/// </summary>
public class CompiledModelCodeGenerationOptions
{
    /// <summary>
    ///     Gets or sets the namespace for model metadata classes.
    /// </summary>
    /// <value> The namespace for model metadata classes. </value>
    public virtual string ModelNamespace { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the type of the corresponding DbContext.
    /// </summary>
    /// <value> The type of the corresponding DbContext. </value>
    public virtual Type ContextType { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the programming language to scaffold for.
    /// </summary>
    /// <value> The programming language to scaffold for. </value>
    public virtual string? Language { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether nullable reference types are enabled.
    /// </summary>
    /// <value> A value indicating whether nullable reference types are enabled. </value>
    public virtual bool UseNullableReferenceTypes { get; set; }
}
