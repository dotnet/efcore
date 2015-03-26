// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Builders
{
    public class ModelBuilderFactory : IModelBuilderFactory
    {
        public virtual ModelBuilder CreateConventionBuilder()
        {
            return new ModelBuilder(CreateConventionSet());
        }

        public virtual ModelBuilder CreateConventionBuilder(Model model)
        {
            Check.NotNull(model, nameof(model));

            return new ModelBuilder(CreateConventionSet(), model);
        }

        protected virtual ConventionSet CreateConventionSet()
        {
            var conventions = new ConventionSet();

            conventions.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention());
            conventions.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention());
            conventions.EntityTypeAddedConventions.Add(new RelationshipDiscoveryConvention());

            var keyConvention = new KeyConvention();

            conventions.KeyAddedConventions.Add(keyConvention);

            conventions.ForeignKeyAddedConventions.Add(new ForeignKeyPropertyDiscoveryConvention());

            conventions.ForeignKeyRemovedConventions.Add(keyConvention);

            return conventions;
        }
    }
}
