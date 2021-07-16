// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     Used to generate C# code for creating an <see cref="IModel" />.
    /// </summary>
    public interface ICSharpSnapshotGenerator
    {
        /// <summary>
        ///     Generates code for creating an <see cref="IModel" />.
        /// </summary>
        /// <param name="builderName"> The <see cref="ModelBuilder" /> variable name. </param>
        /// <param name="model"> The model. </param>
        /// <param name="stringBuilder"> The builder code is added to. </param>
        void Generate(
            string builderName,
            IModel model,
            IndentedStringBuilder stringBuilder);
    }
}
