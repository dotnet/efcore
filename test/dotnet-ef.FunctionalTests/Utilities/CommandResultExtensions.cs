// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.Cli.Utils;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public static class AssertCommand
    {
        public static CommandResult Passes(CommandResult result)
        {
            Assert.Equal(0, result.ExitCode);
            return result;
        }
    }
}
