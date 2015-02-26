// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ModelBuilderFactory
    {
        public virtual ModelBuilder CreateConventionBuilder([NotNull] Model model)
        {
            Check.NotNull(model, nameof(model));

            return new ModelBuilder(model, CreateConventionSet());
        }

        protected virtual ConventionSet CreateConventionSet()
        {
            var conventions = new ConventionSet();

            conventions.EntityTypeAddedConventions.Add(new PropertiesConvention());
            conventions.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention());
            conventions.EntityTypeAddedConventions.Add(new RelationshipDiscoveryConvention());

            conventions.KeyAddedConventions.Add(new KeyConvention());

            conventions.ForeignKeyAddedConventions.Add(new ForeignKeyPropertyDiscoveryConvention());

            conventions.ForeignKeyRemovedConventions.Add(new KeyConvention());

            return conventions;
        }
    }
}
