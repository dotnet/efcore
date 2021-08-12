// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
        private readonly IndentedStringBuilder _builder = new();
        private readonly HashSet<string> _namespaces = new();
        private bool _entityTypeBuilderInitialized;
        private bool _useDataAnnotations;
        private bool _useNullableReferenceTypes;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CSharpDbContextGenerator(
            IProviderConfigurationCodeGenerator providerConfigurationCodeGenerator,
            IAnnotationCodeGenerator annotationCodeGenerator,
            ICSharpHelper cSharpHelper)
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
            string? contextNamespace,
            string? modelNamespace,
            bool useDataAnnotations,
            bool useNullableReferenceTypes,
            bool suppressConnectionStringWarning,
            bool suppressOnConfiguring)
        {
            Check.NotNull(model, nameof(model));

            _useDataAnnotations = useDataAnnotations;
            _useNullableReferenceTypes = useNullableReferenceTypes;

            _builder.Clear();
            _namespaces.Clear();

            _namespaces.Add("System"); // Guid default values require new Guid() which requires this using
            _namespaces.Add("Microsoft.EntityFrameworkCore");
            _namespaces.Add("Microsoft.EntityFrameworkCore.Metadata");

            // The final namespaces list is calculated after code generation, since namespaces may be added during code generation

            var finalContextNamespace = contextNamespace ?? modelNamespace;

            if (!string.IsNullOrEmpty(finalContextNamespace))
            {
                _builder.AppendLine($"namespace {finalContextNamespace}");
                _builder.AppendLine("{");
                _builder.IncrementIndent();
            }

            GenerateClass(
                model,
                contextName,
                connectionString,
                suppressConnectionStringWarning,
                suppressOnConfiguring);

            if (!string.IsNullOrEmpty(finalContextNamespace))
            {
                _builder.DecrementIndent();
                _builder.AppendLine("}");
            }

            var namespaceStringBuilder = new StringBuilder();

            IEnumerable<string> namespaces = _namespaces.OrderBy(
                    ns => ns switch
                    {
                        "System" => 1,
                        var s when s.StartsWith("System", StringComparison.Ordinal) => 2,
                        var s when s.StartsWith("Microsoft", StringComparison.Ordinal) => 3,
                        _ => 4
                    })
                .ThenBy(ns => ns);

            if (finalContextNamespace != modelNamespace && !string.IsNullOrEmpty(modelNamespace))
            {
                namespaces = namespaces.Append(modelNamespace);
            }

            foreach (var @namespace in namespaces)
            {
                namespaceStringBuilder.Append("using ").Append(@namespace).AppendLine(";");
            }

            namespaceStringBuilder.AppendLine();

            return namespaceStringBuilder.ToString() + _builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GenerateClass(
            IModel model,
            string contextName,
            string connectionString,
            bool suppressConnectionStringWarning,
            bool suppressOnConfiguring)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(contextName, nameof(contextName));
            Check.NotNull(connectionString, nameof(connectionString));

            _builder.AppendLine($"public partial class {contextName} : DbContext");
            _builder.AppendLine("{");

            using (_builder.Indent())
            {
                GenerateConstructors(contextName);
                GenerateDbSets(model);
                GenerateEntityTypeErrors(model);
                if (!suppressOnConfiguring)
                {
                    GenerateOnConfiguring(connectionString, suppressConnectionStringWarning);
                }

                GenerateOnModelCreating(model);
            }

            _builder.AppendLine();

            using (_builder.Indent())
            {
                _builder.AppendLine("partial void OnModelCreatingPartial(ModelBuilder modelBuilder);");
            }

            _builder.AppendLine("}");
        }

        private void GenerateConstructors(string contextName)
        {
            _builder.AppendLine($"public {contextName}()")
                .AppendLine("{")
                .AppendLine("}")
                .AppendLine();

            _builder.AppendLine($"public {contextName}(DbContextOptions<{contextName}> options)")
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
                _builder.Append($"public virtual DbSet<{entityType.Name}> {entityType.GetDbSetName()} {{ get; set; }}");

                if (_useNullableReferenceTypes)
                {
                    _builder.Append(" = null!;");
                }

                _builder.AppendLine();
            }

            if (model.GetEntityTypes().Any())
            {
                _builder.AppendLine();
            }
        }

        private void GenerateEntityTypeErrors(IModel model)
        {
            var errors = model.GetEntityTypeErrors();
            foreach (var entityTypeError in errors)
            {
                _builder.AppendLine($"// {entityTypeError.Value} Please see the warning messages.");
            }

            if (errors.Count > 0)
            {
                _builder.AppendLine();
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GenerateOnConfiguring(
            string connectionString,
            bool suppressConnectionStringWarning)
        {
            Check.NotNull(connectionString, nameof(connectionString));

            _builder.AppendLine("protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
            _builder.AppendLine("{");

            using (_builder.Indent())
            {
                _builder.AppendLine("if (!optionsBuilder.IsConfigured)");
                _builder.AppendLine("{");

                using (_builder.Indent())
                {
                    if (!suppressConnectionStringWarning)
                    {
                        _builder.DecrementIndent()
                            .DecrementIndent()
                            .DecrementIndent()
                            .DecrementIndent()
                            .AppendLine("#warning " + DesignStrings.SensitiveInformationWarning)
                            .IncrementIndent()
                            .IncrementIndent()
                            .IncrementIndent()
                            .IncrementIndent();
                    }

                    var useProviderCall = _providerConfigurationCodeGenerator.GenerateUseProvider(
                        connectionString);

                    _builder
                        .AppendLines(_code.Fragment(useProviderCall, "optionsBuilder"), skipFinalNewline: true)
                        .AppendLine(";");
                }

                _builder.AppendLine("}");
            }

            _builder.AppendLine("}");

            _builder.AppendLine();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GenerateOnModelCreating(IModel model)
        {
            Check.NotNull(model, nameof(model));

            _builder.AppendLine("protected override void OnModelCreating(ModelBuilder modelBuilder)");
            _builder.Append("{");

            var annotations = _annotationCodeGenerator
                .FilterIgnoredAnnotations(model.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);

            _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(model, annotations);

            annotations.Remove(CoreAnnotationNames.ProductVersion);
            annotations.Remove(RelationalAnnotationNames.MaxIdentifierLength);
            annotations.Remove(ScaffoldingAnnotationNames.DatabaseName);
            annotations.Remove(ScaffoldingAnnotationNames.EntityTypeErrors);

            var lines = new List<string>();

            GenerateAnnotations(model, annotations, lines);

            if (lines.Count > 0)
            {
                using (_builder.Indent())
                {
                    _builder.AppendLine();
                    _builder.Append("modelBuilder" + lines[0]);

                    using (_builder.Indent())
                    {
                        foreach (var line in lines.Skip(1))
                        {
                            _builder.AppendLine();
                            _builder.Append(line);
                        }
                    }

                    _builder.AppendLine(";");
                }
            }

            using (_builder.Indent())
            {
                foreach (var entityType in model.GetEntityTypes())
                {
                    _entityTypeBuilderInitialized = false;

                    GenerateEntityType(entityType);

                    if (_entityTypeBuilderInitialized)
                    {
                        _builder.AppendLine("});");
                    }
                }

                foreach (var sequence in model.GetSequences())
                {
                    GenerateSequence(sequence);
                }
            }

            _builder.AppendLine();

            using (_builder.Indent())
            {
                _builder.AppendLine("OnModelCreatingPartial(modelBuilder);");
            }

            _builder.AppendLine("}");
        }

        private void InitializeEntityTypeBuilder(IEntityType entityType)
        {
            if (!_entityTypeBuilderInitialized)
            {
                _builder.AppendLine();
                _builder.AppendLine($"modelBuilder.Entity<{entityType.Name}>({EntityLambdaIdentifier} =>");
                _builder.Append("{");
            }

            _entityTypeBuilderInitialized = true;
        }

        private void GenerateEntityType(IEntityType entityType)
        {
            GenerateKey(entityType.FindPrimaryKey(), entityType);

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

            if (_useDataAnnotations)
            {
                // Strip out any annotations handled as attributes - these are already handled when generating
                // the entity's properties
                _ = _annotationCodeGenerator.GenerateDataAnnotationAttributes(entityType, annotations);
            }

            if (!_useDataAnnotations || entityType.GetViewName() != null)
            {
                GenerateTableName(entityType);
            }

            var lines = new List<string>();

            GenerateAnnotations(entityType, annotations, lines);

            AppendMultiLineFluentApi(entityType, lines);

            foreach (var index in entityType.GetIndexes())
            {
                // If there are annotations that cannot be represented using an IndexAttribute then use fluent API even
                // if useDataAnnotations is true.
                var indexAnnotations = _annotationCodeGenerator
                    .FilterIgnoredAnnotations(index.GetAnnotations())
                    .ToDictionary(a => a.Name, a => a);
                _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(index, indexAnnotations);

                if (!_useDataAnnotations || indexAnnotations.Count > 0)
                {
                    GenerateIndex(index);
                }
            }

            foreach (var property in entityType.GetProperties())
            {
                GenerateProperty(property);
            }

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                GenerateRelationship(foreignKey);
            }
        }

        private void AppendMultiLineFluentApi(IEntityType entityType, IList<string> lines)
        {
            if (lines.Count <= 0)
            {
                return;
            }

            InitializeEntityTypeBuilder(entityType);

            using (_builder.Indent())
            {
                _builder.AppendLine();

                _builder.Append(EntityLambdaIdentifier + lines[0]);

                using (_builder.Indent())
                {
                    foreach (var line in lines.Skip(1))
                    {
                        _builder.AppendLine();
                        _builder.Append(line);
                    }
                }

                _builder.AppendLine(";");
            }
        }

        private void GenerateKey(IKey? key, IEntityType entityType)
        {
            if (key == null)
            {
                if (!_useDataAnnotations)
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
                if (key is IConventionKey conventionKey
                    && conventionKey.Properties.SequenceEqual(
                        KeyDiscoveryConvention.DiscoverKeyProperties(
                            conventionKey.DeclaringEntityType,
                            conventionKey.DeclaringEntityType.GetProperties())))
                {
                    return;
                }

                if (!explicitName
                    && _useDataAnnotations)
                {
                    return;
                }
            }

            var lines = new List<string> { $".{nameof(EntityTypeBuilder.HasKey)}({_code.Lambda(key.Properties, "e")})" };

            if (explicitName)
            {
                lines.Add(
                    $".{nameof(RelationalKeyBuilderExtensions.HasName)}({_code.Literal(key.GetName()!)})");
            }

            GenerateAnnotations(key, annotations, lines);

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
                var parameterString = _code.Literal(tableName!);
                if (explicitSchema)
                {
                    parameterString += ", " + _code.Literal(schema!);
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
                var parameterString = _code.Literal(viewName!);
                if (explicitViewSchema)
                {
                    parameterString += ", " + _code.Literal(viewSchema!);
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
                $".{nameof(EntityTypeBuilder.HasIndex)}({_code.Lambda(index.Properties, "e")}, {_code.Literal(index.GetDatabaseName())})"
            };
            annotations.Remove(RelationalAnnotationNames.Name);

            if (index.IsUnique)
            {
                lines.Add($".{nameof(IndexBuilder.IsUnique)}()");
            }

            GenerateAnnotations(index, annotations, lines);

            AppendMultiLineFluentApi(index.DeclaringEntityType, lines);
        }

        private void GenerateProperty(IProperty property)
        {
            var lines = new List<string> { $".{nameof(EntityTypeBuilder.Property)}({_code.Lambda(new[] { property.Name }, "e")})" };

            var annotations = _annotationCodeGenerator
                .FilterIgnoredAnnotations(property.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);
            _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(property, annotations);
            annotations.Remove(ScaffoldingAnnotationNames.ColumnOrdinal);

            if (_useDataAnnotations)
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
                if ((!_useNullableReferenceTypes || property.ClrType.IsValueType)
                    && !property.IsNullable
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
            }

            if (property.TryGetDefaultValue(out var defaultValue))
            {
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

            GenerateAnnotations(property, annotations, lines);

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

        private void GenerateRelationship(IForeignKey foreignKey)
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
                    + (foreignKey.IsUnique ? $"<{foreignKey.PrincipalEntityType.Name}>" : "")
                    + $"({_code.Lambda(foreignKey.PrincipalKey.Properties, "p")})");
            }

            lines.Add(
                $".{nameof(ReferenceReferenceBuilder.HasForeignKey)}"
                + (foreignKey.IsUnique ? $"<{foreignKey.DeclaringEntityType.Name}>" : "")
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

            if (!string.IsNullOrEmpty((string?)foreignKey[RelationalAnnotationNames.Name]))
            {
                canUseDataAnnotations = false;
            }

            GenerateAnnotations(foreignKey, annotations, lines);

            if (!_useDataAnnotations
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

            _builder.AppendLine();
            _builder.Append(lines[0]);

            using (_builder.Indent())
            {
                foreach (var line in lines.Skip(1))
                {
                    _builder.AppendLine();
                    _builder.Append(line);
                }
            }

            _builder.AppendLine(";");
        }

        private void GenerateAnnotations(IAnnotatable annotatable, Dictionary<string, IAnnotation> annotations, List<string> lines)
        {
            foreach (var call in _annotationCodeGenerator.GenerateFluentApiCalls(annotatable, annotations))
            {
                var fluentApiCall = call;

                // Remove optional arguments
                if (fluentApiCall.MethodInfo is { } methodInfo)
                {
                    var methodParameters = methodInfo.GetParameters();

                    var paramOffset = methodInfo.IsStatic ? 1 : 0;

                    for (int i = fluentApiCall.Arguments.Count - 1; i >= 0; i--)
                    {
                        if (!methodParameters[i + paramOffset].HasDefaultValue)
                        {
                            break;
                        }

                        var defaultValue = methodParameters[i + paramOffset].DefaultValue;
                        var argument = fluentApiCall.Arguments[i];

                        if (argument is null && defaultValue is null || argument is not null && argument.Equals(defaultValue))
                        {
                            fluentApiCall = new MethodCallCodeFragment(methodInfo, fluentApiCall.Arguments.Take(i).ToArray());
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                lines.Add(_code.Fragment(fluentApiCall));

                if (fluentApiCall.Namespace is not null)
                {
                    _namespaces.Add(fluentApiCall.Namespace);
                }
            }

            lines.AddRange(annotations.Values.Select(
                a => $".HasAnnotation({_code.Literal(a.Name)}, {_code.UnknownLiteral(a.Value)})"));
        }
    }
}
