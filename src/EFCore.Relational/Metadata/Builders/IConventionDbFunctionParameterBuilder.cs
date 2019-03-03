// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Provides a simple API for configuring a <see cref="IConventionDbFunctionParameter" />.
    /// </summary>
    public interface IConventionDbFunctionParameterBuilder
    {
        IConventionDbFunctionParameter Metadata { get; }

        IConventionDbFunctionParameterBuilder HasNullPropagation(bool supportsNullPropagation, bool fromDataAnnotation = false);

        bool CanSetSupportsNullPropagation(bool supportsNullPropagation, bool fromDataAnnotation = false);

        IConventionDbFunctionParameterBuilder HasStoreType([CanBeNull] string storeType, bool fromDataAnnotation = false);

        bool CanSetStoreType([CanBeNull] string storeType, bool fromDataAnnotation = false);

        IConventionDbFunctionParameterBuilder HasTypeMapping([CanBeNull] RelationalTypeMapping typeMapping, bool fromDataAnnotation = false);

        bool CanSetTypeMapping([CanBeNull] RelationalTypeMapping typeMapping, bool fromDataAnnotation = false);
    }
}
