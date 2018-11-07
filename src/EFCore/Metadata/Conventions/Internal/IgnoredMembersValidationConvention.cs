// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class IgnoredMembersValidationConvention : IModelBuiltConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var ignoredMember in entityType.GetIgnoredMembers())
                {
                    var property = entityType.FindProperty(ignoredMember);
                    if (property != null)
                    {
                        if (property.DeclaringEntityType != entityType)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.InheritedPropertyCannotBeIgnored(
                                    ignoredMember, entityType.DisplayName(), property.DeclaringEntityType.DisplayName()));
                        }

                        Debug.Assert(false);
                    }

                    var navigation = entityType.FindNavigation(ignoredMember);
                    if (navigation != null)
                    {
                        if (navigation.DeclaringEntityType != entityType)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.InheritedPropertyCannotBeIgnored(
                                    ignoredMember, entityType.DisplayName(), navigation.DeclaringEntityType.DisplayName()));
                        }

                        Debug.Assert(false);
                    }
                }
            }

            return modelBuilder;
        }
    }
}
