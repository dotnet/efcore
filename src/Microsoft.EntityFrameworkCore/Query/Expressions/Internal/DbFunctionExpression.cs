// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DbFunctionExpression : Expression
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public enum DbFunctionType
        {
            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            Scalar,

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            TableValued
        }

        private IDbFunction _dbFunction;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbFunctionType FunctionType { get; private set; } = DbFunctionType.Scalar;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type Type => ReturnType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string SchemaName => _dbFunction.Schema;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Name => _dbFunction.Name;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type ReturnType => _dbFunction.ReturnType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ReadOnlyCollection<Expression> Arguments { get; [param: NotNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression OriginalExpression { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbFunctionExpression([NotNull] DbFunctionExpression oldFuncExpression, [NotNull] ReadOnlyCollection<Expression> newArguments)
        {
            Arguments = new ReadOnlyCollection<Expression>(newArguments);
            _dbFunction = oldFuncExpression._dbFunction;
            OriginalExpression = oldFuncExpression.OriginalExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbFunctionExpression([NotNull] IDbFunction dbFunction, [NotNull] MethodCallExpression expression)
        {
            _dbFunction = dbFunction;
            OriginalExpression = expression;

            var methodArgs = expression.Method.GetParameters().Zip(expression.Arguments, (p, a) => new { p.Name, Argument = a });

            Arguments = new ReadOnlyCollection<Expression>(
                            (from p in dbFunction.Parameters
                             join a in methodArgs on p.Name equals a.Name into pas
                             from pa in pas.DefaultIfEmpty()
                             orderby p.ParameterIndex
                             select p.IsIdentifier
                                     ? GerneateIdentiferExpression(p, pa?.Argument)
                                     : p.IsObjectParameter
                                         ? expression.Object
                                           : p.Value != null
                                               ? Expression.Constant(p.Value)
                                               : pa?.Argument).ToList());

            /*if (_dbFunction.ReturnType.GetTypeInfo().IsGenericType == true
                   && _dbFunction.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>))
            {
                FunctionType = DbFunctionType.TableValued;
            }*/
        }

        private Expression GerneateIdentiferExpression(DbFunctionParameter dbFunctionParameter, Expression argunment)
        {
            if (dbFunctionParameter.Value != null)
            {
                return new IdentifierExpression(dbFunctionParameter.Value.ToString());
            }
            else
            {
                if (!(argunment is ConstantExpression))
                    throw new ArgumentException(CoreStrings.DbFunctionIdentifierMustBeCompileTimeConstant(_dbFunction.Name, new[] { dbFunctionParameter }));

                return new IdentifierExpression(argunment as ConstantExpression);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            return base.Accept(visitor);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (!(visitor is TransformingExpressionVisitor) && OriginalExpression != null)
                this.OriginalExpression = visitor.Visit(this.OriginalExpression);

            var newArguments = visitor.Visit(Arguments);

            return newArguments != Arguments
                ? new DbFunctionExpression(this, newArguments)
                : this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string ToString()
        {
            return $"{_dbFunction.Name}({String.Join(",", Arguments.Select(arg => arg.ToString()))}))";
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate([NotNull] ReadOnlyCollection<Expression> arguments)
        {
            Expression exp;

            if ((exp = _dbFunction.Translate(arguments)) != null)
                return exp;

            return null;
        }
    }
}
