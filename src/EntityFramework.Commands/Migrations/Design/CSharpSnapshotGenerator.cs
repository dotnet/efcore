// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Design
{
    public class CSharpSnapshotGenerator
    {
        private readonly CSharpHelper _code;

        public CSharpSnapshotGenerator([NotNull] CSharpHelper codeHelper)
        {
            Check.NotNull(codeHelper, nameof(codeHelper));

            _code = codeHelper;
        }

        public virtual void Generate(
            [NotNull] string builderName,
            [NotNull] IModel model,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(builderName, nameof(builderName));
            Check.NotNull(model, nameof(model));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var annotations = model.Annotations.ToArray();
            if (annotations.Length != 0)
            {
                stringBuilder.Append(builderName);

                using (stringBuilder.Indent())
                {
                    GenerateAnnotations(annotations, stringBuilder);
                }

                stringBuilder.AppendLine(";");
            }

            GenerateEntityTypes(builderName, Sort(model.EntityTypes), stringBuilder);
        }

        private IReadOnlyList<IEntityType> Sort(IReadOnlyList<IEntityType> entityTypes)
        {
            var entityTypeGraph = new Multigraph<IEntityType, int>();
            entityTypeGraph.AddVertices(entityTypes);
            foreach (var entityType in entityTypes.Where(et => et.BaseType != null))
            {
                entityTypeGraph.AddEdge(entityType.BaseType, entityType, 0);
            }
            return entityTypeGraph.TopologicalSort();
        }

        protected virtual void GenerateEntityTypes(
            [NotNull] string builderName,
            [NotNull] IReadOnlyList<IEntityType> entityTypes,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(builderName, nameof(builderName));
            Check.NotNull(entityTypes, nameof(entityTypes));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var entityType in entityTypes)
            {
                stringBuilder.AppendLine();

                GenerateEntityType(builderName, entityType, stringBuilder);
            }

            foreach (var entityType in entityTypes.Where(e => e.GetForeignKeys().Any()))
            {
                stringBuilder.AppendLine();

                GenerateRelationships(builderName, entityType, stringBuilder);
            }
        }

        protected virtual void GenerateEntityType(
            [NotNull] string builderName,
            [NotNull] IEntityType entityType,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(builderName, nameof(builderName));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .Append(builderName)
                .Append(".Entity(")
                .Append(_code.Literal(entityType.Name))
                .AppendLine(", b =>");

            using (stringBuilder.Indent())
            {
                stringBuilder.Append("{");

                using (stringBuilder.Indent())
                {

                    GenerateBaseType(entityType.BaseType, stringBuilder);

                    GenerateProperties(entityType.GetDeclaredProperties(), stringBuilder);

                    GenerateKeys(entityType.GetDeclaredKeys(), entityType.FindDeclaredPrimaryKey(), stringBuilder);

                    GenerateIndexes(entityType.GetDeclaredIndexes(), stringBuilder);

                    GenerateEntityTypeAnnotations(entityType, stringBuilder);
                }

                stringBuilder
                    .AppendLine()
                    .AppendLine("});");
            }
        }

        protected virtual void GenerateRelationships(
            [NotNull] string builderName,
            [NotNull] IEntityType entityType,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(builderName, nameof(builderName));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .Append(builderName)
                .Append(".Entity(")
                .Append(_code.Literal(entityType.Name))
                .AppendLine(", b =>");

            using (stringBuilder.Indent())
            {
                stringBuilder.Append("{");

                using (stringBuilder.Indent())
                {
                    GenerateForeignKeys(entityType.GetDeclaredForeignKeys(), stringBuilder);
                }

                stringBuilder
                    .AppendLine()
                    .AppendLine("});");
            }
        }

        protected virtual void GenerateBaseType([CanBeNull] IEntityType baseType, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            if (baseType != null)
            {
                stringBuilder
                    .AppendLine()
                    .Append("b.HasBaseType(")
                    .Append(_code.Literal(baseType.Name))
                    .AppendLine(");");
            }
        }

        protected virtual void GenerateProperties(
            [NotNull] IEnumerable<IProperty> properties, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var firstProperty = true;
            foreach (var property in properties)
            {
                if (!firstProperty)
                {
                    stringBuilder.AppendLine();
                }
                else
                {
                    firstProperty = false;
                }

                GenerateProperty(property, stringBuilder);
            }
        }

        protected virtual void GenerateProperty(
            [NotNull] IProperty property, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .AppendLine()
                .Append("b.Property<")
                .Append(_code.Reference(property.ClrType.UnwrapEnumType()))
                .Append(">(")
                .Append(_code.Literal(property.Name))
                .Append(")");

            using (stringBuilder.Indent())
            {
                if (property.IsConcurrencyToken)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".IsConcurrencyToken()");
                }

                if (property.IsNullable != (property.ClrType.IsNullableType() && !property.IsPrimaryKey()))
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".IsRequired()");
                }

                if (property.ValueGenerated != ValueGenerated.Never)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(
                            property.ValueGenerated == ValueGenerated.OnAdd
                                ? ".ValueGeneratedOnAdd()"
                                : ".ValueGeneratedOnAddOrUpdate()");
                }

                GeneratePropertyAnnotations(property, stringBuilder);
            }

            stringBuilder.Append(";");
        }

        protected virtual void GeneratePropertyAnnotations([NotNull] IProperty property, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            GenerateAnnotations(property.Annotations.ToArray(), stringBuilder);
        }

        protected virtual void GenerateKeys(
            [NotNull] IEnumerable<IKey> keys, [CanBeNull] IKey primaryKey, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(keys, nameof(keys));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            if (primaryKey != null)
            {
                GenerateKey(primaryKey, stringBuilder, primary: true);
            }

            var firstKey = true;
            foreach (var key in keys.Where(key => key != primaryKey && !key.EntityType.Model.FindReferencingForeignKeys(key).Any()))
            {
                if (!firstKey)
                {
                    stringBuilder.AppendLine();
                }
                else
                {
                    firstKey = false;
                }

                GenerateKey(key, stringBuilder, primary: false);
            }
        }

        protected virtual void GenerateKey(
            [NotNull] IKey key, [NotNull] IndentedStringBuilder stringBuilder, bool primary = false)
        {
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .AppendLine()
                .AppendLine()
                .Append(primary ? "b.HasKey(" : "b.HasAlternateKey(")
                .Append(string.Join(", ", key.Properties.Select(p => _code.Literal(p.Name))))
                .Append(")");

            using (stringBuilder.Indent())
            {
                GenerateAnnotations(key.Annotations.ToArray(), stringBuilder);
            }

            stringBuilder.Append(";");
        }

        protected virtual void GenerateIndexes(
            [NotNull] IEnumerable<IIndex> indexes, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(indexes, nameof(indexes));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var index in indexes)
            {
                stringBuilder.AppendLine();
                GenerateIndex(index, stringBuilder);
            }
        }

        protected virtual void GenerateIndex(
            [NotNull] IIndex index, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .AppendLine()
                .Append("b.HasIndex(")
                .Append(string.Join(", ", index.Properties.Select(p => _code.Literal(p.Name))))
                .Append(")");

            using (stringBuilder.Indent())
            {
                if (index.IsUnique)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".IsUnique()");
                }

                GenerateAnnotations(index.Annotations.ToArray(), stringBuilder);
            }

            stringBuilder.Append(";");
        }

        protected virtual void GenerateEntityTypeAnnotations([NotNull] IEntityType entityType, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var annotations = entityType.Annotations.ToArray();
            if (annotations.Any())
            {
                foreach (var annotation in annotations)
                {
                    stringBuilder
                        .AppendLine()
                        .AppendLine()
                        .Append("b");

                    GenerateAnnotation(annotation, stringBuilder);

                    stringBuilder.Append(";");
                }
            }
        }

        protected virtual void GenerateForeignKeys(
            [NotNull] IEnumerable<IForeignKey> foreignKeys, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(foreignKeys, nameof(foreignKeys));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var firstForeignKey = true;
            foreach (var foreignKey in foreignKeys)
            {
                if (!firstForeignKey)
                {
                    stringBuilder.AppendLine();
                }
                else
                {
                    firstForeignKey = false;
                }

                GenerateForeignKey(foreignKey, stringBuilder);
            }
        }

        protected virtual void GenerateForeignKey(
            [NotNull] IForeignKey foreignKey, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .AppendLine()
                .Append("b.HasOne(")
                .Append(_code.Literal(foreignKey.PrincipalEntityType.Name))
                .Append(")")
                .AppendLine();

            using (stringBuilder.Indent())
            {
                if (foreignKey.IsUnique)
                {
                    stringBuilder
                        .AppendLine(".WithOne()")
                        .Append(".HasForeignKey(")
                        .Append(_code.Literal(foreignKey.DeclaringEntityType.Name))
                        .Append(", ")
                        .Append(string.Join(", ", foreignKey.Properties.Select(p => _code.Literal(p.Name))))
                        .Append(")");

                    GenerateForeignKeyAnnotations(foreignKey, stringBuilder);

                    if (foreignKey.PrincipalKey != foreignKey.PrincipalEntityType.GetPrimaryKey())
                    {
                        stringBuilder
                            .AppendLine()
                            .Append(".HasPrincipalKey(")
                            .Append(_code.Literal(foreignKey.PrincipalEntityType.Name))
                            .Append(", ")
                            .Append(string.Join(", ", foreignKey.PrincipalKey.Properties.Select(p => _code.Literal(p.Name))))
                            .Append(")");
                    }
                }
                else
                {
                    stringBuilder
                        .AppendLine(".WithMany()")
                        .Append(".HasForeignKey(")
                        .Append(string.Join(", ", foreignKey.Properties.Select(p => _code.Literal(p.Name))))
                        .Append(")");

                    GenerateForeignKeyAnnotations(foreignKey, stringBuilder);

                    if (foreignKey.PrincipalKey != foreignKey.PrincipalEntityType.GetPrimaryKey())
                    {
                        stringBuilder
                            .AppendLine()
                            .Append(".HasPrincipalKey(")
                            .Append(string.Join(", ", foreignKey.PrincipalKey.Properties.Select(p => _code.Literal(p.Name))))
                            .Append(")");
                    }
                }
            }

            stringBuilder.Append(";");
        }

        protected virtual void GenerateForeignKeyAnnotations([NotNull] IForeignKey foreignKey, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            GenerateAnnotations(foreignKey.Annotations.ToArray(), stringBuilder);
        }

        protected virtual void GenerateAnnotations(
            [NotNull] IReadOnlyList<IAnnotation> annotations, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var annotation in annotations.Where(annotation => !MigrationsCodeGenerator.IgnoredAnnotations.Contains(annotation.Name)))
            {
                stringBuilder.AppendLine();
                GenerateAnnotation(annotation, stringBuilder);
            }
        }

        protected virtual void GenerateAnnotation(
            [NotNull] IAnnotation annotation, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .Append(".HasAnnotation(")
                .Append(_code.Literal(annotation.Name))
                .Append(", ")
                .Append(_code.UnknownLiteral(annotation.Value))
                .Append(")");
        }
    }
}
