// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ScaffoldingModelAnnotations : RelationalModelAnnotations
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ScaffoldingModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDictionary<string, string> EntityTypeErrors
        {
            get
            {
                var dictionary = (IDictionary<string, string>)Annotations.Metadata[ScaffoldingAnnotationNames.EntityTypeErrors];

                if (dictionary == null)
                {
                    EntityTypeErrors = dictionary = new Dictionary<string, string>();
                }

                return dictionary;
            }
            [param: NotNull]
            set => Annotations.SetAnnotation(
                    ScaffoldingAnnotationNames.EntityTypeErrors,
                    Check.NotNull(value, nameof(value)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string DatabaseName
        {
            get => (string)Annotations.Metadata[ScaffoldingAnnotationNames.DatabaseName];
            [param: CanBeNull]
            set => Annotations.SetAnnotation(
                    ScaffoldingAnnotationNames.DatabaseName,
                    Check.NullButNotEmpty(value, nameof(value)));
        }
    }
}
