// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public interface IEntityAttacher
    {
        void HandleEntity([NotNull] EntityEntry entry);
    }
}
