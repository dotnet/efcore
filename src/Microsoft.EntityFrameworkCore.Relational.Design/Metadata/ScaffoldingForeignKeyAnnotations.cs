// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Scaffolding.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class ScaffoldingForeignKeyAnnotations : RelationalForeignKeyAnnotations
    {
        public ScaffoldingForeignKeyAnnotations([NotNull] IForeignKey foreignKey, [CanBeNull] string providerPrefix)
            : base(foreignKey, providerPrefix)
        {
        }

        public virtual string DependentEndNavigation
        {
            get { return (string)Annotations.GetAnnotation(ScaffoldingAnnotationNames.DependentEndNavigation); }
            [param: CanBeNull] set { Annotations.SetAnnotation(ScaffoldingAnnotationNames.DependentEndNavigation, Check.NullButNotEmpty(value, nameof(value))); }
        }

        public virtual string PrincipalEndNavigation
        {
            get { return (string)Annotations.GetAnnotation(ScaffoldingAnnotationNames.PrincipalEndNavigation); }
            [param: CanBeNull] set { Annotations.SetAnnotation(ScaffoldingAnnotationNames.PrincipalEndNavigation, Check.NullButNotEmpty(value, nameof(value))); }
        }
    }
}
