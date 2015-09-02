// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal.Templating.Compilation
{
    public class CompilationResult
    {
        private readonly Type _type;

        private CompilationResult([CanBeNull] Type type, [NotNull] IEnumerable<string> messages)
        {
            Check.NotNull(messages, nameof(messages));

            _type = type;
            Messages = messages;
        }

        public IEnumerable<string> Messages { get; }

        public Type CompiledType
        {
            get
            {
                if (_type == null)
                {
                    throw new TemplateProcessingException(Messages);
                }

                return _type;
            }
        }

        public static CompilationResult Failed([NotNull] IEnumerable<string> messages)
        {
            Check.NotNull(messages, nameof(messages));

            return new CompilationResult(type: null, messages: messages);
        }

        public static CompilationResult Successful([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return new CompilationResult(type, Enumerable.Empty<string>());
        }
    }
}
