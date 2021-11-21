// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     A service typically implemented by database providers that gives access to annotations used by EF Core Migrations
///     when generating removal operations for various elements of the <see cref="IRelationalModel" />. The annotations
///     stored in the relational model are provided by <see cref="IRelationalAnnotationProvider" />.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public interface IMigrationsAnnotationProvider
{
    /// <summary>
    ///     Gets provider-specific Migrations annotations for the given <see cref="IRelationalModel" />
    ///     when it is being altered.
    /// </summary>
    /// <param name="model">The database model.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRemove(IRelationalModel model);

    /// <summary>
    ///     Gets provider-specific Migrations annotations for the given <see cref="ITable" />
    ///     when it is being removed.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRemove(ITable table);

    /// <summary>
    ///     Gets provider-specific Migrations annotations for the given <see cref="IColumn" />
    ///     when it is being removed.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRemove(IColumn column);

    /// <summary>
    ///     Gets provider-specific annotations for the given <see cref="IView" />
    ///     when it is being removed.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRemove(IView view);

    /// <summary>
    ///     Gets provider-specific annotations for the given <see cref="IViewColumn" />
    ///     when it is being removed.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRemove(IViewColumn column);

    /// <summary>
    ///     Gets provider-specific Migrations annotations for the given <see cref="IUniqueConstraint" />
    ///     when it is being removed.
    /// </summary>
    /// <param name="constraint">The unique constraint.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRemove(IUniqueConstraint constraint);

    /// <summary>
    ///     Gets provider-specific Migrations annotations for the given <see cref="ITableIndex" />
    ///     when it is being removed.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRemove(ITableIndex index);

    /// <summary>
    ///     Gets provider-specific Migrations annotations for the given <see cref="IForeignKeyConstraint" />
    ///     when it is being removed.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRemove(IForeignKeyConstraint foreignKey);

    /// <summary>
    ///     Gets provider-specific Migrations annotations for the given <see cref="ISequence" />
    ///     when it is being removed.
    /// </summary>
    /// <param name="sequence">The sequence.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRemove(ISequence sequence);

    /// <summary>
    ///     Gets provider-specific Migrations annotations for the given <see cref="ICheckConstraint" />
    ///     when it is being removed.
    /// </summary>
    /// <param name="checkConstraint">The check constraint.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRemove(ICheckConstraint checkConstraint);

    /// <summary>
    ///     Gets provider-specific Migrations annotations for the given <see cref="ITable" />
    ///     when it is being renamed.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRename(ITable table)
        => Enumerable.Empty<IAnnotation>();

    /// <summary>
    ///     Gets provider-specific Migrations annotations for the given <see cref="IColumn" />
    ///     when it is being renamed.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRename(IColumn column)
        => Enumerable.Empty<IAnnotation>();

    /// <summary>
    ///     Gets provider-specific Migrations annotations for the given <see cref="ITableIndex" />
    ///     when it is being renamed.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRename(ITableIndex index)
        => Enumerable.Empty<IAnnotation>();

    /// <summary>
    ///     Gets provider-specific Migrations annotations for the given <see cref="ISequence" />
    ///     when it is being renamed.
    /// </summary>
    /// <param name="sequence">The sequence.</param>
    /// <returns>The annotations.</returns>
    IEnumerable<IAnnotation> ForRename(ISequence sequence)
        => Enumerable.Empty<IAnnotation>();
}
