// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     Generates the SQL in <see cref="MigrationCommand" /> objects that can
///     then be executed or scripted from a list of <see cref="MigrationOperation" />s.
/// </summary>
/// <remarks>
///     <para>
///         This class is typically inherited by database providers to customize the SQL generation.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public class MigrationsSqlGenerator : IMigrationsSqlGenerator
{
    private static readonly
        IReadOnlyDictionary<Type, Action<MigrationsSqlGenerator, MigrationOperation, IModel?, MigrationCommandListBuilder>>
        GenerateActions =
            new Dictionary<Type, Action<MigrationsSqlGenerator, MigrationOperation, IModel?, MigrationCommandListBuilder>>
            {
                { typeof(AddColumnOperation), (g, o, m, b) => g.Generate((AddColumnOperation)o, m, b) },
                { typeof(AddForeignKeyOperation), (g, o, m, b) => g.Generate((AddForeignKeyOperation)o, m, b) },
                { typeof(AddPrimaryKeyOperation), (g, o, m, b) => g.Generate((AddPrimaryKeyOperation)o, m, b) },
                { typeof(AddUniqueConstraintOperation), (g, o, m, b) => g.Generate((AddUniqueConstraintOperation)o, m, b) },
                { typeof(AlterColumnOperation), (g, o, m, b) => g.Generate((AlterColumnOperation)o, m, b) },
                { typeof(AlterDatabaseOperation), (g, o, m, b) => g.Generate((AlterDatabaseOperation)o, m, b) },
                { typeof(AlterSequenceOperation), (g, o, m, b) => g.Generate((AlterSequenceOperation)o, m, b) },
                { typeof(AlterTableOperation), (g, o, m, b) => g.Generate((AlterTableOperation)o, m, b) },
                { typeof(AddCheckConstraintOperation), (g, o, m, b) => g.Generate((AddCheckConstraintOperation)o, m, b) },
                { typeof(CreateIndexOperation), (g, o, m, b) => g.Generate((CreateIndexOperation)o, m, b) },
                { typeof(CreateSequenceOperation), (g, o, m, b) => g.Generate((CreateSequenceOperation)o, m, b) },
                { typeof(CreateTableOperation), (g, o, m, b) => g.Generate((CreateTableOperation)o, m, b) },
                { typeof(DropColumnOperation), (g, o, m, b) => g.Generate((DropColumnOperation)o, m, b) },
                { typeof(DropForeignKeyOperation), (g, o, m, b) => g.Generate((DropForeignKeyOperation)o, m, b) },
                { typeof(DropIndexOperation), (g, o, m, b) => g.Generate((DropIndexOperation)o, m, b) },
                { typeof(DropPrimaryKeyOperation), (g, o, m, b) => g.Generate((DropPrimaryKeyOperation)o, m, b) },
                { typeof(DropSchemaOperation), (g, o, m, b) => g.Generate((DropSchemaOperation)o, m, b) },
                { typeof(DropSequenceOperation), (g, o, m, b) => g.Generate((DropSequenceOperation)o, m, b) },
                { typeof(DropTableOperation), (g, o, m, b) => g.Generate((DropTableOperation)o, m, b) },
                { typeof(DropUniqueConstraintOperation), (g, o, m, b) => g.Generate((DropUniqueConstraintOperation)o, m, b) },
                { typeof(DropCheckConstraintOperation), (g, o, m, b) => g.Generate((DropCheckConstraintOperation)o, m, b) },
                { typeof(EnsureSchemaOperation), (g, o, m, b) => g.Generate((EnsureSchemaOperation)o, m, b) },
                { typeof(RenameColumnOperation), (g, o, m, b) => g.Generate((RenameColumnOperation)o, m, b) },
                { typeof(RenameIndexOperation), (g, o, m, b) => g.Generate((RenameIndexOperation)o, m, b) },
                { typeof(RenameSequenceOperation), (g, o, m, b) => g.Generate((RenameSequenceOperation)o, m, b) },
                { typeof(RenameTableOperation), (g, o, m, b) => g.Generate((RenameTableOperation)o, m, b) },
                { typeof(RestartSequenceOperation), (g, o, m, b) => g.Generate((RestartSequenceOperation)o, m, b) },
                { typeof(SqlOperation), (g, o, m, b) => g.Generate((SqlOperation)o, m, b) },
                { typeof(InsertDataOperation), (g, o, m, b) => g.Generate((InsertDataOperation)o, m, b) },
                { typeof(DeleteDataOperation), (g, o, m, b) => g.Generate((DeleteDataOperation)o, m, b) },
                { typeof(UpdateDataOperation), (g, o, m, b) => g.Generate((UpdateDataOperation)o, m, b) }
            };

    /// <summary>
    ///     Creates a new <see cref="MigrationsSqlGenerator" /> instance using the given dependencies.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public MigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies)
    {
        Dependencies = dependencies;

        if (dependencies.LoggingOptions.IsSensitiveDataLoggingEnabled)
        {
            SensitiveLoggingEnabled = true;
        }
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual MigrationsSqlGeneratorDependencies Dependencies { get; }

    private bool SensitiveLoggingEnabled { get; }

    /// <summary>
    ///     The <see cref="IUpdateSqlGenerator" />.
    /// </summary>
    protected virtual IUpdateSqlGenerator SqlGenerator
        => Dependencies.UpdateSqlGenerator;

    /// <summary>
    ///     Gets a comparer that can be used to compare two product versions.
    /// </summary>
    protected virtual IComparer<string> VersionComparer { get; } = new SemanticVersionComparer();

    /// <summary>
    ///     Gets or sets the options to use when generating commands.
    /// </summary>
    protected virtual MigrationsSqlGenerationOptions Options { get; set; }

    /// <summary>
    ///     Generates commands from a list of operations.
    /// </summary>
    /// <param name="operations">The operations.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="options">The options to use when generating commands.</param>
    /// <returns>The list of commands to be executed or scripted.</returns>
    public virtual IReadOnlyList<MigrationCommand> Generate(
        IReadOnlyList<MigrationOperation> operations,
        IModel? model = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
    {
        Options = options;

        var builder = new MigrationCommandListBuilder(Dependencies);
        try
        {
            foreach (var operation in operations)
            {
                Generate(operation, model, builder);
            }
        }
        finally
        {
            Options = MigrationsSqlGenerationOptions.Default;
        }

        return builder.GetCommandList();
    }

    /// <summary>
    ///     Builds commands for the given <see cref="MigrationOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <remarks>
    ///     This method uses a double-dispatch mechanism to call one of the 'Generate' methods that are
    ///     specific to a certain subtype of <see cref="MigrationOperation" />. Typically database providers
    ///     will override these specific methods rather than this method. However, providers can override
    ///     this methods to handle provider-specific operations.
    /// </remarks>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        MigrationOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        var operationType = operation.GetType();
        if (!GenerateActions.TryGetValue(operationType, out var generateAction))
        {
            throw new InvalidOperationException(RelationalStrings.UnknownOperation(GetType().ShortDisplayName(), operationType));
        }

        generateAction(this, operation, model, builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AddColumnOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected virtual void Generate(
        AddColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        if (operation[RelationalAnnotationNames.ColumnOrder] != null)
        {
            Dependencies.MigrationsLogger.ColumnOrderIgnoredWarning(operation);
        }

        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" ADD ");

        ColumnDefinition(operation, model, builder);

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AddForeignKeyOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected virtual void Generate(
        AddForeignKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" ADD ");

        ForeignKeyConstraint(operation, model, builder);

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AddPrimaryKeyOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected virtual void Generate(
        AddPrimaryKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" ADD ");

        PrimaryKeyConstraint(operation, model, builder);

        KeyWithOptions(operation, builder);

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AddUniqueConstraintOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        AddUniqueConstraintOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" ADD ");

        UniqueConstraint(operation, model, builder);

        KeyWithOptions(operation, builder);

        builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        EndStatement(builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AddCheckConstraintOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        AddCheckConstraintOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" ADD ");
        CheckConstraint(operation, model, builder);
        builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        EndStatement(builder);
    }

    /// <summary>
    ///     Can be overridden by database providers to build commands for the given <see cref="AlterColumnOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <remarks>
    ///     Note that the default implementation of this method throws <see cref="NotSupportedException" />. Providers
    ///     must override if they are to support this kind of operation.
    /// </remarks>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        AlterColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
        => throw new NotSupportedException(RelationalStrings.MigrationSqlGenerationMissing(nameof(AlterColumnOperation)));

    /// <summary>
    ///     Can be overridden by database providers to build commands for the given <see cref="AlterDatabaseOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <remarks>
    ///     Note that there is no default implementation of this method. Providers must override if they are to
    ///     support this kind of operation.
    /// </remarks>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        AlterDatabaseOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
    }

    /// <summary>
    ///     Can be overridden by database providers to build commands for the given <see cref="RenameIndexOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <remarks>
    ///     Note that the default implementation of this method throws <see cref="NotSupportedException" />. Providers
    ///     must override if they are to support this kind of operation.
    /// </remarks>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        RenameIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
        => throw new NotSupportedException(RelationalStrings.MigrationSqlGenerationMissing(nameof(RenameIndexOperation)));

    /// <summary>
    ///     Builds commands for the given <see cref="AlterSequenceOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        AlterSequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("ALTER SEQUENCE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

        SequenceOptions(operation, model, builder);

        builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    /// <summary>
    ///     Can be overridden by database providers to build commands for the given <see cref="AlterTableOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <remarks>
    ///     Note that the default implementation of this method does nothing because there is no common metadata
    ///     relating to this operation. Providers only need to override this method if they have some provider-specific
    ///     annotations that must be handled.
    /// </remarks>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        AlterTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
    }

    /// <summary>
    ///     Can be overridden by database providers to build commands for the given <see cref="RenameTableOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <remarks>
    ///     Note that the default implementation of this method throws <see cref="NotSupportedException" />. Providers
    ///     must override if they are to support this kind of operation.
    /// </remarks>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        RenameTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
        => throw new NotSupportedException(RelationalStrings.MigrationSqlGenerationMissing(nameof(RenameTableOperation)));

    /// <summary>
    ///     Builds commands for the given <see cref="CreateIndexOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected virtual void Generate(
        CreateIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        builder.Append("CREATE ");

        if (operation.IsUnique)
        {
            builder.Append("UNIQUE ");
        }

        IndexTraits(operation, model, builder);

        builder
            .Append("INDEX ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .Append(" ON ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" (");

        GenerateIndexColumnList(operation, model, builder);

        builder.Append(")");

        IndexOptions(operation, model, builder);

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    /// <summary>
    ///     Can be overridden by database providers to build commands for the given <see cref="EnsureSchemaOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <remarks>
    ///     Note that the default implementation of this method throws <see cref="NotSupportedException" />. Providers
    ///     must override if they are to support this kind of operation.
    /// </remarks>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        EnsureSchemaOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
        => throw new NotSupportedException(RelationalStrings.MigrationSqlGenerationMissing(nameof(EnsureSchemaOperation)));

    /// <summary>
    ///     Builds commands for the given <see cref="CreateSequenceOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        CreateSequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("CREATE SEQUENCE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

        var typeMapping = Dependencies.TypeMappingSource.GetMapping(operation.ClrType);

        if (operation.ClrType != typeof(long))
        {
            builder
                .Append(" AS ")
                .Append(typeMapping.StoreType);

            // set the typeMapping for use with operation.StartValue (i.e. a long) below
            typeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(long));
        }

        builder
            .Append(" START WITH ")
            .Append(typeMapping.GenerateSqlLiteral(operation.StartValue));

        SequenceOptions(operation, model, builder);

        builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="CreateTableOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected virtual void Generate(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        builder
            .Append("CREATE TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
            .AppendLine(" (");

        using (builder.Indent())
        {
            CreateTableColumns(operation, model, builder);
            CreateTableConstraints(operation, model, builder);
            builder.AppendLine();
        }

        builder.Append(")");

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropColumnOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected virtual void Generate(
        DropColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" DROP COLUMN ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropForeignKeyOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected virtual void Generate(
        DropForeignKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" DROP CONSTRAINT ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    /// <summary>
    ///     Can be overridden by database providers to build commands for the given <see cref="DropIndexOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <remarks>
    ///     Note that the default implementation of this method throws <see cref="NotSupportedException" />. Providers
    ///     must override if they are to support this kind of operation.
    /// </remarks>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected virtual void Generate(
        DropIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
        => throw new NotSupportedException(RelationalStrings.MigrationSqlGenerationMissing(nameof(DropIndexOperation)));

    /// <summary>
    ///     Builds commands for the given <see cref="DropPrimaryKeyOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected virtual void Generate(
        DropPrimaryKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" DROP CONSTRAINT ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropSchemaOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        DropSchemaOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("DROP SCHEMA ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropSequenceOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        DropSequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("DROP SEQUENCE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropTableOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected virtual void Generate(
        DropTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        builder
            .Append("DROP TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropUniqueConstraintOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        DropUniqueConstraintOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" DROP CONSTRAINT ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropCheckConstraintOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        DropCheckConstraintOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" DROP CONSTRAINT ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    /// <summary>
    ///     Can be overridden by database providers to build commands for the given <see cref="RenameColumnOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <remarks>
    ///     Note that the default implementation of this method throws <see cref="NotSupportedException" />. Providers
    ///     must override if they are to support this kind of operation.
    /// </remarks>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        RenameColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
        => throw new NotSupportedException(RelationalStrings.MigrationSqlGenerationMissing(nameof(RenameColumnOperation)));

    /// <summary>
    ///     Can be overridden by database providers to build commands for the given <see cref="RenameSequenceOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <remarks>
    ///     Note that the default implementation of this method throws <see cref="NotSupportedException" />. Providers
    ///     must override if they are to support this kind of operation.
    /// </remarks>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        RenameSequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
        => throw new NotSupportedException(RelationalStrings.MigrationSqlGenerationMissing(nameof(RenameSequenceOperation)));

    /// <summary>
    ///     Builds commands for the given <see cref="RestartSequenceOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        RestartSequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        var longTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(long));

        builder
            .Append("ALTER SEQUENCE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
            .Append(" RESTART");

        if (operation.StartValue.HasValue)
        {
            builder
                .Append(" WITH ")
                .Append(longTypeMapping.GenerateSqlLiteral(operation.StartValue.Value));
        }

        builder
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="SqlOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        SqlOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder.AppendLine(operation.Sql);

        EndStatement(builder, operation.SuppressTransaction);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="InsertDataOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected virtual void Generate(
        InsertDataOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        var sqlBuilder = new StringBuilder();
        foreach (var modificationCommand in GenerateModificationCommands(operation, model))
        {
            SqlGenerator.AppendInsertOperation(
                sqlBuilder,
                modificationCommand,
                0);
        }

        builder.Append(sqlBuilder.ToString());

        if (terminate)
        {
            EndStatement(builder);
        }
    }

    /// <summary>
    ///     Generates the commands that correspond to the given operation.
    /// </summary>
    /// <param name="operation">The data operation to generate commands for.</param>
    /// <param name="model">The model.</param>
    /// <returns>The commands that correspond to the given operation.</returns>
    protected virtual IEnumerable<IReadOnlyModificationCommand> GenerateModificationCommands(
        InsertDataOperation operation,
        IModel? model)
    {
        if (operation.Columns.Length != operation.Values.GetLength(1))
        {
            throw new InvalidOperationException(
                RelationalStrings.InsertDataOperationValuesCountMismatch(
                    operation.Values.GetLength(1), operation.Columns.Length,
                    FormatTable(operation.Table, operation.Schema ?? model?.GetDefaultSchema())));
        }

        if (operation.ColumnTypes != null
            && operation.Columns.Length != operation.ColumnTypes.Length)
        {
            throw new InvalidOperationException(
                RelationalStrings.InsertDataOperationTypesCountMismatch(
                    operation.ColumnTypes.Length, operation.Columns.Length,
                    FormatTable(operation.Table, operation.Schema ?? model?.GetDefaultSchema())));
        }

        if (operation.ColumnTypes == null
            && model == null)
        {
            throw new InvalidOperationException(
                RelationalStrings.InsertDataOperationNoModel(
                    FormatTable(operation.Table, operation.Schema ?? model?.GetDefaultSchema())));
        }

        var propertyMappings = operation.ColumnTypes == null
            ? GetPropertyMappings(operation.Columns, operation.Table, operation.Schema, model)
            : null;

        for (var i = 0; i < operation.Values.GetLength(0); i++)
        {
            var modificationCommand = Dependencies.ModificationCommandFactory.CreateNonTrackedModificationCommand(
                new NonTrackedModificationCommandParameters(
                    operation.Table, operation.Schema ?? model?.GetDefaultSchema(), SensitiveLoggingEnabled));
            modificationCommand.EntityState = EntityState.Added;

            for (var j = 0; j < operation.Columns.Length; j++)
            {
                var name = operation.Columns[j];
                var value = operation.Values[i, j];
                var propertyMapping = propertyMappings?[j];
                var columnType = operation.ColumnTypes?[j];
                var typeMapping = propertyMapping != null
                    ? propertyMapping.TypeMapping
                    : value != null
                        ? Dependencies.TypeMappingSource.FindMapping(value.GetType(), columnType)
                        : Dependencies.TypeMappingSource.FindMapping(columnType!);

                modificationCommand.AddColumnModification(
                    new ColumnModificationParameters(
                        name, originalValue: null, value, propertyMapping?.Property, columnType, typeMapping,
                        read: false, write: true, key: true, condition: false,
                        SensitiveLoggingEnabled, propertyMapping?.Column.IsNullable));
            }

            yield return modificationCommand;
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DeleteDataOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        DeleteDataOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        var sqlBuilder = new StringBuilder();
        foreach (var modificationCommand in GenerateModificationCommands(operation, model))
        {
            SqlGenerator.AppendDeleteOperation(
                sqlBuilder,
                modificationCommand,
                0);
        }

        builder.Append(sqlBuilder.ToString());
        EndStatement(builder);
    }

    /// <summary>
    ///     Generates the commands that correspond to the given operation.
    /// </summary>
    /// <param name="operation">The data operation to generate commands for.</param>
    /// <param name="model">The model.</param>
    /// <returns>The commands that correspond to the given operation.</returns>
    protected virtual IEnumerable<IReadOnlyModificationCommand> GenerateModificationCommands(
        DeleteDataOperation operation,
        IModel? model)
    {
        if (operation.KeyColumns.Length != operation.KeyValues.GetLength(1))
        {
            throw new InvalidOperationException(
                RelationalStrings.DeleteDataOperationValuesCountMismatch(
                    operation.KeyValues.GetLength(1), operation.KeyColumns.Length, FormatTable(operation.Table, operation.Schema)));
        }

        if (operation.KeyColumnTypes != null
            && operation.KeyColumns.Length != operation.KeyColumnTypes.Length)
        {
            throw new InvalidOperationException(
                RelationalStrings.DeleteDataOperationTypesCountMismatch(
                    operation.KeyColumnTypes.Length, operation.KeyColumns.Length, FormatTable(operation.Table, operation.Schema)));
        }

        if (operation.KeyColumnTypes == null
            && model == null)
        {
            throw new InvalidOperationException(
                RelationalStrings.DeleteDataOperationNoModel(
                    FormatTable(operation.Table, operation.Schema)));
        }

        var keyPropertyMappings = operation.KeyColumnTypes == null
            ? GetPropertyMappings(operation.KeyColumns, operation.Table, operation.Schema, model)
            : null;

        for (var i = 0; i < operation.KeyValues.GetLength(0); i++)
        {
            var modificationCommand = Dependencies.ModificationCommandFactory.CreateNonTrackedModificationCommand(
                new NonTrackedModificationCommandParameters(operation.Table, operation.Schema, SensitiveLoggingEnabled));
            modificationCommand.EntityState = EntityState.Deleted;

            for (var j = 0; j < operation.KeyColumns.Length; j++)
            {
                var name = operation.KeyColumns[j];
                var value = operation.KeyValues[i, j];
                var propertyMapping = keyPropertyMappings?[j];
                var columnType = operation.KeyColumnTypes?[j];
                var typeMapping = propertyMapping != null
                    ? propertyMapping.TypeMapping
                    : value != null
                        ? Dependencies.TypeMappingSource.FindMapping(value.GetType(), columnType)
                        : Dependencies.TypeMappingSource.FindMapping(columnType!);

                modificationCommand.AddColumnModification(
                    new ColumnModificationParameters(
                        name, originalValue: null, value, propertyMapping?.Property, columnType, typeMapping,
                        read: false, write: true, key: true, condition: true,
                        SensitiveLoggingEnabled, propertyMapping?.Column.IsNullable));
            }

            yield return modificationCommand;
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="UpdateDataOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        UpdateDataOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        var sqlBuilder = new StringBuilder();
        foreach (var modificationCommand in GenerateModificationCommands(operation, model))
        {
            SqlGenerator.AppendUpdateOperation(
                sqlBuilder,
                modificationCommand,
                0);
        }

        builder.Append(sqlBuilder.ToString());
        EndStatement(builder);
    }

    /// <summary>
    ///     Generates the commands that correspond to the given operation.
    /// </summary>
    /// <param name="operation">The data operation to generate commands for.</param>
    /// <param name="model">The model.</param>
    /// <returns>The commands that correspond to the given operation.</returns>
    protected virtual IEnumerable<IReadOnlyModificationCommand> GenerateModificationCommands(
        UpdateDataOperation operation,
        IModel? model)
    {
        if (operation.KeyColumns.Length != operation.KeyValues.GetLength(1))
        {
            throw new InvalidOperationException(
                RelationalStrings.UpdateDataOperationKeyValuesCountMismatch(
                    operation.KeyValues.GetLength(1), operation.KeyColumns.Length, FormatTable(operation.Table, operation.Schema)));
        }

        if (operation.Columns.Length != operation.Values.GetLength(1))
        {
            throw new InvalidOperationException(
                RelationalStrings.UpdateDataOperationValuesCountMismatch(
                    operation.Values.GetLength(1), operation.Columns.Length, FormatTable(operation.Table, operation.Schema)));
        }

        if (operation.KeyValues.GetLength(0) != operation.Values.GetLength(0))
        {
            throw new InvalidOperationException(
                RelationalStrings.UpdateDataOperationRowCountMismatch(
                    operation.Values.GetLength(0), operation.KeyValues.GetLength(0), FormatTable(operation.Table, operation.Schema)));
        }

        if (operation.KeyColumnTypes != null
            && operation.KeyColumns.Length != operation.KeyColumnTypes.Length)
        {
            throw new InvalidOperationException(
                RelationalStrings.UpdateDataOperationKeyTypesCountMismatch(
                    operation.KeyColumnTypes.Length, operation.KeyColumns.Length, FormatTable(operation.Table, operation.Schema)));
        }

        if (operation.ColumnTypes != null
            && operation.Columns.Length != operation.ColumnTypes.Length)
        {
            throw new InvalidOperationException(
                RelationalStrings.UpdateDataOperationTypesCountMismatch(
                    operation.ColumnTypes.Length, operation.Columns.Length, FormatTable(operation.Table, operation.Schema)));
        }

        if (operation.KeyColumnTypes == null
            && model == null)
        {
            throw new InvalidOperationException(
                RelationalStrings.UpdateDataOperationNoModel(
                    FormatTable(operation.Table, operation.Schema)));
        }

        var keyPropertyMappings = operation.KeyColumnTypes == null
            ? GetPropertyMappings(operation.KeyColumns, operation.Table, operation.Schema, model)
            : null;
        var propertyMappings = operation.ColumnTypes == null
            ? GetPropertyMappings(operation.Columns, operation.Table, operation.Schema, model)
            : null;

        for (var i = 0; i < operation.KeyValues.GetLength(0); i++)
        {
            var modificationCommand = Dependencies.ModificationCommandFactory.CreateNonTrackedModificationCommand(
                new NonTrackedModificationCommandParameters(operation.Table, operation.Schema, SensitiveLoggingEnabled));
            modificationCommand.EntityState = EntityState.Modified;

            for (var j = 0; j < operation.KeyColumns.Length; j++)
            {
                var name = operation.KeyColumns[j];
                var value = operation.KeyValues[i, j];
                var propertyMapping = keyPropertyMappings?[j];
                var columnType = operation.KeyColumnTypes?[j];
                var typeMapping = propertyMapping != null
                    ? propertyMapping.TypeMapping
                    : value != null
                        ? Dependencies.TypeMappingSource.FindMapping(value.GetType(), columnType)
                        : Dependencies.TypeMappingSource.FindMapping(columnType!);

                modificationCommand.AddColumnModification(
                    new ColumnModificationParameters(
                        name, originalValue: null, value, propertyMapping?.Property, columnType, typeMapping,
                        read: false, write: false, key: true, condition: true,
                        SensitiveLoggingEnabled, propertyMapping?.Column.IsNullable));
            }

            for (var j = 0; j < operation.Columns.Length; j++)
            {
                var name = operation.Columns[j];
                var value = operation.Values[i, j];
                var propertyMapping = propertyMappings?[j];
                var columnType = operation.ColumnTypes?[j];
                var typeMapping = propertyMapping != null
                    ? propertyMapping.TypeMapping
                    : value != null
                        ? Dependencies.TypeMappingSource.FindMapping(value.GetType(), columnType)
                        : Dependencies.TypeMappingSource.FindMapping(columnType!);

                modificationCommand.AddColumnModification(
                    new ColumnModificationParameters(
                        name, originalValue: null, value, propertyMapping?.Property, columnType, typeMapping,
                        read: false, write: true, key: false, condition: false,
                        SensitiveLoggingEnabled, propertyMapping?.Column.IsNullable));
            }

            yield return modificationCommand;
        }
    }

    private static string FormatTable(string table, string? schema)
        => schema == null ? table : schema + "." + table;

    private static IColumnMapping[] GetPropertyMappings(
        string[] names,
        string tableName,
        string? schema,
        IModel? model)
    {
        var table = model?.GetRelationalModel().FindTable(tableName, schema ?? model.GetDefaultSchema());
        if (table == null)
        {
            throw new InvalidOperationException(
                RelationalStrings.DataOperationNoTable(
                    FormatTable(tableName, schema)));
        }

        var properties = new IColumnMapping[names.Length];
        for (var i = 0; i < names.Length; i++)
        {
            var name = names[i];
            var column = table.FindColumn(name);
            if (column == null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DataOperationNoProperty(
                        FormatTable(tableName, schema), name));
            }

            properties[i] = column.PropertyMappings.First();
        }

        return properties;
    }

    /// <summary>
    ///     Generates a SQL fragment configuring a sequence in a <see cref="AlterSequenceOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void SequenceOptions(
        AlterSequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
        => SequenceOptions(
            operation.Schema,
            operation.Name,
            operation,
            model,
            builder,
            forAlter: true);

    /// <summary>
    ///     Generates a SQL fragment configuring a sequence in a <see cref="CreateSequenceOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void SequenceOptions(
        CreateSequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
        => SequenceOptions(
            operation.Schema,
            operation.Name,
            operation,
            model,
            builder);

    /// <summary>
    ///     Generates a SQL fragment configuring a sequence with the given options.
    /// </summary>
    /// <param name="schema">The schema that contains the sequence, or <see langword="null" /> to use the default schema.</param>
    /// <param name="name">The sequence name.</param>
    /// <param name="operation">The sequence options.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void SequenceOptions(
        string? schema,
        string name,
        SequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
        => SequenceOptions(schema, name, operation, model, builder, forAlter: false);

    /// <summary>
    ///     Generates a SQL fragment configuring a sequence with the given options.
    /// </summary>
    /// <param name="schema">The schema that contains the sequence, or <see langword="null" /> to use the default schema.</param>
    /// <param name="name">The sequence name.</param>
    /// <param name="operation">The sequence options.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    /// <param name="forAlter">If <see langword="true"/>, then all options are included, even if default.</param>
    protected virtual void SequenceOptions(
        string? schema,
        string name,
        SequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool forAlter)
    {
        var intTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(int));
        var longTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(long));

        builder
            .Append(" INCREMENT BY ")
            .Append(intTypeMapping.GenerateSqlLiteral(operation.IncrementBy));

        if (operation.MinValue != null)
        {
            builder
                .Append(" MINVALUE ")
                .Append(longTypeMapping.GenerateSqlLiteral(operation.MinValue));
        }
        else if (forAlter)
        {
            builder
                .Append(" NO MINVALUE");
        }

        if (operation.MaxValue != null)
        {
            builder
                .Append(" MAXVALUE ")
                .Append(longTypeMapping.GenerateSqlLiteral(operation.MaxValue));
        }
        else if (forAlter)
        {
            builder
                .Append(" NO MAXVALUE");
        }

        builder.Append(operation.IsCyclic ? " CYCLE" : " NO CYCLE");

        if (!operation.IsCached)
        {
            builder
                .Append(" NO CACHE");
        }
        else if (operation.CacheSize != null)
        {
            builder
                .Append(" CACHE ")
                .Append(intTypeMapping.GenerateSqlLiteral(operation.CacheSize.Value));
        }
        else if (forAlter)
        {
            builder
                .Append(" CACHE");
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for the column definitions in a <see cref="CreateTableOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void CreateTableColumns(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        for (var i = 0; i < operation.Columns.Count; i++)
        {
            ColumnDefinition(operation.Columns[i], model, builder);

            if (i != operation.Columns.Count - 1)
            {
                builder.AppendLine(",");
            }
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for a column definition in an <see cref="AddColumnOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void ColumnDefinition(
        AddColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
        => ColumnDefinition(
            operation.Schema,
            operation.Table,
            operation.Name,
            operation,
            model,
            builder);

    /// <summary>
    ///     Generates a SQL fragment for a column definition for the given column metadata.
    /// </summary>
    /// <param name="schema">The schema that contains the table, or <see langword="null" /> to use the default schema.</param>
    /// <param name="table">The table that contains the column.</param>
    /// <param name="name">The column name.</param>
    /// <param name="operation">The column metadata.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void ColumnDefinition(
        string? schema,
        string table,
        string name,
        ColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation.ComputedColumnSql != null)
        {
            ComputedColumnDefinition(schema, table, name, operation, model, builder);

            return;
        }

        var columnType = operation.ColumnType ?? GetColumnType(schema, table, name, operation, model)!;
        builder
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
            .Append(" ")
            .Append(columnType);

        if (operation.Collation != null)
        {
            builder
                .Append(" COLLATE ")
                .Append(operation.Collation);
        }

        builder.Append(operation.IsNullable ? " NULL" : " NOT NULL");

        DefaultValue(operation.DefaultValue, operation.DefaultValueSql, columnType, builder);
    }

    /// <summary>
    ///     Generates a SQL fragment for a computed column definition for the given column metadata.
    /// </summary>
    /// <param name="schema">The schema that contains the table, or <see langword="null" /> to use the default schema.</param>
    /// <param name="table">The table that contains the column.</param>
    /// <param name="name">The column name.</param>
    /// <param name="operation">The column metadata.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void ComputedColumnDefinition(
        string? schema,
        string table,
        string name,
        ColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
        => throw new NotSupportedException(RelationalStrings.MigrationSqlGenerationMissing(nameof(ColumnOperation)));

    /// <summary>
    ///     Gets the store/database type of a column given the provided metadata.
    /// </summary>
    /// <param name="schema">The schema that contains the table, or <see langword="null" /> to use the default schema.</param>
    /// <param name="tableName">The table that contains the column.</param>
    /// <param name="name">The column name.</param>
    /// <param name="operation">The column metadata.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <returns>The database/store type for the column.</returns>
    protected virtual string? GetColumnType(
        string? schema,
        string tableName,
        string name,
        ColumnOperation operation,
        IModel? model)
    {
        var keyOrIndex = false;

        var table = model?.GetRelationalModel().FindTable(tableName, schema);
        var column = table?.FindColumn(name);
        if (column != null)
        {
            if (operation.IsUnicode == column.IsUnicode
                && operation.MaxLength == column.MaxLength
                && operation.Precision == column.Precision
                && operation.Scale == column.Scale
                && operation.IsFixedLength == column.IsFixedLength
                && operation.IsRowVersion == column.IsRowVersion)
            {
                return column.StoreType;
            }

            keyOrIndex = table!.UniqueConstraints.Any(u => u.Columns.Contains(column))
                || table.ForeignKeyConstraints.Any(u => u.Columns.Contains(column))
                || table.Indexes.Any(u => u.Columns.Contains(column));
        }

        return Dependencies.TypeMappingSource.FindMapping(
                operation.ClrType,
                null,
                keyOrIndex,
                operation.IsUnicode,
                operation.MaxLength,
                operation.IsRowVersion,
                operation.IsFixedLength,
                operation.Precision,
                operation.Scale)
            ?.StoreType;
    }

    /// <summary>
    ///     Generates a SQL fragment for the default constraint of a column.
    /// </summary>
    /// <param name="defaultValue">The default value for the column.</param>
    /// <param name="defaultValueSql">The SQL expression to use for the column's default constraint.</param>
    /// <param name="columnType">Store/database type of the column.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void DefaultValue(
        object? defaultValue,
        string? defaultValueSql,
        string? columnType,
        MigrationCommandListBuilder builder)
    {
        if (defaultValueSql != null)
        {
            builder
                .Append(" DEFAULT (")
                .Append(defaultValueSql)
                .Append(")");
        }
        else if (defaultValue != null)
        {
            var typeMapping = (columnType != null
                    ? Dependencies.TypeMappingSource.FindMapping(defaultValue.GetType(), columnType)
                    : null)
                ?? Dependencies.TypeMappingSource.GetMappingForValue(defaultValue);

            builder
                .Append(" DEFAULT ")
                .Append(typeMapping.GenerateSqlLiteral(defaultValue));
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for the constraints of a <see cref="CreateTableOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void CreateTableConstraints(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        CreateTablePrimaryKeyConstraint(operation, model, builder);
        CreateTableUniqueConstraints(operation, model, builder);
        CreateTableCheckConstraints(operation, model, builder);
        CreateTableForeignKeys(operation, model, builder);
    }

    /// <summary>
    ///     Generates a SQL fragment for the foreign key constraints of a <see cref="CreateTableOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void CreateTableForeignKeys(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        foreach (var foreignKey in operation.ForeignKeys)
        {
            builder.AppendLine(",");
            ForeignKeyConstraint(foreignKey, model, builder);
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for a foreign key constraint of an <see cref="AddForeignKeyOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void ForeignKeyConstraint(
        AddForeignKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation.Name != null)
        {
            builder
                .Append("CONSTRAINT ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ");
        }

        builder
            .Append("FOREIGN KEY (")
            .Append(ColumnList(operation.Columns))
            .Append(") REFERENCES ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.PrincipalTable, operation.PrincipalSchema));

        if (operation.PrincipalColumns != null)
        {
            builder
                .Append(" (")
                .Append(ColumnList(operation.PrincipalColumns))
                .Append(")");
        }

        if (operation.OnUpdate != ReferentialAction.NoAction)
        {
            builder.Append(" ON UPDATE ");
            ForeignKeyAction(operation.OnUpdate, builder);
        }

        if (operation.OnDelete != ReferentialAction.NoAction)
        {
            builder.Append(" ON DELETE ");
            ForeignKeyAction(operation.OnDelete, builder);
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for the primary key constraint of a <see cref="CreateTableOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void CreateTablePrimaryKeyConstraint(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation.PrimaryKey != null)
        {
            builder.AppendLine(",");
            PrimaryKeyConstraint(operation.PrimaryKey, model, builder);
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for a primary key constraint of an <see cref="AddPrimaryKeyOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void PrimaryKeyConstraint(
        AddPrimaryKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation.Name != null)
        {
            builder
                .Append("CONSTRAINT ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ");
        }

        builder
            .Append("PRIMARY KEY ");

        IndexTraits(operation, model, builder);

        builder.Append("(")
            .Append(ColumnList(operation.Columns))
            .Append(")");
    }

    /// <summary>
    ///     Generates a SQL fragment for the unique constraints of a <see cref="CreateTableOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void CreateTableUniqueConstraints(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        foreach (var uniqueConstraint in operation.UniqueConstraints)
        {
            builder.AppendLine(",");
            UniqueConstraint(uniqueConstraint, model, builder);
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for a unique constraint of an <see cref="AddUniqueConstraintOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void UniqueConstraint(
        AddUniqueConstraintOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation.Name != null)
        {
            builder
                .Append("CONSTRAINT ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ");
        }

        builder
            .Append("UNIQUE ");

        IndexTraits(operation, model, builder);

        builder.Append("(")
            .Append(ColumnList(operation.Columns))
            .Append(")");
    }

    /// <summary>
    ///     Generates a SQL fragment for the check constraints of a <see cref="CreateTableOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void CreateTableCheckConstraints(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        foreach (var checkConstraint in operation.CheckConstraints)
        {
            builder.AppendLine(",");
            CheckConstraint(checkConstraint, model, builder);
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for a check constraint of an <see cref="AddCheckConstraintOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void CheckConstraint(
        AddCheckConstraintOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation.Name != null)
        {
            builder
                .Append("CONSTRAINT ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ");
        }

        builder
            .Append("CHECK ");

        builder.Append("(")
            .Append(operation.Sql)
            .Append(")");
    }

    /// <summary>
    ///     Generates a SQL fragment for extra with options of a key from a
    ///     <see cref="AddPrimaryKeyOperation" /> or <see cref="AddUniqueConstraintOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void KeyWithOptions(MigrationOperation operation, MigrationCommandListBuilder builder)
    {
    }

    /// <summary>
    ///     Generates a SQL fragment for traits of an index from a <see cref="CreateIndexOperation" />,
    ///     <see cref="AddPrimaryKeyOperation" />, or <see cref="AddUniqueConstraintOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void IndexTraits(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
    }

    /// <summary>
    ///     Returns a SQL fragment for the column list of an index from a <see cref="CreateIndexOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void GenerateIndexColumnList(CreateIndexOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        for (var i = 0; i < operation.Columns.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Columns[i]));

            if (operation.IsDescending is not null && (operation.IsDescending.Length == 0 || operation.IsDescending[i]))
            {
                builder.Append(" DESC");
            }
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for extras (filter, included columns, options) of an index from a <see cref="CreateIndexOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void IndexOptions(CreateIndexOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (!string.IsNullOrEmpty(operation.Filter))
        {
            builder
                .Append(" WHERE ")
                .Append(operation.Filter);
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for the given referential action.
    /// </summary>
    /// <param name="referentialAction">The referential action.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void ForeignKeyAction(
        ReferentialAction referentialAction,
        MigrationCommandListBuilder builder)
    {
        switch (referentialAction)
        {
            case ReferentialAction.Restrict:
                builder.Append("RESTRICT");
                break;
            case ReferentialAction.Cascade:
                builder.Append("CASCADE");
                break;
            case ReferentialAction.SetNull:
                builder.Append("SET NULL");
                break;
            case ReferentialAction.SetDefault:
                builder.Append("SET DEFAULT");
                break;
            default:
                Check.DebugAssert(
                    referentialAction == ReferentialAction.NoAction,
                    "Unexpected value: " + referentialAction);
                break;
        }
    }

    /// <summary>
    ///     Generates a SQL fragment to terminate the SQL command.
    /// </summary>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    /// <param name="suppressTransaction">
    ///     Indicates whether or not transactions should be suppressed while executing the built command.
    /// </param>
    protected virtual void EndStatement(
        MigrationCommandListBuilder builder,
        bool suppressTransaction = false)
        => builder.EndCommand(suppressTransaction);

    /// <summary>
    ///     Concatenates the given column names into a <see cref="ISqlGenerationHelper.DelimitIdentifier(string)" />
    ///     separated list.
    /// </summary>
    /// <param name="columns">The column names.</param>
    /// <returns>The column list.</returns>
    protected virtual string ColumnList(string[] columns)
        => string.Join(", ", columns.Select(Dependencies.SqlGenerationHelper.DelimitIdentifier));

    /// <summary>
    ///     Checks whether or not <see cref="AddColumnOperation" /> supports the passing in the
    ///     old column, which was only added in EF Core 1.1.
    /// </summary>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <returns>
    ///     <see langword="true" /> If the model was generated by EF Core 1.1 or later; <see langword="false" /> if the model is
    ///     <see langword="null" />, has
    ///     no version specified, or was generated by an EF Core version prior to 1.1.
    /// </returns>
    protected virtual bool IsOldColumnSupported(IModel? model)
        => TryGetVersion(model, out var version) && VersionComparer.Compare(version, "1.1.0") >= 0;

    /// <summary>
    ///     Checks whether or not <see cref="RenameTableOperation" /> and <see cref="RenameSequenceOperation" /> use
    ///     the legacy behavior of setting the new name and schema to null when unchanged.
    /// </summary>
    /// <param name="model">The target model.</param>
    /// <returns><see langword="true" /> if the legacy behavior is used.</returns>
    protected virtual bool HasLegacyRenameOperations(IModel? model)
        => !TryGetVersion(model, out var version) || VersionComparer.Compare(version, "2.1.0") < 0;

    /// <summary>
    ///     Gets the product version used to generate the current migration. Providers can use this to preserve
    ///     compatibility with migrations generated using previous versions.
    /// </summary>
    /// <param name="model">The target model.</param>
    /// <param name="version">The version.</param>
    /// <returns><see langword="true" /> if the version could be retrieved.</returns>
    protected virtual bool TryGetVersion([NotNullWhen(true)] IModel? model, [NotNullWhen(true)] out string? version)
    {
        if (!(model?.GetProductVersion() is string versionString))
        {
            version = null;

            return false;
        }

        version = versionString;

        return true;
    }
}
