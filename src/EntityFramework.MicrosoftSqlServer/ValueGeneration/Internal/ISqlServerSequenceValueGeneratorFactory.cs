// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage.Internal;

namespace Microsoft.Data.Entity.ValueGeneration.Internal
{
    public interface ISqlServerSequenceValueGeneratorFactory
    {
        ValueGenerator Create(
            [NotNull] IProperty property,
            [NotNull] SqlServerSequenceValueGeneratorState generatorState,
            [NotNull] ISqlServerConnection connection);
    }
}
