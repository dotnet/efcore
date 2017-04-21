// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Define a parameter for a user defined database function
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class DbFunctionParameterAttribute : Attribute
    {
        /// <summary>
        ///    Sets the index order for this parameter on the parent function
        /// </summary>
        public int ParameterIndex { get; [param: NotNull] set; }
    }
}
