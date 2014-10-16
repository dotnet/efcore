// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.ModelConventions;

namespace Microsoft.Data.Entity.AzureTableStorage.Metadata
{
    public class AtsModelBuilder : ModelBuilder
    {
        public AtsModelBuilder([NotNull] Model model)
            : base(model, AtsConventions())
        {
        }

        private static IList<IEntityTypeConvention> AtsConventions()
        {
            return new List<IEntityTypeConvention>
                {
                    new PropertiesConvention(),
                    new ETagConvention(),
                    new PartitionKeyAndRowKeyConvention()
                };
        }
    }
}
