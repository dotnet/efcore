// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CSharpDbContextGenerator : ICSharpDbContextGenerator
    {
        private const string EntityLambdaIdentifier = "entity";
        private const string Language = "CSharp";

        private readonly ICSharpHelper _code;
        private readonly IProviderConfigurationCodeGenerator _providerConfigurationCodeGenerator;
        private readonly IAnnotationCodeGenerator _annotationCodeGenerator;
        private IndentedStringBuilder _sb;
        private bool _entityTypeBuilderInitialized;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CSharpDbContextGenerator(
            [NotNull] IProviderConfigurationCodeGenerator providerConfigurationCodeGenerator,
            [NotNull] IAnnotationCodeGenerator annotationCodeGenerator,
            [NotNull] ICSharpHelper cSharpHelper)
        {
            Check.NotNull(providerConfigurationCodeGenerator, nameof(providerConfigurationCodeGenerator));
            Check.NotNull(annotationCodeGenerator, nameof(annotationCodeGenerator));
            Check.NotNull(cSharpHelper, nameof(cSharpHelper));

            _providerConfigurationCodeGenerator = providerConfigurationCodeGenerator;
            _annotationCodeGenerator = annotationCodeGenerator;
            _code = cSharpHelper;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string WriteCode(
            IModel model,
            string @namespace,
            string contextName,
            string connectionString,
            bool useDataAnnotations,
            bool suppressConnectionStringWarning)
        {
            Check.NotNull(model, nameof(model));

            _sb = new IndentedStringBuilder();

            _sb.AppendLine("using System;"); // Guid default values require new Guid() which requires this using
            _sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            _sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata;");
            _sb.AppendLine();

            _sb.AppendLine($"namespace {@namespace}");
            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                GenerateClass(
                    model,
                    contextName,
                    connectionString,
                    useDataAnnotations,
                    suppressConnectionStringWarning);
            }

            _sb.AppendLine("}");

            return _sb.ToString();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GenerateClass(
            [NotNull] IModel model,
            [NotNull] string contextName,
            [NotNull] string connectionString,
            bool useDataAnnotations,
            bool suppressConnectionStringWarning)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(contextName, nameof(contextName));
            Check.NotNull(connectionString, nameof(connectionString));

            _sb.AppendLine($"public partial class {contextName} : DbContext");
            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                GenerateConstructors(contextName);
                GenerateDbSets(model);
                GenerateEntityTypeErrors(model);
                GenerateOnConfiguring(connectionString, suppressConnectionStringWarning);
                GenerateOnModelCreating(model, useDataAnnotations);
            }

            _sb.AppendLine();

            using (_sb.Indent())
            {
                _sb.AppendLine("partial void OnModelCreatingPartial(ModelBuilder modelBuilder);");
            }

            _sb.AppendLine("}");
        }

        private void GenerateConstructors(string contextName)
        {
            _sb.AppendLine($"public {contextName}()")
                .AppendLine("{")
                .AppendLine("}")
                .AppendLine();

            _sb.AppendLine($"public {contextName}(DbContextOptions<{contextName}> options)")
                .IncrementIndent()
                .AppendLine(": base(options)")
                .DecrementIndent()
                .AppendLine("{")
                .AppendLine("}")
                .AppendLine();
        }

        private void GenerateDbSets(IModel model)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                _sb.AppendLine(
                    $"public virtual DbSet<{entityType.Name}> {entityType.GetDbSetName()} {{ get; set; }}");
            }

            if (model.GetEntityTypes().Any())
            {
                _sb.AppendLine();
            }
        }

        private void GenerateEntityTypeErrors(IModel model)
        {
            foreach (var entityTypeError in model.GetEntityTypeErrors())
            {
                _sb.AppendLine($"// {entityTypeError.Value} Please see the warning messages.");
            }

            if (model.GetEntityTypeErrors().Count > 0)
            {
                _sb.AppendLine();
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GenerateOnConfiguring(
            [NotNull] string connectionString,
            bool suppressConnectionStringWarning)
        {
            Check.NotNull(connectionString, nameof(connectionString));

            _sb.AppendLine("protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                _sb.AppendLine("if (!optionsBuilder.IsConfigured)");
                _sb.AppendLine("{");

                using (_sb.Indent())
                {
                    if (!suppressConnectionStringWarning)
                    {
                        _sb.DecrementIndent()
                            .DecrementIndent()
                            .DecrementIndent()
                            .DecrementIndent()
                            .AppendLine("#warning " + DesignStrings.SensitiveInformationWarning)
                            .IncrementIndent()
                            .IncrementIndent()
                            .IncrementIndent()
                            .IncrementIndent();
                    }

                    _sb.Append("optionsBuilder");

                    var useProviderCall = _providerConfigurationCodeGenerator.GenerateUseProvider(
                        connectionString,
                        _providerConfigurationCodeGenerator.GenerateProviderOptions());
                    var contextOptions = _providerConfigurationCodeGenerator.GenerateContextOptions();
                    if (contextOptions != null)
                    {
                        useProviderCall = useProviderCall.Chain(contextOptions);
                    }

                    _sb
                        .Append(_code.Fragment(useProviderCall))
                        .AppendLine(";");
                }

                _sb.AppendLine("}");
            }

            _sb.AppendLine("}");

            _sb.AppendLine();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GenerateOnModelCreating(
            [NotNull] IModel model,
            bool useDataAnnotations)
        {
            Check.NotNull(model, nameof(model));

            _sb.AppendLine("protected override void OnModelCreating(ModelBuilder modelBuilder)");
            _sb.Append("{");

            var annotations = model.GetAnnotations().ToList();
            RemoveAnnotation(ref annotations, CoreAnnotationNames.ProductVersion);
            RemoveAnnotation(ref annotations, CoreAnnotationNames.ChangeTrackingStrategy);
            RemoveAnnotation(ref annotations, CoreAnnotationNames.OwnedTypes);
            RemoveAnnotation(ref annotations, ChangeDetector.SkipDetectChangesAnnotation);
            RemoveAnnotation(ref annotations, RelationalAnnotationNames.MaxIdentifierLength);
            RemoveAnnotation(ref annotations, RelationalAnnotationNames.CheckConstraints);
            RemoveAnnotation(ref annotations, ScaffoldingAnnotationNames.DatabaseName);
            RemoveAnnotation(ref annotations, ScaffoldingAnnotationNames.EntityTypeErrors);

            var annotationsToRemove = new List<IAnnotation>();
            annotationsToRemove.AddRange(
                annotations.Where(
                    a => a.Name.StartsWith(RelationalAnnotationNames.SequencePrefix, StringComparison.Ordinal)));

            var lines = new List<string>();

            foreach (var annotation in annotations)
            {
                if (annotation.Value == null
                    || _annotationCodeGenerator.IsHandledByConvention(model, annotation))
                {
                    annotationsToRemove.Add(annotation);
                }
                else
                {
                    var methodCall = _annotationCodeGenerator.GenerateFluentApi(model, annotation);
                    if (methodCall != null)
                    {
                        lines.Add(_code.Fragment(methodCall));
                        annotationsToRemove.Add(annotation);
                    }
                }
            }

            lines.AddRange(GenerateAnnotations(annotations.Except(annotationsToRemove)));

            if (lines.Count > 0)
            {
                using (_sb.Indent())
                {
                    _sb.AppendLine();
                    _sb.Append("modelBuilder" + lines[0]);

                    using (_sb.Indent())
                    {
                        foreach (var line in lines.Skip(1))
                        {
                            _sb.AppendLine();
                            _sb.Append(line);
                        }
                    }

                    _sb.AppendLine(";");
                }
            }

            using (_sb.Indent())
            {
                foreach (var entityType in model.GetEntityTypes())
                {
                    _entityTypeBuilderInitialized = false;

                    GenerateEntityType(entityType, useDataAnnotations);

                    if (_entityTypeBuilderInitialized)
                    {
                        _sb.AppendLine("});");
                    }
                }

                foreach (var sequence in model.GetSequences())
                {
                    GenerateSequence(sequence);
                }
            }

            _sb.AppendLine();

            using (_sb.Indent())
            {
                _sb.AppendLine("OnModelCreatingPartial(modelBuilder);");
            }

            _sb.AppendLine("}");
        }

        private void InitializeEntityTypeBuilder(IEntityType entityType)
        {
            if (!_entityTypeBuilderInitialized)
            {
                _sb.AppendLine();
                _sb.AppendLine($"modelBuilder.Entity<{entityType.Name}>({EntityLambdaIdentifier} =>");
                _sb.Append("{");
            }

            _entityTypeBuilderInitialized = true;
        }

        private void GenerateEntityType(IEntityType entityType, bool useDataAnnotations)
        {
            GenerateKey(entityType.FindPrimaryKey(), entityType, useDataAnnotations);

            var annotations = entityType.GetAnnotations().ToList();
            RemoveAnnotation(ref annotations, CoreAnnotationNames.ConstructorBinding);
            RemoveAnnotation(ref annotations, RelationalAnnotationNames.TableName);
            RemoveAnnotation(ref annotations, RelationalAnnotationNames.Comment);
            RemoveAnnotation(ref annotations, RelationalAnnotationNames.Schema);
            RemoveAnnotation(ref annotations, ScaffoldingAnnotationNames.DbSetName);

            if (!useDataAnnotations)
            {
                GenerateTableName(entityType);
            }

            var annotationsToRemove = new List<IAnnotation>();
            var lines = new List<string>();

            foreach (var annotation in annotations)
            {
                if (annotation.Value == null
                    || _annotationCodeGenerator.IsHandledByConvention(entityType, annotation))
                {
                    annotationsToRemove.Add(annotation);
                }
                else
                {
                    var methodCall = _annotationCodeGenerator.GenerateFluentApi(entityType, annotation);
                    if (methodCall != null)
                    {
                        lines.Add(_code.Fragment(methodCall));
                        annotationsToRemove.Add(annotation);
                    }
                }
            }

            lines.AddRange(GenerateAnnotations(annotations.Except(annotationsToRemove)));

            if (entityType.GetComment() != null)
            {
                lines.Add(
                    $".{nameof(RelationalEntityTypeBuilderExtensions.HasComment)}" +
                    $"({_code.Literal(entityType.GetComment())})");
            }

            AppendMultiLineFluentApi(entityType, lines);

            foreach (var index in entityType.GetIndexes())
            {
                GenerateIndex(index);
            }

            foreach (var property in entityType.GetProperties())
            {
                GenerateProperty(property, useDataAnnotations);
            }

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                GenerateRelationship(foreignKey, useDataAnnotations);
            }
        }

        private void AppendMultiLineFluentApi(IEntityType entityType, IList<string> lines)
        {
            if (lines.Count <= 0)
            {
                return;
            }

            InitializeEntityTypeBuilder(entityType);

            using (_sb.Indent())
            {
                _sb.AppendLine();

                _sb.Append(EntityLambdaIdentifier + lines[0]);

                using (_sb.Indent())
                {
                    foreach (var line in lines.Skip(1))
                    {
                        _sb.AppendLine();
                        _sb.Append(line);
                    }
                }

                _sb.AppendLine(";");
            }
        }

        private void GenerateKey(IKey key, IEntityType entityType, bool useDataAnnotations)
        {
            if (key == null)
            {
                var line = new List<string>
                {
                    $".{nameof(EntityTypeBuilder.HasNoKey)}()"
                };

                AppendMultiLineFluentApi(entityType, line);

                return;
            }

            var annotations = key.GetAnnotations().ToList();

            var explicitName = key.GetName() != key.GetDefaultName();
            RemoveAnnotation(ref annotations, RelationalAnnotationNames.Name);

            if (key.Properties.Count == 1
                && annotations.Count == 0)
            {
                if (key is Key concreteKey
                    && key.Properties.SequenceEqual(
                        KeyDiscoveryConvention.DiscoverKeyProperties(
                            concreteKey.DeclaringEntityType,
                            concreteKey.DeclaringEntityType.GetProperties())))
                {
                    return;
                }

                if (!explicitName
                    && useDataAnnotations)
                {
                    return;
                }
            }

            var lines = new List<string>
            {
                $".{nameof(EntityTypeBuilder.HasKey)}(e => {GenerateLambdaToKey(key.Properties, "e")})"
            };

            if (explicitName)
            {
                lines.Add(
                    $".{nameof(RelationalKeyBuilderExtensions.HasName)}" +
                    $"({_code.Literal(key.GetName())})");
            }

            var annotationsToRemove = new List<IAnnotation>();

            foreach (var annotation in annotations)
            {
                if (annotation.Value == null
                    || _annotationCodeGenerator.IsHandledByConvention(key, annotation))
                {
                    annotationsToRemove.Add(annotation);
                }
                else
                {
                    var methodCall = _annotationCodeGenerator.GenerateFluentApi(key, annotation);
                    if (methodCall != null)
                    {
                        lines.Add(_code.Fragment(methodCall));
                        annotationsToRemove.Add(annotation);
                    }
                }
            }

            lines.AddRange(GenerateAnnotations(annotations.Except(annotationsToRemove)));

            AppendMultiLineFluentApi(key.DeclaringEntityType, lines);
        }

        private void GenerateTableName(IEntityType entityType)
        {
            var tableName = entityType.GetTableName();
            var schema = entityType.GetSchema();
            var defaultSchema = entityType.Model.GetDefaultSchema();

            var explicitSchema = schema != null && schema != defaultSchema;
            var explicitTable = explicitSchema || tableName != null && tableName != entityType.GetDbSetName();

            if (explicitTable)
            {
                var parameterString = _code.Literal(tableName);
                if (explicitSchema)
                {
                    parameterString += ", " + _code.Literal(schema);
                }

                var lines = new List<string>
                {
                    $".{nameof(RelationalEntityTypeBuilderExtensions.ToTable)}({parameterString})"
                };

                AppendMultiLineFluentApi(entityType, lines);
            }
        }

        private void GenerateIndex(IIndex index)
        {
            var lines = new List<string>
            {
                $".{nameof(EntityTypeBuilder.HasIndex)}(e => {GenerateLambdaToKey(index.Properties, "e")})"
            };

            var annotations = index.GetAnnotations().ToList();

            if (!string.IsNullOrEmpty((string)index[RelationalAnnotationNames.Name]))
            {
                lines.Add(
                    $".{nameof(RelationalIndexBuilderExtensions.HasName)}" +
                    $"({_code.Literal(index.GetName())})");
                RemoveAnnotation(ref annotations, RelationalAnnotationNames.Name);
            }

            if (index.IsUnique)
            {
                lines.Add($".{nameof(IndexBuilder.IsUnique)}()");
            }

            if (index.GetFilter() != null)
            {
                lines.Add(
                    $".{nameof(RelationalIndexBuilderExtensions.HasFilter)}" +
                    $"({_code.Literal(index.GetFilter())})");
                RemoveAnnotation(ref annotations, RelationalAnnotationNames.Filter);
            }

            var annotationsToRemove = new List<IAnnotation>();

            foreach (var annotation in annotations)
            {
                if (annotation.Value == null
                    || _annotationCodeGenerator.IsHandledByConvention(index, annotation))
                {
                    annotationsToRemove.Add(annotation);
                }
                else
                {
                    var methodCall = _annotationCodeGenerator.GenerateFluentApi(index, annotation);
                    if (methodCall != null)
                    {
                        lines.Add(_code.Fragment(methodCall));
                        annotationsToRemove.Add(annotation);
                    }
                }
            }

            lines.AddRange(GenerateAnnotations(annotations.Except(annotationsToRemove)));

            AppendMultiLineFluentApi(index.DeclaringEntityType, lines);
        }

        private void GenerateProperty(IProperty property, bool useDataAnnotations)
        {
            var lines = new List<string>
            {
                $".{nameof(EntityTypeBuilder.Property)}(e => e.{property.Name})"
            };

            var annotations = property.GetAnnotations().ToList();

            RemoveAnnotation(ref annotations, RelationalAnnotationNames.ColumnName);
            RemoveAnnotation(ref annotations, RelationalAnnotationNames.ColumnType);
            RemoveAnnotation(ref annotations, CoreAnnotationNames.MaxLength);
            RemoveAnnotation(ref annotations, CoreAnnotationNames.TypeMapping);
            RemoveAnnotation(ref annotations, CoreAnnotationNames.Unicode);
            RemoveAnnotation(ref annotations, RelationalAnnotationNames.DefaultValue);
            RemoveAnnotation(ref annotations, RelationalAnnotationNames.DefaultValueSql);
            RemoveAnnotation(ref annotations, RelationalAnnotationNames.Comment);
            RemoveAnnotation(ref annotations, RelationalAnnotationNames.ComputedColumnSql);
            RemoveAnnotation(ref annotations, RelationalAnnotationNames.IsFixedLength);
            RemoveAnnotation(ref annotations, ScaffoldingAnnotationNames.ColumnOrdinal);

            if (!useDataAnnotations)
            {
                if (!property.IsNullable
                    && property.ClrType.IsNullableType()
                    && !property.IsPrimaryKey())
                {
                    lines.Add($".{nameof(PropertyBuilder.IsRequired)}()");
                }

                var columnName = property.GetColumnName();

                if (columnName != null
                    && columnName != property.Name)
                {
                    lines.Add(
                        $".{nameof(RelationalPropertyBuilderExtensions.HasColumnName)}" +
                        $"({_code.Literal(columnName)})");
                }

                var columnType = property.GetConfiguredColumnType();

                if (columnType != null)
                {
                    lines.Add(
                        $".{nameof(RelationalPropertyBuilderExtensions.HasColumnType)}" +
                        $"({_code.Literal(columnType)})");
                }

                var maxLength = property.GetMaxLength();

                if (maxLength.HasValue)
                {
                    lines.Add(
                        $".{nameof(PropertyBuilder.HasMaxLength)}" +
                        $"({_code.Literal(maxLength.Value)})");
                }
            }

            if (property.IsUnicode() != null)
            {
                lines.Add(
                    $".{nameof(PropertyBuilder.IsUnicode)}" +
                    $"({(property.IsUnicode() == false ? "false" : "")})");
            }

            if (property.IsFixedLength())
            {
                lines.Add(
                    $".{nameof(RelationalPropertyBuilderExtensions.IsFixedLength)}()");
            }

            if (property.GetDefaultValue() != null)
            {
                lines.Add(
                    $".{nameof(RelationalPropertyBuilderExtensions.HasDefaultValue)}" +
                    $"({_code.UnknownLiteral(property.GetDefaultValue())})");
            }

            if (property.GetDefaultValueSql() != null)
            {
                lines.Add(
                    $".{nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql)}" +
                    $"({_code.Literal(property.GetDefaultValueSql())})");
            }

            if (property.GetComment() != null)
            {
                lines.Add(
                    $".{nameof(RelationalPropertyBuilderExtensions.HasComment)}" +
                    $"({_code.Literal(property.GetComment())})");
            }

            if (property.GetComputedColumnSql() != null)
            {
                lines.Add(
                    $".{nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql)}" +
                    $"({_code.Literal(property.GetComputedColumnSql())})");
            }

            var valueGenerated = property.ValueGenerated;
            var isRowVersion = false;
            if (((IConventionProperty)property).GetValueGeneratedConfigurationSource().HasValue
                && RelationalValueGenerationConvention.GetValueGenerated(property) != valueGenerated)
            {
                string methodName;
                switch (valueGenerated)
                {
                    case ValueGenerated.OnAdd:
                        methodName = nameof(PropertyBuilder.ValueGeneratedOnAdd);
                        break;

                    case ValueGenerated.OnAddOrUpdate:
                        isRowVersion = property.IsConcurrencyToken;
                        methodName = isRowVersion
                            ? nameof(PropertyBuilder.IsRowVersion)
                            : nameof(PropertyBuilder.ValueGeneratedOnAddOrUpdate);
                        break;

                    case ValueGenerated.Never:
                        methodName = nameof(PropertyBuilder.ValueGeneratedNever);
                        break;

                    default:
                        methodName = "";
                        break;
                }

                lines.Add($".{methodName}()");
            }

            if (property.IsConcurrencyToken
                && !isRowVersion)
            {
                lines.Add($".{nameof(PropertyBuilder.IsConcurrencyToken)}()");
            }

            var annotationsToRemove = new List<IAnnotation>();

            foreach (var annotation in annotations)
            {
                if (annotation.Value == null
                    || _annotationCodeGenerator.IsHandledByConvention(property, annotation))
                {
                    annotationsToRemove.Add(annotation);
                }
                else
                {
                    var methodCall = _annotationCodeGenerator.GenerateFluentApi(property, annotation);
                    if (methodCall != null)
                    {
                        lines.Add(_code.Fragment(methodCall));
                        annotationsToRemove.Add(annotation);
                    }
                }
            }

            lines.AddRange(GenerateAnnotations(annotations.Except(annotationsToRemove)));

            switch (lines.Count)
            {
                case 1:
                    return;
                case 2:
                    lines = new List<string>
                    {
                        lines[0] + lines[1]
                    };
                    break;
            }

            AppendMultiLineFluentApi(property.DeclaringEntityType, lines);
        }

        private void GenerateRelationship(IForeignKey foreignKey, bool useDataAnnotations)
        {
            var canUseDataAnnotations = true;
            var annotations = foreignKey.GetAnnotations().ToList();

            var lines = new List<string>
            {
                $".{nameof(EntityTypeBuilder.HasOne)}(" + (foreignKey.DependentToPrincipal != null ? $"d => d.{foreignKey.DependentToPrincipal.Name}" : null) + ")",
                $".{(foreignKey.IsUnique ? nameof(ReferenceNavigationBuilder.WithOne) : nameof(ReferenceNavigationBuilder.WithMany))}"
                + $"(" + (foreignKey.PrincipalToDependent != null ? $"p => p.{foreignKey.PrincipalToDependent.Name}" : null) + ")"
            };

            if (!foreignKey.PrincipalKey.IsPrimaryKey())
            {
                canUseDataAnnotations = false;
                lines.Add(
                    $".{nameof(ReferenceReferenceBuilder.HasPrincipalKey)}"
                    + (foreignKey.IsUnique ? $"<{((ITypeBase)foreignKey.PrincipalEntityType).DisplayName()}>" : "")
                    + $"(p => {GenerateLambdaToKey(foreignKey.PrincipalKey.Properties, "p")})");
            }

            lines.Add(
                $".{nameof(ReferenceReferenceBuilder.HasForeignKey)}"
                + (foreignKey.IsUnique ? $"<{((ITypeBase)foreignKey.DeclaringEntityType).DisplayName()}>" : "")
                + $"(d => {GenerateLambdaToKey(foreignKey.Properties, "d")})");

            var defaultOnDeleteAction = foreignKey.IsRequired
                ? DeleteBehavior.Cascade
                : DeleteBehavior.ClientSetNull;

            if (foreignKey.DeleteBehavior != defaultOnDeleteAction)
            {
                canUseDataAnnotations = false;
                lines.Add(
                    $".{nameof(ReferenceReferenceBuilder.OnDelete)}" +
                    $"({_code.Literal(foreignKey.DeleteBehavior)})");
            }

            if (!string.IsNullOrEmpty((string)foreignKey[RelationalAnnotationNames.Name]))
            {
                canUseDataAnnotations = false;
                lines.Add(
                    $".HasConstraintName" +
                    $"({_code.Literal(foreignKey.GetConstraintName())})");
                RemoveAnnotation(ref annotations, RelationalAnnotationNames.Name);
            }

            var annotationsToRemove = new List<IAnnotation>();

            foreach (var annotation in annotations)
            {
                if (annotation.Value == null
                    || _annotationCodeGenerator.IsHandledByConvention(foreignKey, annotation))
                {
                    annotationsToRemove.Add(annotation);
                }
                else
                {
                    var methodCall = _annotationCodeGenerator.GenerateFluentApi(foreignKey, annotation);
                    if (methodCall != null)
                    {
                        canUseDataAnnotations = false;
                        lines.Add(_code.Fragment(methodCall));
                        annotationsToRemove.Add(annotation);
                    }
                }
            }

            lines.AddRange(GenerateAnnotations(annotations.Except(annotationsToRemove)));

            if (!useDataAnnotations
                || !canUseDataAnnotations)
            {
                AppendMultiLineFluentApi(foreignKey.DeclaringEntityType, lines);
            }
        }

        private void GenerateSequence(ISequence sequence)
        {
            var methodName = nameof(RelationalModelBuilderExtensions.HasSequence);

            if (sequence.ClrType != Sequence.DefaultClrType)
            {
                methodName += $"<{_code.Reference(sequence.ClrType)}>";
            }

            var parameters = _code.Literal(sequence.Name);

            if (!string.IsNullOrEmpty(sequence.Schema)
                && sequence.Model.GetDefaultSchema() != sequence.Schema)
            {
                parameters += $", {_code.Literal(sequence.Schema)}";
            }

            var lines = new List<string>
            {
                $"modelBuilder.{methodName}({parameters})"
            };

            if (sequence.StartValue != Sequence.DefaultStartValue)
            {
                lines.Add($".{nameof(SequenceBuilder.StartsAt)}({sequence.StartValue})");
            }

            if (sequence.IncrementBy != Sequence.DefaultIncrementBy)
            {
                lines.Add($".{nameof(SequenceBuilder.IncrementsBy)}({sequence.IncrementBy})");
            }

            if (sequence.MinValue != Sequence.DefaultMinValue)
            {
                lines.Add($".{nameof(SequenceBuilder.HasMin)}({sequence.MinValue})");
            }

            if (sequence.MaxValue != Sequence.DefaultMaxValue)
            {
                lines.Add($".{nameof(SequenceBuilder.HasMax)}({sequence.MaxValue})");
            }

            if (sequence.IsCyclic != Sequence.DefaultIsCyclic)
            {
                lines.Add($".{nameof(SequenceBuilder.IsCyclic)}()");
            }

            if (lines.Count == 2)
            {
                lines = new List<string>
                {
                    lines[0] + lines[1]
                };
            }

            _sb.AppendLine();
            _sb.Append(lines[0]);

            using (_sb.Indent())
            {
                foreach (var line in lines.Skip(1))
                {
                    _sb.AppendLine();
                    _sb.Append(line);
                }
            }

            _sb.AppendLine(";");
        }

        private static string GenerateLambdaToKey(
            IReadOnlyList<IProperty> properties,
            string lambdaIdentifier)
        {
            return properties.Count <= 0
                ? ""
                : properties.Count == 1
                ? $"{lambdaIdentifier}.{properties[0].Name}"
                : $"new {{ {string.Join(", ", properties.Select(p => lambdaIdentifier + "." + p.Name))} }}";
        }

        private static void RemoveAnnotation(ref List<IAnnotation> annotations, string annotationName)
            => annotations.Remove(annotations.SingleOrDefault(a => a.Name == annotationName));

        private IList<string> GenerateAnnotations(IEnumerable<IAnnotation> annotations)
            => annotations.Select(GenerateAnnotation).ToList();

        private string GenerateAnnotation(IAnnotation annotation)
            => $".HasAnnotation({_code.Literal(annotation.Name)}, " +
               $"{_code.UnknownLiteral(annotation.Value)})";
    }
}
