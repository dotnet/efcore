// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class ServicePropertyExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ServiceParameterBinding GetParameterBinding([NotNull] this IServiceProperty serviceProperty)
            => serviceProperty.AsServiceProperty().ParameterBinding;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string ToDebugString([NotNull] this IServiceProperty serviceProperty, bool singleLine = true, [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder.Append(indent);

            if (singleLine)
            {
                builder.Append("Service property: ").Append(serviceProperty.DeclaringType.DisplayName()).Append(".");
            }

            builder.Append(serviceProperty.Name);

            if (serviceProperty.GetFieldName() == null)
            {
                builder.Append(" (no field, ");
            }
            else
            {
                builder.Append(" (").Append(serviceProperty.GetFieldName()).Append(", ");
            }

            builder.Append(serviceProperty.ClrType?.ShortDisplayName()).Append(")");

            if (!singleLine)
            {
                builder.Append(serviceProperty.AnnotationsToDebugString(indent + "  "));
            }

            return builder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ServiceProperty AsServiceProperty([NotNull] this IServiceProperty serviceProperty, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IServiceProperty, ServiceProperty>(serviceProperty, methodName);
    }
}
