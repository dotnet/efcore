// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.ModelConventions;

namespace Microsoft.Data.Entity.AzureTableStorage.Metadata
{
    public class AtsConventionModelBuilder : ConventionModelBuilder
    {
        public AtsConventionModelBuilder([NotNull] Model model)
            : base(model, AtsConventions())
        {
        }

        private static IList<IModelConvention> AtsConventions()
        {
            return new List<IModelConvention>
                {
                    new PropertiesConvention(),
                    new ETagConvention(),
                    new PartitionKeyAndRowKeyConvention()
                };
        }
    }
}
