// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the OnDelete behavior for foreign keys on the join entity type for
    ///     self-referencing skip navigations
    /// </summary>
    public class SqlServerOnDeleteConvention : ISkipNavigationForeignKeyChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerOnDeleteConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public SqlServerOnDeleteConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
        }

        /// <inheritdoc />
        public virtual void ProcessSkipNavigationForeignKeyChanged(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionForeignKey foreignKey,
            IConventionForeignKey oldForeignKey,
            IConventionContext<IConventionForeignKey> context)
        {
            var selfReferencingSkipNavigation = skipNavigationBuilder.Metadata;
            if (foreignKey == null
                || foreignKey.DeleteBehavior != DeleteBehavior.Cascade
                || selfReferencingSkipNavigation.Inverse == null
                || selfReferencingSkipNavigation.TargetEntityType != selfReferencingSkipNavigation.DeclaringEntityType)
            {
                return;
            }

            if (selfReferencingSkipNavigation == selfReferencingSkipNavigation.DeclaringEntityType.GetDeclaredSkipNavigations()
                .First(s => s == selfReferencingSkipNavigation || s == selfReferencingSkipNavigation.Inverse))
            {
                foreignKey.Builder.OnDelete(DeleteBehavior.ClientCascade);
                selfReferencingSkipNavigation.Inverse.ForeignKey?.Builder.OnDelete(null);
            }
        }
    }
}
