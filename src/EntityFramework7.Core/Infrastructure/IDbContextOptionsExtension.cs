// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Infrastructure
{
    public interface IDbContextOptionsExtension
    {
        void ApplyServices([NotNull] EntityFrameworkServicesBuilder builder);
    }
}
