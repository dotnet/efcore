// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class OperationReporterExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void Write(this IOperationReporter reporter, CompilerError error)
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrEmpty(error.FileName))
        {
            builder.Append(error.FileName);

            if (error.Line > 0)
            {
                builder
                    .Append("(")
                    .Append(error.Line);

                if (error.Column > 0)
                {
                    builder
                        .Append(",")
                        .Append(error.Column);
                }

                builder.Append(")");
            }

            builder.Append(" : ");
        }

        builder
            .Append(error.IsWarning ? "warning" : "error")
            .Append(" ")
            .Append(error.ErrorNumber)
            .Append(": ")
            .AppendLine(error.ErrorText);

        if (error.IsWarning)
        {
            reporter.WriteWarning(builder.ToString());
        }
        else
        {
            reporter.WriteError(builder.ToString());
        }
    }
}
