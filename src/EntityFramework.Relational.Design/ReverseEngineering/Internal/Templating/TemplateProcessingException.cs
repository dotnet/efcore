// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal.Templating
{
    public class TemplateProcessingException : Exception
    {
        public TemplateProcessingException([NotNull] IEnumerable<string> messages)
            : base(FormatMessage(messages))
        {
            Messages = messages;
        }

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
