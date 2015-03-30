// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerDbContextCodeGenerator : DbContextCodeGenerator
    {
        private const string _defaultDbContextName = "ModelContext";
        private const string _dbContextSuffix = "Context";

        public SqlServerDbContextCodeGenerator(
            [NotNull] ReverseEngineeringGenerator generator,
            [NotNull] IModel model, [NotNull] string namespaceName,
            [CanBeNull] string className, [NotNull] string connectionString)
            : base(generator, model, namespaceName, className, connectionString)
        {
            Check.NotNull(generator, nameof(generator));
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(namespaceName, nameof(namespaceName));
            Check.NotEmpty(connectionString, nameof(connectionString));
        }

        public override string ClassName
        {
            get
            {
                string className = base.ClassName;
                if (className != null)
                {
                    return className;
                }

                var builder = new SqlConnectionStringBuilder(ConnectionString);
                if (builder.InitialCatalog != null)
                {
                    return CSharpUtilities.Instance.GenerateCSharpIdentifier(builder.InitialCatalog + _dbContextSuffix, null);
                }

                return _defaultDbContextName;
            }
        }


        public override IEnumerable<IEntityType> OrderedEntityTypes()
        {
            // do not configure EntityTypes for which we had an error when generating
            return Model.EntityTypes.OrderBy(e => e.Name)
                .Where(e => ((EntityType)e).FindAnnotation(
                    SqlServerMetadataModelProvider.AnnotationNameEntityTypeError) == null);
        }

        public override void GenerateNavigationsConfiguration(
            IEntityType entityType, IndentedStringBuilder sb)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(sb, nameof(sb));

            var first = true;
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                var dependentEndNavigationPropertyName =
                    (string)foreignKey[SqlServerMetadataModelProvider.AnnotationNameDependentEndNavPropName];
                var principalEndNavigationPropertyName =
                    (string)foreignKey[SqlServerMetadataModelProvider.AnnotationNamePrincipalEndNavPropName];
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.AppendLine();
                    sb.AppendLine();
                }
                sb.Append("entity.");
                sb.Append("Reference");
                sb.Append("<");
                sb.Append(foreignKey.PrincipalEntityType.Name);
                sb.Append(">(d => d.");
                sb.Append(dependentEndNavigationPropertyName);
                sb.Append(")");

                if (foreignKey.IsUnique)
                {
                    sb.Append(".InverseReference(");
                }
                else
                {
                    sb.Append(".InverseCollection(");
                }

                if (!string.IsNullOrEmpty(principalEndNavigationPropertyName))
                {
                    sb.Append("p => p.");
                    sb.Append(principalEndNavigationPropertyName);
                }
                sb.Append(")");

                sb.Append(".ForeignKey");
                if (foreignKey.IsUnique)
                {
                    // If the relationship is 1:1 need to define to which end
                    // the ForeignKey properties belong.
                    sb.Append("<");
                    sb.Append(entityType.Name);
                    sb.Append(">");
                }
                sb.Append("(d => ");
                sb.Append(Generator.ModelUtilities
                    .GenerateLambdaToKey(foreignKey.Properties, "d"));
                sb.Append(");");
            }
        }

        public override bool GenerateProviderSpecificPropertyFacetsConfiguration(
            IProperty property, string entityVariableName, bool hadPreviousAppend,
            IndentedStringBuilder sb)
        {
            Check.NotNull(property, nameof(property));
            Check.NotEmpty(entityVariableName, nameof(entityVariableName));
            Check.NotNull(sb, nameof(sb));

            var useIdentityFacetConfig = GenerateUseIdentityFacetConfiguration(property);
            if (string.IsNullOrEmpty(useIdentityFacetConfig))
            {
                return hadPreviousAppend;
            }

            if (hadPreviousAppend)
            {
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.Append(entityVariableName);
            sb.Append(".Property(e => e.");
            sb.Append(property.Name);
            sb.Append(")");
            using (sb.Indent())
            {
                sb.AppendLine();
                sb.Append(".ForSqlServer()");
                sb.Append(useIdentityFacetConfig);
                sb.Append(";");
            }

            return true;
        }

        public virtual string GenerateUseIdentityFacetConfiguration([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            // output columnType if decimal to define precision and scale
            if (property.SqlServer().ValueGenerationStrategy.HasValue
                && SqlServerValueGenerationStrategy.Identity == property.SqlServer().ValueGenerationStrategy)
            {
                return ".UseIdentity()";
            }

            return null;
        }
    }
}