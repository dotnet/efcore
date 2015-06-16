// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.Templating
{
    public class TemplateProcessingException : Exception
    {
        public TemplateProcessingException([NotNull] IEnumerable<string> messages, [NotNull] string generatedCode)
            : base(FormatMessage(messages))
        {
            Messages = messages;
            GeneratedCode = generatedCode;
        }

        public virtual string GeneratedCode { get; [param: NotNull] private set; }

        public virtual IEnumerable<string> Messages { get; }

        public override string Message
        {
            get { return Strings.TemplateProcessingFailed(FormatMessage(Messages)); }
        }

        private static string FormatMessage([param: NotNull] IEnumerable<string> messages)
        {
            return String.Join(Environment.NewLine, messages);
        }
    }
}
