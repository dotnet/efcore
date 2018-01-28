// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class MutableServicePropertyExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SetParameterBinding([NotNull] this IMutableServiceProperty serviceProperty, [NotNull] ServiceParameterBinding parameterBinding)
            => serviceProperty.AsServiceProperty().ParameterBinding = parameterBinding;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ServiceProperty AsServiceProperty([NotNull] this IMutableServiceProperty serviceProperty, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IMutableServiceProperty, ServiceProperty>(serviceProperty, methodName);
    }
}
