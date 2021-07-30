// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    // ReSharper disable once ArrangeTypeModifiers
    internal partial class DbContextOptimizeCommand
    {
        protected override int Execute(string[] args)
        {
            using var executor = CreateExecutor(args);
            executor.OptimizeContext(
                _outputDir!.Value(),
                _namespace!.Value(),
                Context!.Value());

            return base.Execute(args);
        }
    }
}
