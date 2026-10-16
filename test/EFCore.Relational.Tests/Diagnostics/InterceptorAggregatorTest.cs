// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

public class InterceptorAggregatorTest
{
    [Theory]
    [InlineData(typeof(DbConnectionInterceptorAggregator), typeof(IDbConnectionInterceptor))]
    [InlineData(typeof(DbCommandInterceptorAggregator), typeof(IDbCommandInterceptor))]
    [InlineData(typeof(DbTransactionInterceptorAggregator), typeof(IDbTransactionInterceptor))]
    [InlineData(typeof(SaveChangesInterceptorAggregator), typeof(ISaveChangesInterceptor))]
    [InlineData(typeof(IdentityResolutionInterceptorAggregator), typeof(IIdentityResolutionInterceptor))]
    [InlineData(typeof(QueryExpressionInterceptorAggregator), typeof(IQueryExpressionInterceptor))]
    [InlineData(typeof(MaterializationInterceptorAggregator), typeof(IMaterializationInterceptor))]
    public void Composite_implements_all_interceptor_methods(Type aggregatorType, Type interceptorType)
    {
        var compositeType = Assert.Single(
            aggregatorType.GetNestedTypes(BindingFlags.NonPublic),
            interceptorType.IsAssignableFrom);

        foreach (var method in compositeType.GetInterfaceMap(interceptorType).TargetMethods)
        {
            Assert.Equal(compositeType, method.DeclaringType);
        }
    }
}
