// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Scaffolding.Internal.Templating
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
            => RelationalDesignStrings.TemplateProcessingFailed(FormatMessage(Messages));

        private static string FormatMessage([param: NotNull] IEnumerable<string> messages) 
            => String.Join(Environment.NewLine, messages);
    }
}
