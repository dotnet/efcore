// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     Represents a db function in an <see cref="IModel" />.
    /// </summary>
    public class DbFunction : ConventionalAnnotatable, IDbFunction
    {
        private SortedDictionary<string, DbFunctionParameter> _parameters
            = new SortedDictionary<string, DbFunctionParameter>(StringComparer.OrdinalIgnoreCase);

        private Type _returnType;

        /// <summary>
        ///     The schema where the function lives in the underlying datastore.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; } = String.Empty;

        /// <summary>
        ///     The name of the function in the underlying datastore.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The list of parameters which are passed to the underlying datastores function.
        /// </summary>
        public virtual IReadOnlyList<DbFunctionParameter> Parameters { get { return _parameters.Values.OrderBy(fp => fp.ParameterIndex).ToList(); } }

        /// <summary>
        ///     The .Net method which maps to the function in the underlying datastore
        /// </summary>
        public virtual MethodInfo MethodInfo { get; [param: NotNull] set; }

        /// <summary>
        ///     The return type of the mapped .Net method
        /// </summary>
        public virtual Type ReturnType
        {
            get { return _returnType; }

            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                if (IsValidReturnType(value) == false)
                    throw new ArgumentException(CoreStrings.DbFunctionInvalidReturnType(MethodInfo, value.Name));

                _returnType = value;
            }
        }

        /// <summary>
        ///     <para>
        ///         If set this callback method is executed everytime a dbFunctionExpression is created form this dbFunction.
        ///     </para>
        ///     <returns>
        ///         If this method returns false then the db function is translated into a dbFunctionExpression.
        ///         If this method returns true then the method is executed client side.
        ///     </returns>
        /// </summary>
        public virtual Func<MethodCallExpression, IDbFunction, bool> BeforeDbFunctionExpressionCreateCallback { get; [param: CanBeNull] set; }

        /// <summary>
        ///     If set this callback method is executed after the methodcall is translated to a DbFunctionExpression.
        /// </summary>
        public virtual Action<IDbFunction, DbFunctionExpression> AfterDbFunctionExpressionCreateCallback { get; [param: CanBeNull] set; }

        /// <summary>
        ///     If set this callback is used to translate the .Net method call to a Linq Expression.
        /// </summary>
        public virtual Func<IReadOnlyCollection<Expression>, IDbFunction, Expression> TranslateCallback { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionBuilder Builder { [DebuggerStepThrough] get; [DebuggerStepThrough] [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasTranslateCallback => TranslateCallback != null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbFunction([NotNull] MethodInfo dbFunctionMethodInfo, [NotNull] Model model)
        {
            Check.NotNull(dbFunctionMethodInfo, nameof(dbFunctionMethodInfo));
            Check.NotNull(model, nameof(model));

            if (dbFunctionMethodInfo.IsGenericMethod == true)
                throw new ArgumentException(CoreStrings.DbFunctionGenericMethodNotSupported(dbFunctionMethodInfo));

            Builder = new InternalDbFunctionBuilder(this, model.Builder);

            MethodInfo = dbFunctionMethodInfo;
            ReturnType = MethodInfo.ReturnType;
            Name = dbFunctionMethodInfo.Name;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbFunctionParameter AddParameter([NotNull] DbFunctionParameter parameter)
        {
            Check.NotNull(parameter, nameof(parameter));

            _parameters.Add(parameter.Name, parameter);

            return parameter;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbFunctionParameter AddParameter([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));

            return AddParameter(new DbFunctionParameter(this, name));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbFunctionParameter FindParameter([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));

            DbFunctionParameter item;

            return _parameters.TryGetValue(name, out item) ? item : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void RemoveParameter([NotNull] string name, bool shiftParameters = false)
        {
            Check.NotNull(name, nameof(name));

            if (shiftParameters == true)
            {
                var paramToRemove = FindParameter(name);

                if (paramToRemove == null)
                    return;

                var parameters = Parameters;

                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i].ParameterIndex > paramToRemove.ParameterIndex)
                        Parameters[i].SetParameterIndex(Parameters[i].ParameterIndex - 1, false);
                }
            }

            _parameters.Remove(name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used   
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool BeforeDbFunctionExpressionCreate(MethodCallExpression expression)
            => BeforeDbFunctionExpressionCreateCallback?.Invoke(expression, this) ?? false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used   
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AfterDbFunctionExpressionCreate(DbFunctionExpression dbFuncExpression)
            => AfterDbFunctionExpressionCreateCallback?.Invoke(this, dbFuncExpression);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(IReadOnlyCollection<Expression> arguments)
            => TranslateCallback?.Invoke(arguments, this);

        private bool IsValidReturnType(Type returnType)
        {
            if (returnType.GetTypeInfo().IsGenericType && returnType.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>))
                returnType = Nullable.GetUnderlyingType(returnType);

            return (returnType.GetTypeInfo().IsPrimitive == true
                || returnType == typeof(string)
                || returnType == typeof(DateTime)
                || returnType == typeof(Guid)
                || returnType == typeof(Decimal));
        }
    }
}