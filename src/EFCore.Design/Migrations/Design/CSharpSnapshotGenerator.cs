// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    public class CSharpSnapshotGenerator : ICSharpSnapshotGenerator
    {
        public CSharpSnapshotGenerator([NotNull] CSharpSnapshotGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual CSharpSnapshotGeneratorDependencies Dependencies { get; }

        private ICSharpHelper Code => Dependencies.CSharpHelper;

        public virtual void Generate(string builderName, IModel model, IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(builderName, nameof(builderName));
            Check.NotNull(model, nameof(model));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var annotations = model.GetAnnotations().ToList();

            if (annotations.Any())
            {
                stringBuilder.Append(builderName);

                using (stringBuilder.Indent())
                {
                    GenerateFluentApiForAnnotation(ref annotations, RelationalAnnotationNames.DefaultSchema, nameof(RelationalModelBuilderExtensions.HasDefaultSchema), stringBuilder);

                    GenerateAnnotations(annotations, stringBuilder);
                }

                stringBuilder.AppendLine(";");
            }

            GenerateEntityTypes(builderName, Sort(model.GetEntityTypes().ToList()), stringBuilder);
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

            foreach (var entityType in entityTypes.Where(e => e.GetDeclaredForeignKeys().Any()))
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
                .Append(Code.Literal(entityType.Name))
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
                .Append(Code.Literal(entityType.Name))
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
                    .Append(Code.Literal(baseType.Name))
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
                .Append(Code.Reference(property.ClrType.UnwrapEnumType()))
                .Append(">(")
                .Append(Code.Literal(property.Name))
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
                                : property.ValueGenerated == ValueGenerated.OnUpdate
                                    ? ".ValueGeneratedOnUpdate()"
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

            var annotations = property.GetAnnotations().ToList();

            GenerateFluentApiForAnnotation(ref annotations, RelationalAnnotationNames.ColumnName, nameof(RelationalPropertyBuilderExtensions.HasColumnName), stringBuilder);
            GenerateFluentApiForAnnotation(ref annotations, RelationalAnnotationNames.ColumnType, nameof(RelationalPropertyBuilderExtensions.HasColumnType), stringBuilder);
            GenerateFluentApiForAnnotation(ref annotations, RelationalAnnotationNames.DefaultValueSql, nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql), stringBuilder);
            GenerateFluentApiForAnnotation(ref annotations, RelationalAnnotationNames.ComputedColumnSql, nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql), stringBuilder);
            GenerateFluentApiForAnnotation(ref annotations, RelationalAnnotationNames.DefaultValue, nameof(RelationalPropertyBuilderExtensions.HasDefaultValue), stringBuilder);
            GenerateFluentApiForAnnotation(ref annotations, CoreAnnotationNames.MaxLengthAnnotation, nameof(PropertyBuilder.HasMaxLength), stringBuilder);
            GenerateFluentApiForAnnotation(ref annotations, CoreAnnotationNames.UnicodeAnnotation, nameof(PropertyBuilder.IsUnicode), stringBuilder);

            IgnoreAnnotations(
                annotations, 
                CoreAnnotationNames.ValueGeneratorFactoryAnnotation,
                RelationalAnnotationNames.TypeMapping);

            GenerateAnnotations(annotations, stringBuilder);
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
            foreach (var key in keys.Where(key => key != primaryKey && !key.GetReferencingForeignKeys().Any()))
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
                .Append(string.Join(", ", key.Properties.Select(p => Code.Literal(p.Name))))
                .Append(")");

            using (stringBuilder.Indent())
            {
                var annotations = key.GetAnnotations().ToList();

                GenerateFluentApiForAnnotation(ref annotations, RelationalAnnotationNames.Name, nameof(RelationalKeyBuilderExtensions.HasName), stringBuilder);

                GenerateAnnotations(annotations, stringBuilder);
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
                .Append($"b.{nameof(EntityTypeBuilder.HasIndex)}(")
                .Append(string.Join(", ", index.Properties.Select(p => Code.Literal(p.Name))))
                .Append(")");

            using (stringBuilder.Indent())
            {
                if (index.IsUnique)
                {
                    stringBuilder
                        .AppendLine()
                        .Append($".{nameof(IndexBuilder.IsUnique)}()");
                }

                var annotations = index.GetAnnotations().ToList();

                GenerateFluentApiForAnnotation(ref annotations, RelationalAnnotationNames.Name, nameof(RelationalIndexBuilderExtensions.HasName), stringBuilder);
                GenerateFluentApiForAnnotation(ref annotations, RelationalAnnotationNames.Filter, nameof(RelationalIndexBuilderExtensions.HasFilter), stringBuilder);

                GenerateAnnotations(annotations, stringBuilder);
            }

            stringBuilder.Append(";");
        }

        protected virtual void GenerateEntityTypeAnnotations([NotNull] IEntityType entityType, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var annotations = entityType.GetAnnotations().ToList();
            var tableNameAnnotation = annotations.FirstOrDefault(a => a.Name == RelationalAnnotationNames.TableName);
            var schemaAnnotation = annotations.FirstOrDefault(a => a.Name == RelationalAnnotationNames.Schema);

            stringBuilder
                .AppendLine()
                .AppendLine()
                .Append("b.")
                .Append(nameof(RelationalEntityTypeBuilderExtensions.ToTable))
                .Append("(")
                .Append(Code.Literal((string)tableNameAnnotation?.Value ?? entityType.DisplayName()));
            annotations.Remove(tableNameAnnotation);

            if (schemaAnnotation?.Value != null)
            {
                stringBuilder
                    .Append(",")
                    .Append(Code.Literal((string)schemaAnnotation.Value));
                annotations.Remove(schemaAnnotation);
            }

            stringBuilder.Append(");");

            var discriminatorPropertyAnnotation = annotations.FirstOrDefault(a => a.Name == RelationalAnnotationNames.DiscriminatorProperty);
            var discriminatorValueAnnotation = annotations.FirstOrDefault(a => a.Name == RelationalAnnotationNames.DiscriminatorValue);

            if ((discriminatorPropertyAnnotation ?? discriminatorValueAnnotation) != null)
            {
                stringBuilder
                    .AppendLine()
                    .AppendLine()
                    .Append("b.")
                    .Append(nameof(RelationalEntityTypeBuilderExtensions.HasDiscriminator));

                if (discriminatorPropertyAnnotation?.Value != null)
                {
                    var propertyClrType = entityType.FindProperty((string)discriminatorPropertyAnnotation.Value)?.ClrType;
                    stringBuilder
                        .Append("<")
                        .Append(Code.Reference(propertyClrType.UnwrapEnumType()))
                        .Append(">(")
                        .Append(Code.UnknownLiteral(discriminatorPropertyAnnotation.Value))
                        .Append(")");
                }
                else
                {
                    stringBuilder
                        .Append("()");
                }

                if (discriminatorValueAnnotation?.Value != null)
                {
                    stringBuilder
                        .Append(".")
                        .Append(nameof(DiscriminatorBuilder.HasValue))
                        .Append("(")
                        .Append(Code.UnknownLiteral(discriminatorValueAnnotation.Value))
                        .Append(")");
                }

                stringBuilder.Append(";");

                annotations.Remove(discriminatorPropertyAnnotation);
                annotations.Remove(discriminatorValueAnnotation);
            }

            IgnoreAnnotations(
                annotations,
                RelationshipDiscoveryConvention.NavigationCandidatesAnnotationName,
                RelationshipDiscoveryConvention.AmbiguousNavigationsAnnotationName,
                InversePropertyAttributeConvention.InverseNavigationsAnnotationName);

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
                .Append(Code.Literal(foreignKey.PrincipalEntityType.Name));

            if (foreignKey.DependentToPrincipal != null)
            {
                stringBuilder
                    .Append(", ")
                    .Append(Code.Literal(foreignKey.DependentToPrincipal.Name));
            }

            stringBuilder
                .Append(")")
                .AppendLine();

            using (stringBuilder.Indent())
            {
                if (foreignKey.IsUnique)
                {
                    stringBuilder
                        .Append(".WithOne(");

                    if (foreignKey.PrincipalToDependent != null)
                    {
                        stringBuilder
                            .Append(Code.Literal(foreignKey.PrincipalToDependent.Name));
                    }

                    stringBuilder
                        .AppendLine(")")
                        .Append(".HasForeignKey(")
                        .Append(Code.Literal(foreignKey.DeclaringEntityType.Name))
                        .Append(", ")
                        .Append(string.Join(", ", foreignKey.Properties.Select(p => Code.Literal(p.Name))))
                        .Append(")");

                    GenerateForeignKeyAnnotations(foreignKey, stringBuilder);

                    if (foreignKey.PrincipalKey != foreignKey.PrincipalEntityType.FindPrimaryKey())
                    {
                        stringBuilder
                            .AppendLine()
                            .Append(".HasPrincipalKey(")
                            .Append(Code.Literal(foreignKey.PrincipalEntityType.Name))
                            .Append(", ")
                            .Append(string.Join(", ", foreignKey.PrincipalKey.Properties.Select(p => Code.Literal(p.Name))))
                            .Append(")");
                    }
                }
                else
                {
                    stringBuilder
                        .Append(".WithMany(");

                    if (foreignKey.PrincipalToDependent != null)
                    {
                        stringBuilder
                            .Append(Code.Literal(foreignKey.PrincipalToDependent.Name));
                    }

                    stringBuilder
                        .AppendLine(")")
                        .Append(".HasForeignKey(")
                        .Append(string.Join(", ", foreignKey.Properties.Select(p => Code.Literal(p.Name))))
                        .Append(")");

                    GenerateForeignKeyAnnotations(foreignKey, stringBuilder);

                    if (foreignKey.PrincipalKey != foreignKey.PrincipalEntityType.FindPrimaryKey())
                    {
                        stringBuilder
                            .AppendLine()
                            .Append(".HasPrincipalKey(")
                            .Append(string.Join(", ", foreignKey.PrincipalKey.Properties.Select(p => Code.Literal(p.Name))))
                            .Append(")");
                    }
                }

                if (foreignKey.DeleteBehavior != DeleteBehavior.ClientSetNull)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".OnDelete(")
                        .Append(Code.Literal(foreignKey.DeleteBehavior))
                        .Append(")");
                }
            }

            stringBuilder.Append(";");
        }

        protected virtual void GenerateForeignKeyAnnotations([NotNull] IForeignKey foreignKey, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var annotations = foreignKey.GetAnnotations().ToList();

            GenerateFluentApiForAnnotation(ref annotations,
                RelationalAnnotationNames.Name,
                foreignKey.IsUnique
                    ? nameof(RelationalReferenceReferenceBuilderExtensions.HasConstraintName)
                    : nameof(RelationalReferenceCollectionBuilderExtensions.HasConstraintName),
                stringBuilder);

            GenerateAnnotations(annotations, stringBuilder);
        }

        protected virtual void IgnoreAnnotations(
            [NotNull] IList<IAnnotation> annotations, [NotNull] params string[] annotationNames)
        {
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(annotationNames, nameof(annotationNames));

            foreach (var annotationName in annotationNames)
            {
                var annotation = annotations.FirstOrDefault(a => a.Name == annotationName);
                if (annotation != null)
                {
                    annotations.Remove(annotation);
                }
            }
        }

        protected virtual void GenerateAnnotations(
            [NotNull] IReadOnlyList<IAnnotation> annotations, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var annotation in annotations)
            {
                stringBuilder.AppendLine();
                GenerateAnnotation(annotation, stringBuilder);
            }
        }

        protected virtual void GenerateFluentApiForAnnotation(
            [NotNull] ref List<IAnnotation> annotations,
            [NotNull] string annotationName,
            [NotNull] string fluentApiMethodName,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            var annotation = annotations.FirstOrDefault(a => a.Name == annotationName);

            if (annotation?.Value != null)
            {
                stringBuilder
                    .AppendLine()
                    .Append(".")
                    .Append(fluentApiMethodName)
                    .Append("(")
                    .Append(Code.UnknownLiteral(annotation.Value))
                    .Append(")");

                annotations.Remove(annotation);
            }
        }

        protected virtual void GenerateAnnotation(
            [NotNull] IAnnotation annotation, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .Append(".HasAnnotation(")
                .Append(Code.Literal(annotation.Name))
                .Append(", ")
                .Append(Code.UnknownLiteral(annotation.Value))
                .Append(")");
        }
    }
}
