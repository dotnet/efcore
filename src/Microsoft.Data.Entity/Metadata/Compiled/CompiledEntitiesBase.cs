// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public class CompiledEntitiesBase
    {
        public readonly IEntityType[] EntityTypes;

        public CompiledEntitiesBase([NotNull] params IEntityType[] entityTypes)
        {
            EntityTypes = entityTypes;
        }
    }
}
