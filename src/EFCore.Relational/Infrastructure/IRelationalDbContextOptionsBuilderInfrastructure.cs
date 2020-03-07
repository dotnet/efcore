// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Explicitly implemented by <see cref="RelationalDbContextOptionsBuilder{TBuilder, TExtension}" /> to hide
    ///     methods that are used by database provider extension methods but not intended to be called by application
    ///     developers.
    /// </summary>
    public interface IRelationalDbContextOptionsBuilderInfrastructure
    {
        /// <summary>
        ///     Gets the core options builder.
        /// </summary>
        DbContextOptionsBuilder OptionsBuilder { get; }
    }
}
