// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ValueGeneration.Internal
{
    public interface ISqlServerValueGeneratorCache : IValueGeneratorCache
    {
        SqlServerSequenceValueGeneratorState GetOrAddSequenceState([NotNull] IProperty property);
    }
}
