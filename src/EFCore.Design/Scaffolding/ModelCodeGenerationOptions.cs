// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Represents the options to use while generating code for a model.
/// </summary>
public class ModelCodeGenerationOptions
{
    /// <summary>
    ///     Gets or sets a value indicating whether to use data annotations.
    /// </summary>
    /// <value> A value indicating whether to use data annotations. </value>
    public virtual bool UseDataAnnotations { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to suppress the connection string sensitive information warning.
    /// </summary>
    /// <value> A value indicating whether to suppress the connection string sensitive information warning. </value>
    public virtual bool SuppressConnectionStringWarning { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to suppress generation of the OnConfiguring() method.
    /// </summary>
    /// <value> A value indicating whether to suppress generation of the OnConfiguring() method. </value>
    public virtual bool SuppressOnConfiguring { get; set; }

    /// <summary>
    ///     Gets or sets the namespace of the project.
    /// </summary>
    /// <value>The namespace of the project.</value>
    public virtual string? RootNamespace { get; set; }

    /// <summary>
    ///     Gets or sets the namespace for model classes.
    /// </summary>
    /// <value> The namespace for model classes. </value>
    public virtual string? ModelNamespace { get; set; }

    /// <summary>
    ///     Gets or sets the namespace for context class.
    /// </summary>
    /// <value>The namespace for context class.</value>
    public virtual string? ContextNamespace { get; set; }

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

    /// <summary>
    ///     Gets or sets the DbContext output directory.
    /// </summary>
    /// <value> The DbContext output directory. </value>
    public virtual string? ContextDir { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="DbContext" /> name.
    /// </summary>
    /// <value> The <see cref="DbContext" /> name. </value>
    public virtual string? ContextName { get; set; }

    /// <summary>
    ///     Gets or sets the connection string.
    /// </summary>
    /// <value name="connectionString">The connection string. </value>
    public virtual string? ConnectionString { get; set; }

    /// <summary>
    ///     Gets or sets the root project directory.
    /// </summary>
    /// <value>The directory.</value>
    public virtual string? ProjectDir { get; set; }
}
