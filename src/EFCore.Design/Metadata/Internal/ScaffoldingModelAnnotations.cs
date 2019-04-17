// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ScaffoldingModelAnnotations : RelationalModelAnnotations
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ScaffoldingModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
