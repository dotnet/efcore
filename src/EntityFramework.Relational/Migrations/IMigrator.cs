// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Migrations
{
    public interface IMigrator
    {
        void Migrate([CanBeNull] string targetMigration = null);

        string GenerateScript([CanBeNull] string fromMigration, [CanBeNull] string toMigration, bool idempotent = false);
    }
}
