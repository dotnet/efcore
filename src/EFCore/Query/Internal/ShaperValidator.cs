// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     Validates universal shaper invariants before building a materializer.
/// </remarks>
/// <param name="typeMappingSource">The type mapping source.</param>
/// <param name="queryTrackingBehavior">The query tracking behavior.</param>
[EntityFrameworkInternal]
public class ShaperValidator(ITypeMappingSource typeMappingSource, QueryTrackingBehavior queryTrackingBehavior) : ExpressionVisitor
{
    private readonly HashSet<IEntityType> _visitedEntityTypes = [];
    private readonly bool _validatingTrackAll = queryTrackingBehavior == QueryTrackingBehavior.TrackAll;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Validate(Expression shaper)
    {
        _visitedEntityTypes.Clear();

        Visit(shaper);

        if (_validatingTrackAll)
        {
            foreach (var entityType in _visitedEntityTypes)
            {
                if (entityType.FindOwnership() is { } ownership
                    && !ContainsOwner(ownership.PrincipalEntityType))
                {
                    throw new InvalidOperationException(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner);
                }
            }
        }
    }

    private bool ValidConstant(ConstantExpression constantExpression)
        => constantExpression.Value is null or Array { Length: 0 }
            || typeMappingSource.FindMapping(constantExpression.Type) != null;

    /// <inheritdoc />
    protected override Expression VisitConstant(ConstantExpression constantExpression)
        => !ValidConstant(constantExpression)
            ? throw new InvalidOperationException(
                CoreStrings.ClientProjectionCapturingConstantInTree(constantExpression.Type.DisplayName()))
            : constantExpression;

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (RemoveConvert(methodCallExpression.Object) is ConstantExpression constantInstance
            && !ValidConstant(constantInstance))
        {
            throw new InvalidOperationException(
                CoreStrings.ClientProjectionCapturingConstantInMethodInstance(
                    constantInstance.Type.DisplayName(),
                    methodCallExpression.Method.Name));
        }

        foreach (var argument in methodCallExpression.Arguments)
        {
            if (RemoveConvert(argument) is ConstantExpression constantArgument
                && !ValidConstant(constantArgument))
            {
                throw new InvalidOperationException(
                    CoreStrings.ClientProjectionCapturingConstantInMethodArgument(
                        constantArgument.Type.DisplayName(),
                        methodCallExpression.Method.Name));
            }
        }

        return base.VisitMethodCall(methodCallExpression);
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (_validatingTrackAll
            && extensionExpression is StructuralTypeShaperExpression { StructuralType: IEntityType entityType })
        {
            _visitedEntityTypes.Add(entityType);
        }

        return base.VisitExtension(extensionExpression);
    }

    private bool ContainsOwner(IEntityType? owner)
        => owner is not null
            && (_visitedEntityTypes.Any(owner.IsAssignableFrom) || ContainsOwner(owner.BaseType));

    private static Expression? RemoveConvert(Expression? expression)
    {
        while (expression is { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked })
        {
            expression = RemoveConvert(((UnaryExpression)expression).Operand);
        }

        return expression;
    }
}