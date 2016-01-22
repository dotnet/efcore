// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public interface IMigrationsIdGenerator
    {
        string GenerateId([NotNull] string name);
        string GetName([NotNull] string id);
        bool IsValidId([NotNull] string value);
    }
}
