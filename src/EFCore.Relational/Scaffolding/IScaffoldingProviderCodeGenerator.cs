// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     A service typically implemented by database providers to generate code fragments
    ///     for reverse engineering.
    /// </summary>
    [Obsolete("Use IProviderCodeGenerator instead.")]
    public interface IScaffoldingProviderCodeGenerator
    {
        /// <summary>
        ///     Generates a code fragment like <c>.UseSqlServer("Database=Foo")</c> which can be used in
        ///     the <see cref="DbContext.OnConfiguring" /> method of the generated DbContext.
        /// </summary>
        /// <param name="connectionString"> The connection string to include in the code fragment. </param>
        /// <param name="language"> The programming language to generate, such as 'CSharp'. </param>
        /// <returns> The code fragment, or <c>null</c> if the programming language is not supported. </returns>
        [Obsolete("Use IProviderCodeGenerator.GenerateUseProvider instead.")]
        string GenerateUseProvider([NotNull] string connectionString, [NotNull] string language);
    }
}
