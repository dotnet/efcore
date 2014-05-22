// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers
{
    public static class TestModelExtensions
    {
        public static Model WithEntityType(this Model model, EntityType entityType)
        {
            model.AddEntityType(entityType);
            return model;
        }
    }
}
