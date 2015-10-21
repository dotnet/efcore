// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class ScaffoldingPropertyAnnotations : RelationalPropertyAnnotations
    {
        public ScaffoldingPropertyAnnotations([NotNull] IProperty property, [CanBeNull] string providerPrefix)
            : base(property, providerPrefix)
        {
        }

        public virtual bool? ExplicitValueGeneratedNever
        {
            get { return (bool?)Annotations.GetAnnotation(ScaffoldingAnnotationNames.ExplicitValueGenerationNever); }
            [param: CanBeNull] set { Annotations.SetAnnotation(ScaffoldingAnnotationNames.ExplicitValueGenerationNever, value); }
        }
    }
}
