// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !NET472
namespace System.CodeDom.Compiler
{
    internal class CompilerError
    {
        public string? ErrorText { get; set; }
        public bool IsWarning { get; set; }
    }
}

#endif
