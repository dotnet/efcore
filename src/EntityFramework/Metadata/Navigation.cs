// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Navigation : NamedMetadataBase, INavigation
    {
        private ForeignKey _foreignKey;
        private readonly bool _pointsToPrincipal;

        public Navigation([NotNull] ForeignKey foreignKey, [NotNull] string name, bool pointsToPrincipal)
            : base(Check.NotEmpty(name, "name"))
        {
            Check.NotNull(foreignKey, "foreignKey");

            _foreignKey = foreignKey;
            _pointsToPrincipal = pointsToPrincipal;
        }

        public virtual EntityType EntityType { get; [param: CanBeNull] set; }

        public virtual ForeignKey ForeignKey
        {
            get { return _foreignKey; }
            [param: NotNull] 
            set
            {
                Check.NotNull(value, "value");

                _foreignKey = value;
            }
        }

        public virtual bool PointsToPrincipal
        {
            get { return _pointsToPrincipal; }
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
