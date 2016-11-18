// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Configuration.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DbContextWriter
    {
        private const string EntityLambdaIdentifier = "entity";

        private ScaffoldingUtilities ScaffoldingUtilities { get; }
        private IndentedStringBuilder _sb;
        private ModelConfiguration _model;
        private bool _foundFirstFluentApiForEntity;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbContextWriter(
            [NotNull] ScaffoldingUtilities scaffoldingUtilities,
            [NotNull] CSharpUtilities cSharpUtilities)
        {
            Check.NotNull(scaffoldingUtilities, nameof(scaffoldingUtilities));
            Check.NotNull(cSharpUtilities, nameof(cSharpUtilities));

            ScaffoldingUtilities = scaffoldingUtilities;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string WriteCode(
            [NotNull] ModelConfiguration modelConfiguration)
        {
            Check.NotNull(modelConfiguration, nameof(modelConfiguration));

            _model = modelConfiguration;
            _sb = new IndentedStringBuilder();

            _sb.AppendLine("using System;"); // Guid default values require new Guid() which requires this using
            _sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            _sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata;");
            _sb.AppendLine();
            _sb.AppendLine("namespace " + _model.Namespace());
            _sb.AppendLine("{");
            using (_sb.Indent())
            {
                AddClass();
            }
            _sb.Append("}");

            return _sb.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddClass()
        {
            var className =
                string.IsNullOrWhiteSpace(_model.CustomConfiguration.ContextClassName)
                    ? _model.ClassName()
                    : _model.CustomConfiguration.ContextClassName;
            _sb.AppendLine("public partial class " + className + " : DbContext");
            _sb.AppendLine("{");
            using (_sb.Indent())
            {
                AddDbSetProperties();
                AddEntityTypeErrors();
                AddOnConfiguring();
                _sb.AppendLine();
                AddOnModelCreating();
            }
            _sb.AppendLine("}");
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddOnConfiguring()
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

                    foreach (var optionsBuilderConfig in _model.OnConfiguringConfigurations)
                    {
                        if (optionsBuilderConfig.FluentApiLines.Count == 0)
                        {
                            continue;
                        }

                        _sb.Append("optionsBuilder." + optionsBuilderConfig.FluentApiLines.First());
                        using (_sb.Indent())
                        {
                            foreach (var line in optionsBuilderConfig.FluentApiLines.Skip(1))
                            {
                                _sb.AppendLine();
                                _sb.Append(line);
                            }
                        }
                        _sb.AppendLine(";");
                    }
                }

                _sb.AppendLine("}");
            }
            _sb.AppendLine("}");
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddOnModelCreating()
        {
            _sb.AppendLine("protected override void OnModelCreating(ModelBuilder modelBuilder)");
            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                var first = true;
                foreach (var entityConfig in _model.EntityConfigurations)
                {
                    var fluentApiConfigurations = entityConfig.GetFluentApiConfigurations(_model.CustomConfiguration.UseFluentApiOnly);
                    var propertyConfigurations = entityConfig.GetPropertyConfigurations(_model.CustomConfiguration.UseFluentApiOnly);
                    var relationshipConfigurations = entityConfig.GetRelationshipConfigurations(_model.CustomConfiguration.UseFluentApiOnly);
                    if (fluentApiConfigurations.Count == 0
                        && propertyConfigurations.Count == 0
                        && relationshipConfigurations.Count == 0)
                    {
                        continue;
                    }

                    if (!first)
                    {
                        _sb.AppendLine();
                    }
                    first = false;

                    _sb.AppendLine("modelBuilder.Entity<"
                                   + entityConfig.EntityType.Name + ">("
                                   + EntityLambdaIdentifier + " =>");
                    _sb.AppendLine("{");
                    using (_sb.Indent())
                    {
                        _foundFirstFluentApiForEntity = false;
                        AddEntityFluentApi(fluentApiConfigurations);
                        AddPropertyConfigurations(propertyConfigurations);
                        AddRelationshipConfigurations(relationshipConfigurations);
                    }
                    _sb.AppendLine("});");
                }

                foreach (var sequenceConfig in _model.SequenceConfigurations)
                {
                    if (!first)
                    {
                        _sb.AppendLine();
                    }
                    first = false;

                    _sb.Append("modelBuilder.HasSequence")
                        .Append(!string.IsNullOrEmpty(sequenceConfig.TypeIdentifier) ? "<" + sequenceConfig.TypeIdentifier + ">" : "")
                        .Append("(" + sequenceConfig.NameIdentifier)
                        .Append(!string.IsNullOrEmpty(sequenceConfig.SchemaNameIdentifier) ? ", " + sequenceConfig.SchemaNameIdentifier : "")
                        .Append(")");

                    AddFluentConfigurations(sequenceConfig.FluentApiConfigurations);
                }
            }

            _sb.AppendLine("}");
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddEntityFluentApi(
            [NotNull] List<IFluentApiConfiguration> fluentApiConfigurations)
        {
            Check.NotNull(fluentApiConfigurations, nameof(fluentApiConfigurations));

            foreach (var entityFluentApi in fluentApiConfigurations)
            {
                if (entityFluentApi.FluentApiLines.Count == 0)
                {
                    continue;
                }

                if (_foundFirstFluentApiForEntity)
                {
                    _sb.AppendLine();
                }
                _foundFirstFluentApiForEntity = true;

                _sb.Append(EntityLambdaIdentifier + "." + entityFluentApi.FluentApiLines.First());
                if (entityFluentApi.FluentApiLines.Count > 1)
                {
                    using (_sb.Indent())
                    {
                        foreach (var line in entityFluentApi.FluentApiLines.Skip(1))
                        {
                            _sb.AppendLine();
                            _sb.Append(line);
                        }
                    }
                }
                _sb.AppendLine(";");
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddPropertyConfigurations(
            [NotNull] List<PropertyConfiguration> propertyConfigurations)
        {
            Check.NotNull(propertyConfigurations, nameof(propertyConfigurations));

            foreach (var propertyConfig in propertyConfigurations)
            {
                var fluentApiConfigurations =
                    propertyConfig.GetFluentApiConfigurations(_model.CustomConfiguration.UseFluentApiOnly);
                if (fluentApiConfigurations.Count == 0)
                {
                    continue;
                }

                if (_foundFirstFluentApiForEntity)
                {
                    _sb.AppendLine();
                }
                _foundFirstFluentApiForEntity = true;

                _sb.Append(EntityLambdaIdentifier
                           + ".Property(e => e." + propertyConfig.Property.Name + ")");

                AddFluentConfigurations(fluentApiConfigurations);
            }
        }

        private void AddFluentConfigurations(List<FluentApiConfiguration> fluentApiConfigurations)
        {
            if (fluentApiConfigurations.Count > 1)
            {
                _sb.AppendLine();
                _sb.IncrementIndent();
            }

            var first = true;
            foreach (var fluentApiConfiguration in fluentApiConfigurations)
            {
                if (!first)
                {
                    _sb.AppendLine();
                }
                first = false;

                foreach (var line in fluentApiConfiguration.FluentApiLines)
                {
                    _sb.Append("." + line);
                }
            }

            _sb.AppendLine(";");
            if (fluentApiConfigurations.Count > 1)
            {
                _sb.DecrementIndent();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddRelationshipConfigurations(
            [NotNull] List<RelationshipConfiguration> relationshipConfigurations)
        {
            Check.NotNull(relationshipConfigurations, nameof(relationshipConfigurations));

            foreach (var relationshipConfig in relationshipConfigurations)
            {
                if (_foundFirstFluentApiForEntity)
                {
                    _sb.AppendLine();
                }
                _foundFirstFluentApiForEntity = true;
                ScaffoldingUtilities.LayoutRelationshipConfigurationLines(
                    _sb, EntityLambdaIdentifier, relationshipConfig, "d", "p");
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddDbSetProperties()
        {
            if (!_model.EntityConfigurations.Any())
            {
                return;
            }

            foreach (var entityConfig in _model.EntityConfigurations)
            {
                _sb.AppendLine("public virtual DbSet<"
                               + entityConfig.EntityType.Name
                               + "> " + entityConfig.EntityType.Name
                               + " { get; set; }");
            }

            _sb.AppendLine();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddEntityTypeErrors()
        {
            if (_model.Model.Scaffolding().EntityTypeErrors.Count == 0)
            {
                return;
            }

            foreach (var entityConfig in _model.Model.Scaffolding().EntityTypeErrors)
            {
                _sb.Append("// ")
                    .Append(entityConfig.Value)
                    .AppendLine(" Please see the warning messages.");
            }

            _sb.AppendLine();
        }
    }
}
