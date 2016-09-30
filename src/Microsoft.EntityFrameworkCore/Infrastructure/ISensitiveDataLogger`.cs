// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     An <see cref="ILogger{TCategoryName}" /> for which logging of sensitive data can be enabled or disabled.
    /// </summary>
    /// <typeparam name="T"> The type who's name is used for the logger category name. </typeparam>
    public interface ISensitiveDataLogger<out T> : ILogger<T>, ISensitiveDataLogger
    {
    }
}
