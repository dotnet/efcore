// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal.Templating
{
    public abstract class RazorReverseEngineeringBase
    {
        private TextWriter Output { get; set; }

        public virtual dynamic Model { get; [param: NotNull] set; }

        public virtual ModelUtilities ModelUtilities { get; [param: NotNull] set; }
        public virtual CSharpUtilities CSharpUtilities { get;[param: NotNull] set; }

        public abstract Task ExecuteAsync();

        public virtual Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            // This method is here solely so we can pass the ApiConsistencyTest
            // Async_methods_should_have_overload_with_cancellation_token_and_end_with_async_suffix.
            return Task.FromResult<object>(null);
        }

        public virtual async Task<string> ExecuteTemplateAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var output = new StringBuilder();
            using (var writer = new StringWriter(output))
            {
                Output = writer;
                await ExecuteAsync();
            }
            return output.ToString();
        }

        public virtual void WriteLiteral([NotNull] object value)
        {
            WriteLiteralTo(Output, value);
        }

        public virtual void WriteLiteralTo([NotNull] TextWriter writer, [NotNull] object text)
        {
            if (text != null)
            {
                writer.Write(text.ToString());
            }
        }

        public virtual void Write([NotNull] object value)
        {
            WriteTo(Output, value);
        }

        public virtual void WriteTo([NotNull] TextWriter writer, [NotNull] object content)
        {
            if (content != null)
            {
                writer.Write(content.ToString());
            }
        }
    }
}
