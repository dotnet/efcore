// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !NET461

namespace System.CodeDom.Compiler
{
    internal class CompilerError
    {
        public string? ErrorText { get; set; }
        public bool IsWarning { get; set; }
    }
}

#endif
