// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     Used to generate C# code for migrations.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public class CSharpMigrationsGenerator : MigrationsCodeGenerator
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CSharpMigrationsGenerator" /> class.
    /// </summary>
    /// <param name="dependencies">The base dependencies.</param>
    /// <param name="csharpDependencies">The dependencies.</param>
    public CSharpMigrationsGenerator(
        MigrationsCodeGeneratorDependencies dependencies,
        CSharpMigrationsGeneratorDependencies csharpDependencies)
        : base(dependencies)
    {
        CSharpDependencies = csharpDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual CSharpMigrationsGeneratorDependencies CSharpDependencies { get; }

    private ICSharpHelper Code
        => CSharpDependencies.CSharpHelper;

    /// <summary>
    ///     Gets the file extension code files should use.
    /// </summary>
    /// <value> The file extension. </value>
    public override string FileExtension
        => ".cs";

    /// <summary>
    ///     Gets the programming language supported by this service.
    /// </summary>
    /// <value> The language. </value>
    public override string Language
        => "C#";

    /// <summary>
    ///     Generates the migration code.
    /// </summary>
    /// <param name="migrationNamespace">The migration's namespace.</param>
    /// <param name="migrationName">The migration's name.</param>
    /// <param name="upOperations">The migration's up operations.</param>
    /// <param name="downOperations">The migration's down operations.</param>
    /// <returns>The migration code.</returns>
    public override string GenerateMigration(
        string? migrationNamespace,
        string migrationName,
        IReadOnlyList<MigrationOperation> upOperations,
        IReadOnlyList<MigrationOperation> downOperations)
    {
        var builder = new IndentedStringBuilder();
        var namespaces = new List<string> { "Microsoft.EntityFrameworkCore.Migrations" };
        namespaces.AddRange(GetNamespaces(upOperations.Concat(downOperations)));
        foreach (var n in namespaces.OrderBy(x => x, new NamespaceComparer()).Distinct())
        {
            builder
                .Append("using ")
                .Append(n)
                .AppendLine(";");
        }

        builder
            .AppendLine()
            .AppendLine("#nullable disable");

        // Suppress "Prefer jagged arrays over multidimensional" when we have a seeding operation with a multidimensional array
        if (HasMultidimensionalArray(upOperations.Concat(downOperations)))
        {
            builder
                .AppendLine()
                .AppendLine("#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional");
        }

        if (!string.IsNullOrEmpty(migrationNamespace))
        {
            builder
                .AppendLine()
                .Append("namespace ").AppendLine(Code.Namespace(migrationNamespace))
                .AppendLine("{")
                .IncrementIndent();
        }

        builder
            .AppendLine("/// <inheritdoc />")
            .Append("public partial class ").Append(Code.Identifier(migrationName)).AppendLine(" : Migration")
            .AppendLine("{");
        using (builder.Indent())
        {
            builder
                .AppendLine("/// <inheritdoc />")
                .AppendLine("protected override void Up(MigrationBuilder migrationBuilder)")
                .AppendLine("{");
            using (builder.Indent())
            {
                CSharpDependencies.CSharpMigrationOperationGenerator.Generate("migrationBuilder", upOperations, builder);
            }

            builder
                .AppendLine()
                .AppendLine("}")
                .AppendLine()
                .AppendLine("/// <inheritdoc />")
                .AppendLine("protected override void Down(MigrationBuilder migrationBuilder)")
                .AppendLine("{");
            using (builder.Indent())
            {
                CSharpDependencies.CSharpMigrationOperationGenerator.Generate("migrationBuilder", downOperations, builder);
            }

            builder
                .AppendLine()
                .AppendLine("}");
        }

        builder.AppendLine("}");

        if (!string.IsNullOrEmpty(migrationNamespace))
        {
            builder
                .DecrementIndent()
                .AppendLine("}");
        }

        return builder.ToString();
    }

    private static void AppendAutoGeneratedTag(IndentedStringBuilder builder)
        => builder.AppendLine("// <auto-generated />");

    /// <summary>
    ///     Generates the migration metadata code.
    /// </summary>
    /// <param name="migrationNamespace">The migration's namespace.</param>
    /// <param name="contextType">The migration's <see cref="DbContext" /> type.</param>
    /// <param name="migrationName">The migration's name.</param>
    /// <param name="migrationId">The migration's ID.</param>
    /// <param name="targetModel">The migration's target model.</param>
    /// <returns>The migration metadata code.</returns>
    public override string GenerateMetadata(
        string? migrationNamespace,
        Type contextType,
        string migrationName,
        string migrationId,
        IModel targetModel)
    {
        var builder = new IndentedStringBuilder();
        AppendAutoGeneratedTag(builder);
        var namespaces = new List<string>
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore.Infrastructure",
            "Microsoft.EntityFrameworkCore.Migrations",
            "Microsoft.EntityFrameworkCore.Storage.ValueConversion"
        };
        if (!string.IsNullOrEmpty(contextType.Namespace))
        {
            namespaces.Add(contextType.Namespace);
        }

        namespaces.AddRange(GetNamespaces(targetModel));
        foreach (var n in namespaces.OrderBy(x => x, new NamespaceComparer()).Distinct())
        {
            builder
                .Append("using ")
                .Append(n)
                .AppendLine(";");
        }

        builder
            .AppendLine()
            .AppendLine("#nullable disable");

        if (!string.IsNullOrEmpty(migrationNamespace))
        {
            builder
                .AppendLine()
                .Append("namespace ").AppendLine(Code.Namespace(migrationNamespace))
                .AppendLine("{")
                .IncrementIndent();
        }

        builder
            .Append("[DbContext(typeof(").Append(Code.Reference(contextType)).AppendLine("))]")
            .Append("[Migration(").Append(Code.Literal(migrationId)).AppendLine(")]")
            .Append("partial class ").AppendLine(Code.Identifier(migrationName))
            .AppendLine("{");
        using (builder.Indent())
        {
            builder
                .AppendLine("/// <inheritdoc />")
                .AppendLine("protected override void BuildTargetModel(ModelBuilder modelBuilder)")
                .AppendLine("{")
                .DecrementIndent()
                .DecrementIndent()
                .AppendLine("#pragma warning disable 612, 618")
                .IncrementIndent()
                .IncrementIndent();
            using (builder.Indent())
            {
                // TODO: Optimize. This is repeated below
                CSharpDependencies.CSharpSnapshotGenerator.Generate("modelBuilder", targetModel, builder);
            }

            builder
                .DecrementIndent()
                .DecrementIndent()
                .AppendLine("#pragma warning restore 612, 618")
                .IncrementIndent()
                .IncrementIndent()
                .AppendLine("}");
        }

        builder.AppendLine("}");

        if (!string.IsNullOrEmpty(migrationNamespace))
        {
            builder
                .DecrementIndent()
                .AppendLine("}");
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Generates the model snapshot code.
    /// </summary>
    /// <param name="modelSnapshotNamespace">The model snapshot's namespace.</param>
    /// <param name="contextType">The model snapshot's <see cref="DbContext" /> type.</param>
    /// <param name="modelSnapshotName">The model snapshot's name.</param>
    /// <param name="model">The model.</param>
    /// <returns>The model snapshot code.</returns>
    public override string GenerateSnapshot(
        string? modelSnapshotNamespace,
        Type contextType,
        string modelSnapshotName,
        IModel model)
    {
        var builder = new IndentedStringBuilder();
        AppendAutoGeneratedTag(builder);
        var namespaces = new List<string>
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore.Infrastructure",
            "Microsoft.EntityFrameworkCore.Storage.ValueConversion"
        };
        if (!string.IsNullOrEmpty(contextType.Namespace))
        {
            namespaces.Add(contextType.Namespace);
        }

        namespaces.AddRange(GetNamespaces(model));
        foreach (var n in namespaces.OrderBy(x => x, new NamespaceComparer()).Distinct())
        {
            builder
                .Append("using ")
                .Append(n)
                .AppendLine(";");
        }

        builder
            .AppendLine()
            .AppendLine("#nullable disable");

        if (!string.IsNullOrEmpty(modelSnapshotNamespace))
        {
            builder
                .AppendLine()
                .Append("namespace ").AppendLine(Code.Namespace(modelSnapshotNamespace))
                .AppendLine("{")
                .IncrementIndent();
        }

        builder
            .Append("[DbContext(typeof(").Append(Code.Reference(contextType)).AppendLine("))]")
            .Append("partial class ").Append(Code.Identifier(modelSnapshotName)).AppendLine(" : ModelSnapshot")
            .AppendLine("{");
        using (builder.Indent())
        {
            builder
                .AppendLine("protected override void BuildModel(ModelBuilder modelBuilder)")
                .AppendLine("{")
                .DecrementIndent()
                .DecrementIndent()
                .AppendLine("#pragma warning disable 612, 618")
                .IncrementIndent()
                .IncrementIndent();
            using (builder.Indent())
            {
                CSharpDependencies.CSharpSnapshotGenerator.Generate("modelBuilder", model, builder);
            }

            builder
                .DecrementIndent()
                .DecrementIndent()
                .AppendLine("#pragma warning restore 612, 618")
                .IncrementIndent()
                .IncrementIndent()
                .AppendLine("}");
        }

        builder.AppendLine("}");

        if (!string.IsNullOrEmpty(modelSnapshotNamespace))
        {
            builder
                .DecrementIndent()
                .AppendLine("}");
        }

        return builder.ToString();
    }

    private bool HasMultidimensionalArray(IEnumerable<MigrationOperation> operations)
    {
        return operations.Any(
            o =>
                (o is InsertDataOperation insertDataOperation
                    && IsMultidimensional(insertDataOperation.Values))
                || (o is UpdateDataOperation updateDataOperation
                    && (IsMultidimensional(updateDataOperation.Values) || IsMultidimensional(updateDataOperation.KeyValues)))
                || (o is DeleteDataOperation deleteDataOperation
                    && IsMultidimensional(deleteDataOperation.KeyValues)));

        static bool IsMultidimensional(Array array)
            => array.GetLength(0) > 1 && array.GetLength(1) > 1;
    }
}
