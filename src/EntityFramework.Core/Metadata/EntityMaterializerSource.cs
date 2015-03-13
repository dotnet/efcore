// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class EntityMaterializerSource : IEntityMaterializerSource
    {
        private static readonly MethodInfo _readValue
            = typeof(IValueReader).GetTypeInfo().GetDeclaredMethods("ReadValue").Single();

        private static readonly MethodInfo _isNull
            = typeof(IValueReader).GetTypeInfo().GetDeclaredMethods("IsNull").Single();

        private readonly IMemberMapper _memberMapper;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected EntityMaterializerSource()
        {
        }

        public EntityMaterializerSource([NotNull] IMemberMapper memberMapper)
        {
            Check.NotNull(memberMapper, nameof(memberMapper));

            _memberMapper = memberMapper;
        }

        public virtual Expression CreateReadValueExpression(Expression valueReader, Type type, int index)
        {
            Check.NotNull(valueReader, nameof(valueReader));
            Check.NotNull(type, nameof(type));

            var unwrappedTargetMemberType = type.UnwrapNullableType();
            var underlyingTargetMemberType = unwrappedTargetMemberType.UnwrapEnumType();
            var indexExpression = Expression.Constant(index);

            Expression readValueExpression
                = Expression.Call(
                    valueReader,
                    _readValue.MakeGenericMethod(underlyingTargetMemberType),
                    indexExpression);

            if (underlyingTargetMemberType != type)
            {
                readValueExpression
                    = Expression.Convert(readValueExpression, type);
            }

            if (type.IsNullableType())
            {
                readValueExpression
                    = Expression.Condition(
                        Expression.Call(valueReader, _isNull, indexExpression),
                        Expression.Constant(null, type),
                        readValueExpression);
            }

            return readValueExpression;
        }

        public virtual Expression CreateMaterializeExpression(
            IEntityType entityType,
            Expression valueReaderExpression,
            int[] indexMap = null)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(valueReaderExpression, nameof(valueReaderExpression));

            var materializer = entityType as IEntityMaterializer;

            if (materializer != null)
            {
                return Expression.Call(
                    Expression.Constant(materializer),
                    ((Func<IValueReader, object>)materializer.CreateEntity).GetMethodInfo(),
                    valueReaderExpression);
            }

            if (!entityType.HasClrType)
            {
                throw new InvalidOperationException(Strings.NoClrType(entityType.Name));
            }

            if (entityType.IsAbstract)
            {
                throw new InvalidOperationException(Strings.CannotMaterializeAbstractType(entityType));
            }

            var instanceVariable = Expression.Variable(entityType.Type, "instance");

            var blockExpressions
                = new List<Expression>
                    {
                        Expression.Assign(
                            instanceVariable,
                            Expression.New(entityType.Type.GetDeclaredConstructor(null)))
                    };

            blockExpressions.AddRange(
                from mapping in _memberMapper.MapPropertiesToMembers(entityType)
                let propertyInfo = mapping.Item2 as PropertyInfo
                let targetMember
                    = propertyInfo != null
                        ? Expression.Property(instanceVariable, propertyInfo)
                        : Expression.Field(instanceVariable, (FieldInfo)mapping.Item2)
                select
                    Expression.Assign(
                        targetMember,
                        CreateReadValueExpression(
                            valueReaderExpression,
                            targetMember.Type,
                            indexMap?[mapping.Item1.Index] ?? mapping.Item1.Index)));

            blockExpressions.Add(instanceVariable);

            return Expression.Block(new[] { instanceVariable }, blockExpressions);
        }
    }
}
