// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.Templating.Compilation
{
    public class CompilationResult
    {
        private readonly Type _type;

        private CompilationResult([NotNull] string generatedCode, [CanBeNull] Type type, [NotNull] IEnumerable<string> messages)
        {
            Check.NotNull(generatedCode, nameof(generatedCode));
            Check.NotNull(messages, nameof(messages));

            _type = type;
            GeneratedCode = generatedCode;
            Messages = messages;
        }

        public IEnumerable<string> Messages { get; }

        public string GeneratedCode { get; }

        public Type CompiledType
        {
            get
            {
                if (_type == null)
                {
                    throw new TemplateProcessingException(Messages, GeneratedCode);
                }

                return _type;
            }
        }

        public static CompilationResult Failed([NotNull] string generatedCode, [NotNull] IEnumerable<string> messages)
        {
            Check.NotNull(generatedCode, nameof(generatedCode));
            Check.NotNull(messages, nameof(messages));

            return new CompilationResult(generatedCode, type: null, messages: messages);
        }

        public static CompilationResult Successful([NotNull] string generatedCode, [NotNull] Type type)
        {
            Check.NotNull(generatedCode, nameof(generatedCode));
            Check.NotNull(type, nameof(type));

            return new CompilationResult(generatedCode, type, Enumerable.Empty<string>());
        }
    }
}
