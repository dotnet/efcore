// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     A base class inherited by each EF Core migration.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
public abstract class Migration
{
    /// <summary>
    ///     The migration identifier for the empty database.
    /// </summary>
    public const string InitialDatabase = "0";

    private IModel? _targetModel;
    private List<MigrationOperation>? _upOperations;
    private List<MigrationOperation>? _downOperations;

    /// <summary>
    ///     The <see cref="IModel" /> that the database will map to after the migration has been applied.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    public virtual IModel TargetModel
    {
        get
        {
            IModel Create()
            {
                var modelBuilder = new ModelBuilder();
                BuildTargetModel(modelBuilder);

                return (IModel)modelBuilder.Model;
            }

            return _targetModel ??= Create();
        }
    }

    /// <summary>
    ///     The <see cref="MigrationOperation" />s that will migrate the database 'up'.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         That is, those operations that need to be applied to the database
    ///         to take it from the state left in by the previous migration so that it is up-to-date
    ///         with regard to this migration.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public virtual IReadOnlyList<MigrationOperation> UpOperations
        => _upOperations ??= BuildOperations(Up);

    /// <summary>
    ///     The <see cref="MigrationOperation" />s that will migrate the database 'down'.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         That is, those operations that need to be applied to the database
    ///         to take it from the state left in by this migration so that it returns to the
    ///         state that it was in before this migration was applied.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public virtual IReadOnlyList<MigrationOperation> DownOperations
        => _downOperations ??= BuildOperations(Down);

    /// <summary>
    ///     The name of the current database provider.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This can be used to write conditional code in the migration such that different changes
    ///         can be made to the database depending on the type of database being used.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    ///     </para>
    /// </remarks>
    [DisallowNull]
    public virtual string? ActiveProvider { get; set; }

    /// <summary>
    ///     Implemented to build the <see cref="TargetModel" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The <see cref="ModelBuilder" /> to use to build the model.</param>
    protected virtual void BuildTargetModel(ModelBuilder modelBuilder)
    {
    }

    /// <summary>
    ///     Builds the operations that will migrate the database 'up'.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         That is, builds the operations that will take the database from the state left in by the
    ///         previous migration so that it is up-to-date with regard to this migration.
    ///     </para>
    ///     <para>
    ///         This method must be overridden in each class that inherits from <see cref="Migration" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="migrationBuilder">The <see cref="MigrationBuilder" /> that will build the operations.</param>
    protected abstract void Up(MigrationBuilder migrationBuilder);

    /// <summary>
    ///     Builds the operations that will migrate the database 'down'.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         That is, builds the operations that will take the database from the state left in by
    ///         this migration so that it returns to the state that it was in before this migration was applied.
    ///     </para>
    ///     <para>
    ///         This method must be overridden in each class that inherits from <see cref="Migration" /> if
    ///         both 'up' and 'down' migrations are to be supported. If it is not overridden, then calling it
    ///         will throw and it will not be possible to migrate in the 'down' direction.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="migrationBuilder">The <see cref="MigrationBuilder" /> that will build the operations.</param>
    protected virtual void Down(MigrationBuilder migrationBuilder)
        => throw new NotSupportedException(RelationalStrings.MigrationDownMissing);

    private List<MigrationOperation> BuildOperations(Action<MigrationBuilder> buildAction)
    {
        var migrationBuilder = new MigrationBuilder(ActiveProvider);
        buildAction(migrationBuilder);

        return migrationBuilder.Operations;
    }
}
