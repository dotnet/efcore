// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Scaffolding.Metadata.Internal;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class ScaffoldingPropertyAnnotations : RelationalPropertyAnnotations
    {
        public ScaffoldingPropertyAnnotations([NotNull] IProperty property, [CanBeNull] string providerPrefix)
            : base(property, providerPrefix)
        {
        }

        public virtual int ColumnOrdinal
        {
            get { return (int)Annotations.GetAnnotation(ScaffoldingAnnotationNames.ColumnOrdinal); }
            set { Annotations.SetAnnotation(ScaffoldingAnnotationNames.ColumnOrdinal, value); }
        }
    }
}
