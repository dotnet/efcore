// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class ScaffoldingMetadataExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ScaffoldingModelAnnotations Scaffolding([NotNull] this IModel model)
            => new ScaffoldingModelAnnotations(Check.NotNull(model, nameof(model)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ScaffoldingPropertyAnnotations Scaffolding([NotNull] this IProperty property)
            => new ScaffoldingPropertyAnnotations(Check.NotNull(property, nameof(property)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ScaffoldingEntityTypeAnnotations Scaffolding([NotNull] this IEntityType entityType)
            => new ScaffoldingEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));
    }
}
