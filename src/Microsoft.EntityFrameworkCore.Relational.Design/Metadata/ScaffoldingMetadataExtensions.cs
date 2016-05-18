// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public static class ScaffoldingMetadataExtensions
    {
        public static ScaffoldingModelAnnotations Scaffolding([NotNull] this IModel model)
            => new ScaffoldingModelAnnotations(Check.NotNull(model, nameof(model)));

        public static ScaffoldingPropertyAnnotations Scaffolding([NotNull] this IProperty property)
            => new ScaffoldingPropertyAnnotations(Check.NotNull(property, nameof(property)));
    }
}
