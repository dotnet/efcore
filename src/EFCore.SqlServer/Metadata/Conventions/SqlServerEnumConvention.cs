// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that creates check constraint for Enum column in a model.
    /// </summary>
    public class SqlServerEnumConvention : IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerEnumConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public SqlServerEnumConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    var typeMapping = property.FindTypeMapping();
                    var propertyType = property.GetIdentifyingMemberInfo()?.GetMemberType();
                    if ((propertyType?.IsEnum ?? false)
                        && typeMapping != null
                        && !propertyType.IsDefined(typeof(FlagsAttribute), true))
                    {
                        var enumValues = Enum.GetValues(propertyType);
                        if (enumValues.Length <= 0)
                        {
                            continue;
                        }

                        var sql = new StringBuilder();
                        sql.Append("[");
                        sql.Append(property.GetColumnName());
                        sql.Append("] IN("); ;
                        foreach (var item in enumValues)
                        {
                            var value = ((RelationalTypeMapping)typeMapping).GenerateSqlLiteral(item);
                            sql.Append($"{value}, ");
                        }

                        sql.Remove(sql.Length - 2, 2);
                        sql.Append(")");

                        var constraintName = $"CK_{entityType.GetTableName()}_{property.GetColumnName()}_Enum_Constraint";
                        entityType.AddCheckConstraint(constraintName, sql.ToString());
                    }
                }
            }
        }
    }
}
