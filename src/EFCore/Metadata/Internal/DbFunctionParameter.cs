// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DbFunctionParameter : ConventionalAnnotatable, IDbFunctionParameter
    {
        private object _value;
        private bool _isObjectParameter = false;
        private Type _parameterType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbFunctionParameter([NotNull] DbFunction parentFunction, [NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(parentFunction, nameof(parentFunction));

            Name = name;
            Parent = parentFunction;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbFunction Parent { get; [param: NotNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type ParameterType { get { return _parameterType; } [param: NotNull] set { SetParameterType(value); } }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int ParameterIndex { get; [param: NotNull] private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object Value
        {
            get { return _value; }

            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _value = value;
                ParameterType = value.GetType();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsObjectParameter
        {
            get { return _isObjectParameter; }

            [param: NotNull]
            set
            {
                _isObjectParameter = value;
                ParameterType = Parent.MethodInfo.DeclaringType;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsIdentifier { get; [param: NotNull] set; } = false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsParams { get; [param: NotNull] set; } = false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private void SetParameterType(Type parameterType)
        {
            bool isValidType = IsValidParameterType(parameterType);

            if (isValidType == false && IsIdentifier == false && IsObjectParameter == false)
                throw new ArgumentException(CoreStrings.DbFunctionInvalidParameterType(Parent.MethodInfo, Name, parameterType.Name));

            if (isValidType == false && IsObjectParameter == true && Parent.TranslateCallback == null)
                throw new ArgumentException(CoreStrings.DbFunctionInvalidTypeObjectParamMissingTranslate(Parent.MethodInfo, Name, parameterType.Name));

            _parameterType = parameterType;
        }

        private bool IsValidParameterType(Type parameterType)
        {
            Type typeToCheck;

            typeToCheck = parameterType.IsArray ? parameterType.GetElementType() : parameterType;

            if (typeToCheck.GetTypeInfo().IsGenericType && typeToCheck.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>))
                typeToCheck = Nullable.GetUnderlyingType(typeToCheck);

            return (typeToCheck.GetTypeInfo().IsPrimitive == true
                || typeToCheck == typeof(string)
                || typeToCheck == typeof(DateTime)
                || typeToCheck == typeof(object)
                || typeToCheck == typeof(Guid)
                || typeToCheck == typeof(Decimal)
                || typeToCheck.GetTypeInfo().IsEnum == true);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetParameterIndex(int index, bool insert = false)
        {
            if (insert == true)
            {
                ParameterIndex = -1;

                var parameters = Parent.Parameters;

                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i].ParameterIndex >= index)
                        parameters[i].ParameterIndex += 1;
                }
            }

            ParameterIndex = index;
        }
    }
}
