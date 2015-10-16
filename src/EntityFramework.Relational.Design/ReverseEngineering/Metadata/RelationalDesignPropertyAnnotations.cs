// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Metadata
{
    public class RelationalDesignPropertyAnnotations : RelationalPropertyAnnotations
    {
        public RelationalDesignPropertyAnnotations([NotNull] IProperty property, [CanBeNull] string providerPrefix)
            : base(property, providerPrefix)
        {
        }

        public virtual bool? ExplicitValueGeneratedNever
        {
            get { return (bool?)Annotations.GetAnnotation(RelationalDesignAnnotationNames.ExplicitValueGenerationNever); }
            [param: CanBeNull] set { SetExplicitValueGeneratedNever(value); }
        }

        protected virtual bool SetExplicitValueGeneratedNever([CanBeNull] bool? value)
            => Annotations.SetAnnotation(RelationalDesignAnnotationNames.ExplicitValueGenerationNever, value);
    }
}
