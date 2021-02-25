// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     Used to generate C# code for creating <see cref="MigrationOperation" /> objects.
    /// </summary>
    public interface ICSharpMigrationOperationGenerator
    {
        /// <summary>
        ///     Generates code for creating <see cref="MigrationOperation" /> objects.
        /// </summary>
        /// <param name="builderName"> The <see cref="MigrationOperation" /> variable name. </param>
        /// <param name="operations"> The operations. </param>
        /// <param name="builder"> The builder code is added to. </param>
        void Generate(
            [NotNull] string builderName,
            [NotNull] IReadOnlyList<MigrationOperation> operations,
            [NotNull] IndentedStringBuilder builder);
    }
}
