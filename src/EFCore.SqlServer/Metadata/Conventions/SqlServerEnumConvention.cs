// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that sets a flag on the model to always skip detecting changes if no entity type is using the
    ///     <see cref="ChangeTrackingStrategy.Snapshot" /> strategy.
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
                foreach( var property in entityType.GetProperties())
                {
                    if(property.PropertyInfo.GetType().IsEnum)
                    {
                        property.SetIsNullable(false);
                        string sql = $"CHECK ({property.Name} IN(";
                        var enumNames = property.PropertyInfo.GetType().GetEnumNames();
                        foreach (var item in enumNames)
                        {
                            sql += $"'{item}', ";
                        }
                        sql = sql.Remove(sql.Length - 2);
                        sql = sql + "))";
                        string constraintName = $"CK_{entityType.Name}_{property.Name}_EnumConstraint";
                        entityType.AddCheckConstraint(constraintName, sql);
                    }
                }
            }

            //TODO: Ask Core team.
            //modelBuilder.HasAnnotation(SQLserverAnnot.EnumCheckConstraint, "true");
        }
    }
}
