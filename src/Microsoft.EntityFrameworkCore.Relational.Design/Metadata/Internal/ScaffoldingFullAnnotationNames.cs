// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal
{
    public class ScaffoldingFullAnnotationNames : RelationalFullAnnotationNames
    {
        protected ScaffoldingFullAnnotationNames(string prefix)
            : base(prefix)
        {
            UseProviderMethodName = ScaffoldingAnnotationNames.UseProviderMethodName;
            ColumnOrdinal = ScaffoldingAnnotationNames.ColumnOrdinal;
            DependentEndNavigation = ScaffoldingAnnotationNames.DependentEndNavigation;
            PrincipalEndNavigation = ScaffoldingAnnotationNames.PrincipalEndNavigation;
            EntityTypeErrors = ScaffoldingAnnotationNames.EntityTypeErrors;
        }

        public new static ScaffoldingFullAnnotationNames Instance { get; }
            = new ScaffoldingFullAnnotationNames(ScaffoldingAnnotationNames.Prefix);

        public readonly string UseProviderMethodName;
        public readonly string ColumnOrdinal;
        public readonly string DependentEndNavigation;
        public readonly string PrincipalEndNavigation;
        public readonly string EntityTypeErrors;
    }
}
