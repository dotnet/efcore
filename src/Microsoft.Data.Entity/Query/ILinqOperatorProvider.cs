// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
