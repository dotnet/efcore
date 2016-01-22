// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Entity.Relational.Design.FunctionalTests.Compilation
{
    public class CompilationResult
    {
        private readonly Type _type;

        private CompilationResult(Type type, IEnumerable<string> messages)
        {
            _type = type;
            Messages = messages;
        }

        public IEnumerable<string> Messages { get; }

        public static CompilationResult Failed(IEnumerable<string> messages)
        {
            return new CompilationResult(type: null, messages: messages);
        }

        public static CompilationResult Successful(Type type)
        {
            return new CompilationResult(type, Enumerable.Empty<string>());
        }
    }
}
