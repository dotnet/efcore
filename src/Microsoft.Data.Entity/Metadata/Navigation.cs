// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Navigation : INavigation
    {
        private readonly string _name;
        private readonly ForeignKey _foreignKey;

        public Navigation([NotNull] ForeignKey foreignKey, [NotNull] string name)
        {
            Check.NotNull(foreignKey, "foreignKey");
            Check.NotEmpty(name, "name");

            _foreignKey = foreignKey;
            _name = name;
        }

        public virtual string Name
        {
            get { return _name; }
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
