// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Design
{
    public interface IOperationResultHandler
    {
        int Version { get; }
        void OnResult([CanBeNull] object value);
        void OnError([NotNull] string type, [NotNull] string message, [NotNull] string stackTrace);
    }
}
