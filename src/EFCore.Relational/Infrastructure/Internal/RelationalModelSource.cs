// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalModelSource : ModelSource
    {
        public RelationalModelSource([NotNull] ModelSourceDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
