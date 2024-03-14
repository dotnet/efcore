// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Identifies the <see cref="RuntimeModel" /> implementation that should be used for a given context.
/// </summary>
/// <remarks>
///     <para>
///         This attribute will usually be generated with the compiled model and doesn't need to be specified in your code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-dbcontext-pooling">Using DbContext pooling</see> for more information and examples.
///     </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DbContextModelAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DbContextAttribute" /> class.
    /// </summary>
    /// <param name="contextType">The associated context.</param>
    /// <param name="modelType">The compiled model.</param>
    public DbContextModelAttribute(Type contextType, Type modelType)
    {
        Check.NotNull(contextType, nameof(contextType));

        ContextType = contextType;
        ModelType = modelType;
    }

    /// <summary>
    ///     Gets the associated context.
    /// </summary>
    public Type ContextType { get; }

    /// <summary>
    ///     Gets the compiled model.
    /// </summary>
    public Type ModelType { get; }
}
