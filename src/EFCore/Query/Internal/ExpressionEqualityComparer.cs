// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;

// ReSharper disable SwitchStatementMissingSomeCases
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery
namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int GetHashCode(Expression obj)
        {
            if (obj == null)
            {
                return 0;
            }

            unchecked
            {
                var hashCode = (int)obj.NodeType;

                hashCode += (hashCode * 397) ^ obj.Type.GetHashCode();

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
                            hashCode += hashCode * 397 ^ unaryExpression.Method.GetHashCode();
                        }

                        hashCode += (hashCode * 397) ^ GetHashCode(unaryExpression.Operand);

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

                        hashCode += (hashCode * 397) ^ GetHashCode(binaryExpression.Left);
                        hashCode += (hashCode * 397) ^ GetHashCode(binaryExpression.Right);

                        break;
                    }
                    case ExpressionType.TypeIs:
                    {
                        var typeBinaryExpression = (TypeBinaryExpression)obj;

                        hashCode += (hashCode * 397) ^ GetHashCode(typeBinaryExpression.Expression);
                        hashCode += (hashCode * 397) ^ typeBinaryExpression.TypeOperand.GetHashCode();

                        break;
                    }
                    case ExpressionType.Constant:
                    {
                        var constantExpression = (ConstantExpression)obj;

                        if (constantExpression.Value != null
                            && !(constantExpression.Value is IQueryable))
                        {
                            hashCode += (hashCode * 397) ^ constantExpression.Value.GetHashCode();
                        }

                        break;
                    }
                    case ExpressionType.Parameter:
                    {
                        var parameterExpression = (ParameterExpression)obj;

                        hashCode += hashCode * 397;
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (parameterExpression.Name != null)
                        {
                            hashCode ^= parameterExpression.Name.GetHashCode();
                        }

                        break;
                    }
                    case ExpressionType.MemberAccess:
                    {
                        var memberExpression = (MemberExpression)obj;

                        hashCode += (hashCode * 397) ^ memberExpression.Member.GetHashCode();
                        hashCode += (hashCode * 397) ^ GetHashCode(memberExpression.Expression);

                        break;
                    }
                    case ExpressionType.Call:
                    {
                        var methodCallExpression = (MethodCallExpression)obj;

                        hashCode += (hashCode * 397) ^ methodCallExpression.Method.GetHashCode();
                        hashCode += (hashCode * 397) ^ GetHashCode(methodCallExpression.Object);
                        hashCode += (hashCode * 397) ^ GetHashCode(methodCallExpression.Arguments);

                        break;
                    }
                    case ExpressionType.Lambda:
                    {
                        var lambdaExpression = (LambdaExpression)obj;

                        hashCode += (hashCode * 397) ^ lambdaExpression.ReturnType.GetHashCode();
                        hashCode += (hashCode * 397) ^ GetHashCode(lambdaExpression.Body);
                        hashCode += (hashCode * 397) ^ GetHashCode(lambdaExpression.Parameters);

                        break;
                    }
                    case ExpressionType.New:
                    {
                        var newExpression = (NewExpression)obj;

                        hashCode += (hashCode * 397) ^ newExpression.Constructor.GetHashCode();

                        if (newExpression.Members != null)
                        {
                            for (var i = 0; i < newExpression.Members.Count; i++)
                            {
                                hashCode += (hashCode * 397) ^ newExpression.Members[i].GetHashCode();
                            }
                        }

                        hashCode += (hashCode * 397) ^ GetHashCode(newExpression.Arguments);

                        break;
                    }
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.NewArrayBounds:
                    {
                        var newArrayExpression = (NewArrayExpression)obj;

                        hashCode += (hashCode * 397) ^ GetHashCode(newArrayExpression.Expressions);

                        break;
                    }
                    case ExpressionType.Invoke:
                    {
                        var invocationExpression = (InvocationExpression)obj;

                        hashCode += (hashCode * 397) ^ GetHashCode(invocationExpression.Expression);
                        hashCode += (hashCode * 397) ^ GetHashCode(invocationExpression.Arguments);

                        break;
                    }
                    case ExpressionType.MemberInit:
                    {
                        var memberInitExpression = (MemberInitExpression)obj;

                        hashCode += (hashCode * 397) ^ GetHashCode(memberInitExpression.NewExpression);

                        for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
                        {
                            var memberBinding = memberInitExpression.Bindings[i];

                            hashCode += (hashCode * 397) ^ memberBinding.Member.GetHashCode();
                            hashCode += (hashCode * 397) ^ (int)memberBinding.BindingType;

                            switch (memberBinding.BindingType)
                            {
                                case MemberBindingType.Assignment:
                                    var memberAssignment = (MemberAssignment)memberBinding;
                                    hashCode += (hashCode * 397) ^ GetHashCode(memberAssignment.Expression);
                                    break;
                                case MemberBindingType.ListBinding:
                                    var memberListBinding = (MemberListBinding)memberBinding;
                                    for (var j = 0; j < memberListBinding.Initializers.Count; j++)
                                    {
                                        hashCode += (hashCode * 397) ^ GetHashCode(memberListBinding.Initializers[j].Arguments);
                                    }
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }

                        break;
                    }
                    case ExpressionType.ListInit:
                    {
                        var listInitExpression = (ListInitExpression)obj;

                        hashCode += (hashCode * 397) ^ GetHashCode(listInitExpression.NewExpression);

                        for (var i = 0; i < listInitExpression.Initializers.Count; i++)
                        {
                            hashCode += (hashCode * 397) ^ GetHashCode(listInitExpression.Initializers[i].Arguments);
                        }

                        break;
                    }
                    case ExpressionType.Conditional:
                    {
                        var conditionalExpression = (ConditionalExpression)obj;

                        hashCode += (hashCode * 397) ^ GetHashCode(conditionalExpression.Test);
                        hashCode += (hashCode * 397) ^ GetHashCode(conditionalExpression.IfTrue);
                        hashCode += (hashCode * 397) ^ GetHashCode(conditionalExpression.IfFalse);

                        break;
                    }
                    case ExpressionType.Default:
                    {
                        hashCode += (hashCode * 397) ^ obj.Type.GetHashCode();
                        break;
                    }
                    case ExpressionType.Extension:
                    {
                        if (obj is NullConditionalExpression nullConditionalExpression)
                        {
                            hashCode += (hashCode * 397) ^ GetHashCode(nullConditionalExpression.AccessOperation);
                        }
                        else
                        {
                            hashCode += (hashCode * 397) ^ obj.GetHashCode();
                        }

                        break;
                    }
                    default:
                        throw new NotImplementedException();
                }

                return hashCode;
            }
        }

        private int GetHashCode<T>(IList<T> expressions)
            where T : Expression
        {
            var hashCode = 0;

            for (var i = 0; i < expressions.Count; i++)
            {
                hashCode += (hashCode * 397) ^ GetHashCode(expressions[i]);
            }

            return hashCode;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Equals(Expression x, Expression y) => new ExpressionComparer().Compare(x, y);

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
                    default:
                        throw new NotImplementedException();
                }
            }

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

                if (a.Value is EnumerableQuery
                    && b.Value is EnumerableQuery)
                {
                    return false; // EnumerableQueries are opaque
                }

                if (a.Value is IQueryable
                    && b.Value is IQueryable
                    && a.Value.GetType() == b.Value.GetType())
                {
                    return true;
                }

                return Equals(a.Value, b.Value);
            }

            private bool CompareParameter(ParameterExpression a, ParameterExpression b)
            {
                if (_parameterScope != null)
                {
                    if (_parameterScope.TryGetValue(a, out ParameterExpression mapped))
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
            {
                if (a is NullConditionalExpression nullConditionalExpressionA
                    && b is NullConditionalExpression nullConditionalExpressionB)
                {
                    return Compare(
                        nullConditionalExpressionA.AccessOperation,
                        nullConditionalExpressionB.AccessOperation);
                }

                return a.Equals(b);
            }

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
                        throw new NotImplementedException();
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

                    value = default(TValue);

                    return false;
                }
            }
        }
    }
}
