// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An object that represents a WHEN...THEN... construct in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class CaseWhenClause
{
    /// <summary>
    ///     Creates a new instance of the <see cref="CaseWhenClause" /> class.
    /// </summary>
    /// <param name="test">A value to compare with <see cref="CaseExpression.Operand" /> or condition to evaluate.</param>
    /// <param name="result">A value to return if test succeeds.</param>
    public CaseWhenClause(SqlExpression test, SqlExpression result)
    {
        Test = test;
        Result = result;
    }

    /// <summary>
    ///     The value to compare with <see cref="CaseExpression.Operand" /> or the condition to evaluate.
    /// </summary>
    public virtual SqlExpression Test { get; }

    /// <summary>
    ///     The value to return if <see cref="Test" /> succeeds.
    /// </summary>
    public virtual SqlExpression Result { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is CaseWhenClause caseWhenClause
                && Equals(caseWhenClause));

    private bool Equals(CaseWhenClause caseWhenClause)
        => Test.Equals(caseWhenClause.Test)
            && Result.Equals(caseWhenClause.Result);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Test, Result);
}
