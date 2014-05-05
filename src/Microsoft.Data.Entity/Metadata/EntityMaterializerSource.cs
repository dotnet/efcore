// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class EntityMaterializerSource
    {
        private static readonly MethodInfo _readValue
            = typeof(IValueReader).GetTypeInfo().GetDeclaredMethods("ReadValue").Single();

        private static readonly MethodInfo _isNull
            = typeof(IValueReader).GetTypeInfo().GetDeclaredMethods("IsNull").Single();

        private readonly ThreadSafeDictionaryCache<Type, Func<IValueReader, object>> _cache
            = new ThreadSafeDictionaryCache<Type, Func<IValueReader, object>>();

        private readonly MemberMapper _memberMapper;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected EntityMaterializerSource()
        {
        }

        public EntityMaterializerSource([NotNull] MemberMapper memberMapper)
        {
            Check.NotNull(memberMapper, "memberMapper");

            _memberMapper = memberMapper;
        }

        public virtual Func<IValueReader, object> GetMaterializer([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            var materializer = entityType as IEntityMaterializer;
            if (materializer != null)
            {
                return materializer.CreatEntity;
            }

            if (!entityType.HasClrType)
            {
                throw new InvalidOperationException(Strings.FormatNoClrType(entityType.Name));
            }

            return _cache.GetOrAdd(entityType.Type, k => BuildDelegate(entityType));
        }

        private Func<IValueReader, object> BuildDelegate(IEntityType entityType)
        {
            var memberMappings = _memberMapper.MapPropertiesToMembers(entityType);
            var clrType = entityType.Type;

            var readerParameter = Expression.Parameter(typeof(IValueReader), "valueReader");
            var instanceVariable = Expression.Variable(clrType, "instance");

            var blockExpressions = new List<Expression>
                {
                    Expression.Assign(instanceVariable, Expression.New(clrType.GetDeclaredConstructor(null)))
                };

            foreach (var mapping in memberMappings)
            {
                var propertyInfo = mapping.Item2 as PropertyInfo;
                var fieldInfo = mapping.Item2 as FieldInfo;

                var targetType = propertyInfo != null
                    ? propertyInfo.PropertyType
                    : fieldInfo.FieldType;

                var indexExpression = Expression.Constant(mapping.Item1.Index);

                Expression callReaderExpression = Expression.Call(
                    readerParameter,
                    _readValue.MakeGenericMethod(targetType),
                    indexExpression);

                if (targetType.IsNullableType())
                {
                    callReaderExpression = Expression.Condition(
                        Expression.Call(readerParameter, _isNull, indexExpression),
                        Expression.Constant(null, targetType),
                        callReaderExpression);
                }

                if (propertyInfo != null)
                {
                    blockExpressions.Add(
                        Expression.Assign(Expression.Property(instanceVariable, propertyInfo),
                            callReaderExpression));
                }
                else
                {
                    blockExpressions.Add(
                        Expression.Assign(Expression.Field(instanceVariable, fieldInfo),
                            callReaderExpression));
                }
            }

            blockExpressions.Add(instanceVariable);

            return Expression.Lambda<Func<IValueReader, object>>(Expression.Block(new[] { instanceVariable }, blockExpressions), readerParameter).Compile();
        }
    }
}
