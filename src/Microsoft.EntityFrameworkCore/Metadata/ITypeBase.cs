// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     TODO: ComplexType docs
    /// </summary>
    public interface ITypeBase : IAnnotatable
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IModel Model { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        Type ClrType { get; }
    }
}
