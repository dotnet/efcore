// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        private static readonly MethodInfo _convert
            = typeof(EntityMaterializerSource).GetTypeInfo().GetDeclaredMethods("Convert").Single();

        private readonly ThreadSafeDictionaryCache<Type, Func<object[], object>> _cache
            = new ThreadSafeDictionaryCache<Type, Func<object[], object>>();

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

        public virtual Func<object[], object> GetMaterializer([NotNull] IEntityType entityType)
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

        private Func<object[], object> BuildDelegate(IEntityType entityType)
        {
            var memberMappings = _memberMapper.MapPropertiesToMembers(entityType);
            var clrType = entityType.Type;

            var bufferParameter = Expression.Parameter(typeof(object[]), "valueBuffer");
            var instanceVariable = Expression.Variable(clrType, "instance");

            var blockExpressions = new List<Expression>
                {
                    Expression.Assign(instanceVariable, Expression.New(clrType.GetDeclaredConstructor(null)))
                };

            foreach (var mapping in memberMappings)
            {
                var callConvertExpression = Expression.Call(
                    _convert,
                    Expression.ArrayAccess(bufferParameter, Expression.Constant(mapping.Item1.Index)));

                var propertyInfo = mapping.Item2 as PropertyInfo;
                if (propertyInfo != null)
                {
                    blockExpressions.Add(
                        Expression.Assign(Expression.Property(instanceVariable, propertyInfo),
                            Expression.Convert(
                                callConvertExpression,
                                propertyInfo.PropertyType)));
                }
                else
                {
                    var fieldInfo = (FieldInfo)mapping.Item2;

                    blockExpressions.Add(
                        Expression.Assign(Expression.Field(instanceVariable, fieldInfo),
                            Expression.Convert(
                                callConvertExpression,
                                fieldInfo.FieldType)));
                }
            }

            blockExpressions.Add(instanceVariable);

            return Expression.Lambda<Func<object[], object>>(Expression.Block(new[] { instanceVariable }, blockExpressions), bufferParameter).Compile();
        }

        // TODO: This is a temporary workaround for conveting DBNull into null
        // TODO: It is probably just an example of type conversions such that it can be handled more generally
        private static object Convert(object value)
        {
            return value is DBNull ? null : value;
        }
    }
}
