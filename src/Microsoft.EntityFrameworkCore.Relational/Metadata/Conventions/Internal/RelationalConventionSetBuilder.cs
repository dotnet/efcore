// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class RelationalConventionSetBuilder : IConventionSetBuilder
    {
        private readonly IRelationalTypeMapper _typeMapper;
        private readonly DbContext _context;
        private readonly IDbSetFinder _setFinder;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected RelationalConventionSetBuilder(
            [NotNull] IRelationalTypeMapper typeMapper,
            [CanBeNull] ICurrentDbContext currentContext,
            [CanBeNull] IDbSetFinder setFinder)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
            _context = currentContext?.Context;
            _setFinder = setFinder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionSet AddConventions(ConventionSet conventionSet)
        {
            RelationshipDiscoveryConvention relationshipDiscoveryConvention = new RelationalRelationshipDiscoveryConvention(_typeMapper);
            InversePropertyAttributeConvention inversePropertyAttributeConvention = new RelationalInversePropertyAttributeConvention(_typeMapper);

            ReplaceConvention(conventionSet.EntityTypeAddedConventions, (PropertyDiscoveryConvention)new RelationalPropertyDiscoveryConvention(_typeMapper));
            ReplaceConvention(conventionSet.EntityTypeAddedConventions, inversePropertyAttributeConvention);
            ReplaceConvention(conventionSet.EntityTypeAddedConventions, relationshipDiscoveryConvention);

            ReplaceConvention(conventionSet.EntityTypeIgnoredConventions, inversePropertyAttributeConvention);

            ReplaceConvention(conventionSet.BaseEntityTypeSetConventions, inversePropertyAttributeConvention);
            ReplaceConvention(conventionSet.BaseEntityTypeSetConventions, relationshipDiscoveryConvention);

            ReplaceConvention(conventionSet.EntityTypeMemberIgnoredConventions, inversePropertyAttributeConvention);
            ReplaceConvention(conventionSet.EntityTypeMemberIgnoredConventions, relationshipDiscoveryConvention);

            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, (ForeignKeyAttributeConvention)new RelationalForeignKeyAttributeConvention(_typeMapper));

            ReplaceConvention(conventionSet.NavigationAddedConventions, inversePropertyAttributeConvention);
            ReplaceConvention(conventionSet.NavigationAddedConventions, relationshipDiscoveryConvention);

            ReplaceConvention(conventionSet.NavigationRemovedConventions, relationshipDiscoveryConvention);

            ReplaceConvention(conventionSet.ModelBuiltConventions, (PropertyMappingValidationConvention)new RelationalPropertyMappingValidationConvention(_typeMapper));

            var relationalColumnAttributeConvention = new RelationalColumnAttributeConvention();
            conventionSet.PropertyAddedConventions.Add(relationalColumnAttributeConvention);

            conventionSet.EntityTypeAddedConventions.Add(new RelationalTableAttributeConvention());

            conventionSet.BaseEntityTypeSetConventions.Add(new DiscriminatorConvention());

            conventionSet.BaseEntityTypeSetConventions.Add(new TableNameFromDbSetConvention(_context, _setFinder));

            conventionSet.PropertyFieldChangedConventions.Add(relationalColumnAttributeConvention);

            return conventionSet;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ReplaceConvention<T1, T2>([NotNull] IList<T1> conventionsList, [NotNull] T2 newConvention)
            where T2 : T1
        {
            var oldConvention = conventionsList.OfType<T2>().FirstOrDefault();
            if (oldConvention == null)
            {
                throw new InvalidOperationException();
            }
            var index = conventionsList.IndexOf(oldConvention);
            conventionsList.RemoveAt(index);
            conventionsList.Insert(index, newConvention);
        }
    }
}
