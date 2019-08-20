// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;

// ReSharper disable SwitchStatementMissingSomeCases
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery
namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {
        /// <summary>
        ///     Creates a new <see cref="ExpressionEqualityComparer" />.
        /// </summary>
        private ExpressionEqualityComparer()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static ExpressionEqualityComparer Instance { get; } = new ExpressionEqualityComparer();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int GetHashCode(Expression obj)
        {
            if (obj == null)
            {
                return 0;
            }

            unchecked
            {
                var hash = new HashCode();
                hash.Add(obj.NodeType);
                hash.Add(obj.Type);

                switch (obj.NodeType)
                {
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                    case ExpressionType.UnaryPlus:
                    {
                        var unaryExpression = (UnaryExpression)obj;

                        if (unaryExpression.Method != null)
                        {
                            hash.Add(unaryExpression.Method);
                        }

                        hash.Add(unaryExpression.Operand, this);

                        break;
                    }
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Coalesce:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.RightShift:
                    case ExpressionType.LeftShift:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.Power:
                    {
                        var binaryExpression = (BinaryExpression)obj;

                        hash.Add(binaryExpression.Left, this);
                        hash.Add(binaryExpression.Right, this);

                        break;
                    }
                    case ExpressionType.TypeIs:
                    {
                        var typeBinaryExpression = (TypeBinaryExpression)obj;

                        hash.Add(typeBinaryExpression.Expression, this);
                        hash.Add(typeBinaryExpression.TypeOperand);

                        break;
                    }
                    case ExpressionType.Constant:
                    {
                        var constantExpression = (ConstantExpression)obj;

                        if (constantExpression.Value != null
                            && !(constantExpression.Value is IQueryable))
                        {
                            hash.Add(constantExpression.Value);
                        }

                        break;
                    }
                    case ExpressionType.Parameter:
                    {
                        var parameterExpression = (ParameterExpression)obj;

                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (parameterExpression.Name != null)
                        {
                            hash.Add(parameterExpression.Name);
                        }

                        break;
                    }
                    case ExpressionType.MemberAccess:
                    {
                        var memberExpression = (MemberExpression)obj;

                        hash.Add(memberExpression.Member);
                        hash.Add(memberExpression.Expression, this);

                        break;
                    }
                    case ExpressionType.Call:
                    {
                        var methodCallExpression = (MethodCallExpression)obj;

                        hash.Add(methodCallExpression.Method);
                        hash.Add(methodCallExpression.Object, this);
                        AddListToHash(ref hash, methodCallExpression.Arguments);

                        break;
                    }
                    case ExpressionType.Lambda:
                    {
                        var lambdaExpression = (LambdaExpression)obj;

                        hash.Add(lambdaExpression.ReturnType);
                        hash.Add(lambdaExpression.Body, this);
                        AddListToHash(ref hash, lambdaExpression.Parameters);

                        break;
                    }
                    case ExpressionType.New:
                    {
                        var newExpression = (NewExpression)obj;

                        hash.Add(newExpression.Constructor);

                        if (newExpression.Members != null)
                        {
                            for (var i = 0; i < newExpression.Members.Count; i++)
                            {
                                hash.Add(newExpression.Members[i]);
                            }
                        }

                        AddListToHash(ref hash, newExpression.Arguments);

                        break;
                    }
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.NewArrayBounds:
                    {
                        var newArrayExpression = (NewArrayExpression)obj;
                        AddListToHash(ref hash, newArrayExpression.Expressions);

                        break;
                    }
                    case ExpressionType.Invoke:
                    {
                        var invocationExpression = (InvocationExpression)obj;

                        hash.Add(invocationExpression.Expression, this);
                        AddListToHash(ref hash, invocationExpression.Arguments);

                        break;
                    }
                    case ExpressionType.MemberInit:
                    {
                        var memberInitExpression = (MemberInitExpression)obj;

                        hash.Add(memberInitExpression.NewExpression, this);

                        for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
                        {
                            var memberBinding = memberInitExpression.Bindings[i];

                            hash.Add(memberBinding.Member);
                            hash.Add(memberBinding.BindingType);

                            switch (memberBinding.BindingType)
                            {
                                case MemberBindingType.Assignment:
                                    var memberAssignment = (MemberAssignment)memberBinding;
                                    hash.Add(memberAssignment.Expression, this);
                                    break;
                                case MemberBindingType.ListBinding:
                                    var memberListBinding = (MemberListBinding)memberBinding;
                                    for (var j = 0; j < memberListBinding.Initializers.Count; j++)
                                    {
                                        AddListToHash(ref hash, memberListBinding.Initializers[j].Arguments);
                                    }

                                    break;
                                default:
                                    throw new NotImplementedException($"Unhandled binding type: {memberBinding}");
                            }
                        }

                        break;
                    }
                    case ExpressionType.ListInit:
                    {
                        var listInitExpression = (ListInitExpression)obj;

                        hash.Add(listInitExpression.NewExpression, this);

                        for (var i = 0; i < listInitExpression.Initializers.Count; i++)
                        {
                            AddListToHash(ref hash, listInitExpression.Initializers[i].Arguments);
                        }

                        break;
                    }
                    case ExpressionType.Conditional:
                    {
                        var conditionalExpression = (ConditionalExpression)obj;

                        hash.Add(conditionalExpression.Test, this);
                        hash.Add(conditionalExpression.IfTrue, this);
                        hash.Add(conditionalExpression.IfFalse, this);

                        break;
                    }
                    case ExpressionType.Default:
                    {
                        hash.Add(obj.Type);
                        break;
                    }
                    case ExpressionType.Extension:
                    {
                        hash.Add(obj);
                        break;
                    }
                    case ExpressionType.Index:
                    {
                        var indexExpression = (IndexExpression)obj;

                        hash.Add(indexExpression.Indexer);
                        hash.Add(indexExpression.Object, this);
                        AddListToHash(ref hash, indexExpression.Arguments);

                        break;
                    }
                    default:
                        throw new NotImplementedException($"Unhandled expression node type: {obj.NodeType}");
                }

                return hash.ToHashCode();
            }
        }

        private void AddListToHash<T>(ref HashCode hash, IReadOnlyList<T> expressions)
            where T : Expression
        {
            for (var i = 0; i < expressions.Count; i++)
            {
                hash.Add(expressions[i], this);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Equals(Expression x, Expression y) => new ExpressionComparer().Compare(x, y);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool SequenceEquals(IEnumerable<Expression> x, IEnumerable<Expression> y)
        {
            if (x == null
                || y == null)
            {
                return false;
            }

            if (x.Count() != y.Count())
            {
                return false;
            }

            var comparer = new ExpressionComparer();

            return x.Zip(y, (l, r) => comparer.Compare(l, r)).All(r => r);
        }

        private sealed class ExpressionComparer
        {
            private ScopedDictionary<ParameterExpression, ParameterExpression> _parameterScope;

            public bool Compare(Expression a, Expression b)
            {
                if (a == b)
                {
                    return true;
                }

                if (a == null
                    || b == null)
                {
                    return false;
                }

                if (a.NodeType != b.NodeType)
                {
                    return false;
                }

                if (a.Type != b.Type)
                {
                    return false;
                }

                switch (a.NodeType)
                {
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                    case ExpressionType.UnaryPlus:
                        return CompareUnary((UnaryExpression)a, (UnaryExpression)b);
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Coalesce:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.RightShift:
                    case ExpressionType.LeftShift:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.Power:
                        return CompareBinary((BinaryExpression)a, (BinaryExpression)b);
                    case ExpressionType.TypeIs:
                        return CompareTypeIs((TypeBinaryExpression)a, (TypeBinaryExpression)b);
                    case ExpressionType.Conditional:
                        return CompareConditional((ConditionalExpression)a, (ConditionalExpression)b);
                    case ExpressionType.Constant:
                        return CompareConstant((ConstantExpression)a, (ConstantExpression)b);
                    case ExpressionType.Parameter:
                        return CompareParameter((ParameterExpression)a, (ParameterExpression)b);
                    case ExpressionType.MemberAccess:
                        return CompareMemberAccess((MemberExpression)a, (MemberExpression)b);
                    case ExpressionType.Call:
                        return CompareMethodCall((MethodCallExpression)a, (MethodCallExpression)b);
                    case ExpressionType.Lambda:
                        return CompareLambda((LambdaExpression)a, (LambdaExpression)b);
                    case ExpressionType.New:
                        return CompareNew((NewExpression)a, (NewExpression)b);
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.NewArrayBounds:
                        return CompareNewArray((NewArrayExpression)a, (NewArrayExpression)b);
                    case ExpressionType.Invoke:
                        return CompareInvocation((InvocationExpression)a, (InvocationExpression)b);
                    case ExpressionType.MemberInit:
                        return CompareMemberInit((MemberInitExpression)a, (MemberInitExpression)b);
                    case ExpressionType.ListInit:
                        return CompareListInit((ListInitExpression)a, (ListInitExpression)b);
                    case ExpressionType.Extension:
                        return CompareExtension(a, b);
                    case ExpressionType.Default:
                        return CompareDefault((DefaultExpression)a, (DefaultExpression)b);
                    case ExpressionType.Index:
                        return CompareIndex((IndexExpression)a, (IndexExpression)b);
                    default:
                        throw new NotImplementedException($"Unhandled expression node type: {a.NodeType}");
                }
            }

            private bool CompareDefault(DefaultExpression a, DefaultExpression b)
                => a.Type == b.Type;

            private bool CompareUnary(UnaryExpression a, UnaryExpression b)
                => Equals(a.Method, b.Method)
                   && a.IsLifted == b.IsLifted
                   && a.IsLiftedToNull == b.IsLiftedToNull
                   && Compare(a.Operand, b.Operand);

            private bool CompareBinary(BinaryExpression a, BinaryExpression b)
                => Equals(a.Method, b.Method)
                   && a.IsLifted == b.IsLifted
                   && a.IsLiftedToNull == b.IsLiftedToNull
                   && Compare(a.Left, b.Left)
                   && Compare(a.Right, b.Right);

            private bool CompareTypeIs(TypeBinaryExpression a, TypeBinaryExpression b)
                => a.TypeOperand == b.TypeOperand
                   && Compare(a.Expression, b.Expression);

            private bool CompareConditional(ConditionalExpression a, ConditionalExpression b)
                => Compare(a.Test, b.Test)
                   && Compare(a.IfTrue, b.IfTrue)
                   && Compare(a.IfFalse, b.IfFalse);

            private static bool CompareConstant(ConstantExpression a, ConstantExpression b)
            {
                if (a.Value == b.Value)
                {
                    return true;
                }

                if (a.Value == null
                    || b.Value == null)
                {
                    return false;
                }

                return a.IsEntityQueryable()
                       && b.IsEntityQueryable()
                       && a.Value.GetType() == b.Value.GetType()
                    ? true
                    : Equals(a.Value, b.Value);
            }

            private bool CompareParameter(ParameterExpression a, ParameterExpression b)
            {
                if (_parameterScope != null)
                {
                    if (_parameterScope.TryGetValue(a, out var mapped))
                    {
                        return mapped.Name == b.Name
                               && mapped.Type == b.Type;
                    }
                }

                return a.Name == b.Name
                       && a.Type == b.Type;
            }

            private bool CompareMemberAccess(MemberExpression a, MemberExpression b)
                => Equals(a.Member, b.Member)
                   && Compare(a.Expression, b.Expression);

            private bool CompareMethodCall(MethodCallExpression a, MethodCallExpression b)
                => Equals(a.Method, b.Method)
                   && Compare(a.Object, b.Object)
                   && CompareExpressionList(a.Arguments, b.Arguments);

            private bool CompareLambda(LambdaExpression a, LambdaExpression b)
            {
                var n = a.Parameters.Count;

                if (b.Parameters.Count != n)
                {
                    return false;
                }

                // all must have same type
                for (var i = 0; i < n; i++)
                {
                    if (a.Parameters[i].Type != b.Parameters[i].Type)
                    {
                        return false;
                    }
                }

                var save = _parameterScope;

                _parameterScope = new ScopedDictionary<ParameterExpression, ParameterExpression>(_parameterScope);

                try
                {
                    for (var i = 0; i < n; i++)
                    {
                        _parameterScope.Add(a.Parameters[i], b.Parameters[i]);
                    }

                    return Compare(a.Body, b.Body);
                }
                finally
                {
                    _parameterScope = save;
                }
            }

            private bool CompareNew(NewExpression a, NewExpression b)
                => Equals(a.Constructor, b.Constructor)
                   && CompareExpressionList(a.Arguments, b.Arguments)
                   && CompareMemberList(a.Members, b.Members);

            private bool CompareExpressionList(IReadOnlyList<Expression> a, IReadOnlyList<Expression> b)
            {
                if (Equals(a, b))
                {
                    return true;
                }

                if (a == null
                    || b == null)
                {
                    return false;
                }

                if (a.Count != b.Count)
                {
                    return false;
                }

                for (int i = 0, n = a.Count; i < n; i++)
                {
                    if (!Compare(a[i], b[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            private static bool CompareMemberList(IReadOnlyList<MemberInfo> a, IReadOnlyList<MemberInfo> b)
            {
                if (ReferenceEquals(a, b))
                {
                    return true;
                }

                if (a == null
                    || b == null)
                {
                    return false;
                }

                if (a.Count != b.Count)
                {
                    return false;
                }

                for (int i = 0, n = a.Count; i < n; i++)
                {
                    if (!Equals(a[i], b[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool CompareNewArray(NewArrayExpression a, NewArrayExpression b)
                => CompareExpressionList(a.Expressions, b.Expressions);

            private bool CompareExtension(Expression a, Expression b)
                => a.Equals(b);

            private bool CompareInvocation(InvocationExpression a, InvocationExpression b)
                => Compare(a.Expression, b.Expression)
                   && CompareExpressionList(a.Arguments, b.Arguments);

            private bool CompareMemberInit(MemberInitExpression a, MemberInitExpression b)
                => Compare(a.NewExpression, b.NewExpression)
                   && CompareBindingList(a.Bindings, b.Bindings);

            private bool CompareBindingList(IReadOnlyList<MemberBinding> a, IReadOnlyList<MemberBinding> b)
            {
                if (ReferenceEquals(a, b))
                {
                    return true;
                }

                if (a == null
                    || b == null)
                {
                    return false;
                }

                if (a.Count != b.Count)
                {
                    return false;
                }

                for (int i = 0, n = a.Count; i < n; i++)
                {
                    if (!CompareBinding(a[i], b[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool CompareBinding(MemberBinding a, MemberBinding b)
            {
                if (a == b)
                {
                    return true;
                }

                if (a == null
                    || b == null)
                {
                    return false;
                }

                if (a.BindingType != b.BindingType)
                {
                    return false;
                }

                if (!Equals(a.Member, b.Member))
                {
                    return false;
                }

                switch (a.BindingType)
                {
                    case MemberBindingType.Assignment:
                        return CompareMemberAssignment((MemberAssignment)a, (MemberAssignment)b);
                    case MemberBindingType.ListBinding:
                        return CompareMemberListBinding((MemberListBinding)a, (MemberListBinding)b);
                    case MemberBindingType.MemberBinding:
                        return CompareMemberMemberBinding((MemberMemberBinding)a, (MemberMemberBinding)b);
                    default:
                        throw new InvalidOperationException("Unhandled member binding type: " + a.BindingType);
                }
            }

            private bool CompareMemberAssignment(MemberAssignment a, MemberAssignment b)
                => Equals(a.Member, b.Member)
                   && Compare(a.Expression, b.Expression);

            private bool CompareMemberListBinding(MemberListBinding a, MemberListBinding b)
                => Equals(a.Member, b.Member)
                   && CompareElementInitList(a.Initializers, b.Initializers);

            private bool CompareMemberMemberBinding(MemberMemberBinding a, MemberMemberBinding b)
                => Equals(a.Member, b.Member)
                   && CompareBindingList(a.Bindings, b.Bindings);

            private bool CompareListInit(ListInitExpression a, ListInitExpression b)
                => Compare(a.NewExpression, b.NewExpression)
                   && CompareElementInitList(a.Initializers, b.Initializers);

            private bool CompareElementInitList(IReadOnlyList<ElementInit> a, IReadOnlyList<ElementInit> b)
            {
                if (ReferenceEquals(a, b))
                {
                    return true;
                }

                if (a == null
                    || b == null)
                {
                    return false;
                }

                if (a.Count != b.Count)
                {
                    return false;
                }

                for (int i = 0, n = a.Count; i < n; i++)
                {
                    if (!CompareElementInit(a[i], b[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool CompareElementInit(ElementInit a, ElementInit b)
                => Equals(a.AddMethod, b.AddMethod)
                   && CompareExpressionList(a.Arguments, b.Arguments);

            private bool CompareIndex(IndexExpression a, IndexExpression b)
                => Equals(a.Indexer, b.Indexer)
                   && Compare(a.Object, b.Object)
                   && CompareExpressionList(a.Arguments, b.Arguments);

            private class ScopedDictionary<TKey, TValue>
            {
                private readonly ScopedDictionary<TKey, TValue> _previous;
                private readonly Dictionary<TKey, TValue> _map;

                public ScopedDictionary(ScopedDictionary<TKey, TValue> previous)
                {
                    _previous = previous;
                    _map = new Dictionary<TKey, TValue>();
                }

                public void Add(TKey key, TValue value) => _map.Add(key, value);

                public bool TryGetValue(TKey key, out TValue value)
                {
                    for (var scope = this; scope != null; scope = scope._previous)
                    {
                        if (scope._map.TryGetValue(key, out value))
                        {
                            return true;
                        }
                    }

                    value = default;

                    return false;
                }
            }
        }
    }
}
