// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ModelBuilderFactory : IModelBuilderFactory
    {
        public virtual ModelBuilder CreateConventionBuilder(Model model)
        {
            Check.NotNull(model, nameof(model));

            return new ModelBuilder(model, CreateConventionSet());
        }

        protected virtual ConventionSet CreateConventionSet()
        {
            var conventions = new ConventionSet();

            conventions.EntityTypeAddedConventions.Add(new PropertiesConvention());
            conventions.EntityTypeAddedConventions.Add(new KeyConvention());
            conventions.EntityTypeAddedConventions.Add(new RelationshipDiscoveryConvention());

            conventions.ForeignKeyAddedConventions.Add(new ForeignKeyPropertyDiscoveryConvention());

            return conventions;
        }
    }
}
