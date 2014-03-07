// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Materialization
{
    public class ReflectionMaterializerFactory : IMaterializerFactory
    {
        public IMaterializer CreateMaterializer(IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return new ReflectionMaterializer(entityType);
        }
    }
}
