// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CSharpDbContextGenerator
    {
        private const string EntityLambdaIdentifier = "entity";

        private readonly CSharpUtilities _cSharpUtilities;
        private readonly IScaffoldingHelper _scaffoldingHelper;
        private IndentedStringBuilder _sb;
        private bool _entityTypeBuilderInitialized;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CSharpDbContextGenerator(
            [NotNull] IScaffoldingHelper scaffoldingHelper,
            [NotNull] CSharpUtilities cSharpUtilities)
        {
            Check.NotNull(scaffoldingHelper, nameof(scaffoldingHelper));
            Check.NotNull(cSharpUtilities, nameof(cSharpUtilities));

            _scaffoldingHelper = scaffoldingHelper;
            _cSharpUtilities = cSharpUtilities;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string WriteCode(
            [NotNull] IModel model,
            [NotNull] string @namespace,
            [NotNull] string contextName,
            [NotNull] string connectionString,
            bool useDataAnnotations)
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
                GenerateClass(model, contextName, connectionString, useDataAnnotations);
            }

            _sb.Append("}");

            return _sb.ToString();
        }

        private void GenerateClass(IModel model, string contextName, string connectionString, bool useDataAnnotations)
        {
            _sb.AppendLine($"public partial class {contextName} : DbContext");
            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                GenerateDbSets(model);
                GenerateEntityTypeErrors(model);
                GenerateOnConfiguring(connectionString);
                GenerateOnModelCreating(model, useDataAnnotations);
            }

            _sb.AppendLine("}");
        }

        private void GenerateDbSets(IModel model)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                _sb.AppendLine($"public virtual DbSet<{entityType.Name}> {entityType.Scaffolding().DbSetName} {{ get; set; }}");
            }

            if (model.GetEntityTypes().Any())
            {
                _sb.AppendLine();
            }
        }

        private void GenerateEntityTypeErrors(IModel model)
        {
            foreach (var entityTypeError in model.Scaffolding().EntityTypeErrors)
            {
                _sb.AppendLine($"// {entityTypeError.Value} Please see the warning messages.");
            }

            if (model.Scaffolding().EntityTypeErrors.Any())
            {
                _sb.AppendLine();
            }
        }

        private void GenerateOnConfiguring(string connectionString)
        {
            _sb.AppendLine("protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                _sb.AppendLine("if (!optionsBuilder.IsConfigured)");
                _sb.AppendLine("{");

                using (_sb.Indent())
                {
                    _sb.AppendLine("#warning " + DesignStrings.SensitiveInformationWarning);

                    _sb.AppendLine($"optionsBuilder.{_scaffoldingHelper.GetProviderOptionsBuilder(connectionString)}");
                }

                _sb.AppendLine("}");
            }
            _sb.AppendLine("}");

            _sb.AppendLine();
        }


        private void GenerateOnModelCreating(IModel model, bool useDataAnnotations)
        {
            _sb.AppendLine("protected override void OnModelCreating(ModelBuilder modelBuilder)");
            _sb.Append("{");

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

                foreach (var sequence in model.Relational().Sequences)
                {
                    GenerateSequence(sequence);
                }
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
            GenerateKey(entityType.FindPrimaryKey(), useDataAnnotations);

            if (!useDataAnnotations)
            {
                GenerateTableName(entityType);
            }

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

        private void GenerateKey(IKey key, bool useDataAnnotations)
        {
            if (key == null)
            {
                return;
            }

            var explicitName = key.Relational().Name != new RelationalKeyAnnotations(key).GetDefaultName();

            if (key.Properties.Count == 1)
            {

                if (key is Key concreteKey
                    && key.Properties.SequenceEqual(new KeyDiscoveryConvention().DiscoverKeyProperties(concreteKey.DeclaringEntityType, concreteKey.DeclaringEntityType.GetProperties().ToList())))
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
                lines.Add($".{nameof(RelationalKeyBuilderExtensions.HasName)}({_cSharpUtilities.DelimitString(key.Relational().Name)})");
            }

            AppendMultiLineFluentApi(key.DeclaringEntityType, lines);
        }

        private void GenerateTableName(IEntityType entityType)
        {
            var tableName = entityType.Relational().TableName;
            var schema = entityType.Relational().Schema;
            var defaultSchema = entityType.Model.Relational().DefaultSchema;

            var explicitSchema = schema != null && schema != defaultSchema;
            var explicitTable = explicitSchema || tableName != null && tableName != entityType.Scaffolding().DbSetName;

            if (explicitTable)
            {
                var parameterString = _cSharpUtilities.DelimitString(tableName);
                if (explicitSchema)
                {
                    parameterString += ", " + _cSharpUtilities.DelimitString(schema);
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

            if (!string.IsNullOrEmpty(index.Relational().Name))
            {
                lines.Add($".{nameof(RelationalIndexBuilderExtensions.HasName)}({_cSharpUtilities.DelimitString(index.Relational().Name)})");
            }

            if (index.IsUnique)
            {
                lines.Add($".{nameof(IndexBuilder.IsUnique)}()");
            }

            if (index.Relational().Filter != null)
            {
                lines.Add($".{nameof(RelationalIndexBuilderExtensions.HasFilter)}({_cSharpUtilities.DelimitString(index.Relational().Filter)})");
            }

            AppendMultiLineFluentApi(index.DeclaringEntityType, lines);
        }

        private void GenerateProperty(IProperty property, bool useDataAnnotations)
        {
            var lines = new List<string>
            {
                $".{nameof(EntityTypeBuilder.Property)}(e => e.{property.Name})"
            };

            if (!useDataAnnotations)
            {
                if (!property.IsNullable
                    && property.ClrType.IsNullableType()
                    && !property.IsPrimaryKey())
                {
                    lines.Add($".{nameof(PropertyBuilder.IsRequired)}()");
                }

                var columnName = property.Relational().ColumnName;

                if (columnName != null
                    && columnName != property.Name)
                {
                    lines.Add($".{nameof(RelationalPropertyBuilderExtensions.HasColumnName)}({_cSharpUtilities.DelimitString(columnName)})");
                }

                var columnType = property.Relational().ColumnType;

                if (columnType != null)
                {
                    lines.Add($".{nameof(RelationalPropertyBuilderExtensions.HasColumnType)}({_cSharpUtilities.DelimitString(columnType)})");
                }

                var maxLength = property.GetMaxLength();

                if (maxLength.HasValue)
                {
                    lines.Add($".{nameof(PropertyBuilder.HasMaxLength)}({_cSharpUtilities.GenerateLiteral(maxLength.Value)})");
                }
            }

            if (property.Relational().DefaultValue != null)
            {
                lines.Add($".{nameof(RelationalPropertyBuilderExtensions.HasDefaultValue)}({_cSharpUtilities.GenerateLiteral((dynamic)property.Relational().DefaultValue)})");
            }

            if (property.Relational().DefaultValueSql != null)
            {
                lines.Add($".{nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql)}({_cSharpUtilities.DelimitString(property.Relational().DefaultValueSql)})");
            }

            if (property.Relational().ComputedColumnSql != null)
            {
                lines.Add($".{nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql)}({_cSharpUtilities.DelimitString(property.Relational().ComputedColumnSql)})");
            }

            var valueGenerated = property.ValueGenerated;
            if (((Property)property).GetValueGeneratedConfigurationSource().HasValue
                && new RelationalValueGeneratorConvention().GetValueGenerated((Property)property) != valueGenerated)
            {
                string methodName;
                switch (valueGenerated)
                {
                    case ValueGenerated.OnAdd:
                        methodName = nameof(PropertyBuilder.ValueGeneratedOnAdd);
                        break;

                    case ValueGenerated.OnAddOrUpdate:
                        methodName = nameof(PropertyBuilder.ValueGeneratedOnAddOrUpdate);
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
            var lines = new List<string>
            {
                $".{nameof(EntityTypeBuilder.HasOne)}(d => d.{foreignKey.DependentToPrincipal.Name})",
                $".{(foreignKey.IsUnique ? nameof(ReferenceNavigationBuilder.WithOne) : nameof(ReferenceNavigationBuilder.WithMany))}"
                + $"(p => p.{foreignKey.PrincipalToDependent.Name})"
            };

            if (!foreignKey.PrincipalKey.IsPrimaryKey())
            {
                canUseDataAnnotations = false;
                lines.Add($".{nameof(ReferenceReferenceBuilder.HasPrincipalKey)}"
                    + $"{(foreignKey.IsUnique ? $"<{foreignKey.PrincipalEntityType.DisplayName()}>" : "")}"
                    + $"(p => {GenerateLambdaToKey(foreignKey.PrincipalKey.Properties, "p")})");
            }

            lines.Add($".{nameof(ReferenceReferenceBuilder.HasForeignKey)}"
                      + $"{(foreignKey.IsUnique ? $"<{foreignKey.DeclaringEntityType.DisplayName()}>" : "")}"
                      + $"(d => {GenerateLambdaToKey(foreignKey.Properties, "d")})");

            var defaultOnDeleteAction = foreignKey.IsRequired
                ? DeleteBehavior.Cascade
                : DeleteBehavior.Restrict;

            if (foreignKey.DeleteBehavior != defaultOnDeleteAction)
            {
                canUseDataAnnotations = false;
                lines.Add($".{nameof(ReferenceReferenceBuilder.OnDelete)}({_cSharpUtilities.GenerateLiteral(foreignKey.DeleteBehavior)})");
            }

            if (foreignKey.Relational().Name !=
                RelationalForeignKeyAnnotations.GetDefaultForeignKeyName(
                    foreignKey.DeclaringEntityType.Relational().TableName,
                    foreignKey.PrincipalEntityType.Relational().TableName,
                    foreignKey.Properties.Select(p => p.Relational().ColumnName)))
            {
                canUseDataAnnotations = false;
                lines.Add($".{nameof(RelationalReferenceReferenceBuilderExtensions.HasConstraintName)}({_cSharpUtilities.DelimitString(foreignKey.Relational().Name)})");
            }

            if (!useDataAnnotations || !canUseDataAnnotations)
            {
                AppendMultiLineFluentApi(foreignKey.DeclaringEntityType, lines);
            }
        }

        private void GenerateSequence(ISequence sequence)
        {
            var methodName = nameof(RelationalModelBuilderExtensions.HasSequence);

            if (sequence.ClrType != Sequence.DefaultClrType)
            {
                methodName += $"<{_cSharpUtilities.GetTypeName(sequence.ClrType)}>";
            }

            var parameters = _cSharpUtilities.DelimitString(sequence.Name);

            if (string.IsNullOrEmpty(sequence.Schema)
                && sequence.Model.Relational().DefaultSchema != sequence.Schema)
            {
                parameters += $", {_cSharpUtilities.DelimitString(sequence.Schema)}";
            }

            var lines = new List<string>
            {
                $"modelBuilder.{methodName}({parameters})"
            };

            if (sequence.StartValue != Sequence.DefaultStartValue)
            {
                lines.Add($".{nameof(RelationalSequenceBuilder.StartsAt)}({sequence.StartValue})");
            }

            if (sequence.IncrementBy != Sequence.DefaultIncrementBy)
            {
                lines.Add($".{nameof(RelationalSequenceBuilder.IncrementsBy)}({sequence.IncrementBy})");
            }

            if (sequence.MinValue != Sequence.DefaultMinValue)
            {
                lines.Add($".{nameof(RelationalSequenceBuilder.HasMin)}({sequence.MinValue})");
            }

            if (sequence.MaxValue != Sequence.DefaultMaxValue)
            {
                lines.Add($".{nameof(RelationalSequenceBuilder.HasMax)}({sequence.MaxValue})");
            }

            if (sequence.IsCyclic != Sequence.DefaultIsCyclic)
            {
                lines.Add($".{nameof(RelationalSequenceBuilder.IsCyclic)}()");
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

        private string GenerateLambdaToKey(
            IReadOnlyList<IProperty> properties,
            string lambdaIdentifier)
        {
            if (properties.Count <= 0)
            {
                return "";
            }

            return properties.Count == 1
                ? $"{lambdaIdentifier}.{properties[0].Name}"
                : $"new {{ {string.Join(", ", properties.Select(p => lambdaIdentifier + "." + p.Name))} }}";
        }
    }
}
