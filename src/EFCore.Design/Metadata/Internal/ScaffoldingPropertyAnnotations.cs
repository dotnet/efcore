// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class ScaffoldingPropertyAnnotations : RelationalPropertyAnnotations
    {
        public ScaffoldingPropertyAnnotations([NotNull] IProperty property)
            : base(property)
        {
        }

        public virtual int ColumnOrdinal
        {
            get => (int)Annotations.GetAnnotation(ScaffoldingAnnotationNames.ColumnOrdinal);
            set => Annotations.SetAnnotation(ScaffoldingAnnotationNames.ColumnOrdinal, value);
        }
    }
}
