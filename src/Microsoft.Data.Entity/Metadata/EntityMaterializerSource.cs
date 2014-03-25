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

        public virtual Func<object[], object> GetMaterializer([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            var materializer = entityType as IEntityMaterializer;
            if (materializer != null)
            {
                return materializer.CreatEntity;
            }

            return _cache.GetOrAdd(entityType.Type, k => BuildDelegate(entityType));
        }

        private static Func<object[], object> BuildDelegate(IEntityType entityType)
        {
            // TODO: assumes no inheritance hierarchy for entity

            var fields = FindFields(entityType);
            var clrType = entityType.Type;

            var bufferParameter = Expression.Parameter(typeof(object[]), "valueBuffer");
            var instanceVariable = Expression.Variable(clrType, "instance");

            var blockExpressions = new List<Expression>();
            blockExpressions.Add(Expression.Assign(instanceVariable, Expression.New(clrType.GetDeclaredConstructor(null))));

            foreach (var field in fields)
            {
                blockExpressions.Add(
                    Expression.Assign(Expression.Field(instanceVariable, field.Item2),
                        Expression.Convert(
                            Expression.Call(
                                _convert,
                                Expression.ArrayAccess(bufferParameter, Expression.Constant(field.Item1.Index))),
                            field.Item2.FieldType)));
            }

            blockExpressions.Add(instanceVariable);

            return Expression.Lambda<Func<object[], object>>(Expression.Block(new[] { instanceVariable }, blockExpressions), bufferParameter).Compile();
        }

        private static IEnumerable<Tuple<IProperty, FieldInfo>> FindFields(IEntityType entityType)
        {
            // TODO: This currently assumes auto-properties with current compiler naming
            // TODO: Need better way to find field name and/or annotation in model for field name
            // TODO: Also assumes no inheritance hierarchy for entity

            var allFields = entityType.Type.GetRuntimeFields().ToArray();
            return entityType.Properties
                .Where(p => p.IsClrProperty)
                .Select(p => Tuple.Create(p, allFields.Single(f => f.Name == "<" + p.Name + ">k__BackingField")));
        }

        // TODO: This is a temporary workaround for conveting DBNull into null
        // TODO: It is probably just an example of type conversions such that it can be handled more generally
        private static object Convert(object value)
        {
            return value is DBNull ? null : value;
        }
    }
}
