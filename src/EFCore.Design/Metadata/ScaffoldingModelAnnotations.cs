// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class ScaffoldingModelAnnotations : RelationalModelAnnotations
    {
        public ScaffoldingModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        public virtual IDictionary<string, string> EntityTypeErrors
        {
            get
            {
                var dictionary = (IDictionary<string, string>)Annotations.GetAnnotation(
                    ScaffoldingAnnotationNames.EntityTypeErrors);

                if (dictionary == null)
                {
                    EntityTypeErrors = dictionary = new Dictionary<string, string>();
                }

                return dictionary;
            }
            [param: NotNull]
            set
            {
                Annotations.SetAnnotation(
                    ScaffoldingAnnotationNames.EntityTypeErrors,
                    Check.NotNull(value, nameof(value)));
            }
        }

        public virtual string DatabaseName
        {
            get { return (string)Annotations.GetAnnotation(ScaffoldingAnnotationNames.DatabaseName); }
            [param: CanBeNull]
            set
            {
                Annotations.SetAnnotation(
                    ScaffoldingAnnotationNames.DatabaseName,
                    Check.NullButNotEmpty(value, nameof(value)));
            }
        }
    }
}
