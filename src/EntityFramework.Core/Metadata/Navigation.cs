// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Navigation : PropertyBase, INavigation
    {
        private ForeignKey _foreignKey;

        public Navigation([NotNull] string name, [NotNull] ForeignKey foreignKey)
            : base(Check.NotEmpty(name, nameof(name)))
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            _foreignKey = foreignKey;
        }

        public virtual ForeignKey ForeignKey
        {
            get { return _foreignKey; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _foreignKey = value;
            }
        }

        public override EntityType DeclaringEntityType
            => this.PointsToPrincipal()
                ? ForeignKey.DeclaringEntityType
                : ForeignKey.PrincipalEntityType;

        IForeignKey INavigation.ForeignKey => ForeignKey;

        public override string ToString() => DeclaringEntityType + "." + Name;
    }
}
