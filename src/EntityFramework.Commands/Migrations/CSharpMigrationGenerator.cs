// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class CSharpMigrationGenerator : MigrationCodeGenerator
    {
        private readonly CSharpHelper _code;
        private readonly CSharpMigrationOperationGenerator _operationGenerator;
        private readonly CSharpModelGenerator _modelGenerator;

        public CSharpMigrationGenerator(
            [NotNull] CSharpHelper code,
            [NotNull] CSharpMigrationOperationGenerator operationGenerator,
            [NotNull] CSharpModelGenerator modelGenerator)
        {
            Check.NotNull(code, nameof(code));
            Check.NotNull(operationGenerator, nameof(operationGenerator));
            Check.NotNull(modelGenerator, nameof(modelGenerator));

            _code = code;
            _operationGenerator = operationGenerator;
            _modelGenerator = modelGenerator;
        }

        public override string Language => ".cs";

        public override string Generate(
            string migrationNamespace,
            string migrationName,
            IReadOnlyList<MigrationOperation> upOperations,
            IReadOnlyList<MigrationOperation> downOperations)
        {
            Check.NotEmpty(migrationNamespace, nameof(migrationNamespace));
            Check.NotEmpty(migrationName, nameof(migrationName));
            Check.NotNull(upOperations, nameof(upOperations));
            Check.NotNull(downOperations, nameof(downOperations));

            var builder = new IndentedStringBuilder();
            builder
                .AppendLine("using System.Collections.Generic;")
                .AppendLine("using Microsoft.Data.Entity.Migrations;")
                .AppendLine("using Microsoft.Data.Entity.Migrations.Builders;")
                .AppendLine("using Microsoft.Data.Entity.Migrations.Operations;")
                .AppendLine()
                .Append("namespace ").AppendLine(_code.Identifier(migrationNamespace))
                .AppendLine("{");
            using (builder.Indent())
            {
                builder
                    .Append("public partial class ").Append(_code.Identifier(migrationName)).AppendLine(" : Migration")
                    .AppendLine("{");
                using (builder.Indent())
                {
                    builder
                        .AppendLine("public override void Up(MigrationBuilder migration)")
                        .AppendLine("{");
                    using (builder.Indent())
                    {
                        _operationGenerator.Generate("migration", upOperations, builder);
                    }
                    builder
                        .AppendLine("}")
                        .AppendLine()
                        .AppendLine("public override void Down(MigrationBuilder migration)")
                        .AppendLine("{");
                    using (builder.Indent())
                    {
                        _operationGenerator.Generate("migration", downOperations, builder);
                    }
                    builder.AppendLine("}");
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");

            return builder.ToString();
        }

        public override string GenerateMetadata(
            string migrationNamespace,
            Type contextType,
            string migrationName,
            string migrationId,
            string productVersion,
            IModel targetModel)
        {
            Check.NotEmpty(migrationNamespace, nameof(migrationNamespace));
            Check.NotNull(contextType, nameof(contextType));
            Check.NotEmpty(migrationName, nameof(migrationName));
            Check.NotEmpty(migrationId, nameof(migrationId));
            Check.NotEmpty(productVersion, nameof(productVersion));
            Check.NotNull(targetModel, nameof(targetModel));

            var builder = new IndentedStringBuilder();
            builder
                .AppendLine("using System;")
                .AppendLine("using Microsoft.Data.Entity;")
                .AppendLine("using Microsoft.Data.Entity.Metadata;")
                .AppendLine("using Microsoft.Data.Entity.Migrations.Infrastructure;")
                .Append("using ").Append(contextType.Namespace).AppendLine(";")
                .AppendLine()
                .Append("namespace ").AppendLine(_code.Identifier(migrationNamespace))
                .AppendLine("{");
            using (builder.Indent())
            {
                builder
                    .Append("[ContextType(typeof(").Append(_code.Reference(contextType)).AppendLine("))]")
                    .Append("partial class ").AppendLine(_code.Identifier(migrationName))
                    .AppendLine("{");
                using (builder.Indent())
                {
                    builder
                        .AppendLine("public override string Id")
                        .AppendLine("{");
                    using (builder.Indent())
                    {
                        builder.Append("get { return ").Append(_code.Literal(migrationId)).AppendLine("; }");
                    }
                    builder
                        .AppendLine("}")
                        .AppendLine()
                        .AppendLine("public override string ProductVersion")
                        .AppendLine("{");
                    using (builder.Indent())
                    {
                        builder.Append("get { return ").Append(_code.Literal(productVersion)).AppendLine("; }");
                    }
                    builder
                        .AppendLine("}")
                        .AppendLine()
                        .AppendLine("public override void BuildTargetModel(ModelBuilder builder)")
                        .AppendLine("{");
                    using (builder.Indent())
                    {
                        // TODO: Optimize. This is repeated below
                        _modelGenerator.Generate(targetModel, builder);
                    }
                    builder.AppendLine("}");
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");

            return builder.ToString();
        }

        public override string GenerateSnapshot(
            string modelSnapshotNamespace,
            Type contextType,
            string modelSnapshotName,
            IModel model)
        {
            Check.NotEmpty(modelSnapshotNamespace, nameof(modelSnapshotNamespace));
            Check.NotNull(contextType, nameof(contextType));
            Check.NotEmpty(modelSnapshotName, nameof(modelSnapshotName));
            Check.NotNull(model, nameof(model));

            var builder = new IndentedStringBuilder();
            builder
                .AppendLine("using System;")
                .AppendLine("using Microsoft.Data.Entity;")
                .AppendLine("using Microsoft.Data.Entity.Metadata;")
                .AppendLine("using Microsoft.Data.Entity.Migrations.Infrastructure;")
                .Append("using ").Append(contextType.Namespace).AppendLine(";")
                .AppendLine()
                .Append("namespace ").AppendLine(_code.Identifier(modelSnapshotNamespace))
                .AppendLine("{");
            using (builder.Indent())
            {
                builder
                    .Append("[ContextType(typeof(").Append(_code.Reference(contextType)).AppendLine("))]")
                    .Append("partial class ").Append(_code.Identifier(modelSnapshotName)).AppendLine(" : ModelSnapshot")
                    .AppendLine("{");
                using (builder.Indent())
                {
                    builder
                        .AppendLine("public override void BuildModel(ModelBuilder builder)")
                        .AppendLine("{");
                    using (builder.Indent())
                    {
                        _modelGenerator.Generate(model, builder);
                    }
                    builder.AppendLine("}");
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}
