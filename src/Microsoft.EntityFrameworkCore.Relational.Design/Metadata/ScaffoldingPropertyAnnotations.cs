// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class ScaffoldingPropertyAnnotations : RelationalPropertyAnnotations
    {
        public ScaffoldingPropertyAnnotations([NotNull] IProperty property)
            : base(property, ScaffoldingFullAnnotationNames.Instance)
        {
        }

        public virtual int ColumnOrdinal
        {
            get { return (int)Annotations.GetAnnotation(ScaffoldingFullAnnotationNames.Instance.ColumnOrdinal, null); }
            set { Annotations.SetAnnotation(ScaffoldingFullAnnotationNames.Instance.ColumnOrdinal, null, value); }
        }
    }
}
