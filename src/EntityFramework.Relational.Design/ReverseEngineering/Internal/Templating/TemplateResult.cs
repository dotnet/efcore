// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal.Templating
{
    public class TemplateResult
    {
        public virtual string GeneratedText { get; [param: NotNull] set; }

        public virtual TemplateProcessingException ProcessingException { get; [param: NotNull] set; }
    }
}
