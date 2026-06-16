// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A node containing an expression expressing the retrieval of a value which doesn't change across executions of the query.
/// </summary>
/// <remarks>
///     <para>
///         When the expression tree is compiled, the value will be retrieved from the expression, and a
///         <see cref="ConstantExpression" /> expression can directly reference the result.
///     </para>
///     <para>
///         When the expression tree is translated to source code instead (in query pre-compilation), the expression can be rendered out
///         separately, to be assigned to a private static readonly field, and this node is replaced by a reference to that field.
///     </para>
/// </remarks>
[DebuggerDisplay("{Microsoft.EntityFrameworkCore.Query.ExpressionPrinter.Print(this), nq}"),
 Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
public class RuntimeConstantExpression : Expression, IPrintableExpression
{
    private readonly ConstantExpression _constantExpression;

    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RuntimeConstantExpression(string name, Expression initializeExpression)
    {
        var type = initializeExpression.Type;
        Value = Lambda<Func<object>>(Convert(initializeExpression, typeof(object)), null).Compile()();
        _constantExpression = Constant(Value, type);

        InitializeExpression = initializeExpression;
        Name = char.ToUpper(name[0]) + name[1..];
        Type = type;
    }

    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression InitializeExpression { get; }

    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object Value { get; }

    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name { get; }

    /// <inheritdoc />
    public override Type Type { get; }

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    public override bool CanReduce
        => true;

    /// <inheritdoc />
    public override Expression Reduce()
        => _constantExpression;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var initializeExpression = visitor.Visit(InitializeExpression);

        return Update(initializeExpression);
    }

    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RuntimeConstantExpression Update(Expression initializeExpression)
        => initializeExpression != InitializeExpression
            ? new RuntimeConstantExpression(Name, initializeExpression)
            : this;

    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("[RUNTIME Constant: ");
        expressionPrinter.Visit(InitializeExpression);
        expressionPrinter.Append(" | Storage: ");
        expressionPrinter.Append(Name);
        expressionPrinter.Append("]");
    }
}
