// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class ForeignKeyExtensions
    {
        public static Navigation GetNavigationToPrincipal([NotNull] this ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return foreignKey.EntityType.Navigations.SingleOrDefault(
                navigation => navigation.ForeignKey == foreignKey && navigation.PointsToPrincipal);
        }

        public static Navigation GetNavigationToDependent([NotNull] this ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return foreignKey.ReferencedEntityType.Navigations.SingleOrDefault(
                navigation => navigation.ForeignKey == foreignKey && !navigation.PointsToPrincipal);
        }
    }
}
