// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Internal;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Translates a Roslyn syntax tree into a LINQ expression tree.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class CSharpToLinqTranslator : CSharpSyntaxVisitor<Expression>
{
    private static readonly SymbolDisplayFormat QualifiedTypeNameSymbolDisplayFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    private Compilation? _compilation;

#pragma warning disable CS8618 // Uninitialized non-nullable fields. We check _compilation to make sure LoadCompilation was invoked.
    private DbContext _userDbContext;
    private Assembly? _additionalAssembly;
    private INamedTypeSymbol _userDbContextSymbol;
    private INamedTypeSymbol _formattableStringSymbol;
#pragma warning restore CS8618

    private SemanticModel _semanticModel = null!;

    private static MethodInfo? _stringConcatMethod;
    private static MethodInfo? _stringFormatMethod;
    private static MethodInfo? _formattableStringFactoryCreateMethod;

    /// <summary>
    ///     Loads the given <see cref="Compilation" /> and prepares to translate queries using the given <see cref="DbContext" />.
    /// </summary>
    /// <param name="compilation">A <see cref="Compilation" /> containing the syntax nodes to be translated.</param>
    /// <param name="userDbContext">An instance of the user's <see cref="DbContext" />.</param>
    /// <param name="additionalAssembly">An optional additional assemblies to resolve CLR types from.</param>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual void Load(Compilation compilation, DbContext userDbContext, Assembly? additionalAssembly = null)
    {
        _compilation = compilation;
        _userDbContext = userDbContext;
        _additionalAssembly = additionalAssembly;
        _userDbContextSymbol = GetTypeSymbolOrThrow(userDbContext.GetType().FullName!);
        _formattableStringSymbol = GetTypeSymbolOrThrow("System.FormattableString");

        INamedTypeSymbol GetTypeSymbolOrThrow(string fullyQualifiedMetadataName)
            => _compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)
                ?? throw new InvalidOperationException("Could not find type symbol for: " + fullyQualifiedMetadataName);
    }

    private readonly Stack<ImmutableDictionary<string, ParameterExpression>> _parameterStack
        = new(new[] { ImmutableDictionary<string, ParameterExpression>.Empty });

    private readonly Dictionary<ISymbol, MemberExpression?> _dataFlowsIn = new(SymbolEqualityComparer.Default);

    /// <summary>
    ///     Translates a Roslyn syntax tree into a LINQ expression tree.
    /// </summary>
    /// <param name="node">The Roslyn syntax node to be translated.</param>
    /// <param name="semanticModel">
    ///     The <see cref="SemanticModel" /> for the Roslyn <see cref="SyntaxTree" /> of which <paramref name="node" /> is a part.
    /// </param>
    /// <returns>A LINQ expression tree translated from the provided <paramref name="node" />.</returns>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual Expression Translate(SyntaxNode node, SemanticModel semanticModel)
    {
        if (_compilation is null)
        {
            throw new InvalidOperationException(DesignStrings.CompilationMustBeLoaded);
        }

        Check.DebugAssert(
            ReferenceEquals(semanticModel.SyntaxTree, node.SyntaxTree),
            "Provided semantic model doesn't match the provided syntax node");

        _semanticModel = semanticModel;

        // Perform data flow analysis to detect all variables flowing into the query (e.g. captured variables)
        _dataFlowsIn.Clear();
        foreach (var flowsIn in _semanticModel.AnalyzeDataFlow(node).DataFlowsIn)
        {
            _dataFlowsIn[flowsIn] = null;
        }

        var result = Visit(node);

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
    ///     This method gets called when the expression context provides an expected CLR type. For example, in <c>Foo(x)</c>, x gets visited
    ///     with an expected type based on <c>Foo</c>'s parameter; this may determine how x gets translated, require a LINQ Convert node, or
    ///     similar. In contrast, in <c>var y = x</c>, there is no context providing an expected type, and the type of <c>x</c> simply
    ///     bubbles out.
    /// </summary>
    [return: NotNullIfNotNull("node")]
    private Expression? Visit(SyntaxNode? node, Type? expectedType)
    {
        if (expectedType is null)
        {
            return Visit(node);
        }

        var result = node switch
        {
            ArgumentSyntax s => VisitArgument(s, expectedType),

            // For lambdas, we generate a different node based on the expected type (e.g. an Action<T> rather than a Func<T, T2>, even if
            // the lambda body does return a T2).
            SimpleLambdaExpressionSyntax s => VisitLambdaExpression(s, expectedType),
            ParenthesizedLambdaExpressionSyntax s => VisitLambdaExpression(s, expectedType),

            _ => Visit(node),
        };

        // TODO: Insert necessary Convert nodes etc. when the expected and actual types differ

        return result;
    }

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
            throw new InvalidOperationException(DesignStrings.NoAnonymousSymbol + " " + anonymousObjectCreation);
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
            var parameterType = ResolveType(parameter.Type)
                ?? throw new InvalidOperationException(
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
        => VisitArgument(argument, expectedType: null);

    private Expression VisitArgument(ArgumentSyntax argument, Type? expectedType)
    {
        if (!argument.RefKindKeyword.IsKind(SyntaxKind.None))
        {
            throw new InvalidOperationException($"Argument with ref/out: {argument}");
        }

        return Visit(argument.Expression, expectedType);
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

        if (Nullable.GetUnderlyingType(left.Type) == right.Type)
        {
            right = Convert(right, left.Type);
        }
        else if (Nullable.GetUnderlyingType(right.Type) == left.Type)
        {
            left = Convert(left, right.Type);
        }

        // https://learn.microsoft.com/dotnet/api/Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax
        return binary.Kind() switch
        {
            // String concatenation
            SyntaxKind.AddExpression
                when left.Type == typeof(string) && right.Type == typeof(string)
                => Add(
                    left, right,
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

            // For bitwise operations over enums, we cast the enum to its underlying type before the bitwise operation, and then back to the
            // enum afterwards (this is corresponds to the LINQ expression tree that the compiler generates)
            SyntaxKind.BitwiseOrExpression when left.Type.IsEnum || right.Type.IsEnum
                => Convert(
                    Or(Convert(left, left.Type.GetEnumUnderlyingType()), Convert(right, right.Type.GetEnumUnderlyingType())), left.Type),
            SyntaxKind.BitwiseAndExpression when left.Type.IsEnum || right.Type.IsEnum
                => Convert(
                    And(Convert(left, left.Type.GetEnumUnderlyingType()), Convert(right, right.Type.GetEnumUnderlyingType())), left.Type),
            SyntaxKind.ExclusiveOrExpression when left.Type.IsEnum || right.Type.IsEnum
                => Convert(
                    ExclusiveOr(Convert(left, left.Type.GetEnumUnderlyingType()), Convert(right, right.Type.GetEnumUnderlyingType())),
                    left.Type),

            SyntaxKind.BitwiseOrExpression => Or(left, right),
            SyntaxKind.BitwiseAndExpression => And(left, right),
            SyntaxKind.ExclusiveOrExpression => ExclusiveOr(left, right),

            SyntaxKind.EqualsExpression => Equal(left, right),
            SyntaxKind.NotEqualsExpression => NotEqual(left, right),
            SyntaxKind.LessThanExpression => LessThan(left, right),
            SyntaxKind.LessThanOrEqualExpression => LessThanOrEqual(left, right),
            SyntaxKind.GreaterThanExpression => GreaterThan(left, right),
            SyntaxKind.GreaterThanOrEqualExpression => GreaterThanOrEqual(left, right),
            SyntaxKind.IsExpression => TypeIs(
                left, right is ConstantExpression { Value : Type type }
                    ? type
                    : throw new InvalidOperationException(
                        $"Encountered {SyntaxKind.IsExpression} with non-constant type right argument: {right}")),
            SyntaxKind.AsExpression => TypeAs(
                left, right is ConstantExpression { Value : Type type }
                    ? type
                    : throw new InvalidOperationException(
                        $"Encountered {SyntaxKind.AsExpression} with non-constant type right argument: {right}")),
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
        var arguments = elementAccessExpression.ArgumentList.Arguments;
        var visitedExpression = Visit(elementAccessExpression.Expression);

        switch (_semanticModel.GetTypeInfo(elementAccessExpression.Expression).ConvertedType)
        {
            case IArrayTypeSymbol:
                Check.DebugAssert(
                    elementAccessExpression.ArgumentList.Arguments.Count == 1,
                    $"ElementAccessExpressionSyntax over array with {arguments.Count} arguments");
                return ArrayIndex(visitedExpression, Visit(arguments[0].Expression));

            case INamedTypeSymbol:
                var property = visitedExpression.Type
                    .GetProperties()
                    .Select(p => new { Property = p, IndexParameters = p.GetIndexParameters() })
                    .Where(
                        t => t.IndexParameters.Length == arguments.Count
                            && t.IndexParameters
                                .Select(p => p.ParameterType)
                                .SequenceEqual(arguments.Select(a => ResolveType(a.Expression))))
                    .Select(t => t.Property)
                    .FirstOrDefault();

                Check.DebugAssert(property?.GetMethod is not null, "No matching property found for ElementAccessExpressionSyntax");

                return Call(visitedExpression, property.GetMethod, arguments.Select(a => Visit(a.Expression)));

            case null:
                throw new InvalidOperationException(
                    $"No type for expression {elementAccessExpression.Expression} in {nameof(ElementAccessExpressionSyntax)}");

            default:
                throw new NotImplementedException($"{nameof(ElementAccessExpressionSyntax)} over non-array");
        }
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

        ITypeSymbol typeSymbol;
        switch (symbol)
        {
            case INamedTypeSymbol s:
                return Constant(ResolveType(s));
            case ILocalSymbol s:
                typeSymbol = s.Type;
                break;
            case IFieldSymbol s:
                typeSymbol = s.Type;
                break;
            case IPropertySymbol s:
                typeSymbol = s.Type;
                break;
            case null:
                throw new InvalidOperationException($"Identifier without symbol: {identifierName}");
            default:
                throw new UnreachableException($"IdentifierName of type {symbol.GetType().Name}: {identifierName}");
        }

        // TODO: Separate out EF Core-specific logic (EF Core would extend this visitor)
        if (typeSymbol.Name.Contains("DbSet"))
        {
            throw new NotImplementedException("DbSet local symbol");
        }

        // We have an identifier which isn't in our parameters stack.

        // First, if the identifier type is the user's DbContext type (e.g. DbContext local variable, or field/property),
        // return a constant over that.
        if (typeSymbol.Equals(_userDbContextSymbol, SymbolEqualityComparer.Default))
        {
            return Constant(_userDbContext);
        }

        // The Translate entry point into the translator uses Roslyn's data flow analysis to locate all local variables flowing in
        // (e.g. captured variables), and populates the _dataFlowsIn dictionary with them (with null values).
        if (symbol is ILocalSymbol localSymbol && _dataFlowsIn.TryGetValue(localSymbol, out var memberExpression))
        {
            // The first time we see a flowing-in variable, we create MemberExpression for it and cache it in _dataFlowsIn.
            return memberExpression
                ?? (_dataFlowsIn[localSymbol] =
                    Field(
                        Constant(new FakeClosureFrameClass()),
                        new FakeFieldInfo(
                            typeof(FakeClosureFrameClass),
                            ResolveType(localSymbol.Type),
                            localSymbol.Name,
                            localSymbol.NullableAnnotation is NullableAnnotation.NotAnnotated)));
        }

        throw new InvalidOperationException(
            $"Encountered unknown identifier name '{identifierName}', which doesn't correspond to a lambda parameter or captured variable");
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
    public override Expression VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax interpolatedString)
    {
        var formatBuilder = new StringBuilder();
        var arguments = new List<Expression>();
        foreach (var fragment in interpolatedString.Contents)
        {
            switch (fragment)
            {
                case InterpolatedStringTextSyntax text:
                    formatBuilder.Append(text);
                    break;
                case InterpolationSyntax interpolation:
                    var interpolationExpression = Visit(interpolation.Expression);
                    if (interpolationExpression.Type != typeof(object))
                    {
                        interpolationExpression = Convert(interpolationExpression, typeof(object));
                    }

                    arguments.Add(interpolationExpression);
                    formatBuilder.Append('{').Append(arguments.Count - 1).Append('}');
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        // Return a call to string.Format(), unless we have an implicit conversion to FormattableString, in which case return a call to
        // FormattableStringFactory.Create().
        return Call(
            _semanticModel.GetTypeInfo(interpolatedString).ConvertedType switch
            {
                { } t when t.Equals(_formattableStringSymbol, SymbolEqualityComparer.Default)
                    => _formattableStringFactoryCreateMethod ??= typeof(FormattableStringFactory).GetMethod(
                        nameof(FormattableStringFactory.Create), [typeof(string), typeof(object[])])!,

                _ => _stringFormatMethod ??= typeof(string).GetMethod(nameof(string.Format), [typeof(string), typeof(object[])])!
            },
            Constant(formatBuilder.ToString()),
            NewArrayInit(typeof(object), arguments));
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

        // First, if the method return type is the user's DbContext type (e.g. DbContext local variable, or field/property), return a
        // constant over that DbContext type; the invocation can serve as the root for a LINQ query we can precompile.
        if (methodSymbol.ReturnType.Equals(_userDbContextSymbol, SymbolEqualityComparer.Default))
        {
            return Constant(_userDbContext);
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
            var typeTypeParameterMap = new Dictionary<string, Type>(GetTypeTypeParameters(methodSymbol.ContainingType));

            var definitionMethodInfos = declaringType.GetMethods()
                .Where(
                    m =>
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
                throw new InvalidOperationException(
                    $"Invocation: Found {definitionMethodInfos.Length} matches for generic method: {invocation}");
            }

            var definitionMethodInfo = definitionMethodInfos[0];
            var typeParams = methodSymbol.TypeArguments.Select(a => ResolveType(a)).ToArray();
            methodInfo = definitionMethodInfo.MakeGenericMethod(typeParams);
        }
        else
        {
            // Non-generic method
            var reducedMethodSymbol = methodSymbol.ReducedFrom ?? methodSymbol;

            methodInfo = declaringType.GetMethod(
                methodSymbol.Name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                reducedMethodSymbol.Parameters.Select(p => ResolveType(p.Type)).ToArray());

            if (methodInfo is null)
            {
                throw new InvalidOperationException(
                    $"Invocation: couldn't find method '{methodSymbol.Name}' on type '{declaringType.Name}': {invocation}");
            }
        }

        // We have the reflection MethodInfo for the method, prepare the arguments.

        // We can have less arguments than parameters when the method has optional parameters; fill in the missing ones with the default
        // value.
        // If the method also has a "params" parameter, we also need to take care of that - the syntactic arguments will need to be packed
        // into the "params" array etc.
        var parameters = methodInfo.GetParameters();
        var sourceArguments = invocation.ArgumentList.Arguments;
        var destArguments = new Expression?[parameters.Length];
        var paramIndex = 0;

        // At the syntactic level, an extension method invocation looks like a normal instance's.
        // Prepend the instance to the argument list.
        // TODO: Test invoking extension without extension syntax (as static)
        if (methodSymbol is { IsExtensionMethod: true /*, ReceiverType: { } */ })
        {
            destArguments[0] = instance;
            paramIndex = 1;
            instance = null;
        }

        for (var sourceArgIndex = 0; paramIndex < parameters.Length; paramIndex++)
        {
            var parameter = parameters[paramIndex];
            if (parameter.IsDefined(typeof(ParamArrayAttribute)))
            {
                // We've reached a "params" parameter; pack all the remaining args (possibly zero) into a NewArrayExpression
                var elementType = parameter.ParameterType.GetElementType()!;
                var paramsArguments = new Expression[sourceArguments.Count - sourceArgIndex];
                for (var paramsArgIndex = 0; sourceArgIndex < sourceArguments.Count; sourceArgIndex++, paramsArgIndex++)
                {
                    var arg = invocation.ArgumentList.Arguments[sourceArgIndex];
                    Check.DebugAssert(arg.NameColon is null, "Named argument in params");

                    paramsArguments[paramsArgIndex] = Visit(arg);
                }

                destArguments[paramIndex] = NewArrayInit(elementType, paramsArguments);
                Check.DebugAssert(paramIndex == parameters.Length - 1, "Parameters after params");
                break;
            }

            if (sourceArgIndex >= sourceArguments.Count)
            {
                // Fewer arguments than there are parameters - we have optional parameters.
                Check.DebugAssert(parameter.IsOptional, "Missing non-optional argument");

                destArguments[paramIndex] = Constant(
                    parameter.DefaultValue is null && parameter.ParameterType.IsValueType
                        ? Activator.CreateInstance(parameter.ParameterType)
                        : parameter.DefaultValue,
                    parameter.ParameterType);
                continue;
            }

            var argument = invocation.ArgumentList.Arguments[sourceArgIndex++];

            // Positional argument
            if (argument.NameColon is null)
            {
                destArguments[paramIndex] = Visit(argument, parameter.ParameterType);
                continue;
            }

            // Named argument
            throw new NotImplementedException("Named argument");
        }

        Check.DebugAssert(destArguments.All(a => a is not null), "arguments.All(a => a is not null)");

        return Call(instance, methodInfo, destArguments!);

        IEnumerable<KeyValuePair<string, Type>> GetTypeTypeParameters(INamedTypeSymbol typeSymbol)
        {
            // TODO: We match Roslyn type parameters by name, not sure that's right; also for the method's generic type parameters

            if (typeSymbol.ContainingType is INamedTypeSymbol containingTypeSymbol)
            {
                foreach (var kvp in GetTypeTypeParameters(containingTypeSymbol))
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
                yield return new KeyValuePair<string, Type>(typeParamSymbol.Name, typeParamType);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitLiteralExpression(LiteralExpressionSyntax literal)
        => _semanticModel.GetTypeInfo(literal) is { ConvertedType: ITypeSymbol type }
            ? Constant(literal.Token.Value, ResolveType(type))
            : Constant(literal.Token.Value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccess)
    {
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

        switch (objectCreation.Initializer)
        {
            // No initializers, just return the NewExpression
            case null or { Expressions: [] }:
                return newExpression;

            // Assignment initializer (new Blog { Name = "foo" })
            case { Expressions: [AssignmentExpressionSyntax, ..] }:
                return MemberInit(
                    newExpression,
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
                                throw new InvalidOperationException(
                                    $"ObjectCreation: couldn't find initialized member '{lValueSymbol.Name}': {e}");
                            }

                            return Bind(memberInfo, Visit(value));
                        }));

            // Non-assignment initializer => list initializer (new List<int> { 1, 2, 3 })
            default:
                // Find the correct Add() method on the collection type
                // TODO: This doesn't work if there are multiple Add() methods (contrived). Complete solution would be to find the base
                // TODO: type for all initializer expressions and find an Add overload of that type (or a superclass thereof)
                var addMethod = type.GetMethods().SingleOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1);
                if (addMethod is null)
                {
                    throw new InvalidOperationException(
                        $"Couldn't find single Add method on type '{type.Name}', required for list initializer");
                }

                // TODO: Dictionary initializer, where each ElementInit has more than one expression

                return ListInit(
                    newExpression,
                    objectCreation.Initializer.Expressions.Select(e => ElementInit(addMethod, Visit(e))));
        }
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
        => VisitLambdaExpression(lambda);

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

            SyntaxKind.AddressOfExpression => throw NotSupportedInExpressionTrees(),
            SyntaxKind.IndexExpression => throw NotSupportedInExpressionTrees(),
            SyntaxKind.PointerIndirectionExpression => throw NotSupportedInExpressionTrees(),
            SyntaxKind.PreDecrementExpression => throw NotSupportedInExpressionTrees(),
            SyntaxKind.PreIncrementExpression => throw NotSupportedInExpressionTrees(),

            _ => throw new UnreachableException(
                $"Unexpected syntax kind '{unary.Kind()}' when visiting a {nameof(PrefixUnaryExpressionSyntax)}")
        };

        NotSupportedException NotSupportedInExpressionTrees()
            => throw new UnreachableException(
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

            SyntaxKind.PostIncrementExpression => throw NotSupportedInExpressionTrees(),
            SyntaxKind.PostDecrementExpression => throw NotSupportedInExpressionTrees(),

            _ => throw new UnreachableException(
                $"Unexpected syntax kind '{unary.Kind()}' when visiting a {nameof(PostfixUnaryExpressionSyntax)}")
        };

        NotSupportedException NotSupportedInExpressionTrees()
            => throw new UnreachableException(
                $"Unary expression of type {unary.Kind()} is not supported in expression trees");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitQueryExpression(QueryExpressionSyntax node)
        => throw new NotSupportedException(DesignStrings.QueryComprehensionSyntaxNotSupportedInPrecompiledQueries);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax lambda)
        => VisitLambdaExpression(lambda);

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

    private Expression VisitLambdaExpression(AnonymousFunctionExpressionSyntax lambda, Type? expectedType = null)
    {
        if (lambda.ExpressionBody is null)
        {
            throw new NotSupportedException("Lambda with null expression body");
        }

        if (lambda.Modifiers.Any())
        {
            throw new NotSupportedException("Lambda with modifiers not supported: " + lambda.Modifiers);
        }

        if (!lambda.AsyncKeyword.IsKind(SyntaxKind.None))
        {
            throw new NotSupportedException("Async lambdas are not supported");
        }

        var lambdaParameters = lambda switch
        {
            SimpleLambdaExpressionSyntax simpleLambda => SyntaxFactory.SingletonSeparatedList(simpleLambda.Parameter),
            ParenthesizedLambdaExpressionSyntax parenthesizedLambda => parenthesizedLambda.ParameterList.Parameters,

            _ => throw new UnreachableException()
        };

        var translatedParameters = new List<ParameterExpression>();
        foreach (var parameter in lambdaParameters)
        {
            if (_semanticModel.GetDeclaredSymbol(parameter) is not { } parameterSymbol
                || ResolveType(parameterSymbol.Type) is not { } parameterType)
            {
                throw new InvalidOperationException("Could not found symbol for parameter lambda: " + parameter);
            }

            translatedParameters.Add(Parameter(parameterType, parameter.Identifier.Text));
        }

        _parameterStack.Push(
            _parameterStack.Peek()
                .AddRange(
                    translatedParameters.Select(
                        p => new KeyValuePair<string, ParameterExpression>(p.Name ?? throw new NotImplementedException(), p))));

        try
        {
            var body = Visit(lambda.ExpressionBody);

            return expectedType switch
            {
                // If there's no contextual expected type, we allow the lambda's type to be inferred from its parameters and the body's
                // return type.
                null => Lambda(body, translatedParameters),

                // This is for the case where the expected type is Action<T>, but the lambda body does return something, which needs to get
                // ignored; for example, the ExecuteUpdateAsync setter parameter is Action<UpdateSettersBuilder>, but the function is
                // invoked with ExecuteUpdateAsync(s => s.SetProperty(...)), and SetProperty() returns UpdateSettersBuilder for further
                // chaining. In this case, the body's return type is an UpdateSettersBuilder, meaning that the type of the constructed
                // lambda here would be Func<UpdateSettersBuilder, UpdateSettersBuilder>, and not Action<UpdateSettersBuilder> as
                // ExecuteUpdateAsync's signature requires.
                // Identify this case, and explicitly type the returned lambda as an Action when necessary.
                _ when expectedType.IsGenericType && expectedType.IsAssignableTo(typeof(MulticastDelegate))
                    => Lambda(expectedType, body, translatedParameters),

                _ when expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(Expression<>)
                    => Lambda(expectedType.GetGenericArguments()[0], body, translatedParameters),

                _ => throw new UnreachableException()
            };
        }
        finally
        {
            _parameterStack.Pop();
        }
    }

    /// <summary>
    ///     Given a Roslyn type symbol, returns a .NET reflection <see cref="Type" />.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to be translated.</param>
    /// <returns>A .NET reflection <see cref="Type" /> that corresponds to <paramref name="typeSymbol" />.</returns>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual Type TranslateType(ITypeSymbol typeSymbol)
        => ResolveType(typeSymbol);

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
                    properties.Select(p => p.Name).ToArray(),
                    out var anonymousTypeGenericDefinition);
                Check.DebugAssert(found, "Anonymous type not found");

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

            case IArrayTypeSymbol arrayTypeSymbol:
                // The ContainingAssembly of array type symbols can be null; recurse down the element types (down to the non-array element
                // type) to get the assembly.
                var containingAssembly = arrayTypeSymbol.ContainingAssembly;
                ITypeSymbol currentSymbol = arrayTypeSymbol;
                while (containingAssembly is null && currentSymbol is IArrayTypeSymbol { ElementType: var nestedTypeSymbol })
                {
                    currentSymbol = nestedTypeSymbol;
                    containingAssembly = currentSymbol.ContainingAssembly;
                }

                return GetClrTypeFromAssembly(
                    containingAssembly,
                    typeSymbol.ToDisplayString(QualifiedTypeNameSymbolDisplayFormat));

            default:
                return GetClrTypeFromAssembly(
                    typeSymbol.ContainingAssembly,
                    typeSymbol.ToDisplayString(QualifiedTypeNameSymbolDisplayFormat));
        }

        Type GetClrType(INamedTypeSymbol symbol)
        {
            if (symbol.SpecialType == SpecialType.System_Nullable_T)
            {
                return typeof(Nullable<>);
            }

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

        Type GetClrTypeFromAssembly(IAssemblySymbol? assemblySymbol, string name)
            => (assemblySymbol is null
                    ? Type.GetType(name)!
                    : Type.GetType($"{name}, {assemblySymbol.Name}"))
                // If we can't find the Type, check the assembly where the user's DbContext type lives; this is primarily to support
                // testing, where user code is in an assembly that's built as part of the the test and loaded into a specific
                // AssemblyLoadContext (which gets unloaded later).
                ?? _additionalAssembly?.GetType(name)
                ?? throw new InvalidOperationException(
                    $"Couldn't resolve CLR type '{name}' in assembly '{assemblySymbol?.Name}'");

        Dictionary<string[], Type> LoadAnonymousTypes(IAssemblySymbol assemblySymbol)
        {
            Assembly? assembly;
            try
            {
                assembly = Assembly.Load(assemblySymbol.Name);
            }
            catch (FileNotFoundException)
            {
                // If we can't find the assembly, use the assembly where the user's DbContext type lives; this is primarily to support
                // testing, where user code is in an assembly that's built as part of the the test and loaded into a specific
                // AssemblyLoadContext (which gets unloaded later).
                assembly = _additionalAssembly
                    ?? throw new InvalidOperationException($"Could not load assembly for IAssemblySymbol '{assemblySymbol.Name}'");
            }

            // Get all the anonymous type in the assembly, and index them by the ordered names of their properties.
            // Note that anonymous types are generic, so we don't have property types in the key.

            // TODO: An alternative strategy would be to just generate the types as we need them (with ref.emit) - that's probably safer.
            // TODO: Though it may mean that the resulting CLR Type can't be anonymous (Type.IsAnonymousType()) - not sure that matters.
            return assembly.GetTypes()
                .Where(t => t.IsAnonymousType())
                .ToDictionary(t => t.GetProperties().Select(x => x.Name).ToArray(), t => t, new ArrayStructuralComparer<string>());
        }
    }

    private sealed class ArrayStructuralComparer<T> : IEqualityComparer<T[]>
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
    private sealed class FakeClosureFrameClass;

    private sealed class FakeFieldInfo(
        Type declaringType,
        Type fieldType,
        string name,
        bool isNonNullableReferenceType)
        : FieldInfo, IParameterNullabilityInfo
    {
        public bool IsNonNullableReferenceType { get; } = isNonNullableReferenceType;

        public override object[] GetCustomAttributes(bool inherit)
            => [];

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            => [];

        public override bool IsDefined(Type attributeType, bool inherit)
            => false;

        public override Type DeclaringType { get; } = declaringType;

        public override string Name { get; } = name;

        public override Type? ReflectedType
            => null;

        // We implement GetValue since ExpressionTreeFuncletizer calls it to get the parameter value. In AOT generation time, we obviously
        // have no parameter values, nor do we need them for the first part of the query pipeline.
        public override object? GetValue(object? obj)
            => FieldType.IsValueType
                ? Activator.CreateInstance(FieldType)
                : FieldType == typeof(string)
                    ? "<dummy>"
                    : null;

        public override void SetValue(
            object? obj,
            object? value,
            BindingFlags invokeAttr,
            Binder? binder,
            CultureInfo? culture)
            => throw new NotSupportedException();

        public override FieldAttributes Attributes
            => FieldAttributes.Public;

        public override RuntimeFieldHandle FieldHandle
            => throw new NotSupportedException();

        public override Type FieldType { get; } = fieldType;
    }

    private sealed class FakeConstructorInfo(Type type, ParameterInfo[] parameters) : ConstructorInfo
    {
        public override object[] GetCustomAttributes(bool inherit)
            => [];

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            => [];

        public override bool IsDefined(Type attributeType, bool inherit)
            => false;

        public override Type DeclaringType { get; } = type;

        public override string Name
            => ".ctor";

        public override Type ReflectedType
            => DeclaringType;

        public override MethodImplAttributes GetMethodImplementationFlags()
            => MethodImplAttributes.Managed;

        public override ParameterInfo[] GetParameters()
            => parameters;

        public override MethodAttributes Attributes
            => MethodAttributes.Public;

        public override RuntimeMethodHandle MethodHandle
            => throw new NotSupportedException();

        public override object Invoke(
            object? obj,
            BindingFlags invokeAttr,
            Binder? binder,
            object?[]? parameters,
            CultureInfo? culture)
            => throw new NotSupportedException();

        public override object Invoke(
            BindingFlags invokeAttr,
            Binder? binder,
            object?[]? parameters,
            CultureInfo? culture)
            => throw new NotSupportedException();
    }

    private sealed class FakeParameterInfo(string name, Type parameterType, int position) : ParameterInfo
    {
        public override ParameterAttributes Attributes
            => ParameterAttributes.In;

        public override string? Name { get; } = name;
        public override Type ParameterType { get; } = parameterType;
        public override int Position { get; } = position;

        public override MemberInfo Member
            => throw new NotSupportedException();
    }
}
