// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An interface that allows printing via <see cref="ExpressionPrinter" />.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IPrintableExpression
    {
        /// <summary>
        ///     Creates a printable string representation of the given expression using <see cref="ExpressionPrinter" />.
        /// </summary>
        /// <param name="expressionPrinter"> The expression printer to use. </param>
        void Print([NotNull] ExpressionPrinter expressionPrinter);
    }
}
