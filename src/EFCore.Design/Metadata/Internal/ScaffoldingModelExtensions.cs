// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class ScaffoldingModelExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyDictionary<string, string> GetEntityTypeErrors(this IReadOnlyModel model)
            => (IReadOnlyDictionary<string, string>?)model[ScaffoldingAnnotationNames.EntityTypeErrors] ?? new Dictionary<string, string>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IDictionary<string, string> GetOrCreateEntityTypeErrors(this IReadOnlyModel model)
        {
            var errors = (IDictionary<string, string>?)model[ScaffoldingAnnotationNames.EntityTypeErrors];
            if (errors == null)
            {
                errors = new Dictionary<string, string>();
                (model as IMutableModel)?.SetEntityTypeErrors(errors);
            }

            return errors;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void SetEntityTypeErrors(this IMutableModel model, IDictionary<string, string> value)
            => model.SetAnnotation(
                ScaffoldingAnnotationNames.EntityTypeErrors,
                value);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string? GetDatabaseName(this IReadOnlyModel model)
            => (string?)model[ScaffoldingAnnotationNames.DatabaseName];

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void SetDatabaseName(this IMutableModel model, string? value)
            => model.SetAnnotation(
                ScaffoldingAnnotationNames.DatabaseName,
                value);
    }
}
