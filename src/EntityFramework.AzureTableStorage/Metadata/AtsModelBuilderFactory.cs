// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Metadata
{
    public class AtsModelBuilderFactory : IModelBuilderFactory
    {
        public virtual ConventionModelBuilder CreateConventionBuilder([NotNull] Model model)
        {
            Check.NotNull(model, "model");

            return new AtsConventionModelBuilder(model);
        }
    }
}
