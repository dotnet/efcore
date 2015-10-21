// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class ScaffoldingModelAnnotations : RelationalModelAnnotations
    {
        public ScaffoldingModelAnnotations([NotNull] IModel model, [CanBeNull] string providerPrefix)
            : base(model, providerPrefix)
        {
        }

        public virtual string UseProviderMethodName
        {
            get { return (string)Annotations.GetAnnotation(ScaffoldingAnnotationNames.UseProviderMethodName); }
            [param: CanBeNull] set { Annotations.SetAnnotation(ScaffoldingAnnotationNames.UseProviderMethodName, Check.NullButNotEmpty(value, nameof(value))); }
        }
    }
}
