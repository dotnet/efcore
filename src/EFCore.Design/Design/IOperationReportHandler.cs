// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Design
{
    public interface IOperationReportHandler
    {
        int Version { get; }
        void OnError([NotNull] string message);
        void OnWarning([NotNull] string message);
        void OnInformation([NotNull] string message);
        void OnVerbose([NotNull] string message);
    }
}
