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

        private readonly ICSharpHelper _code;
        private readonly IProviderConfigurationCodeGenerator _providerConfigurationCodeGenerator;
        private readonly IAnnotationCodeGenerator _annotationCodeGenerator;
        private IndentedStringBuilder _sb = null!;
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
            string contextName,
            string connectionString,
            string contextNamespace,
            string modelNamespace,
            bool useDataAnnotations,
            bool suppressConnectionStringWarning,
            bool suppressOnConfiguring)
        {
            Check.NotNull(model, nameof(model));

            _sb = new IndentedStringBuilder();

            _sb.AppendLine("using System;"); // Guid default values require new Guid() which requires this using
            _sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            _sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata;");

            var finalContextNamespace = contextNamespace ?? modelNamespace;

            if (finalContextNamespace != modelNamespace)
            {
                _sb.AppendLine(string.Concat("using ", modelNamespace, ";"));
            }

            _sb.AppendLine();

            _sb.AppendLine("#nullable disable");
            _sb.AppendLine();

            _sb.AppendLine($"namespace {finalContextNamespace}");
            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                GenerateClass(
                    model,
                    contextName,
                    connectionString,
                    useDataAnnotations,
                    suppressConnectionStringWarning,
                    suppressOnConfiguring);
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
            bool suppressConnectionStringWarning,
            bool suppressOnConfiguring)
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
                if (!suppressOnConfiguring)
                {
                    GenerateOnConfiguring(connectionString, suppressConnectionStringWarning);
                }

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
                        connectionString);

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

            var annotations = _annotationCodeGenerator
                .FilterIgnoredAnnotations(model.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);

            _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(model, annotations);

            annotations.Remove(CoreAnnotationNames.ProductVersion);
            annotations.Remove(RelationalAnnotationNames.MaxIdentifierLength);
            annotations.Remove(ScaffoldingAnnotationNames.DatabaseName);
            annotations.Remove(ScaffoldingAnnotationNames.EntityTypeErrors);

            var lines = new List<string>();

            lines.AddRange(
                _annotationCodeGenerator.GenerateFluentApiCalls(model, annotations).Select(m => _code.Fragment(m))
                    .Concat(GenerateAnnotations(annotations.Values)));

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

            var annotations = _annotationCodeGenerator
                .FilterIgnoredAnnotations(entityType.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);
            _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(entityType, annotations);

            annotations.Remove(RelationalAnnotationNames.TableName);
            annotations.Remove(RelationalAnnotationNames.Schema);
            annotations.Remove(RelationalAnnotationNames.ViewName);
            annotations.Remove(RelationalAnnotationNames.ViewSchema);
            annotations.Remove(ScaffoldingAnnotationNames.DbSetName);
            annotations.Remove(RelationalAnnotationNames.ViewDefinitionSql);

            if (useDataAnnotations)
            {
                // Strip out any annotations handled as attributes - these are already handled when generating
                // the entity's properties
                _ = _annotationCodeGenerator.GenerateDataAnnotationAttributes(entityType, annotations);
            }

            if (!useDataAnnotations || entityType.GetViewName() != null)
            {
                GenerateTableName(entityType);
            }

            var lines = new List<string>(
                _annotationCodeGenerator.GenerateFluentApiCalls(entityType, annotations).Select(m => _code.Fragment(m))
                    .Concat(GenerateAnnotations(annotations.Values)));

            AppendMultiLineFluentApi(entityType, lines);

            foreach (var index in entityType.GetIndexes())
            {
                // If there are annotations that cannot be represented using an IndexAttribute then use fluent API even
                // if useDataAnnotations is true.
                var indexAnnotations = _annotationCodeGenerator
                    .FilterIgnoredAnnotations(index.GetAnnotations())
                    .ToDictionary(a => a.Name, a => a);
                _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(index, indexAnnotations);

                if (!useDataAnnotations || indexAnnotations.Count > 0)
                {
                    GenerateIndex(index);
                }
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
                if (!useDataAnnotations)
                {
                    var line = new List<string> { $".{nameof(EntityTypeBuilder.HasNoKey)}()" };

                    AppendMultiLineFluentApi(entityType, line);
                }

                return;
            }

            var annotations = _annotationCodeGenerator
                .FilterIgnoredAnnotations(key.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);
            _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(key, annotations);

            var explicitName = key.GetName() != key.GetDefaultName();
            annotations.Remove(RelationalAnnotationNames.Name);

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

            var lines = new List<string> { $".{nameof(EntityTypeBuilder.HasKey)}({_code.Lambda(key.Properties, "e")})" };

            if (explicitName)
            {
                lines.Add(
                    $".{nameof(RelationalKeyBuilderExtensions.HasName)}({_code.Literal(key.GetName())})");
            }

            lines.AddRange(
                _annotationCodeGenerator.GenerateFluentApiCalls(key, annotations).Select(m => _code.Fragment(m))
                    .Concat(GenerateAnnotations(annotations.Values)));

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

                var lines = new List<string> { $".{nameof(RelationalEntityTypeBuilderExtensions.ToTable)}({parameterString})" };

                AppendMultiLineFluentApi(entityType, lines);
            }

            var viewName = entityType.GetViewName();
            var viewSchema = entityType.GetViewSchema();

            var explicitViewSchema = viewSchema != null && viewSchema != defaultSchema;
            var explicitViewTable = explicitViewSchema || viewName != null;

            if (explicitViewTable)
            {
                var parameterString = _code.Literal(viewName);
                if (explicitViewSchema)
                {
                    parameterString += ", " + _code.Literal(viewSchema);
                }

                var lines = new List<string> { $".{nameof(RelationalEntityTypeBuilderExtensions.ToView)}({parameterString})" };

                AppendMultiLineFluentApi(entityType, lines);
            }
        }

        private void GenerateIndex(IIndex index)
        {
            var annotations = _annotationCodeGenerator
                .FilterIgnoredAnnotations(index.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);
            _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(index, annotations);

            var lines = new List<string>
            {
                $".{nameof(EntityTypeBuilder.HasIndex)}({_code.Lambda(index.Properties, "e")}, "
                + $"{_code.Literal(index.GetDatabaseName())})"
            };
            annotations.Remove(RelationalAnnotationNames.Name);

            if (index.IsUnique)
            {
                lines.Add($".{nameof(IndexBuilder.IsUnique)}()");
            }

            lines.AddRange(
                _annotationCodeGenerator.GenerateFluentApiCalls(index, annotations).Select(m => _code.Fragment(m))
                    .Concat(GenerateAnnotations(annotations.Values)));

            AppendMultiLineFluentApi(index.DeclaringEntityType, lines);
        }

        private void GenerateProperty(IProperty property, bool useDataAnnotations)
        {
            var lines = new List<string> { $".{nameof(EntityTypeBuilder.Property)}({_code.Lambda(new[] { property.Name }, "e")})" };

            var annotations = _annotationCodeGenerator
                .FilterIgnoredAnnotations(property.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);
            _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(property, annotations);
            annotations.Remove(ScaffoldingAnnotationNames.ColumnOrdinal);

            if (useDataAnnotations)
            {
                // Strip out any annotations handled as attributes - these are already handled when generating
                // the entity's properties
                // Only relational ones need to be removed here. Core ones are already removed by FilterIgnoredAnnotations
                annotations.Remove(RelationalAnnotationNames.ColumnName);
                annotations.Remove(RelationalAnnotationNames.ColumnType);

                _ = _annotationCodeGenerator.GenerateDataAnnotationAttributes(property, annotations);
            }
            else
            {
                if (!property.IsNullable
                    && property.ClrType.IsNullableType()
                    && !property.IsPrimaryKey())
                {
                    lines.Add($".{nameof(PropertyBuilder.IsRequired)}()");
                }

                var columnType = property.GetConfiguredColumnType();
                if (columnType != null)
                {
                    lines.Add(
                        $".{nameof(RelationalPropertyBuilderExtensions.HasColumnType)}({_code.Literal(columnType)})");
                    annotations.Remove(RelationalAnnotationNames.ColumnType);
                }

                var maxLength = property.GetMaxLength();
                if (maxLength.HasValue)
                {
                    lines.Add(
                        $".{nameof(PropertyBuilder.HasMaxLength)}({_code.Literal(maxLength.Value)})");
                }
            }

            var precision = property.GetPrecision();
            var scale = property.GetScale();
            if (precision != null && scale != null && scale != 0)
            {
                lines.Add(
                    $".{nameof(PropertyBuilder.HasPrecision)}({_code.Literal(precision.Value)}, {_code.Literal(scale.Value)})");
            }
            else if (precision != null)
            {
                lines.Add(
                    $".{nameof(PropertyBuilder.HasPrecision)}({_code.Literal(precision.Value)})");
            }

            if (property.IsUnicode() != null)
            {
                lines.Add(
                    $".{nameof(PropertyBuilder.IsUnicode)}({(property.IsUnicode() == false ? "false" : "")})");
            }

            var defaultValue = property.GetDefaultValue();
            if (defaultValue == DBNull.Value)
            {
                lines.Add($".{nameof(RelationalPropertyBuilderExtensions.HasDefaultValue)}()");
                annotations.Remove(RelationalAnnotationNames.DefaultValue);
            }
            else if (defaultValue != null)
            {
                lines.Add(
                    $".{nameof(RelationalPropertyBuilderExtensions.HasDefaultValue)}({_code.UnknownLiteral(defaultValue)})");
                annotations.Remove(RelationalAnnotationNames.DefaultValue);
            }

            var valueGenerated = property.ValueGenerated;
            var isRowVersion = false;
            if (((IConventionProperty)property).GetValueGeneratedConfigurationSource() is ConfigurationSource
                valueGeneratedConfigurationSource
                && valueGeneratedConfigurationSource != ConfigurationSource.Convention
                && ValueGenerationConvention.GetValueGenerated(property) != valueGenerated)
            {
                var methodName = valueGenerated switch
                {
                    ValueGenerated.OnAdd => nameof(PropertyBuilder.ValueGeneratedOnAdd),
                    ValueGenerated.OnAddOrUpdate => property.IsConcurrencyToken
                        ? nameof(PropertyBuilder.IsRowVersion)
                        : nameof(PropertyBuilder.ValueGeneratedOnAddOrUpdate),
                    ValueGenerated.OnUpdate => nameof(PropertyBuilder.ValueGeneratedOnUpdate),
                    ValueGenerated.Never => nameof(PropertyBuilder.ValueGeneratedNever),
                    _ => throw new InvalidOperationException(DesignStrings.UnhandledEnumValue($"{nameof(ValueGenerated)}.{valueGenerated}"))
                };

                lines.Add($".{methodName}()");
            }

            if (property.IsConcurrencyToken
                && !isRowVersion)
            {
                lines.Add($".{nameof(PropertyBuilder.IsConcurrencyToken)}()");
            }

            lines.AddRange(
                _annotationCodeGenerator.GenerateFluentApiCalls(property, annotations).Select(m => _code.Fragment(m))
                    .Concat(GenerateAnnotations(annotations.Values)));

            switch (lines.Count)
            {
                case 1:
                    return;
                case 2:
                    lines = new List<string> { lines[0] + lines[1] };
                    break;
            }

            AppendMultiLineFluentApi(property.DeclaringEntityType, lines);
        }

        private void GenerateRelationship(IForeignKey foreignKey, bool useDataAnnotations)
        {
            var canUseDataAnnotations = true;
            var annotations = _annotationCodeGenerator
                .FilterIgnoredAnnotations(foreignKey.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);
            _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(foreignKey, annotations);

            var lines = new List<string>
            {
                $".{nameof(EntityTypeBuilder.HasOne)}("
                + (foreignKey.DependentToPrincipal != null ? $"d => d.{foreignKey.DependentToPrincipal.Name}" : null)
                + ")",
                $".{(foreignKey.IsUnique ? nameof(ReferenceNavigationBuilder.WithOne) : nameof(ReferenceNavigationBuilder.WithMany))}"
                + "("
                + (foreignKey.PrincipalToDependent != null ? $"p => p.{foreignKey.PrincipalToDependent.Name}" : null)
                + ")"
            };

            if (!foreignKey.PrincipalKey.IsPrimaryKey())
            {
                canUseDataAnnotations = false;
                lines.Add(
                    $".{nameof(ReferenceReferenceBuilder.HasPrincipalKey)}"
                    + (foreignKey.IsUnique ? $"<{foreignKey.PrincipalEntityType.DisplayName()}>" : "")
                    + $"({_code.Lambda(foreignKey.PrincipalKey.Properties, "p")})");
            }

            lines.Add(
                $".{nameof(ReferenceReferenceBuilder.HasForeignKey)}"
                + (foreignKey.IsUnique ? $"<{foreignKey.DeclaringEntityType.DisplayName()}>" : "")
                + $"({_code.Lambda(foreignKey.Properties, "d")})");

            var defaultOnDeleteAction = foreignKey.IsRequired
                ? DeleteBehavior.Cascade
                : DeleteBehavior.ClientSetNull;

            if (foreignKey.DeleteBehavior != defaultOnDeleteAction)
            {
                canUseDataAnnotations = false;
                lines.Add(
                    $".{nameof(ReferenceReferenceBuilder.OnDelete)}({_code.Literal(foreignKey.DeleteBehavior)})");
            }

            if (!string.IsNullOrEmpty((string)foreignKey[RelationalAnnotationNames.Name]))
            {
                canUseDataAnnotations = false;
            }

            lines.AddRange(
                _annotationCodeGenerator.GenerateFluentApiCalls(foreignKey, annotations).Select(m => _code.Fragment(m))
                    .Concat(GenerateAnnotations(annotations.Values)));

            if (!useDataAnnotations
                || !canUseDataAnnotations)
            {
                AppendMultiLineFluentApi(foreignKey.DeclaringEntityType, lines);
            }
        }

        private void GenerateSequence(ISequence sequence)
        {
            var methodName = nameof(RelationalModelBuilderExtensions.HasSequence);

            if (sequence.Type != Sequence.DefaultClrType)
            {
                methodName += $"<{_code.Reference(sequence.Type)}>";
            }

            var parameters = _code.Literal(sequence.Name);

            if (!string.IsNullOrEmpty(sequence.Schema)
                && sequence.Model.GetDefaultSchema() != sequence.Schema)
            {
                parameters += $", {_code.Literal(sequence.Schema)}";
            }

            var lines = new List<string> { $"modelBuilder.{methodName}({parameters})" };

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
                lines = new List<string> { lines[0] + lines[1] };
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

        private IList<string> GenerateAnnotations(IEnumerable<IAnnotation> annotations)
            => annotations.Select(
                a => $".HasAnnotation({_code.Literal(a.Name)}, {_code.UnknownLiteral(a.Value)})").ToList();
    }
}
