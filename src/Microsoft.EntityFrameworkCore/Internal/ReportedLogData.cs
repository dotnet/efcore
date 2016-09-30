// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ReportedLogData<TState> : IReportedLogData
    {
        private readonly Lazy<string> _message;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReportedLogData(
            [CanBeNull] TState state,
            [CanBeNull] Exception exception,
            [CanBeNull] Func<TState, Exception, string> formatter)
        {
            _message = new Lazy<string>(
                () =>
                {
                    var builder = new StringBuilder();
                    if (formatter != null)
                    {
                        builder.Append(formatter(state, exception));
                    }
                    else if (state != null)
                    {
                        builder.Append(state);

                        if (exception != null)
                        {
                            builder
                                .AppendLine()
                                .Append(exception);
                        }
                    }

                    return builder.ToString();

                });
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string ToString()
            => _message.Value;

        string IReportedLogData.Message
            => ToString();
    }
}
