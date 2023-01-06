// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static System.Linq.Expressions.Expression;
namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CSharpToLinqTranslator : CSharpSyntaxVisitor<Expression>, ICSharpToLinqTranslator
{
    private static readonly SymbolDisplayFormat QualifiedTypeNameSymbolDisplayFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    private Compilation? _compilation;

#pragma warning disable CS8618 // Uninitialized non-nullable fields. We check _compilation to make sure LoadCompilation was invoked.
    private DbContext _userDbContext;
    private Type _userDbContextType;
    private INamedTypeSymbol _userDbContextSymbol;
    private INamedTypeSymbol _dbSetSymbol;
#pragma warning restore CS8618

    private SemanticModel _semanticModel = null!;

    private static MethodInfo? _stringConcatMethod;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void Load(Compilation compilation, DbContext userDbContext)
    {
        _compilation = compilation;
        _userDbContext = userDbContext;
        _userDbContextType = userDbContext.GetType();
        _userDbContextSymbol = GetTypeSymbolOrThrow(_userDbContextType.FullName!);
        _dbSetSymbol = GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.DbSet`1");

        INamedTypeSymbol GetTypeSymbolOrThrow(string fullyQualifiedMetadataName)
            => _compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)
                ?? throw new InvalidOperationException("Could not find type symbol for: " + fullyQualifiedMetadataName);
    }

    private readonly Stack<ImmutableDictionary<string, ParameterExpression>> _parameterStack
        = new(new[] { ImmutableDictionary<string, ParameterExpression>.Empty });

    private readonly Dictionary<ISymbol, MemberExpression?> _capturedVariables = new(SymbolEqualityComparer.Default);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Expression Translate(SyntaxNode node, SemanticModel semanticModel)
    {
        if (_compilation is null)
        {
            throw new InvalidOperationException("A compilation must be loaded.");
        }

        Check.DebugAssert(
            ReferenceEquals(semanticModel.SyntaxTree, node.SyntaxTree),
            "Provided semantic model doesn't match the provided syntax node");

        _semanticModel = semanticModel;

        // Perform data flow analysis to detect all captured data (closure parameters)
        _capturedVariables.Clear();
        foreach (var captured in _semanticModel.AnalyzeDataFlow(node).Captured)
        {
            _capturedVariables[captured] = null;
        }

        var result = Visit(node);

        // TODO: Sanity check: make sure all captured variables in _capturedVariables have non-null values
        // (i.e. have been encountered and referenced)

        Debug.Assert(_parameterStack.Count == 1);
        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull("node")]
    public override Expression? Visit(SyntaxNode? node)
        => base.Visit(node);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax anonymousObjectCreation)
    {
        // Creating an actual anonymous object means creating a new type, which can only be done with Reflection.Emit.
        // At least for EF's purposes, it doesn't matter, so we build a placeholder.
        if (_semanticModel.GetSymbolInfo(anonymousObjectCreation).Symbol is not IMethodSymbol constructorSymbol)
        {
            throw new InvalidOperationException(
                "Could not find symbol for anonymous object creation initializer: " + anonymousObjectCreation);
        }

        var anonymousType = ResolveType(constructorSymbol.ContainingType);

        var parameters = constructorSymbol.Parameters.ToArray();

        var parameterInfos = new ParameterInfo[parameters.Length];
        var memberInfos = new MemberInfo[parameters.Length];
        var arguments = new Expression[parameters.Length];

        foreach (var initializer in anonymousObjectCreation.Initializers)
        {
            // If the initializer's name isn't explicitly specified, infer it from the initializer's expression like the compiler does
            var name = initializer.NameEquals is not null
                ? initializer.NameEquals.Name.Identifier.Text
                : initializer.Expression is MemberAccessExpressionSyntax memberAccess
                    ? memberAccess.Name.Identifier.Text
                    : throw new InvalidOperationException(
                        $"AnonymousObjectCreation: unnamed initializer with non-MemberAccess expression: {initializer.Expression}");

            var position = Array.FindIndex(parameters, p => p.Name == name);
            var parameter = parameters[position];
            var parameterType = ResolveType(parameter.Type) ?? throw new InvalidOperationException(
                "Could not resolve type symbol for: " + parameter.Type);

            parameterInfos[position] = new FakeParameterInfo(name, parameterType, position);
            arguments[position] = Visit(initializer.Expression);
            memberInfos[position] = anonymousType.GetProperty(parameter.Name)!;
        }

        return New(
            new FakeConstructorInfo(anonymousType, parameterInfos),
            arguments: arguments,
            memberInfos);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitArgument(ArgumentSyntax argument)
    {
        if (!argument.RefKindKeyword.IsKind(SyntaxKind.None))
        {
            throw new InvalidOperationException($"Argument with ref/out: {argument}");
        }

        return Visit(argument.Expression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitArrayCreationExpression(ArrayCreationExpressionSyntax arrayCreation)
    {
        if (_semanticModel.GetTypeInfo(arrayCreation).Type is not IArrayTypeSymbol arrayTypeSymbol)
        {
            throw new InvalidOperationException($"ArrayCreation: non-array type symbol: {arrayCreation}");
        }

        if (arrayTypeSymbol.Rank > 1)
        {
            throw new NotImplementedException($"ArrayCreation: multi-dimensional array: {arrayCreation}");
        }

        var elementType = ResolveType(arrayTypeSymbol.ElementType);
        Check.DebugAssert(elementType is not null, "elementType is not null");

        return arrayCreation.Initializer is null
            ? NewArrayBounds(elementType, Visit(arrayCreation.Type.RankSpecifiers[0].Sizes[0]))
            : NewArrayInit(elementType, arrayCreation.Initializer.Expressions.Select(e => Visit(e)));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitBinaryExpression(BinaryExpressionSyntax binary)
    {
        var left = Visit(binary.Left);
        var right = Visit(binary.Right);

        // https://learn.microsoft.com/dotnet/api/Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax
        return binary.Kind() switch
        {
            // String concatenation
            SyntaxKind.AddExpression
                when left.Type == typeof(string) && right.Type == typeof(string)
                => Add(left, right,
                    _stringConcatMethod ??=
                        typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) })),

            SyntaxKind.AddExpression => Add(left, right),
            SyntaxKind.SubtractExpression => Subtract(left, right),
            SyntaxKind.MultiplyExpression => Multiply(left, right),
            SyntaxKind.DivideExpression => Divide(left, right),
            SyntaxKind.ModuloExpression => Modulo(left, right),
            SyntaxKind.LeftShiftExpression => LeftShift(left, right),
            SyntaxKind.RightShiftExpression => RightShift(left, right),
            // TODO UnsignedRightShiftExpression
            SyntaxKind.LogicalOrExpression => OrElse(left, right),
            SyntaxKind.LogicalAndExpression => AndAlso(left, right),
            SyntaxKind.BitwiseOrExpression => Or(left, right),
            SyntaxKind.BitwiseAndExpression => And(left, right),
            SyntaxKind.ExclusiveOrExpression => ExclusiveOr(left, right),
            SyntaxKind.EqualsExpression => Equal(left, right),
            SyntaxKind.NotEqualsExpression => NotEqual(left, right),
            SyntaxKind.LessThanExpression => LessThan(left, right),
            SyntaxKind.LessThanOrEqualExpression => LessThanOrEqual(left, right),
            SyntaxKind.GreaterThanExpression => GreaterThan(left, right),
            SyntaxKind.GreaterThanOrEqualExpression => GreaterThanOrEqual(left, right),
            SyntaxKind.IsExpression => TypeIs(left, right is ConstantExpression { Value : Type type }
                ? type
                : throw new InvalidOperationException($"Encountered {SyntaxKind.IsExpression} with non-constant type right argument: {right}")),
            SyntaxKind.AsExpression => TypeAs(left, right is ConstantExpression { Value : Type type }
                ? type
                : throw new InvalidOperationException($"Encountered {SyntaxKind.AsExpression} with non-constant type right argument: {right}")),
            SyntaxKind.CoalesceExpression => Coalesce(left, right),

            _ => throw new ArgumentOutOfRangeException($"BinaryExpressionSyntax with {binary.Kind()}")
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitCastExpression(CastExpressionSyntax cast)
        => Convert(Visit(cast.Expression), ResolveType(cast.Type));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitConditionalExpression(ConditionalExpressionSyntax conditional)
        => Condition(
            Visit(conditional.Condition),
            Visit(conditional.WhenTrue),
            Visit(conditional.WhenFalse));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitElementAccessExpression(ElementAccessExpressionSyntax elementAccessExpression)
    {
        var visitedExpression = Visit(elementAccessExpression.Expression);

        var expressionType = _semanticModel.GetTypeInfo(elementAccessExpression.Expression).ConvertedType;
        if (expressionType is null)
        {
            throw new InvalidOperationException(
                $"No type for expression {elementAccessExpression.Expression} in {nameof(ElementAccessExpressionSyntax)}");
        }

        if (expressionType is IArrayTypeSymbol)
        {
            Check.DebugAssert(elementAccessExpression.ArgumentList.Arguments.Count == 1,
                $"ElementAccessExpressionSyntax over array with {elementAccessExpression.ArgumentList.Arguments.Count} arguments");

            var visitedArgument = Visit(elementAccessExpression.ArgumentList.Arguments[0].Expression);

            return ArrayIndex(visitedExpression, visitedArgument);
        }

        throw new NotImplementedException($"{nameof(ElementAccessExpressionSyntax)} over non-array");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitIdentifierName(IdentifierNameSyntax identifierName)
    {
        if (_parameterStack.Peek().TryGetValue(identifierName.Identifier.Text, out var parameter))
        {
            return parameter;
        }

        var symbol = _semanticModel.GetSymbolInfo(identifierName).Symbol;

        ILocalSymbol localSymbol;
        switch (symbol)
        {
            case INamedTypeSymbol typeSymbol:
                return Constant(ResolveType(typeSymbol));
            case ILocalSymbol ls:
                localSymbol = ls;
                break;
            case null:
                throw new InvalidOperationException($"Identifier without symbol: {identifierName}");
            default:
                throw new NotImplementedException($"IdentifierName of type {symbol.GetType().Name}: {identifierName}");
        }

        // TODO: Separate out EF Core-specific logic (EF Core would extend this visitor)
        if (localSymbol.Type.Name.Contains("DbSet"))
        {
            var queryRootType = ResolveType(localSymbol.Type)!;
            // TODO: Decide what to actually return for query root
            return Constant(null, queryRootType);
        }

        // We have an identifier which isn't in our parameters stack - it's a closure parameter.
        // Check if this closure parameter has already been referenced elsewhere in the query (two references to the
        // same local variable)
        // TODO: Test closure over class member (not local variable)
        if (!_capturedVariables.TryGetValue(localSymbol, out var memberExpression))
        {
            throw new InvalidOperationException(
                $"Encountered unknown identifier name {identifierName}, which doesn't correspond to a lambda parameter or captured variable");
        }

        // We haven't seen this captured variable yet
        if (memberExpression is null)
        {
            memberExpression = _capturedVariables[localSymbol] = _capturedVariables[localSymbol] =
                Field(
                    Constant(new FakeClosureFrameClass()),
                    new FakeFieldInfo(
                        typeof(FakeClosureFrameClass),
                        ResolveType(localSymbol.Type),
                        localSymbol.Name));
        }

        return memberExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax implicitArrayCreation)
    {
        if (_semanticModel.GetTypeInfo(implicitArrayCreation).Type is not IArrayTypeSymbol arrayTypeSymbol)
        {
            throw new InvalidOperationException($"ArrayCreation: non-array type symbol: {implicitArrayCreation}");
        }

        if (arrayTypeSymbol.Rank > 1)
        {
            throw new NotImplementedException($"ArrayCreation: multi-dimensional array: {implicitArrayCreation}");
        }

        var elementType = ResolveType(arrayTypeSymbol.ElementType);
        Check.DebugAssert(elementType is not null, "elementType is not null");

        var initializers = implicitArrayCreation.Initializer.Expressions.Select(e => Visit(e));

        return NewArrayInit(elementType, initializers);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitInvocationExpression(InvocationExpressionSyntax invocation)
    {
        if (_semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
        {
            throw new InvalidOperationException("Could not find symbol for method invocation: " + invocation);
        }

        var declaringType = ResolveType(methodSymbol.ContainingType);

        Expression? instance = null;
        if (!methodSymbol.IsStatic || methodSymbol.IsExtensionMethod)
        {
            // In normal method calls (the ones we support), the invocation node is composed on top of a member access
            if (invocation.Expression is not MemberAccessExpressionSyntax { Expression: var receiver })
            {
                throw new NotSupportedException($"Invocation over non-member access: {invocation}");
            }

            instance = Visit(receiver);
        }

        MethodInfo? methodInfo;

        if (methodSymbol.IsGenericMethod)
        {
            var originalDefinition = methodSymbol.OriginalDefinition;
            if (originalDefinition.ReducedFrom is not null)
            {
                originalDefinition = originalDefinition.ReducedFrom;
            }

            // To accurately find the right open generic method definition based on the Roslyn symbol, we need to create a mapping between
            // generic type parameter names (based on the Roslyn side) and .NET reflection Types representing those type parameters.
            // This includes both type parameters immediately on the generic method, as well as type parameters from the method's
            // containing type (and recursively, its containing types)
            var typeTypeParameterMap = new Dictionary<string, Type>(Foo(methodSymbol.ContainingType));

            IEnumerable<KeyValuePair<string, Type>> Foo(INamedTypeSymbol typeSymbol)
            {
                // TODO: We match Roslyn type parameters by name, not sure that's right; also for the method's generic type parameters

                if (typeSymbol.ContainingType is INamedTypeSymbol containingTypeSymbol)
                {
                    foreach (var kvp in Foo(containingTypeSymbol))
                    {
                        yield return kvp;
                    }
                }

                var type = ResolveType(typeSymbol);
                var genericArguments = type.GetGenericArguments();

                Check.DebugAssert(
                    genericArguments.Length == typeSymbol.TypeParameters.Length,
                    "genericArguments.Length == typeSymbol.TypeParameters.Length");

                foreach (var (typeParamSymbol, typeParamType) in typeSymbol.TypeParameters.Zip(genericArguments))
                {
                    // Check.DebugAssert(typeParamSymbol.Name == typeParamType.Name, "typeParamSymbol.Name == type.Name");

                    yield return new KeyValuePair<string, Type>(typeParamSymbol.Name, typeParamType);
                }
            }

            var definitionMethodInfos = declaringType.GetMethods()
                .Where(m =>
                {
                    if (m.Name == methodSymbol.Name
                        && m.IsGenericMethodDefinition
                        && m.GetGenericArguments() is var candidateGenericArguments
                        && candidateGenericArguments.Length == originalDefinition.TypeParameters.Length
                        && m.GetParameters() is var candidateParams
                        && candidateParams.Length == originalDefinition.Parameters.Length)
                    {
                        var methodTypeParameterMap = new Dictionary<string, Type>(typeTypeParameterMap);

                        // Prepare a dictionary that will be used to resolve generic type parameters (ITypeParameterSymbol) to the
                        // corresponding reflection Type. This is needed to correctly (and recursively) resolve the type of parameters
                        // below.
                        foreach (var (symbol, type) in methodSymbol.TypeParameters.Zip(candidateGenericArguments))
                        {
                            if (symbol.Name != type.Name)
                            {
                                return false;
                            }

                            methodTypeParameterMap[symbol.Name] = type;
                        }

                        for (var i = 0; i < candidateParams.Length; i++)
                        {
                            var translatedParamType = ResolveType(originalDefinition.Parameters[i].Type, methodTypeParameterMap);
                            if (translatedParamType != candidateParams[i].ParameterType)
                            {
                                return false;
                            }
                        }

                        return true;
                    }

                    return false;
                }).ToArray();

            if (definitionMethodInfos.Length != 1)
            {
                throw new InvalidOperationException($"Invocation: Found {definitionMethodInfos.Length} matches for generic method: {invocation}");
            }

            var definitionMethodInfo = definitionMethodInfos[0];
            var typeParams = methodSymbol.TypeArguments.Select(a => ResolveType(a)).ToArray();
            methodInfo = definitionMethodInfo.MakeGenericMethod(typeParams);
        }
        else
        {
            // Non-generic method

            // TODO: private/internal binding flags
            var reducedMethodSymbol = methodSymbol.ReducedFrom ?? methodSymbol;

            methodInfo = declaringType.GetMethod(
                methodSymbol.Name,
                reducedMethodSymbol.Parameters.Select(p => ResolveType(p.Type)).ToArray());

            if (methodInfo is null)
            {
                throw new InvalidOperationException($"Invocation: couldn't find method '{methodSymbol.Name}' on type '{declaringType.Name}': {invocation}");
            }
        }

        // We have the reflection MethodInfo for the method, prepare the arguments.

        // We can have less arguments than parameters when the method has optional parameters.
        // Fill in the missing ones with the default value
        var parameters = methodInfo.GetParameters();
        var arguments = new Expression?[parameters.Length];
        var destArgBase = 0;
        var destArgIndex = 0;

        // At the syntactic level, an extension method invocation looks like a normal instance's.
        // Prepend the instance to the argument list.
        // TODO: Test invoking extension without extension syntax (as static)
        if (methodSymbol is { IsExtensionMethod: true /*, ReceiverType: { } */ })
        {
            arguments[0] = instance;
            instance = null;
            destArgBase = destArgIndex = 1;
        }

        var sourceArguments = invocation.ArgumentList.Arguments;
        for (var sourceArgIndex = 0; sourceArgIndex < sourceArguments.Count; sourceArgIndex++, destArgIndex++)
        {
            var argument = invocation.ArgumentList.Arguments[sourceArgIndex];

            // Positional argument
            if (argument.NameColon is null)
            {
                arguments[destArgIndex] = Visit(sourceArguments[sourceArgIndex]);
                continue;
            }

            // Named argument
            throw new NotImplementedException("Named argument");
        }

        // We can have less arguments than parameters when the method has optional parameters.
        // Fill in the missing ones with the default value
        if (sourceArguments.Count < parameters.Length)
        {
            for (var paramIndex = destArgBase; paramIndex < parameters.Length; paramIndex++)
            {
                if (arguments[paramIndex] is null)
                {
                    var parameter = parameters[paramIndex];
                    Check.DebugAssert(parameter.IsOptional, "Missing non-optional argument");
                    arguments[paramIndex] = Constant(
                        parameter.DefaultValue is null && parameter.ParameterType.IsValueType
                            ? Activator.CreateInstance(parameter.ParameterType)
                            : parameter.DefaultValue,
                        parameter.ParameterType);
                }
            }
        }

        Check.DebugAssert(arguments.All(a => a is not null), "arguments.All(a => a is not null)");

        // TODO: Generic type arguments
        return Call(instance, methodInfo, arguments!);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitLiteralExpression(LiteralExpressionSyntax literal)
    {
        // We call GetTypeInfo to get the type for null literals
        var typeInfo = _semanticModel.GetTypeInfo(literal);

        return Constant(
            literal.Token.Value,
            ResolveType(
                typeInfo.ConvertedType ?? throw new InvalidOperationException("No converted type for null literal")));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccess)
    {
        // Identify DbSet property access on the user's context type
        // TODO: support DbSet via DbContext.Set<T>() (and its STET counterpart)
        // TODO: identify DbSet access on non-local context (e.g. new BlogContext().Blogs...
        if (_semanticModel.GetSymbolInfo(memberAccess).Symbol is IPropertySymbol propertySymbol
            && SymbolEqualityComparer.Default.Equals(propertySymbol.Type.OriginalDefinition, _dbSetSymbol)
            && _semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is ILocalSymbol contextTypeSymbol
            && SymbolEqualityComparer.Default.Equals(contextTypeSymbol.Type.OriginalDefinition, _userDbContextSymbol))
        {
            // TODO: Cache these properties?

            // We have a DbSet property access.
            if (_userDbContextType.GetProperty(propertySymbol.Name) is not { CanRead: true } propertyInfo)
            {
                throw new InvalidOperationException(
                    $"Couldn't find referenced property {propertySymbol.Name} on context type {_userDbContextType.Name}");
            }

            var dbSet = propertyInfo.GetMethod!.Invoke(_userDbContext, null)!;
            var entityType = (IEntityType)dbSet.GetType().GetProperty(nameof(DbSet<object>.EntityType))!.GetMethod!
                .Invoke(dbSet, null)!;

            return new EntityQueryRootExpression(entityType);
        }

        var expression = Visit(memberAccess.Expression);

        if (_semanticModel.GetSymbolInfo(memberAccess).Symbol is not ISymbol memberSymbol)
        {
            throw new InvalidOperationException($"MemberAccess: Couldn't find symbol for member: {memberAccess}");
        }

        var containingType = ResolveType(memberSymbol.ContainingType);
        var memberInfo = memberSymbol switch
        {
            IPropertySymbol p => (MemberInfo?)containingType.GetProperty(p.Name),
            IFieldSymbol f => containingType.GetField(f.Name),
            INamedTypeSymbol t => containingType.GetNestedType(t.Name),

            null => throw new InvalidOperationException($"MemberAccess: Couldn't find symbol for member: {memberAccess}"),
            _ => throw new NotSupportedException($"MemberAccess: unsupported member symbol '{memberSymbol.GetType().Name}': {memberAccess}")
        };

        switch (memberInfo)
        {
            case Type nestedType:
                return Constant(nestedType);

            case null:
                throw new InvalidOperationException($"MemberAccess: couldn't find member '{memberSymbol.Name}': {memberAccess}");
        }

        // Enum field constant
        if (containingType.IsEnum)
        {
            return Constant(Enum.Parse(containingType, memberInfo.Name), containingType);
        }

        // array.Length
        if (expression.Type.IsArray && memberInfo.Name == "Length")
        {
            if (expression.Type.GetArrayRank() != 1)
            {
                throw new NotImplementedException("MemberAccess on multi-dimensional array");
            }

            return ArrayLength(expression);
        }

        return MakeMemberAccess(
            expression is ConstantExpression { Value: Type } ? null : expression,
            memberInfo);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitObjectCreationExpression(ObjectCreationExpressionSyntax objectCreation)
    {
        if (_semanticModel.GetSymbolInfo(objectCreation).Symbol is not IMethodSymbol constructorSymbol)
        {
            throw new InvalidOperationException($"ObjectCreation: couldn't find IMethodSymbol for constructor: {objectCreation}");
        }

        Check.DebugAssert(constructorSymbol.MethodKind == MethodKind.Constructor, "constructorSymbol.MethodKind == MethodKind.Constructor");

        var type = ResolveType(constructorSymbol.ContainingType);

        // Find the reflection constructor that matches the constructor symbol's signature
        var parameterTypes = constructorSymbol.Parameters.Select(ps => ResolveType(ps.Type)).ToArray();
        var constructor = type.GetConstructor(parameterTypes);

        var newExpression = constructor is not null
            ? New(
                constructor,
                objectCreation.ArgumentList?.Arguments.Select(a => Visit(a)) ?? Array.Empty<Expression>())
            : parameterTypes.Length == 0 // For structs, there's no actual parameterless constructor
                ? New(type)
                : throw new InvalidOperationException($"ObjectCreation: Missing constructor: {objectCreation}");

        if (objectCreation.Initializer is null)
        {
            return newExpression;
        }

        var bindings =
            objectCreation.Initializer.Expressions.Select(
                e =>
                {
                    if (e is not AssignmentExpressionSyntax { Left: var lValue, Right: var value })
                    {
                        throw new NotSupportedException(
                            $"ObjectCreation: non-assignment initializer expression of type '{e.GetType().Name}': {objectCreation}");
                    }

                    var lValueSymbol = _semanticModel.GetSymbolInfo(lValue).Symbol;
                    var memberInfo = lValueSymbol switch
                    {
                        IPropertySymbol p => (MemberInfo?)type.GetProperty(p.Name),
                        IFieldSymbol f => type.GetField(f.Name),

                        _ => throw new InvalidOperationException(
                            $"ObjectCreation: unsupported initializer for member of type '{lValueSymbol?.GetType().Name}': {e}")
                    };

                    if (memberInfo is null)
                    {
                        throw new InvalidOperationException($"ObjectCreation: couldn't find initialized member '{lValueSymbol.Name}': {e}");
                    }

                    return Bind(memberInfo, Visit(value));
                });

        return MemberInit(newExpression, bindings);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitParenthesizedExpression(ParenthesizedExpressionSyntax parenthesized)
        => Visit(parenthesized.Expression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax lambda)
    {
        if (lambda.ExpressionBody is null)
        {
            throw new NotSupportedException("Lambda with null expression body");
        }

        if (lambda.Modifiers.Any())
        {
            throw new NotImplementedException("Lambda with modifiers: " + lambda.Modifiers);
        }

        if (!lambda.AsyncKeyword.IsKind(SyntaxKind.None))
        {
            throw new NotImplementedException("Async lambda");
        }

        var translatedParameters = new List<ParameterExpression>();
        foreach (var parameter in lambda.ParameterList.Parameters)
        {
            if (_semanticModel.GetDeclaredSymbol(parameter) is not { } parameterSymbol ||
                ResolveType(parameterSymbol.Type) is not { } parameterType)
            {
                throw new InvalidOperationException("Could not found symbol for parameter lambda: " + parameter);
            }

            translatedParameters.Add(Parameter(parameterType, parameter.Identifier.Text));
        }

        _parameterStack.Push(_parameterStack.Peek()
            .AddRange(translatedParameters.Select(p => new KeyValuePair<string, ParameterExpression>(p.Name ?? throw new NotImplementedException(), p))));

        try
        {
            var body = Visit(lambda.ExpressionBody);
            return Lambda(body, translatedParameters);
        }
        finally
        {
            _parameterStack.Pop();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitPredefinedType(PredefinedTypeSyntax predefinedType)
        => Constant(ResolveType(predefinedType), typeof(Type));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax unary)
    {
        var operand = Visit(unary.Operand);

        // https://learn.microsoft.com/dotnet/api/Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax

        return unary.Kind() switch
        {
            SyntaxKind.UnaryPlusExpression => UnaryPlus(operand),
            SyntaxKind.UnaryMinusExpression => Negate(operand),
            SyntaxKind.BitwiseNotExpression => Not(operand),
            SyntaxKind.LogicalNotExpression => Not(operand),

            SyntaxKind.AddressOfExpression => throw NotSupported(),
            SyntaxKind.IndexExpression => throw NotSupported(),
            SyntaxKind.PointerIndirectionExpression => throw NotSupported(),
            SyntaxKind.PreDecrementExpression => throw NotSupported(),
            SyntaxKind.PreIncrementExpression => throw NotSupported(),

            _ => throw new ArgumentOutOfRangeException(
                $"Unexpected syntax kind '{unary.Kind()}' when visiting a {nameof(PrefixUnaryExpressionSyntax)}")
        };

        NotSupportedException NotSupported()
            => throw new NotSupportedException(
                $"Unary expression of type {unary.Kind()} is not supported in expression trees");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax unary)
    {
        var operand = Visit(unary.Operand);

        // https://learn.microsoft.com/dotnet/api/Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax

        return unary.Kind() switch
        {
            SyntaxKind.SuppressNullableWarningExpression => operand,

            SyntaxKind.PostIncrementExpression => throw NotSupported(),
            SyntaxKind.PostDecrementExpression => throw NotSupported(),

            _ => throw new InvalidOperationException($"Unexpected syntax kind '{unary.Kind()}' when visiting a {nameof(PostfixUnaryExpressionSyntax)}")
        };

        NotSupportedException NotSupported()
            => throw new NotSupportedException(
                $"Unary expression of type {unary.Kind()} is not supported in expression trees");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax lambda)
    {
        if (lambda.ExpressionBody is null)
        {
            throw new NotSupportedException("SimpleLambda with null expression body");
        }

        if (lambda.Modifiers.Any())
        {
            throw new NotImplementedException("SimpleLambda with modifiers: " + lambda.Modifiers);
        }

        if (!lambda.AsyncKeyword.IsKind(SyntaxKind.None))
        {
            throw new NotImplementedException("SimpleLambda with async keyword");
        }

        var paramName = lambda.Parameter.Identifier.Text;
        if (_semanticModel.GetDeclaredSymbol(lambda.Parameter) is not { } parameterSymbol ||
            ResolveType(parameterSymbol.Type) is not { } parameterType)
        {
            throw new InvalidOperationException("Could not found symbol for parameter lambda: " + lambda.Parameter);
        }

        var parameter = Parameter(parameterType, paramName);
        _parameterStack.Push(_parameterStack.Peek().SetItem(paramName, parameter));

        try
        {
            var body = Visit(lambda.ExpressionBody);
            return Lambda(body, parameter);
        }
        finally
        {
            _parameterStack.Pop();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitTypeOfExpression(TypeOfExpressionSyntax typeOf)
    {
        if (_semanticModel.GetSymbolInfo(typeOf.Type).Symbol is not ITypeSymbol typeSymbol)
        {
            throw new InvalidOperationException(
                "Could not find symbol for typeof() expression: " + typeOf);
        }

        var type = ResolveType(typeSymbol);
        return Constant(type, typeof(Type));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression DefaultVisit(SyntaxNode node)
        => throw new NotSupportedException($"Unsupported syntax node of type '{node.GetType()}': {node}");

    private Type ResolveType(SyntaxNode node)
        => _semanticModel.GetTypeInfo(node).Type is { } typeSymbol
            ? ResolveType(typeSymbol)
            : throw new InvalidOperationException("Could not find type symbol for: " + node);

    private Type ResolveType(ITypeSymbol typeSymbol, Dictionary<string, Type>? genericParameterMap = null)
    {
        switch (typeSymbol)
        {
            case INamedTypeSymbol { IsAnonymousType: true } anonymousTypeSymbol:
                _anonymousTypeDefinitions ??= LoadAnonymousTypes(anonymousTypeSymbol.ContainingAssembly);
                var properties = anonymousTypeSymbol.GetMembers().OfType<IPropertySymbol>().ToArray();
                var found = _anonymousTypeDefinitions.TryGetValue(
                    properties.Select(p => p.Name).OrderBy(p => p).ToArray(),
                    out var anonymousTypeGenericDefinition);
                Debug.Assert(found, "Anonymous type not found");

                var constructorParameters = anonymousTypeGenericDefinition!.GetConstructors()[0].GetParameters();
                var genericTypeArguments = new Type[constructorParameters.Length];

                for (var i = 0; i < constructorParameters.Length; i++)
                {
                    genericTypeArguments[i] =
                        ResolveType(properties.FirstOrDefault(p => p.Name == constructorParameters[i].Name)!.Type);
                }

                // TODO: Cache closed anonymous types

                return anonymousTypeGenericDefinition.MakeGenericType(genericTypeArguments);

            case INamedTypeSymbol { IsDefinition: true } genericTypeSymbol:
                return GetClrType(genericTypeSymbol);

            case INamedTypeSymbol { IsGenericType: true } genericTypeSymbol:
            {
                var definition = GetClrType(genericTypeSymbol.OriginalDefinition);
                var typeArguments = genericTypeSymbol.TypeArguments.Select(a => ResolveType(a, genericParameterMap)).ToArray();
                return definition.MakeGenericType(typeArguments);
            }

            case ITypeParameterSymbol typeParameterSymbol:
                return genericParameterMap?.TryGetValue(typeParameterSymbol.Name, out var type) == true
                    ? type
                    : throw new InvalidOperationException($"Unknown generic type parameter symbol {typeParameterSymbol}");

            case INamedTypeSymbol namedTypeSymbol:
                return GetClrType(namedTypeSymbol);

            default:
                return GetClrTypeFromAssembly(
                    typeSymbol.ContainingAssembly,
                    typeSymbol.ToDisplayString(QualifiedTypeNameSymbolDisplayFormat));
        }

        Type GetClrType(INamedTypeSymbol symbol)
        {
            var name = symbol.ContainingType is null
                ? typeSymbol.ToDisplayString(QualifiedTypeNameSymbolDisplayFormat)
                : typeSymbol.Name;

            if (symbol.IsGenericType)
            {
                name += '`' + symbol.Arity.ToString();
            }

            if (symbol.ContainingType is not null)
            {
                var containingType = ResolveType(symbol.ContainingType);

                return containingType.GetNestedType(name)
                    ?? throw new InvalidOperationException(
                        $"Couldn't find nested type '{name}' on containing type '{containingType.Name}'");
            }

            return GetClrTypeFromAssembly(typeSymbol.ContainingAssembly, name);
        }

        static Type GetClrTypeFromAssembly(IAssemblySymbol? assemblySymbol, string name)
            => (assemblySymbol is null
                    ? Type.GetType(name)!
                    : Type.GetType($"{name}, {assemblySymbol.Name}"))
                ?? throw new InvalidOperationException(
                    $"Couldn't resolve CLR type '{name}' in assembly '{assemblySymbol?.Name}'");

        Dictionary<string[], Type> LoadAnonymousTypes(IAssemblySymbol assemblySymbol)
        {
            var assembly = Assembly.Load(assemblySymbol.Name);

            return assembly.GetTypes()
                .Where(t => t.IsAnonymousType())
                .ToDictionary(
                    t => t.GetProperties().Select(x => x.Name).OrderBy(p => p).ToArray(),
                    t => t,
                    new ArrayStructuralComparer<string>());
        }
    }

    private class ArrayStructuralComparer<T> : IEqualityComparer<T[]>
    {
        public bool Equals(T[]? x, T[]? y)
            => x is null ? y is null : y is not null && x.SequenceEqual(y);

        public int GetHashCode(T[] obj)
        {
            var hashcode = new HashCode();

            foreach (var value in obj)
            {
                hashcode.Add(value);
            }

            return hashcode.ToHashCode();
        }
    }

    private Dictionary<string[], Type>? _anonymousTypeDefinitions;

    [CompilerGenerated]
    private class FakeClosureFrameClass
    {
    }

    private class FakeFieldInfo : FieldInfo
    {
        public FakeFieldInfo(Type declaringType, Type fieldType, string name)
        {
            DeclaringType = declaringType;
            FieldType = fieldType;
            Name = name;
        }

        public override object[] GetCustomAttributes(bool inherit)
            => Array.Empty<object>();

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            => Array.Empty<object>();

        public override bool IsDefined(Type attributeType, bool inherit)
            => false;

        public override Type DeclaringType { get; }

        public override string Name { get; }

        public override Type? ReflectedType => null;

        // We implement GetValue since ParameterExtractingExpressionVisitor calls it to get the parameter value. In
        // AOT generation time, we obviously have no parameter values, nor do we need them for the first part of the
        // query pipeline.
        public override object? GetValue(object? obj)
            => FieldType.IsValueType ? Activator.CreateInstance(FieldType) : null;

        public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder,
            CultureInfo? culture)
            => throw new NotSupportedException();

        public override FieldAttributes Attributes
            => FieldAttributes.Public;

        public override RuntimeFieldHandle FieldHandle
            => throw new NotSupportedException();

        public override Type FieldType { get; }
    }

    private class FakeConstructorInfo : ConstructorInfo
    {
        private readonly ParameterInfo[] _parameters;

        public FakeConstructorInfo(Type type, ParameterInfo[] parameters)
        {
            DeclaringType = type;
            _parameters = parameters;
        }

        public override object[] GetCustomAttributes(bool inherit)
            => Array.Empty<object>();

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            => Array.Empty<object>();

        public override bool IsDefined(Type attributeType, bool inherit)
            => false;

        public override Type DeclaringType { get; }

        public override string Name
            => ".ctor";

        public override Type ReflectedType
            => DeclaringType;

        public override MethodImplAttributes GetMethodImplementationFlags()
            => MethodImplAttributes.Managed;

        public override ParameterInfo[] GetParameters()
            => _parameters;

        public override MethodAttributes Attributes
            => MethodAttributes.Public;

        public override RuntimeMethodHandle MethodHandle
            => throw new NotSupportedException();

        public override object Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters,
            CultureInfo? culture)
            => throw new NotSupportedException();

        public override object Invoke(BindingFlags invokeAttr, Binder? binder, object?[]? parameters,
            CultureInfo? culture)
            => throw new NotSupportedException();
    }

    private class FakeParameterInfo : ParameterInfo
    {
        public FakeParameterInfo(string name, Type parameterType, int position)
            => (Name, ParameterType, Position) = (name, parameterType, position);

        public override ParameterAttributes Attributes
            => ParameterAttributes.In;

        public override string? Name { get; }
        public override Type ParameterType { get; }
        public override int Position { get; }

        public override MemberInfo Member
            => throw new NotSupportedException();
    }
}
