// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public static class NavigationExtensions
    {
        public static bool IsDependentToPrincipal([NotNull] this INavigation navigation)
            => Check.NotNull(navigation, nameof(navigation)).ForeignKey.DependentToPrincipal == navigation;

        public static bool IsCollection([NotNull] this INavigation navigation)
        {
            Check.NotNull(navigation, nameof(navigation));

            return !navigation.IsDependentToPrincipal() && !navigation.ForeignKey.IsUnique;
        }

        public static INavigation FindInverse([NotNull] this INavigation navigation)
        {
            Check.NotNull(navigation, nameof(navigation));

            return navigation.IsDependentToPrincipal()
                ? navigation.ForeignKey.PrincipalToDependent
                : navigation.ForeignKey.DependentToPrincipal;
        }

        public static IEntityType GetTargetType([NotNull] this INavigation navigation)
        {
            Check.NotNull(navigation, nameof(navigation));

            return navigation.IsDependentToPrincipal()
                ? navigation.ForeignKey.PrincipalEntityType
                : navigation.ForeignKey.DeclaringEntityType;
        }
    }
}
