// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IProperty : IPropertyBase
    {
        Type PropertyType { get; }
        bool IsNullable { get; }
        ValueGenerationStrategy ValueGenerationStrategy { get; }
        int Index { get; }
        int ShadowIndex { get; }
        int OriginalValueIndex { get; }
        bool IsClrProperty { get; }
        bool IsConcurrencyToken { get; }
    }
}
