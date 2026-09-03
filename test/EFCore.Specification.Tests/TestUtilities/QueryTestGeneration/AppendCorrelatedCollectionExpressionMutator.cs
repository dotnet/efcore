// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;

public class AppendCorrelatedCollectionExpressionMutator(DbContext context) : ExpressionMutator(context)
{
    private bool ContainsCollectionNavigation(Type type)
        => Context.Model.FindEntityType(type)?.GetNavigations().Any(n => n.IsCollection) ?? false;

    public override bool IsValid(Expression expression)
        => IsQueryableResult(expression)
            && IsEntityType(expression.Type.GetGenericArguments()[0])
            && ContainsCollectionNavigation(expression.Type.GetGenericArguments()[0]);

    public override Expression Apply(Expression expression, Random random)
    {
        var typeArgument = expression.Type.GetGenericArguments()[0];
        var navigations = Context.Model.FindEntityType(typeArgument)!.GetNavigations().Where(n => n.IsCollection).ToList();

        var i = random.Next(navigations.Count);
        var navigation = navigations[i];

        var collectionElementType = navigation.ForeignKey.DeclaringEntityType.ClrType;
        var listType = typeof(List<>).MakeGenericType(collectionElementType);

        var select = QueryableMethods.Select.MakeGenericMethod(typeArgument, listType);
        var where = EnumerableMethods.Where.MakeGenericMethod(collectionElementType);
        var toList = EnumerableMethods.ToList.MakeGenericMethod(collectionElementType);

        var outerPrm = Expression.Parameter(typeArgument, "outerPrm");
        var innerPrm = Expression.Parameter(collectionElementType, "innerPrm");

        var outerLambdaBody = Expression.Call(
            toList,
            Expression.Call(
                where,
                Expression.Property(outerPrm, navigation.PropertyInfo!),
                Expression.Lambda(Expression.Constant(true), innerPrm)));

        var resultExpression = Expression.Call(
            select,
            expression,
            Expression.Lambda(outerLambdaBody, outerPrm));

        return resultExpression;
    }
}
