// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     Used to generate C# code for creating an <see cref="IModel" />.
    /// </summary>
    public class CSharpSnapshotGenerator : ICSharpSnapshotGenerator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CSharpSnapshotGenerator" /> class.
        /// </summary>
        /// <param name="dependencies"> The dependencies. </param>
        public CSharpSnapshotGenerator(CSharpSnapshotGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual CSharpSnapshotGeneratorDependencies Dependencies { get; }

        private ICSharpHelper Code
            => Dependencies.CSharpHelper;

        /// <summary>
        ///     Generates code for creating an <see cref="IModel" />.
        /// </summary>
        /// <param name="builderName"> The <see cref="ModelBuilder" /> variable name. </param>
        /// <param name="model"> The model. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        public virtual void Generate(string builderName, IModel model, IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(builderName, nameof(builderName));
            Check.NotNull(model, nameof(model));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var annotations = Dependencies.AnnotationCodeGenerator
                .FilterIgnoredAnnotations(model.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);

            var productVersion = model.GetProductVersion();

            if (annotations.Count > 0 || productVersion != null)
            {
                stringBuilder.Append(builderName);

                using (stringBuilder.Indent())
                {
                    // Temporary patch: specifically exclude some annotations which are known to produce identical Fluent API calls across different
                    // providers, generating them as raw annotations instead.
                    var ambiguousAnnotations = RemoveAmbiguousFluentApiAnnotations(
                        annotations,
                        name => name.EndsWith(":ValueGenerationStrategy", StringComparison.Ordinal)
                            || name.EndsWith(":IdentityIncrement", StringComparison.Ordinal)
                            || name.EndsWith(":IdentitySeed", StringComparison.Ordinal)
                            || name.EndsWith(":HiLoSequenceName", StringComparison.Ordinal)
                            || name.EndsWith(":HiLoSequenceSchema", StringComparison.Ordinal));

                    foreach (var methodCallCodeFragment in
                        Dependencies.AnnotationCodeGenerator.GenerateFluentApiCalls(model, annotations))
                    {
                        stringBuilder
                            .AppendLine()
                            .Append(Code.Fragment(methodCallCodeFragment));
                    }

                    IEnumerable<IAnnotation> remainingAnnotations = annotations.Values;
                    if (productVersion != null)
                    {
                        remainingAnnotations = remainingAnnotations.Append(
                            new Annotation(CoreAnnotationNames.ProductVersion, productVersion));
                    }

                    GenerateAnnotations(remainingAnnotations.Concat(ambiguousAnnotations), stringBuilder);
                }

                stringBuilder.AppendLine(";");
            }

            foreach (var sequence in model.GetSequences())
            {
                GenerateSequence(builderName, sequence, stringBuilder);
            }

            GenerateEntityTypes(builderName, model.GetEntityTypesInHierarchicalOrder(), stringBuilder);
        }

        /// <summary>
        ///     Generates code for <see cref="IEntityType" /> objects.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="entityTypes"> The entity types. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateEntityTypes(
            string builderName,
            IReadOnlyList<IEntityType> entityTypes,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(builderName, nameof(builderName));
            Check.NotNull(entityTypes, nameof(entityTypes));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var entityType in entityTypes.Where(
                e => e.FindOwnership() == null))
            {
                stringBuilder.AppendLine();

                GenerateEntityType(builderName, entityType, stringBuilder);
            }

            foreach (var entityType in entityTypes.Where(
                e => e.FindOwnership() == null
                    && (e.GetDeclaredForeignKeys().Any()
                        || e.GetDeclaredReferencingForeignKeys().Any(fk => fk.IsOwnership))))
            {
                stringBuilder.AppendLine();

                GenerateEntityTypeRelationships(builderName, entityType, stringBuilder);
            }

            foreach (var entityType in entityTypes.Where(
                e => e.FindOwnership() == null
                    && e.GetDeclaredNavigations().Any(n => !n.IsOnDependent && !n.ForeignKey.IsOwnership)))
            {
                stringBuilder.AppendLine();

                GenerateEntityTypeNavigations(builderName, entityType, stringBuilder);
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="IEntityType" />.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateEntityType(
            string builderName,
            IEntityType entityType,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(builderName, nameof(builderName));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var ownership = entityType.FindOwnership();
            var ownerNavigation = ownership?.PrincipalToDependent!.Name;

            var entityTypeName = entityType.Name;
            if (ownerNavigation != null
                && entityType.HasSharedClrType
                && entityTypeName == ownership!.PrincipalEntityType.GetOwnedName(entityType.ClrType.ShortDisplayName(), ownerNavigation))
            {
                entityTypeName = entityType.ClrType.DisplayName();
            }

            stringBuilder
                .Append(builderName)
                .Append(
                    ownerNavigation != null
                        ? ownership!.IsUnique ? ".OwnsOne(" : ".OwnsMany("
                        : ".Entity(")
                .Append(Code.Literal(entityTypeName));

            if (ownerNavigation != null)
            {
                stringBuilder
                    .Append(", ")
                    .Append(Code.Literal(ownerNavigation));
            }

            if (builderName.StartsWith("b", StringComparison.Ordinal))
            {
                // ReSharper disable once InlineOutVariableDeclaration
                var counter = 1;
                if (builderName.Length > 1
                    && int.TryParse(builderName[1..], out counter))
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

                    GenerateKeys(
                        builderName,
                        entityType.GetDeclaredKeys(),
                        entityType.BaseType == null ? entityType.FindPrimaryKey() : null,
                        stringBuilder);

                    GenerateIndexes(builderName, entityType.GetDeclaredIndexes(), stringBuilder);

                    GenerateEntityTypeAnnotations(builderName, entityType, stringBuilder);

                    GenerateCheckConstraints(builderName, entityType, stringBuilder);

                    if (ownerNavigation != null)
                    {
                        GenerateRelationships(builderName, entityType, stringBuilder);

                        GenerateNavigations(
                            builderName, entityType.GetDeclaredNavigations()
                                .Where(n => !n.IsOnDependent && !n.ForeignKey.IsOwnership), stringBuilder);
                    }

                    GenerateData(builderName, entityType.GetProperties(), entityType.GetSeedData(providerValues: true), stringBuilder);
                }

                stringBuilder
                    .AppendLine("});");
            }
        }

        /// <summary>
        ///     Generates code for owned entity types.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="ownerships"> The foreign keys identifying each entity type. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateOwnedTypes(
            string builderName,
            IEnumerable<IForeignKey> ownerships,
            IndentedStringBuilder stringBuilder)
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

        /// <summary>
        ///     Generates code for an owned entity types.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="ownership"> The foreign key identifying the entity type. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateOwnedType(
            string builderName,
            IForeignKey ownership,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(ownership, nameof(ownership));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            GenerateEntityType(builderName, ownership.DeclaringEntityType, stringBuilder);
        }

        /// <summary>
        ///     Generates code for the relationships of an <see cref="IEntityType" />.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateEntityTypeRelationships(
            string builderName,
            IEntityType entityType,
            IndentedStringBuilder stringBuilder)
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

        /// <summary>
        ///     Generates code for the relationships of an <see cref="IEntityType" />.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateRelationships(
            string builderName,
            IEntityType entityType,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(builderName, nameof(builderName));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            GenerateForeignKeys(builderName, entityType.GetDeclaredForeignKeys(), stringBuilder);

            GenerateOwnedTypes(builderName, entityType.GetDeclaredReferencingForeignKeys().Where(fk => fk.IsOwnership), stringBuilder);

            GenerateNavigations(
                builderName, entityType.GetDeclaredNavigations()
                    .Where(n => n.IsOnDependent || (!n.IsOnDependent && n.ForeignKey.IsOwnership)), stringBuilder);
        }

        /// <summary>
        ///     Generates code for the base type of an <see cref="IEntityType" />.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="baseType"> The base entity type. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateBaseType(
            string builderName,
            IEntityType? baseType,
            IndentedStringBuilder stringBuilder)
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

        /// <summary>
        ///     Generates code for an <see cref="ISequence" />.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="sequence"> The sequence. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateSequence(
            string builderName,
            ISequence sequence,
            IndentedStringBuilder stringBuilder)
        {
            stringBuilder
                .AppendLine()
                .Append(builderName)
                .Append(".HasSequence");

            if (sequence.Type != Sequence.DefaultClrType)
            {
                stringBuilder
                    .Append("<")
                    .Append(Code.Reference(sequence.Type))
                    .Append(">");
            }

            stringBuilder
                .Append("(")
                .Append(Code.Literal(sequence.Name));

            if (!string.IsNullOrEmpty(sequence.Schema)
                && sequence.Model.GetDefaultSchema() != sequence.Schema)
            {
                stringBuilder
                    .Append(", ")
                    .Append(Code.Literal(sequence.Schema));
            }

            stringBuilder.Append(")");

            using (stringBuilder.Indent())
            {
                if (sequence.StartValue != Sequence.DefaultStartValue)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".StartsAt(")
                        .Append(Code.Literal(sequence.StartValue))
                        .Append(")");
                }

                if (sequence.IncrementBy != Sequence.DefaultIncrementBy)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".IncrementsBy(")
                        .Append(Code.Literal(sequence.IncrementBy))
                        .Append(")");
                }

                if (sequence.MinValue != Sequence.DefaultMinValue)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".HasMin(")
                        .Append(Code.Literal(sequence.MinValue))
                        .Append(")");
                }

                if (sequence.MaxValue != Sequence.DefaultMaxValue)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".HasMax(")
                        .Append(Code.Literal(sequence.MaxValue))
                        .Append(")");
                }

                if (sequence.IsCyclic != Sequence.DefaultIsCyclic)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".IsCyclic()");
                }
            }

            stringBuilder.AppendLine(";");
        }

        /// <summary>
        ///     Generates code for <see cref="IProperty" /> objects.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="properties"> The properties. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateProperties(
            string builderName,
            IEnumerable<IProperty> properties,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var property in properties)
            {
                GenerateProperty(builderName, property, stringBuilder);
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="IProperty" />.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="property"> The property. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateProperty(
            string builderName,
            IProperty property,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(property, nameof(property));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var clrType = FindValueConverter(property)?.ProviderClrType.MakeNullable(property.IsNullable)
                ?? property.ClrType;

            stringBuilder
                .AppendLine()
                .Append(builderName)
                .Append(".Property<")
                .Append(Code.Reference(clrType))
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

                if (property.IsNullable != (clrType.IsNullableType() && !property.IsPrimaryKey()))
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
                                    : property.ValueGenerated == ValueGenerated.OnUpdateSometimes
                                        ? ".ValueGeneratedOnUpdateSometimes()"
                                        : ".ValueGeneratedOnAddOrUpdate()");
                }

                GeneratePropertyAnnotations(property, stringBuilder);
            }

            stringBuilder.AppendLine(";");
        }

        /// <summary>
        ///     Generates code for the annotations on an <see cref="IProperty" />.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GeneratePropertyAnnotations(IProperty property, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var annotations = Dependencies.AnnotationCodeGenerator
                .FilterIgnoredAnnotations(property.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);

            GenerateFluentApiForMaxLength(property, stringBuilder);
            GenerateFluentApiForPrecisionAndScale(property, stringBuilder);
            GenerateFluentApiForIsUnicode(property, stringBuilder);

            stringBuilder
                .AppendLine()
                .Append(".")
                .Append(nameof(RelationalPropertyBuilderExtensions.HasColumnType))
                .Append("(")
                .Append(
                    Code.Literal(
                        property.GetColumnType()
                        ?? Dependencies.RelationalTypeMappingSource.GetMapping(property).StoreType))
                .Append(")");
            annotations.Remove(RelationalAnnotationNames.ColumnType);

            GenerateFluentApiForDefaultValue(property, stringBuilder);
            annotations.Remove(RelationalAnnotationNames.DefaultValue);

            // Temporary patch: specifically exclude some annotations which are known to produce identical Fluent API calls across different
            // providers, generating them as raw annotations instead.
            var ambiguousAnnotations = RemoveAmbiguousFluentApiAnnotations(
                annotations,
                name => name.EndsWith(":ValueGenerationStrategy", StringComparison.Ordinal)
                    || name.EndsWith(":IdentityIncrement", StringComparison.Ordinal)
                    || name.EndsWith(":IdentitySeed", StringComparison.Ordinal)
                    || name.EndsWith(":HiLoSequenceName", StringComparison.Ordinal)
                    || name.EndsWith(":HiLoSequenceSchema", StringComparison.Ordinal));

            foreach (var methodCallCodeFragment in
                Dependencies.AnnotationCodeGenerator.GenerateFluentApiCalls(property, annotations))
            {
                stringBuilder
                    .AppendLine()
                    .Append(Code.Fragment(methodCallCodeFragment));
            }

            GenerateAnnotations(annotations.Values.Concat(ambiguousAnnotations), stringBuilder);
        }

        private ValueConverter? FindValueConverter(IProperty property)
            => property.GetValueConverter()
                ?? (property.FindTypeMapping()
                    ?? Dependencies.RelationalTypeMappingSource.FindMapping(property))?.Converter;

        /// <summary>
        ///     Generates code for <see cref="IKey" /> objects.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="keys"> The keys. </param>
        /// <param name="primaryKey"> The primary key. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateKeys(
            string builderName,
            IEnumerable<IKey> keys,
            IKey? primaryKey,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(keys, nameof(keys));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            if (primaryKey != null)
            {
                GenerateKey(builderName, primaryKey, stringBuilder, primary: true);
            }

            if (primaryKey?.DeclaringEntityType.IsOwned() != true)
            {
                foreach (var key in keys.Where(
                    key => key != primaryKey
                        && (!key.GetReferencingForeignKeys().Any()
                            || key.GetAnnotations().Any(a => a.Name != RelationalAnnotationNames.UniqueConstraintMappings))))
                {
                    GenerateKey(builderName, key, stringBuilder);
                }
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="IKey" />.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="key"> The key. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        /// <param name="primary">A value indicating whether the key is primary. </param>
        protected virtual void GenerateKey(
            string builderName,
            IKey key,
            IndentedStringBuilder stringBuilder,
            bool primary = false)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(key, nameof(key));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .AppendLine()
                .Append(builderName)
                .Append(primary ? ".HasKey(" : ".HasAlternateKey(")
                .Append(string.Join(", ", key.Properties.Select(p => Code.Literal(p.Name))))
                .Append(")");

            using (stringBuilder.Indent())
            {
                GenerateKeyAnnotations(key, stringBuilder);
            }

            stringBuilder.AppendLine(";");
        }

        /// <summary>
        ///     Generates code for the annotations on a key.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateKeyAnnotations(IKey key, IndentedStringBuilder stringBuilder)
        {
            var annotations = Dependencies.AnnotationCodeGenerator
                .FilterIgnoredAnnotations(key.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);

            foreach (var methodCallCodeFragment in
                Dependencies.AnnotationCodeGenerator.GenerateFluentApiCalls(key, annotations))
            {
                stringBuilder
                    .AppendLine()
                    .Append(Code.Fragment(methodCallCodeFragment));
            }

            GenerateAnnotations(annotations.Values, stringBuilder);
        }

        /// <summary>
        ///     Generates code for <see cref="IIndex" /> objects.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="indexes"> The indexes. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateIndexes(
            string builderName,
            IEnumerable<IIndex> indexes,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(indexes, nameof(indexes));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var index in indexes)
            {
                GenerateIndex(builderName, index, stringBuilder);
            }
        }

        /// <summary>
        ///     Generates code an <see cref="IIndex" />.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="index"> The index. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateIndex(
            string builderName,
            IIndex index,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(index, nameof(index));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            // Note - method names below are meant to be hard-coded
            // because old snapshot files will fail if they are changed
            stringBuilder
                .AppendLine()
                .Append(builderName)
                .Append(".HasIndex(");

            if (index.Name == null)
            {
                stringBuilder
                    .Append(string.Join(", ", index.Properties.Select(p => Code.Literal(p.Name))));
            }
            else
            {
                stringBuilder
                    .Append("new[] { ")
                    .Append(string.Join(", ", index.Properties.Select(p => Code.Literal(p.Name))))
                    .Append(" }, ")
                    .Append(Code.Literal(index.Name));
            }

            stringBuilder.Append(")");

            using (stringBuilder.Indent())
            {
                if (index.IsUnique)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".IsUnique()");
                }

                GenerateIndexAnnotations(index, stringBuilder);
            }

            stringBuilder.AppendLine(";");
        }

        /// <summary>
        ///     Generates code for the annotations on an index.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateIndexAnnotations(
            IIndex index,
            IndentedStringBuilder stringBuilder)
        {
            var annotations = Dependencies.AnnotationCodeGenerator
                .FilterIgnoredAnnotations(index.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);

            // Temporary patch: specifically exclude some annotations which are known to produce identical Fluent API calls across different
            // providers, generating them as raw annotations instead.
            var ambiguousAnnotations = RemoveAmbiguousFluentApiAnnotations(
                annotations,
                name => name.EndsWith(":Include", StringComparison.Ordinal));

            foreach (var methodCallCodeFragment in
                Dependencies.AnnotationCodeGenerator.GenerateFluentApiCalls(index, annotations))
            {
                stringBuilder
                    .AppendLine()
                    .Append(Code.Fragment(methodCallCodeFragment));
            }

            GenerateAnnotations(annotations.Values.Concat(ambiguousAnnotations), stringBuilder);
        }

        /// <summary>
        ///     Generates code for the annotations on an entity type.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateEntityTypeAnnotations(
            string builderName,
            IEntityType entityType,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var annotationList = entityType.GetAnnotations().ToList();

            var discriminatorPropertyAnnotation = annotationList.FirstOrDefault(a => a.Name == CoreAnnotationNames.DiscriminatorProperty);
            var discriminatorMappingCompleteAnnotation =
                annotationList.FirstOrDefault(a => a.Name == CoreAnnotationNames.DiscriminatorMappingComplete);
            var discriminatorValueAnnotation = annotationList.FirstOrDefault(a => a.Name == CoreAnnotationNames.DiscriminatorValue);

            var annotations = Dependencies.AnnotationCodeGenerator
                .FilterIgnoredAnnotations(entityType.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);

            var tableNameAnnotation = annotations.Find(RelationalAnnotationNames.TableName);
            if (tableNameAnnotation?.Value != null
                || entityType.BaseType == null)
            {
                var tableName = (string?)tableNameAnnotation?.Value ?? entityType.GetTableName();
                if (tableName != null
                    || tableNameAnnotation != null)
                {
                    var schemaAnnotation = annotations.Find(RelationalAnnotationNames.Schema);
                    stringBuilder
                        .AppendLine()
                        .Append(builderName)
                        .Append(".ToTable(");

                    if (tableName == null
                        && schemaAnnotation == null)
                    {
                        stringBuilder.Append("(string)");
                    }

                    stringBuilder.Append(Code.UnknownLiteral(tableName));

                    if (tableNameAnnotation != null)
                    {
                        annotations.Remove(tableNameAnnotation.Name);
                    }

                    var isExcludedAnnotation = annotations.Find(RelationalAnnotationNames.IsTableExcludedFromMigrations);
                    if (schemaAnnotation != null)
                    {
                        stringBuilder
                            .Append(", ");

                        if (schemaAnnotation.Value == null
                            && ((bool?)isExcludedAnnotation?.Value) != true)
                        {
                            stringBuilder.Append("(string)");
                        }

                        stringBuilder.Append(Code.UnknownLiteral(schemaAnnotation.Value));
                    }

                    if (isExcludedAnnotation != null)
                    {
                        if (((bool?)isExcludedAnnotation.Value) == true)
                        {
                            if (entityType.IsOwned())
                            {
                                // Issue #23173
                                stringBuilder
                                    .Append(", excludedFromMigrations: true");
                            }
                            else
                            {
                                stringBuilder
                                    .Append(", t => t.ExcludeFromMigrations()");
                            }
                        }

                        annotations.Remove(isExcludedAnnotation.Name);
                    }

                    stringBuilder.AppendLine(");");
                }
            }
            annotations.Remove(RelationalAnnotationNames.Schema);

            var viewNameAnnotation = annotations.Find(RelationalAnnotationNames.ViewName);
            if (viewNameAnnotation?.Value != null
                || entityType.BaseType == null)
            {
                var viewName = (string?)viewNameAnnotation?.Value ?? entityType.GetViewName();
                if (viewName != null
                    || viewNameAnnotation != null)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(builderName)
                        .Append(".ToView(")
                        .Append(Code.UnknownLiteral(viewName));
                    if (viewNameAnnotation != null)
                    {
                        annotations.Remove(viewNameAnnotation.Name);
                    }

                    var viewSchemaAnnotation = annotations.Find(RelationalAnnotationNames.ViewSchema);
                    if (viewSchemaAnnotation?.Value != null)
                    {
                        stringBuilder
                            .Append(", ")
                            .Append(Code.Literal((string)viewSchemaAnnotation.Value));
                        annotations.Remove(viewSchemaAnnotation.Name);
                    }

                    stringBuilder.AppendLine(");");
                }
            }
            annotations.Remove(RelationalAnnotationNames.ViewSchema);
            annotations.Remove(RelationalAnnotationNames.ViewDefinitionSql);

            var functionNameAnnotation = annotations.Find(RelationalAnnotationNames.FunctionName);
            if (functionNameAnnotation?.Value != null
                || entityType.BaseType == null)
            {
                var functionName = (string?)functionNameAnnotation?.Value ?? entityType.GetFunctionName();
                if (functionName != null
                    || functionNameAnnotation != null)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(builderName)
                        .Append(".ToFunction(")
                        .Append(Code.UnknownLiteral(functionName))
                        .AppendLine(");");
                    if (functionNameAnnotation != null)
                    {
                        annotations.Remove(functionNameAnnotation.Name);
                    }
                }
            }

            var sqlQueryAnnotation = annotations.Find(RelationalAnnotationNames.SqlQuery);
            if (sqlQueryAnnotation?.Value != null
                || entityType.BaseType == null)
            {
                var sqlQuery = (string?)sqlQueryAnnotation?.Value ?? entityType.GetSqlQuery();
                if (sqlQuery != null
                    || sqlQueryAnnotation != null)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(builderName)
                        .Append(".ToSqlQuery(")
                        .Append(Code.UnknownLiteral(sqlQuery))
                        .AppendLine(");");
                    if (sqlQueryAnnotation != null)
                    {
                        annotations.Remove(sqlQueryAnnotation.Name);
                    }
                }
            }

            if ((discriminatorPropertyAnnotation?.Value
                    ?? discriminatorMappingCompleteAnnotation?.Value
                    ?? discriminatorValueAnnotation?.Value)
                != null)
            {
                stringBuilder
                    .AppendLine()
                    .Append(builderName)
                    .Append(".")
                    .Append("HasDiscriminator");

                if (discriminatorPropertyAnnotation?.Value != null)
                {
                    var discriminatorProperty = entityType.FindProperty((string)discriminatorPropertyAnnotation.Value)!;
                    var propertyClrType = FindValueConverter(discriminatorProperty)?.ProviderClrType
                            .MakeNullable(discriminatorProperty.IsNullable)
                        ?? discriminatorProperty.ClrType;
                    stringBuilder
                        .Append("<")
                        .Append(Code.Reference(propertyClrType))
                        .Append(">(")
                        .Append(Code.Literal((string)discriminatorPropertyAnnotation.Value))
                        .Append(")");
                }
                else
                {
                    stringBuilder
                        .Append("()");
                }

                if (discriminatorMappingCompleteAnnotation?.Value != null)
                {
                    var value = discriminatorMappingCompleteAnnotation.Value;

                    stringBuilder
                        .Append(".")
                        .Append("IsComplete")
                        .Append("(")
                        .Append(Code.UnknownLiteral(value))
                        .Append(")");
                }

                if (discriminatorValueAnnotation?.Value != null)
                {
                    var value = discriminatorValueAnnotation.Value;
                    var discriminatorProperty = entityType.FindDiscriminatorProperty();
                    if (discriminatorProperty != null)
                    {
                        var valueConverter = FindValueConverter(discriminatorProperty);
                        if (valueConverter != null)
                        {
                            value = valueConverter.ConvertToProvider(value);
                        }
                    }

                    stringBuilder
                        .Append(".")
                        .Append("HasValue")
                        .Append("(")
                        .Append(Code.UnknownLiteral(value))
                        .Append(")");
                }

                stringBuilder.AppendLine(";");
            }

            var fluentApiCalls = Dependencies.AnnotationCodeGenerator.GenerateFluentApiCalls(entityType, annotations);
            if (fluentApiCalls.Count > 0 || annotations.Count > 0)
            {
                stringBuilder
                    .AppendLine()
                    .Append(builderName);

                using (stringBuilder.Indent())
                {
                    foreach (var methodCallCodeFragment in fluentApiCalls)
                    {
                        stringBuilder
                            .AppendLine()
                            .Append(Code.Fragment(methodCallCodeFragment));
                    }

                    GenerateAnnotations(annotations.Values, stringBuilder);

                    stringBuilder
                        .AppendLine(";");
                }
            }
        }

        /// <summary>
        ///     Generates code for <see cref="ICheckConstraint" /> objects.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateCheckConstraints(
            string builderName,
            IEntityType entityType,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var constraintsForEntity = entityType.GetCheckConstraints();

            foreach (var checkConstraint in constraintsForEntity)
            {
                stringBuilder.AppendLine();

                GenerateCheckConstraint(builderName, checkConstraint, stringBuilder);
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="ICheckConstraint" />.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="checkConstraint"> The check constraint. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateCheckConstraint(
            string builderName,
            ICheckConstraint checkConstraint,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(checkConstraint, nameof(checkConstraint));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .Append(builderName)
                .Append(".HasCheckConstraint(")
                .Append(Code.Literal(checkConstraint.ModelName))
                .Append(", ")
                .Append(Code.Literal(checkConstraint.Sql));

            if (checkConstraint.Name != (checkConstraint.GetDefaultName() ?? checkConstraint.ModelName))
            {
                stringBuilder
                    .Append(", c => c.HasName(")
                    .Append(Code.Literal(checkConstraint.Name))
                    .Append(")");
            }

            stringBuilder.AppendLine(");");
        }

        /// <summary>
        ///     Generates code for <see cref="IForeignKey" /> objects.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="foreignKeys"> The foreign keys. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateForeignKeys(
            string builderName,
            IEnumerable<IForeignKey> foreignKeys,
            IndentedStringBuilder stringBuilder)
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

        /// <summary>
        ///     Generates code for an <see cref="IForeignKey" />.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateForeignKey(
            string builderName,
            IForeignKey foreignKey,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            if (!foreignKey.IsOwnership)
            {
                stringBuilder
                    .Append(builderName)
                    .Append(".HasOne(")
                    .Append(Code.Literal(foreignKey.PrincipalEntityType.Name))
                    .Append(", ")
                    .Append(
                        foreignKey.DependentToPrincipal == null
                            ? Code.UnknownLiteral(null)
                            : Code.Literal(foreignKey.DependentToPrincipal.Name));
            }
            else
            {
                stringBuilder
                    .Append(builderName)
                    .Append(".WithOwner(");

                if (foreignKey.DependentToPrincipal != null)
                {
                    stringBuilder
                        .Append(Code.Literal(foreignKey.DependentToPrincipal.Name));
                }
            }

            stringBuilder
                .Append(")")
                .AppendLine();

            using (stringBuilder.Indent())
            {
                if (foreignKey.IsUnique
                    && !foreignKey.IsOwnership)
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
                    if (!foreignKey.IsOwnership)
                    {
                        stringBuilder
                            .Append(".WithMany(");

                        if (foreignKey.PrincipalToDependent != null)
                        {
                            stringBuilder
                                .Append(Code.Literal(foreignKey.PrincipalToDependent.Name));
                        }

                        stringBuilder
                            .AppendLine(")");
                    }

                    stringBuilder
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

                if (!foreignKey.IsOwnership)
                {
                    if (foreignKey.DeleteBehavior != DeleteBehavior.ClientSetNull)
                    {
                        stringBuilder
                            .AppendLine()
                            .Append(".OnDelete(")
                            .Append(Code.Literal(foreignKey.DeleteBehavior))
                            .Append(")");
                    }

                    if (foreignKey.IsRequired)
                    {
                        stringBuilder
                            .AppendLine()
                            .Append(".IsRequired()");
                    }
                }
            }

            stringBuilder.AppendLine(";");
        }

        /// <summary>
        ///     Generates code for the annotations on a foreign key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateForeignKeyAnnotations(
            IForeignKey foreignKey,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var annotations = Dependencies.AnnotationCodeGenerator
                .FilterIgnoredAnnotations(foreignKey.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);

            foreach (var methodCallCodeFragment in
                Dependencies.AnnotationCodeGenerator.GenerateFluentApiCalls(foreignKey, annotations))
            {
                stringBuilder
                    .AppendLine()
                    .Append(Code.Fragment(methodCallCodeFragment));
            }

            GenerateAnnotations(annotations.Values, stringBuilder);
        }

        /// <summary>
        ///     Generates code for the navigations of an <see cref="IEntityType" />.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateEntityTypeNavigations(
            string builderName,
            IEntityType entityType,
            IndentedStringBuilder stringBuilder)
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
                    GenerateNavigations(
                        "b", entityType.GetDeclaredNavigations()
                            .Where(n => !n.IsOnDependent && !n.ForeignKey.IsOwnership), stringBuilder);
                }

                stringBuilder.AppendLine("});");
            }
        }

        /// <summary>
        ///     Generates code for <see cref="INavigation" /> objects.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="navigations"> The navigations. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateNavigations(
            string builderName,
            IEnumerable<INavigation> navigations,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(navigations, nameof(navigations));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var navigation in navigations)
            {
                stringBuilder.AppendLine();

                GenerateNavigation(builderName, navigation, stringBuilder);
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="INavigation" />.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="navigation"> The navigation. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateNavigation(
            string builderName,
            INavigation navigation,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(builderName, nameof(builderName));
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .Append(builderName)
                .Append(".Navigation(")
                .Append(Code.Literal(navigation.Name))
                .Append(")");

            using (stringBuilder.Indent())
            {
                if (!navigation.IsOnDependent
                    && !navigation.IsCollection
                    && navigation.ForeignKey.IsRequiredDependent)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".IsRequired()");
                }

                GenerateNavigationAnnotations(navigation, stringBuilder);
            }

            stringBuilder.AppendLine(";");
        }

        /// <summary>
        ///     Generates code for the annotations on a navigation.
        /// </summary>
        /// <param name="navigation"> The navigation. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateNavigationAnnotations(
            INavigation navigation,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var annotations = Dependencies.AnnotationCodeGenerator
                .FilterIgnoredAnnotations(navigation.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);

            foreach (var methodCallCodeFragment in
                Dependencies.AnnotationCodeGenerator.GenerateFluentApiCalls(navigation, annotations))
            {
                stringBuilder
                    .AppendLine()
                    .Append(Code.Fragment(methodCallCodeFragment));
            }

            GenerateAnnotations(annotations.Values, stringBuilder);
        }

        /// <summary>
        ///     Generates code for annotations.
        /// </summary>
        /// <param name="annotations"> The annotations. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateAnnotations(
            IEnumerable<IAnnotation> annotations,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var annotation in annotations)
            {
                stringBuilder.AppendLine();
                GenerateAnnotation(annotation, stringBuilder);
            }
        }

        /// <summary>
        ///     Generates code for an annotation which does not have a fluent API call.
        /// </summary>
        /// <param name="annotation"> The annotation. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateAnnotation(
            IAnnotation annotation,
            IndentedStringBuilder stringBuilder)
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

        /// <summary>
        ///     Generates code for data seeding.
        /// </summary>
        /// <param name="builderName"> The name of the builder variable. </param>
        /// <param name="properties"> The properties to generate. </param>
        /// <param name="data"> The data to be seeded. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        protected virtual void GenerateData(
            string builderName,
            IEnumerable<IProperty> properties,
            IEnumerable<IDictionary<string, object?>> data,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(data, nameof(data));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var dataList = data.ToList();
            if (dataList.Count == 0)
            {
                return;
            }

            var propertiesToOutput = properties.ToList();

            stringBuilder
                .AppendLine()
                .Append(builderName)
                .Append(".")
                .Append(nameof(EntityTypeBuilder.HasData))
                .AppendLine("(");

            using (stringBuilder.Indent())
            {
                var firstDatum = true;
                foreach (var o in dataList)
                {
                    if (!firstDatum)
                    {
                        stringBuilder.AppendLine(",");
                    }
                    else
                    {
                        firstDatum = false;
                    }

                    stringBuilder
                        .AppendLine("new")
                        .AppendLine("{");

                    using (stringBuilder.Indent())
                    {
                        var firstProperty = true;
                        foreach (var property in propertiesToOutput)
                        {
                            if (o.TryGetValue(property.Name, out var value)
                                && value != null)
                            {
                                if (!firstProperty)
                                {
                                    stringBuilder.AppendLine(",");
                                }
                                else
                                {
                                    firstProperty = false;
                                }

                                stringBuilder
                                    .Append(Code.Identifier(property.Name))
                                    .Append(" = ")
                                    .Append(Code.UnknownLiteral(value));
                            }
                        }

                        stringBuilder.AppendLine();
                    }

                    stringBuilder.Append("}");
                }
            }

            stringBuilder
                .AppendLine(");");
        }

        private void GenerateFluentApiForMaxLength(
            IProperty property,
            IndentedStringBuilder stringBuilder)
        {
            if (property.GetMaxLength() is int maxLength)
            {
                stringBuilder
                    .AppendLine()
                    .Append(".")
                    .Append(nameof(PropertyBuilder.HasMaxLength))
                    .Append("(")
                    .Append(Code.Literal(maxLength))
                    .Append(")");
            }
        }

        private void GenerateFluentApiForPrecisionAndScale(
            IProperty property,
            IndentedStringBuilder stringBuilder)
        {
            if (property.GetPrecision() is int precision)
            {
                stringBuilder
                    .AppendLine()
                    .Append(".")
                    .Append(nameof(PropertyBuilder.HasPrecision))
                    .Append("(")
                    .Append(Code.UnknownLiteral(precision));

                if (property.GetScale() is int scale)
                {
                    if (scale != 0)
                    {
                        stringBuilder
                            .Append(", ")
                            .Append(Code.UnknownLiteral(scale));
                    }
                }

                stringBuilder.Append(")");
            }
        }

        private void GenerateFluentApiForIsUnicode(
            IProperty property,
            IndentedStringBuilder stringBuilder)
        {
            if (property.IsUnicode() is bool unicode)
            {
                stringBuilder
                    .AppendLine()
                    .Append(".")
                    .Append(nameof(PropertyBuilder.IsUnicode))
                    .Append("(")
                    .Append(Code.Literal(unicode))
                    .Append(")");
            }
        }

        private void GenerateFluentApiForDefaultValue(
            IProperty property,
            IndentedStringBuilder stringBuilder)
        {
            if (!property.TryGetDefaultValue(out var defaultValue))
            {
                return;
            }

            stringBuilder
                .AppendLine()
                .Append(".")
                .Append(nameof(RelationalPropertyBuilderExtensions.HasDefaultValue))
                .Append("(");

            if (defaultValue != DBNull.Value)
            {
                stringBuilder
                    .Append(
                        Code.UnknownLiteral(
                            FindValueConverter(property) is ValueConverter valueConverter
                                ? valueConverter.ConvertToProvider(defaultValue)
                                : defaultValue));
            }

            stringBuilder
                .Append(")");
        }

        private static IReadOnlyList<IAnnotation> RemoveAmbiguousFluentApiAnnotations(
            Dictionary<string, IAnnotation> annotations,
            Func<string, bool> annotationNameMatcher)
        {
            List<IAnnotation>? ambiguousAnnotations = null;

            foreach (var (name, annotation) in annotations)
            {
                if (annotationNameMatcher(name))
                {
                    annotations.Remove(name);
                    ambiguousAnnotations ??= new List<IAnnotation>();
                    ambiguousAnnotations.Add(annotation);
                }
            }

            return (IReadOnlyList<IAnnotation>?)ambiguousAnnotations ?? ImmutableList<IAnnotation>.Empty;
        }
    }
}
