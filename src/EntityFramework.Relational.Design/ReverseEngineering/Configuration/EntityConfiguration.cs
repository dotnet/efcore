// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class EntityConfiguration
    {
        public EntityConfiguration([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            EntityType = entityType;
        }

        public virtual IEntityType EntityType { get; [param: NotNull] private set; }
        public virtual List<FacetConfiguration> FacetConfigurations { get; } = new List<FacetConfiguration>();
        public virtual List<PropertyConfiguration> PropertyConfigurations { get; } = new List<PropertyConfiguration>();
        public virtual List<NavigationConfiguration> NavigationConfigurations { get; } = new List<NavigationConfiguration>();
    }
}
