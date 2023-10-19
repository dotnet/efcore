// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents creation of a structural type instance in <see cref="ShapedQueryExpression.ShaperExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public class StructuralTypeShaperExpression : Expression, IPrintableExpression
{
    private static readonly MethodInfo CreateUnableToDiscriminateExceptionMethod
        = typeof(StructuralTypeShaperExpression).GetTypeInfo().GetDeclaredMethod(nameof(CreateUnableToDiscriminateException))!;

    [UsedImplicitly]
    private static Exception CreateUnableToDiscriminateException(ITypeBase type, object discriminator)
        => new InvalidOperationException(CoreStrings.UnableToDiscriminate(type.DisplayName(), discriminator.ToString()));

    /// <summary>
    ///     Creates a new instance of the <see cref="StructuralTypeShaperExpression" /> class.
    /// </summary>
    /// <param name="type">The entity or complex type to shape.</param>
    /// <param name="valueBufferExpression">An expression of ValueBuffer to get values for properties of the type.</param>
    /// <param name="nullable">A bool value indicating whether this instance can be null.</param>
    public StructuralTypeShaperExpression(
        ITypeBase type,
        Expression valueBufferExpression,
        bool nullable)
        : this(type, valueBufferExpression, nullable, null)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="StructuralTypeShaperExpression" /> class.
    /// </summary>
    /// <param name="type">The entity or complex type to shape.</param>
    /// <param name="valueBufferExpression">An expression of ValueBuffer to get values for properties of the type.</param>
    /// <param name="nullable">Whether this instance can be null.</param>
    /// <param name="materializationCondition">
    ///     An expression of <see cref="Func{ValueBuffer, ITypeBase}" /> to determine which structural type to materialize.
    /// </param>
    protected StructuralTypeShaperExpression(
        ITypeBase type,
        Expression valueBufferExpression,
        bool nullable,
        LambdaExpression? materializationCondition)
    {
        if (materializationCondition == null)
        {
            materializationCondition = GenerateMaterializationCondition(type, nullable);
        }
        else if (materializationCondition.Parameters.Count != 1
                 || materializationCondition.Parameters[0].Type != typeof(ValueBuffer)
                 || materializationCondition.ReturnType != (type is IEntityType ? typeof(IEntityType) : typeof(IComplexType)))
        {
            throw new InvalidOperationException(CoreStrings.QueryEntityMaterializationConditionWrongShape(type.DisplayName()));
        }

        StructuralType = type;
        ValueBufferExpression = valueBufferExpression;
        IsNullable = nullable;
        MaterializationCondition = materializationCondition!;
    }

    /// <summary>
    ///     Creates an expression to throw an exception when we're unable to determine the structural type to materialize based on
    ///     discriminator value.
    /// </summary>
    /// <param name="type">The entity type for which materialization was requested.</param>
    /// <param name="discriminatorValue">The expression containing value of discriminator.</param>
    /// <returns>
    ///     An expression of <see cref="Func{ValueBuffer, IEntityType}" /> representing materilization condition for the entity type.
    /// </returns>
    protected static Expression CreateUnableToDiscriminateExceptionExpression(ITypeBase type, Expression discriminatorValue)
        => Block(
            Throw(
                Call(
                    CreateUnableToDiscriminateExceptionMethod,
                    Constant(type),
                    Convert(discriminatorValue, typeof(object)))),
            Constant(null, typeof(IEntityType)));

    /// <summary>
    ///     Creates an expression of <see cref="Func{ValueBuffer, ITypeBase}" /> to determine which type to materialize.
    /// </summary>
    /// <param name="type">The type to create materialization condition for.</param>
    /// <param name="nullable">Whether this instance can be null.</param>
    /// <returns>
    ///     An expression of <see cref="Func{ValueBuffer, ITypeBase}" /> representing materialization condition for the type.
    /// </returns>
    protected virtual LambdaExpression GenerateMaterializationCondition(ITypeBase type, bool nullable)
    {
        var valueBufferParameter = Parameter(typeof(ValueBuffer));

        if (type is IComplexType complexType)
        {
            return Lambda(Constant(complexType, typeof(IComplexType)), valueBufferParameter);
        }

        var entityType = (IEntityType)type;
        Expression body;
        var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToArray();
        var discriminatorProperty = entityType.FindDiscriminatorProperty();
        if (discriminatorProperty != null)
        {
            var discriminatorValueVariable = Variable(discriminatorProperty.ClrType, "discriminator");
            var expressions = new List<Expression>
            {
                Assign(
                    discriminatorValueVariable,
                    valueBufferParameter.CreateValueBufferReadValueExpression(
                        discriminatorProperty.ClrType, discriminatorProperty.GetIndex(), discriminatorProperty))
            };

            var exception = CreateUnableToDiscriminateExceptionExpression(entityType, discriminatorValueVariable);

            var discriminatorComparer = discriminatorProperty.GetKeyValueComparer();
            if (discriminatorComparer.IsDefault())
            {
                var switchCases = new SwitchCase[concreteEntityTypes.Length];
                for (var i = 0; i < concreteEntityTypes.Length; i++)
                {
                    var discriminatorValue = Constant(concreteEntityTypes[i].GetDiscriminatorValue(), discriminatorProperty.ClrType);
                    switchCases[i] = SwitchCase(Constant(concreteEntityTypes[i], typeof(IEntityType)), discriminatorValue);
                }

                expressions.Add(Switch(discriminatorValueVariable, exception, switchCases));
            }
            else
            {
                var conditions = exception;
                for (var i = concreteEntityTypes.Length - 1; i >= 0; i--)
                {
                    conditions = Condition(
                        discriminatorComparer.ExtractEqualsBody(
                            discriminatorValueVariable,
                            Constant(
                                concreteEntityTypes[i].GetDiscriminatorValue(),
                                discriminatorProperty.ClrType)),
                        Constant(concreteEntityTypes[i], typeof(IEntityType)),
                        conditions);
                }

                expressions.Add(conditions);
            }

            body = Block(new[] { discriminatorValueVariable }, expressions);
        }
        else
        {
            body = Constant(concreteEntityTypes.Length == 1 ? concreteEntityTypes[0] : entityType, typeof(IEntityType));
        }

        if (entityType.FindPrimaryKey() == null
            && nullable)
        {
            // If there's no nullable key and we're generating a nullable shaper, generate checks for any non-null property; if all are
            // null, return null for the entity instance.
            body = Condition(
                entityType.GetProperties()
                    .Select(
                        p => NotEqual(
                            valueBufferParameter.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                            Constant(null)))
                    .Aggregate(OrElse),
                body,
                Default(typeof(IEntityType)));
        }

        return Lambda(body, valueBufferParameter);
    }

    /// <summary>
    ///     The entity or complex type being shaped.
    /// </summary>
    public virtual ITypeBase StructuralType { get; }

    /// <summary>
    ///     The expression representing a <see cref="ValueBuffer" /> to get values from that are used to create the instance.
    /// </summary>
    public virtual Expression ValueBufferExpression { get; }

    /// <summary>
    ///     A value indicating whether this instance can be null.
    /// </summary>
    public virtual bool IsNullable { get; }

    /// <summary>
    ///     The materialization condition to use for shaping this structural type.
    /// </summary>
    public virtual LambdaExpression MaterializationCondition { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var valueBufferExpression = visitor.Visit(ValueBufferExpression);

        return Update(valueBufferExpression);
    }

    /// <summary>
    ///     Changes the structural type being shaped by this shaper.
    /// </summary>
    /// <param name="type">The new type to use.</param>
    /// <returns>This expression if the type was not changed, or a new expression with the updated type.</returns>
    public virtual StructuralTypeShaperExpression WithType(ITypeBase type)
        => type != StructuralType
            ? new StructuralTypeShaperExpression(type, ValueBufferExpression, IsNullable, materializationCondition: null)
            : this;

    /// <summary>
    ///     Assigns nullability for this shaper, indicating whether it can shape null instances or not.
    /// </summary>
    /// <param name="nullable">A value indicating if the shaper is nullable.</param>
    /// <returns>This expression if nullability not changed, or an expression with updated nullability.</returns>
    public virtual StructuralTypeShaperExpression MakeNullable(bool nullable = true)
        => IsNullable != nullable
            // Marking nullable requires re-computation of materialization condition
            ? new StructuralTypeShaperExpression(StructuralType, ValueBufferExpression, nullable, materializationCondition: null)
            : this;

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="valueBufferExpression">The <see cref="ValueBufferExpression" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual StructuralTypeShaperExpression Update(Expression valueBufferExpression)
        => valueBufferExpression != ValueBufferExpression
            ? new StructuralTypeShaperExpression(StructuralType, valueBufferExpression, IsNullable, MaterializationCondition)
            : this;

    /// <inheritdoc />
    public override Type Type
        => StructuralType.ClrType;

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine(nameof(StructuralTypeShaperExpression) + ": ");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.AppendLine(StructuralType.Name);
            expressionPrinter.AppendLine(nameof(ValueBufferExpression) + ": ");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Visit(ValueBufferExpression);
                expressionPrinter.AppendLine();
            }

            expressionPrinter.Append(nameof(IsNullable) + ": ");
            expressionPrinter.AppendLine(IsNullable.ToString());
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string DebuggerDisplay()
        => $"{StructuralType.DisplayName()} ({(IsNullable ? "nullable" : "required")})";
}
