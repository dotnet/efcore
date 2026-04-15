// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     A base class inherited by database providers that gives access to annotations used by EF Core Migrations
///     when generating removal operations for various elements of the <see cref="IRelationalModel" />.
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
public class MigrationsAnnotationProvider : IMigrationsAnnotationProvider
{
    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public MigrationsAnnotationProvider(MigrationsAnnotationProviderDependencies dependencies)
        => Dependencies = dependencies;

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual MigrationsAnnotationProviderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRemove(IRelationalModel model)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRemove(ITable table)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRemove(IColumn column)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRemove(IView view)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRemove(IViewColumn column)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRemove(IUniqueConstraint constraint)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRemove(ITableIndex index)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRemove(IForeignKeyConstraint foreignKey)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRemove(ISequence sequence)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRemove(ICheckConstraint checkConstraint)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRename(ITable table)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRename(IColumn column)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRename(ITableIndex index)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> ForRename(ISequence sequence)
        => [];
}
