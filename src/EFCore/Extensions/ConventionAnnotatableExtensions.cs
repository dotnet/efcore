// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IConventionAnnotatable" />.
    /// </summary>
    public static class ConventionAnnotatableExtensions
    {
        /// <summary>
        ///     Adds annotations to an object.
        /// </summary>
        /// <param name="annotatable"> The object to add the annotations to. </param>
        /// <param name="annotations"> The annotations to be added. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void AddAnnotations(
            [NotNull] this IConventionAnnotatable annotatable,
            [NotNull] IEnumerable<IConventionAnnotation> annotations,
            bool fromDataAnnotation = false)
        {
            foreach (var annotation in annotations)
            {
                annotatable.AddAnnotation(annotation.Name, annotation.Value, fromDataAnnotation);
            }
        }
    }
}
