// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class ScaffoldingForeignKeyAnnotations : RelationalForeignKeyAnnotations
    {
        public ScaffoldingForeignKeyAnnotations([NotNull] IForeignKey foreignKey)
            : base(foreignKey, ScaffoldingFullAnnotationNames.Instance)
        {
        }

        public virtual string DependentEndNavigation
        {
            get { return (string)Annotations.GetAnnotation(ScaffoldingFullAnnotationNames.Instance.DependentEndNavigation, null); }
            [param: CanBeNull] set { Annotations.SetAnnotation(ScaffoldingFullAnnotationNames.Instance.DependentEndNavigation, null, Check.NullButNotEmpty(value, nameof(value))); }
        }

        public virtual string PrincipalEndNavigation
        {
            get { return (string)Annotations.GetAnnotation(ScaffoldingFullAnnotationNames.Instance.PrincipalEndNavigation, null); }
            [param: CanBeNull]
            set
            {
                Annotations.SetAnnotation(
                    ScaffoldingFullAnnotationNames.Instance.PrincipalEndNavigation,
                    null,
                    Check.NullButNotEmpty(value, nameof(value)));
            }
        }
    }
}
