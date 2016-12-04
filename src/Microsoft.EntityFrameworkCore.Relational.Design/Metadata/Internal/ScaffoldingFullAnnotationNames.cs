// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ScaffoldingFullAnnotationNames : RelationalFullAnnotationNames
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected ScaffoldingFullAnnotationNames(string prefix)
            : base(prefix)
        {
            UseProviderMethodName = ScaffoldingAnnotationNames.UseProviderMethodName;
            ColumnOrdinal = ScaffoldingAnnotationNames.ColumnOrdinal;
            DependentEndNavigation = ScaffoldingAnnotationNames.DependentEndNavigation;
            PrincipalEndNavigation = ScaffoldingAnnotationNames.PrincipalEndNavigation;
            EntityTypeErrors = ScaffoldingAnnotationNames.EntityTypeErrors;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new static ScaffoldingFullAnnotationNames Instance { get; }
            = new ScaffoldingFullAnnotationNames(ScaffoldingAnnotationNames.Prefix);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public readonly string UseProviderMethodName;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public readonly string ColumnOrdinal;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public readonly string DependentEndNavigation;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public readonly string PrincipalEndNavigation;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public readonly string EntityTypeErrors;
    }
}
