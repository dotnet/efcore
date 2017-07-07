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

                    IgnoreAnnotationTypes(annotations, RelationalAnnotationNames.DbFunction);

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

            foreach (var entityType in entityTypes.Where(e => !e.HasDefiningNavigation()))
            {
                stringBuilder.AppendLine();

                GenerateEntityType(builderName, entityType, stringBuilder);
            }

            foreach (var entityType in entityTypes.Where(e =>
                !e.HasDefiningNavigation()
                && (e.GetDeclaredForeignKeys().Any()
                    || e.GetDeclaredReferencingForeignKeys().Any(fk => fk.IsOwnership))))
            {
                stringBuilder.AppendLine();

                GenerateEntityTypeRelationships(builderName, entityType, stringBuilder);
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
                .Append(entityType.HasDefiningNavigation()
                    ? ".OwnsOne("
                    : ".Entity(")
                .Append(Code.Literal(entityType.Name));

            if (entityType.HasDefiningNavigation())
            {
                stringBuilder
                    .Append(", ")
                    .Append(Code.Literal(entityType.DefiningNavigationName));
            }

            if (builderName.StartsWith("b", StringComparison.Ordinal))
            {
                var counter = 1;
                if (builderName.Length > 1
                    && int.TryParse(builderName.Substring(1, builderName.Length - 1), out counter))
                {
                    counter++;
                }

                builderName = "b" + (counter == 0 ? "" : counter.ToString());
            }
            else
            {
                builderName = "b";
            }

            stringBuilder
                .Append(", ")
                .Append(builderName)
                .AppendLine(" =>");

            using (stringBuilder.Indent())
            {
                stringBuilder.Append("{");

                using (stringBuilder.Indent())
                {
                    GenerateBaseType(builderName, entityType.BaseType, stringBuilder);

                    GenerateProperties(builderName, entityType.GetDeclaredProperties(), stringBuilder);

                    if (!entityType.HasDefiningNavigation())
                    {
                        GenerateKeys(builderName, entityType.GetDeclaredKeys(), entityType.FindDeclaredPrimaryKey(), stringBuilder);
                    }

                    GenerateIndexes(builderName, entityType.GetDeclaredIndexes(), stringBuilder);

                    GenerateEntityTypeAnnotations(builderName, entityType, stringBuilder);

                    if (entityType.HasDefiningNavigation())
                    {
                        GenerateRelationships(builderName, entityType, stringBuilder);
                    }
                }

                stringBuilder
                    .AppendLine("});");
            }
        }

        protected virtual void GenerateOwnedTypes(
            [NotNull] string builderName,
            [NotNull] IEnumerable<IForeignKey> ownerships,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(ownerships, nameof(ownerships));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var ownership in ownerships)
            {
                stringBuilder.AppendLine();

                GenerateOwnedType(builderName, ownership, stringBuilder);
            }
        }

        protected virtual void GenerateOwnedType(
            [NotNull] string builderName,
            [NotNull] IForeignKey ownership,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(ownership, nameof(ownership));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            GenerateEntityType(builderName, ownership.DeclaringEntityType, stringBuilder);
        }

        protected virtual void GenerateEntityTypeRelationships(
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
                    GenerateRelationships("b", entityType, stringBuilder);
                }

                stringBuilder.AppendLine("});");
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

            GenerateForeignKeys(builderName, entityType.GetDeclaredForeignKeys(), stringBuilder);
            
            GenerateOwnedTypes(builderName, entityType.GetDeclaredReferencingForeignKeys().Where(fk => fk.IsOwnership), stringBuilder);
        }

        protected virtual void GenerateBaseType(
            [NotNull] string builderName,
            [CanBeNull] IEntityType baseType,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            if (baseType != null)
            {
                stringBuilder
                    .AppendLine()
                    .Append(builderName)
                    .Append(".HasBaseType(")
                    .Append(Code.Literal(baseType.Name))
                    .AppendLine(");");
            }
        }

        protected virtual void GenerateProperties(
            [NotNull] string builderName,
            [NotNull] IEnumerable<IProperty> properties,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
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

                GenerateProperty(builderName, property, stringBuilder);
            }
        }

        protected virtual void GenerateProperty(
            [NotNull] string builderName,
            [NotNull] IProperty property,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(property, nameof(property));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .AppendLine()
                .Append(builderName)
                .Append(".Property<")
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
            [NotNull] string builderName,
            [NotNull] IEnumerable<IKey> keys,
            [CanBeNull] IKey primaryKey,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(keys, nameof(keys));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            if (primaryKey != null)
            {
                GenerateKey(builderName, primaryKey, stringBuilder, primary: true);
            }

            var firstKey = true;
            foreach (var key in keys.Where(key => key != primaryKey
                                                  && (!key.GetReferencingForeignKeys().Any()
                                                      || key.GetAnnotations().Any())))
            {
                if (!firstKey)
                {
                    stringBuilder.AppendLine();
                }
                else
                {
                    firstKey = false;
                }

                GenerateKey(builderName, key, stringBuilder);
            }
        }

        protected virtual void GenerateKey(
            [NotNull] string builderName,
            [NotNull] IKey key,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool primary = false)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(key, nameof(key));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .AppendLine()
                .AppendLine()
                .Append(builderName)
                .Append(primary ? ".HasKey(" : ".HasAlternateKey(")
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
            [NotNull] string builderName,
            [NotNull] IEnumerable<IIndex> indexes,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(indexes, nameof(indexes));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var index in indexes)
            {
                stringBuilder.AppendLine();
                GenerateIndex(builderName, index, stringBuilder);
            }
        }

        protected virtual void GenerateIndex(
            [NotNull] string builderName,
            [NotNull] IIndex index,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(index, nameof(index));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .AppendLine()
                .Append(builderName)
                .Append(".HasIndex(")
                .Append(string.Join(", ", index.Properties.Select(p => Code.Literal(p.Name))))
                .Append(")");

            using (stringBuilder.Indent())
            {
                if (index.IsUnique)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".IsUnique()");
                }

                var annotations = index.GetAnnotations().ToList();

                GenerateFluentApiForAnnotation(ref annotations, RelationalAnnotationNames.Name, nameof(RelationalIndexBuilderExtensions.HasName), stringBuilder);
                GenerateFluentApiForAnnotation(ref annotations, RelationalAnnotationNames.Filter, nameof(RelationalIndexBuilderExtensions.HasFilter), stringBuilder);

                GenerateAnnotations(annotations, stringBuilder);
            }

            stringBuilder.Append(";");
        }

        protected virtual void GenerateEntityTypeAnnotations(
            [NotNull] string builderName,
            [NotNull] IEntityType entityType,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var annotations = entityType.GetAnnotations().ToList();
            var tableNameAnnotation = annotations.FirstOrDefault(a => a.Name == RelationalAnnotationNames.TableName);
            var schemaAnnotation = annotations.FirstOrDefault(a => a.Name == RelationalAnnotationNames.Schema);

            stringBuilder
                .AppendLine()
                .AppendLine()
                .Append(builderName)
                .Append(".")
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

            stringBuilder.AppendLine(");");

            var discriminatorPropertyAnnotation = annotations.FirstOrDefault(a => a.Name == RelationalAnnotationNames.DiscriminatorProperty);
            var discriminatorValueAnnotation = annotations.FirstOrDefault(a => a.Name == RelationalAnnotationNames.DiscriminatorValue);

            if ((discriminatorPropertyAnnotation ?? discriminatorValueAnnotation) != null)
            {
                stringBuilder
                    .AppendLine()
                    .Append(builderName)
                    .Append(".")
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

                stringBuilder.AppendLine(";");

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
                        .Append(builderName);

                    GenerateAnnotation(annotation, stringBuilder);

                    stringBuilder
                        .Append(";")
                        .AppendLine();
                }
            }
        }

        protected virtual void GenerateForeignKeys(
            [NotNull] string builderName,
            [NotNull] IEnumerable<IForeignKey> foreignKeys,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(foreignKeys, nameof(foreignKeys));
            Check.NotNull(stringBuilder, nameof(stringBuilder));
            
            foreach (var foreignKey in foreignKeys)
            {
                stringBuilder.AppendLine();

                GenerateForeignKey(builderName, foreignKey, stringBuilder);
            }
        }

        protected virtual void GenerateForeignKey(
            [NotNull] string builderName,
            [NotNull] IForeignKey foreignKey,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .Append(builderName)
                .Append(".HasOne(")
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

            stringBuilder.AppendLine(";");
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

        protected virtual void IgnoreAnnotationTypes(
            [NotNull] IList<IAnnotation> annotations, [NotNull] params string[] annotationPrefixes)
        {
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(annotationPrefixes, nameof(annotationPrefixes));

            foreach (var ignoreAnnotation in annotations.Where(a => annotationPrefixes.Any(pre => a.Name.StartsWith(pre, StringComparison.OrdinalIgnoreCase))).ToList())
            {
                annotations.Remove(ignoreAnnotation);
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
