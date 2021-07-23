// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Relational-specific extension methods for <see cref="IModel" />.
    /// </summary>
    public static class RelationalModelExtensions
    {
        /// <summary>
        ///     Returns the relational service dependencies.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="methodName"> The name of the calling method. </param>
        /// <returns> The relational service dependencies. </returns>
        public static RelationalModelDependencies GetRelationalDependencies(
            this IModel model, [CallerMemberName]string methodName = "")
            => (RelationalModelDependencies?)model
                .FindRuntimeAnnotation(RelationalAnnotationNames.ModelDependencies)?.Value
                ?? throw new InvalidOperationException(CoreStrings.ModelNotFinalized(methodName));
    }
}
