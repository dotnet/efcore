// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that creates check constraint for Enum column in a model.
    /// </summary>
    public class SqlServerEnumConvention : IModelFinalizedConvention
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

        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach( var property in entityType.GetDeclaredProperties())
                {
                    if(property.GetIdentifyingMemberInfo()?.GetType().IsEnum ?? false
                        && (property.FindTypeMapping()?.Converter ?? property.GetValueConverter())?.ProviderClrType == typeof(string))
                    {
                        string sql = $"CHECK ({property.Name} IN(";
                        var enumNames = property.PropertyInfo.GetType().GetEnumNames();
                        foreach (var item in enumNames)
                        {
                            sql += $"'{item}', ";
                        }
                        sql = sql.Remove(sql.Length - 2);
                        sql = sql + "))";
                        string constraintName = $"CK_{entityType.Name}_{property.Name}_Enum_Constraint";
                        entityType.AddCheckConstraint(constraintName, sql);
                    }
                }
            }
        }
    }
}
