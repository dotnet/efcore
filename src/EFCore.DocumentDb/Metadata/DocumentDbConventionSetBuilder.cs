// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class DocumentDbConventionSetBuilder : IConventionSetBuilder
    {
        private readonly ICurrentDbContext _context;
        private readonly IDbSetFinder _setFinder;

        public DocumentDbConventionSetBuilder(ICurrentDbContext context, IDbSetFinder setFinder)
        {
            _context = context;
            _setFinder = setFinder;
        }
        public ConventionSet AddConventions(ConventionSet conventionSet)
        {
            conventionSet.BaseEntityTypeChangedConventions.Add(new DiscriminatorConvention());
            conventionSet.BaseEntityTypeChangedConventions.Add(
                new CollectionNameFromDbSetConvention(_context?.Context, _setFinder));

            return conventionSet;
        }
    }
}
