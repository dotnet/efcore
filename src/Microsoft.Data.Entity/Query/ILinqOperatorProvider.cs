// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.Data.Entity.Query
{
    public interface ILinqOperatorProvider
    {
        MethodInfo SelectMany { get; }
        MethodInfo Join { get; }
        MethodInfo GroupJoin { get; }
        MethodInfo Select { get; }
        MethodInfo OrderBy { get; }
        MethodInfo ThenBy { get; }
        MethodInfo Where { get; }
        MethodInfo ToSequence { get; }
    }
}
