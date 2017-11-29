// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Scaffolding;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestProviderScaffoldingCodeGenerator : IScaffoldingProviderCodeGenerator
    {
        public string GenerateUseProvider(string connectionString, string language)
            => $@".UseTestProvider(""{connectionString}"")";
    }
}
