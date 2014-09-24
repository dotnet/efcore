// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Navigation : PropertyBase, INavigation
    {
        private ForeignKey _foreignKey;
        private bool _pointsToPrincipal;

        public Navigation([NotNull] string name, [NotNull] ForeignKey foreignKey, bool pointsToPrincipal)
            : base(Check.NotEmpty(name, "name"))
        {
            Check.NotNull(foreignKey, "foreignKey");

            _foreignKey = foreignKey;
            _pointsToPrincipal = pointsToPrincipal;
        }

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
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _pointsToPrincipal = value;
            }
        }

        public override EntityType EntityType
        {
            get
            {
                return PointsToPrincipal
                    ? ForeignKey.EntityType
                    : ForeignKey.ReferencedEntityType;
            }
        }

        IForeignKey INavigation.ForeignKey
        {
            get { return ForeignKey; }
        }
    }
}
