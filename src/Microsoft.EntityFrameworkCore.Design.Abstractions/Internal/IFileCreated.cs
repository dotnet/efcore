// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public interface IFileCreated : IOperationResult
    {
        string Id { get; }
        string FilePath { get; }
        string ContentType { get; }
    }
}
