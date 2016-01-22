// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
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

        public virtual IDictionary<string, string> EntityTypeErrors
        {
            get
            {
                var dictionary = (IDictionary<string, string>)Annotations.Metadata[ScaffoldingAnnotationNames.AnnotationPrefix + ScaffoldingAnnotationNames.EntityTypeErrors];

                if (dictionary == null)
                {
                    EntityTypeErrors = dictionary = new Dictionary<string, string>();
                }

                return dictionary;
            }
            [param: NotNull] set { Annotations.SetAnnotation(ScaffoldingAnnotationNames.EntityTypeErrors, Check.NotNull(value, nameof(value))); }
        }
    }
}
