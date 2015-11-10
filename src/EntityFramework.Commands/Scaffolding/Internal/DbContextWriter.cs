// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Scaffolding.Internal.Configuration;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    public class DbContextWriter
    {
        private const string EntityLambdaIdentifier = "entity";

        private ScaffoldingUtilities ScaffoldingUtilities { get; }
        private IndentedStringBuilder _sb;
        private ModelConfiguration _model;
        private bool _foundFirstFluentApiForEntity;

        public DbContextWriter(
            [NotNull] ScaffoldingUtilities scaffoldingUtilities,
            [NotNull] CSharpUtilities cSharpUtilities)
        {
            Check.NotNull(scaffoldingUtilities, nameof(scaffoldingUtilities));
            Check.NotNull(cSharpUtilities, nameof(cSharpUtilities));

            ScaffoldingUtilities = scaffoldingUtilities;
        }

        public virtual string WriteCode(
            [NotNull] ModelConfiguration modelConfiguration)
        {
            Check.NotNull(modelConfiguration, nameof(modelConfiguration));

            _model = modelConfiguration;
            _sb = new IndentedStringBuilder();

            _sb.AppendLine("using Microsoft.Data.Entity;");
            _sb.AppendLine("using Microsoft.Data.Entity.Metadata;");
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
                AddOnConfiguring();
                AddOnModelCreating();
                AddDbSetProperties();
                AddEntityTypeErrors();
            }
            _sb.AppendLine("}");
        }

        public virtual void AddOnConfiguring()
        {
            _sb.AppendLine("protected override void OnConfiguring(DbContextOptionsBuilder options)");
            _sb.AppendLine("{");
            using (_sb.Indent())
            {
                foreach (var optionsBuilderConfig in _model.OnConfiguringConfigurations)
                {
                    _sb.AppendLine("options." + optionsBuilderConfig.FluentApi + ";");
                }
            }
            _sb.AppendLine("}");
        }

        public virtual void AddOnModelCreating()
        {
            _sb.AppendLine();
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
            }

            _sb.AppendLine("}");
        }

        public virtual void AddEntityFluentApi(
            [NotNull] List<IFluentApiConfiguration> fluentApiConfigurations)
        {
            Check.NotNull(fluentApiConfigurations, nameof(fluentApiConfigurations));

            foreach (var entityFluentApi in fluentApiConfigurations)
            {
                if (_foundFirstFluentApiForEntity)
                {
                    _sb.AppendLine();
                }
                _foundFirstFluentApiForEntity = true;
                _sb.AppendLine(EntityLambdaIdentifier + "." + entityFluentApi.FluentApi + ";");
            }
        }

        public virtual void AddPropertyConfigurations(
            [NotNull] List<PropertyConfiguration> propertyConfigurations)
        {
            Check.NotNull(propertyConfigurations, nameof(propertyConfigurations));

            foreach (var propertyConfig in propertyConfigurations)
            {
                var propertyConfigurationLines =
                    ScaffoldingUtilities.LayoutPropertyConfigurationLines(
                        propertyConfig, "property", "    ", _model.CustomConfiguration.UseFluentApiOnly);
                if (_foundFirstFluentApiForEntity)
                {
                    _sb.AppendLine();
                }
                _foundFirstFluentApiForEntity = true;

                if (propertyConfigurationLines.Count == 1)
                {
                    foreach (var line in propertyConfigurationLines)
                    {
                        _sb.AppendLine(EntityLambdaIdentifier
                            + ".Property(e => e." + propertyConfig.Property.Name + ")" + line + ";");
                    }
                }
                else
                {
                    _sb.AppendLine(EntityLambdaIdentifier
                        + ".Property(e => e." + propertyConfig.Property.Name + ")");
                    using (_sb.Indent())
                    {
                        var lineCount = 0;
                        foreach (string line in propertyConfigurationLines)
                        {
                            var outputLine = line;
                            if (++lineCount == propertyConfigurationLines.Count)
                            {
                                outputLine = line + ";";
                            }
                            _sb.AppendLine(outputLine);
                        }
                    }
                }
            }
        }

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

        public virtual void AddDbSetProperties()
        {
            _sb.AppendLine();
            foreach(var entityConfig in _model.EntityConfigurations)
            {
                _sb.AppendLine("public virtual DbSet<"
                    + entityConfig.EntityType.Name
                    + "> " + entityConfig.EntityType.Name
                    + " { get; set; }");
            }
        }

        public virtual void AddEntityTypeErrors()
        {
            if (_model.Model.Scaffolding().EntityTypeErrors.Count == 0)
            {
                return;
            }

            _sb.AppendLine();
            foreach (var entityConfig in _model.Model.Scaffolding().EntityTypeErrors)
            {
                _sb.Append("// ")
                    .Append(entityConfig.Value)
                    .AppendLine(" Please see the warning messages.");
            }
        }
    }
}
