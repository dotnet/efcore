// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Design
{
    public class VBMigrationsGenerator : MigrationsCodeGenerator
    {
        private readonly VBHelper _code;
        private readonly VBMigrationOperationGenerator _operationGenerator;
        private readonly VBSnapshotGenerator _modelGenerator;

        public VBMigrationsGenerator(
            [NotNull] VBHelper codeHelper,
            [NotNull] VBMigrationOperationGenerator operationGenerator,
            [NotNull] VBSnapshotGenerator modelGenerator)
        {
            Check.NotNull(codeHelper, nameof(codeHelper));
            Check.NotNull(operationGenerator, nameof(operationGenerator));
            Check.NotNull(modelGenerator, nameof(modelGenerator));

            _code = codeHelper;
            _operationGenerator = operationGenerator;
            _modelGenerator = modelGenerator;
        }

        public override string FileExtension => ".vb";

        public override string GenerateMigration(
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
            var namespaces = new List<string>
            {
                "System",
                "System.Collections.Generic",
                "Microsoft.Data.Entity.Migrations"
            };
            namespaces.AddRange(GetNamespaces(upOperations.Concat(downOperations)));
            foreach (var n in namespaces.Distinct())
            {
                builder
                    .Append("Imports ")
                    .Append(n)
                    .AppendLine();
            }
            builder
                .AppendLine()
                .Append("Namespace ").AppendLine(_code.Namespace(migrationNamespace))
                .AppendLine();
            using (builder.Indent())
            {
                builder
                    .Append("Public Partial Class ").AppendLine(_code.Identifier(migrationName)).AppendLine("Inherits Migration")
                    .AppendLine();
                using (builder.Indent())
                {
                    builder
                        .AppendLine("Protected Overrides Sub Up(migrationBuilder As MigrationBuilder)")
                        .AppendLine();
                    using (builder.Indent())
                    {
                        _operationGenerator.Generate("migrationBuilder", upOperations, builder);
                    }
                    builder
                        .AppendLine()
                        .AppendLine("End Sub")
                        .AppendLine()
                        .AppendLine("Protected Overrides Sub Down(migrationBuilder As MigrationBuilder)")
                        .AppendLine();
                    using (builder.Indent())
                    {
                        _operationGenerator.Generate("migrationBuilder", downOperations, builder);
                    }
                    builder
                        .AppendLine()
                        .AppendLine("End Sub");
                }
                builder.AppendLine("End Class");
            }
            builder.AppendLine("End Namespace");

            return builder.ToString();
        }

        public override string GenerateMetadata(
            string migrationNamespace,
            Type contextType,
            string migrationName,
            string migrationId,
            IModel targetModel)
        {
            Check.NotEmpty(migrationNamespace, nameof(migrationNamespace));
            Check.NotNull(contextType, nameof(contextType));
            Check.NotEmpty(migrationName, nameof(migrationName));
            Check.NotEmpty(migrationId, nameof(migrationId));
            Check.NotNull(targetModel, nameof(targetModel));

            var builder = new IndentedStringBuilder();
            var namespaces = new List<string>
            {
                "System",
                "Microsoft.Data.Entity",
                "Microsoft.Data.Entity.Infrastructure",
                "Microsoft.Data.Entity.Metadata",
                "Microsoft.Data.Entity.Migrations",
                contextType.Namespace
            };
            namespaces.AddRange(GetNamespaces(targetModel));
            foreach (var n in namespaces.Distinct())
            {
                builder
                    .Append("Imports ")
                    .Append(n)
                    .AppendLine();
            }
            builder
                .AppendLine()
                .Append("Namespace ").AppendLine(_code.Namespace(migrationNamespace))
                .AppendLine();
            using (builder.Indent())
            {
                builder
                    .Append("<DbContext(GetType(").Append(_code.Reference(contextType)).AppendLine("))>")
                    .Append("<Migration(").Append(_code.Literal(migrationId)).AppendLine(")>")
                    .Append("Partial Class ").AppendLine(_code.Identifier(migrationName))
                    .AppendLine();
                using (builder.Indent())
                {
                    builder
                        .AppendLine("Protected Overrides Sub BuildTargetModel(modelBuilder As ModelBuilder)")
                        .AppendLine();
                    using (builder.Indent())
                    {
                        // TODO: Optimize. This is repeated below
                        _modelGenerator.Generate("modelBuilder", targetModel, builder);
                    }
                    builder.AppendLine("End Sub");
                }
                builder.AppendLine("End Class");
            }
            builder.AppendLine("End Namespace");

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
            var namespaces = new List<string>
            {
                "System",
                "Microsoft.Data.Entity",
                "Microsoft.Data.Entity.Infrastructure",
                "Microsoft.Data.Entity.Metadata",
                "Microsoft.Data.Entity.Migrations",
                contextType.Namespace
            };
            namespaces.AddRange(GetNamespaces(model));
            foreach (var n in namespaces.Distinct())
            {
                builder
                    .Append("Imports ")
                    .Append(n)
                    .AppendLine();
            }
            builder
                .AppendLine()
                .Append("Namespace ").AppendLine(_code.Namespace(modelSnapshotNamespace))
                .AppendLine();
            using (builder.Indent())
            {
                builder
                    .Append("<DbContext(GetType(").Append(_code.Reference(contextType)).AppendLine("))>")
                    .Append("Partial Class ").AppendLine(_code.Identifier(modelSnapshotName)).AppendLine("Inherits ModelSnapshot")
                    .AppendLine();
                using (builder.Indent())
                {
                    builder
                        .AppendLine("Protected Overrides Sub BuildModel(modelBuilder As ModelBuilder)")
                        .AppendLine();
                    using (builder.Indent())
                    {
                        _modelGenerator.Generate("modelBuilder", model, builder);
                    }
                    builder.AppendLine("End Sub");
                }
                builder.AppendLine("End Class");
            }
            builder.AppendLine("End Namespace");

            return builder.ToString();
        }
    }
}
