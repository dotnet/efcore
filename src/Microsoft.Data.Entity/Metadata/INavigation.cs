// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface INavigation
    {
        string Name { get; }
        IEntityType EntityType { get; }
        IForeignKey ForeignKey { get; }

        void SetOrAddEntity([NotNull] object ownerEntity, [NotNull] object relatedEntity);
    }
}
