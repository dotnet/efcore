// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        IEntityType IPropertyBase.EntityType
        {
            get { return EntityType; }
        }

        IForeignKey INavigation.ForeignKey
        {
            get { return ForeignKey; }
        }
    }
}
