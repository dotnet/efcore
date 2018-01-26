// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class DocumentDbModelCustomizer : ModelCustomizer
    {
        public DocumentDbModelCustomizer([NotNull] ModelCustomizerDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override void FindSets(ModelBuilder modelBuilder, DbContext context)
        {
            base.FindSets(modelBuilder, context);

            var sets = Dependencies.SetFinder.CreateClrTypeDbSetMapping(context);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes().Cast<EntityType>())
            {
                if (entityType.BaseType == null
                    && sets.ContainsKey(entityType.ClrType))
                {
                    entityType.Builder.DocumentDb(ConfigurationSource.Convention)
                        .ToCollection(sets[entityType.ClrType].Name);
                }
            }
        }
    }
}
