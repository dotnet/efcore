// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the OnDelete behavior for foreign keys on the join entity type for
    ///     self-referencing skip navigations
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and SQL Azure databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    public class SqlServerOnDeleteConvention : CascadeDeleteConvention, ISkipNavigationForeignKeyChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerOnDeleteConvention" />.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
        public SqlServerOnDeleteConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies)
        {
            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Relational provider-specific dependencies for this service.
        /// </summary>
        protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

        /// <inheritdoc />
        public virtual void ProcessSkipNavigationForeignKeyChanged(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionForeignKey? foreignKey,
            IConventionForeignKey? oldForeignKey,
            IConventionContext<IConventionForeignKey> context)
        {
            if (foreignKey is not null && foreignKey.IsInModel)
            {
                foreignKey.Builder.OnDelete(GetTargetDeleteBehavior(foreignKey));
            }
        }

        /// <inheritdoc />
        protected override DeleteBehavior GetTargetDeleteBehavior(IConventionForeignKey foreignKey)
        {
            var deleteBehavior = base.GetTargetDeleteBehavior(foreignKey);
            if (deleteBehavior != DeleteBehavior.Cascade)
            {
                return deleteBehavior;
            }

            if (foreignKey.IsBaseLinking())
            {
                return DeleteBehavior.ClientCascade;
            }

            var selfReferencingSkipNavigation = foreignKey.GetReferencingSkipNavigations()
                .FirstOrDefault(s => s.Inverse != null && s.TargetEntityType == s.DeclaringEntityType);
            if (selfReferencingSkipNavigation == null)
            {
                return deleteBehavior;
            }

            if (selfReferencingSkipNavigation
                == selfReferencingSkipNavigation.DeclaringEntityType.GetDeclaredSkipNavigations()
                    .First(s => s == selfReferencingSkipNavigation || s == selfReferencingSkipNavigation.Inverse)
                && selfReferencingSkipNavigation != selfReferencingSkipNavigation.Inverse)
            {
                selfReferencingSkipNavigation.Inverse!.ForeignKey?.Builder.OnDelete(
                    GetTargetDeleteBehavior(selfReferencingSkipNavigation.Inverse.ForeignKey));
                return DeleteBehavior.ClientCascade;
            }

            return deleteBehavior;
        }
    }
}
