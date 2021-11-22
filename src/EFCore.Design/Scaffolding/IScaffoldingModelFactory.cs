// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Used to create an <see cref="IModel" /> from a <see cref="DatabaseModel" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-scaffolding">Reverse engineering (scaffolding) an existing database</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface IScaffoldingModelFactory
{
    /// <summary>
    ///     Creates an <see cref="IModel" /> from a <see cref="DatabaseModel" />.
    /// </summary>
    /// <param name="databaseModel">The database model.</param>
    /// <param name="options">The options to use while creating the model.</param>
    /// <returns>The model.</returns>
    IModel Create(DatabaseModel databaseModel, ModelReverseEngineerOptions options);
}
