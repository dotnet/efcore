// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public interface IDependentKeyValueFactorySource
    {
        // Note: This is cast to a generic type, but the generic type is
        // not known by the implementing class
        object DependentKeyValueFactory { get; [param: NotNull] set; }
    }
}
