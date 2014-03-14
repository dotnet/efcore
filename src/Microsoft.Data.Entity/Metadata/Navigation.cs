// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Navigation : NamedMetadataBase, INavigation
    {
        private readonly ForeignKey _foreignKey;

        public Navigation([NotNull] ForeignKey foreignKey, [NotNull] string name)
            : base(Check.NotEmpty(name, "name"))
        {
            Check.NotNull(foreignKey, "foreignKey");

            _foreignKey = foreignKey;
        }

        public virtual EntityType EntityType { get; [param: CanBeNull] set; }

        public virtual ForeignKey ForeignKey
        {
            get { return _foreignKey; }
        }

        public virtual void SetOrAddEntity(object ownerEntity, object relatedEntity)
        {
            Check.NotNull(ownerEntity, "ownerEntity");
            Check.NotNull(relatedEntity, "relatedEntity");

            // TODO: Check if value already set before setting again
            // TODO: Handle o/c mapping mechanisms for patterns other than read/write property
            // TODO: Handle shadow state
            // TODO: Handle nulls/removals
            EntityType.Type.GetAnyProperty(Name).SetValue(ownerEntity, relatedEntity);
        }

        IEntityType INavigation.EntityType
        {
            get { return EntityType; }
        }

        IForeignKey INavigation.ForeignKey
        {
            get { return ForeignKey; }
        }
    }
}
