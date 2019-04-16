// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Builds the model for a given context. This default implementation builds the model by calling
    ///         <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> on the context.
    ///     </para>
    ///     <para>
    ///         Also, entity types found as <see cref="DbSet{TEntity}" /> properties on the context are mapped
    ///         to tables named for the DbSet property names, and public static methods on the context marked with
    ///         <see cref="DbFunctionAttribute" /> are mapped to database functions.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalModelCustomizer : ModelCustomizer
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalModelCustomizer" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public RelationalModelCustomizer([NotNull] ModelCustomizerDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     <para>
        ///         Performs additional configuration of the model in addition to what is discovered by convention. This implementation
        ///         builds the model for a given context by calling <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" />
        ///         on the context.
        ///     </para>
        ///     <para>
        ///         Also, entity types found as <see cref="DbSet{TEntity}" /> properties on the context are mapped
        ///         to tables named for the DbSet property names, and public static methods on the context marked with
        ///         <see cref="DbFunctionAttribute" /> are mapped to database functions.
        ///     </para>
        /// </summary>
        /// <param name="modelBuilder">
        ///     The builder being used to construct the model.
        /// </param>
        /// <param name="context">
        ///     The context instance that the model is being created for.
        /// </param>
        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            FindDbFunctions(modelBuilder, context);

            base.Customize(modelBuilder, context);
        }

        /// <summary>
        ///     Adds to the model function mappings found as public static methods on the context marked with
        ///     the <see cref="DbFunctionAttribute" />.
        /// </summary>
        /// <param name="modelBuilder"> The <see cref="ModelBuilder" /> being used to build the model. </param>
        /// <param name="context"> The context to find function methods on. </param>
        protected virtual void FindDbFunctions([NotNull] ModelBuilder modelBuilder, [NotNull] DbContext context)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(context, nameof(context));

            var contextType = context.GetType();

            while (contextType != typeof(DbContext))
            {
                var functions = contextType.GetMethods(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                        | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Where(mi => mi.GetCustomAttributes(typeof(DbFunctionAttribute)).Any());

                foreach (var function in functions)
                {
                    modelBuilder.HasDbFunction(function);
                }

                contextType = contextType.BaseType;
            }
        }

        /// <summary>
        ///     Adds the entity types found in <see cref="DbSet{TEntity}" /> properties on the context to the model.
        /// </summary>
        /// <param name="modelBuilder"> The <see cref="ModelBuilder" /> being used to build the model. </param>
        /// <param name="context"> The context to find <see cref="DbSet{TEntity}" /> properties on. </param>
        protected override void FindSets(ModelBuilder modelBuilder, DbContext context)
        {
            base.FindSets(modelBuilder, context);

            var sets = Dependencies.SetFinder.CreateClrTypeDbSetMapping(context);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes().Cast<EntityType>())
            {
                if (entityType.BaseType == null
                    && sets.ContainsKey(entityType.ClrType))
                {
                    entityType.Builder.Relational(ConfigurationSource.Convention).ToTable(sets[entityType.ClrType].Name);
                }
            }
        }
    }
}
