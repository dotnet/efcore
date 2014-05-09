// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata
{
    public interface IValueReader
    {
        bool IsNull(int index);
        T ReadValue<T>(int index);
        int Count { get; }
    }
}
