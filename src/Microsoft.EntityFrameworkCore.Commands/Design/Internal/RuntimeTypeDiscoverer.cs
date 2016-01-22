// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class RuntimeTypeDiscoverer
    {
        private readonly IModel _model;
        private readonly IStateManager _stateManager;
        private readonly IInternalEntityEntryFactory _entityEntryFactory;

        // TODO also include provider-specific primitive types
        private static readonly Type[] _primitiveTypes =
        {
            typeof(string),
            typeof(short),
            typeof(short?),
            typeof(ushort),
            typeof(ushort?),
            typeof(int),
            typeof(int?),
            typeof(uint),
            typeof(uint?),
            typeof(long),
            typeof(long?),
            typeof(ulong),
            typeof(ulong?),
            typeof(decimal),
            typeof(decimal?),
            typeof(float),
            typeof(float?),
            typeof(double),
            typeof(double?),
            typeof(bool),
            typeof(bool?),
            typeof(char),
            typeof(char?),
            typeof(byte),
            typeof(byte?),
            typeof(TimeSpan),
            typeof(TimeSpan?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?),
            typeof(Guid),
            typeof(Guid?)
        };

        private static readonly Type[] _primitiveTypesCollections;

        static RuntimeTypeDiscoverer()
        {
            _primitiveTypesCollections = _primitiveTypes.Select(t => typeof(ICollection<>).MakeGenericType(t)).ToArray();
        }

        private List<Type> _propertyTypes;
        private List<Type> _navPropTypes;
        private List<Type> _entityTypes;
        private List<Type> _entityTypesCollections;
        private List<Type> _entityTypesGroupings;
        private List<Type> _keyTypes;

        public RuntimeTypeDiscoverer([NotNull] IModel model,
            [NotNull] IStateManager stateManager,
            [NotNull] IInternalEntityEntryFactory entityEntryFactory)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(entityEntryFactory, nameof(entityEntryFactory));

            _model = model;
            _stateManager = stateManager;
            _entityEntryFactory = entityEntryFactory;
        }

        public virtual ICollection<MemberInfo> Discover([CanBeNull] params Assembly[] assemblies)
        {
            var members = new List<MemberInfo>();

            var keys = (from e in _model.GetEntityTypes()
                        from key in e.GetKeys()
                        select key).ToList();

            members.AddRange(keys.Select(GetIdentityMapGetType));

            _keyTypes = (from key in keys
                         select key.Properties.Count > 1 ? typeof(object[]) : key.Properties.First().ClrType).Distinct().ToList();

            _entityTypes = (from e in _model.GetEntityTypes()
                            where e.HasClrType()
                            select e.ClrType).ToList();

            _entityTypesCollections = (from t in _entityTypes
                                       from collection in new[]
                                       {
                                           typeof(IEnumerable<>),
                                           typeof(IAsyncEnumerable<>),
                                           typeof(IOrderedEnumerable<>),
                                           typeof(ICollection<>),
                                           typeof(IList<>),
                                           typeof(HashSet<>),
                                           typeof(List<>)
                                       }
                                       select collection.MakeGenericType(t)).ToList();

            _propertyTypes = (from e in _model.GetEntityTypes()
                              from p in e.GetProperties()
                              where p.ClrType != null
                              select p.ClrType).Distinct().ToList();

            _entityTypesGroupings = (from t in _entityTypes
                                     from k in _keyTypes
                                     from collection in new[]
                                     {
                                         typeof(IAsyncGrouping<,>),
                                         typeof(IGrouping<,>)
                                     }
                                     select collection.MakeGenericType(k, t)).ToList();

            _navPropTypes = (from e in _model.GetEntityTypes()
                             from p in e.GetNavigations()
                             where p.DeclaringEntityType?.HasClrType() != null
                             select p.DeclaringEntityType.ClrType).Distinct().ToList();

            if (assemblies != null)
            {
                var genericMethods = from assembly in assemblies
                                     from t in assembly.DefinedTypes
                                     from m in t.DeclaredMethods
                                     where m.GetCustomAttributes<CallsMakeGenericMethodAttribute>().Any()
                                     from boundMethod in MakeGenericMethods(m)
                                     select boundMethod;

                members.AddRange(genericMethods);
            }

            members.AddRange(_entityTypes.Select(t => t.GetTypeInfo()));
            members.AddRange(_entityTypesCollections.Select(t => t.GetTypeInfo()));

            members.AddRange(from entityType in _model.GetEntityTypes()
                             where entityType is EntityType
                             where entityType.HasClrType()
                             from type in GetRuntimeTypes(entityType)
                             select type.GetTypeInfo());

            return members;
        }

        private IEnumerable<MethodInfo> MakeGenericMethods(MethodInfo declaringMethod)
        {
            var usages = declaringMethod.GetCustomAttributes<CallsMakeGenericMethodAttribute>();
            foreach (var usage in usages)
            {
                var methodDefinition = usage.FindMethodInfo(declaringMethod);

                if (methodDefinition == null)
                {
                    throw new InvalidOperationException(CommandsStrings.InvalidCallsMakeGenericMethodAttribute(declaringMethod.DeclaringType.FullName + "." + declaringMethod.Name));
                }

                foreach (var typeArgs in BindTypeArguments(new ArraySegment<Type>(usage.TypeArguments)).Distinct())
                {
                    MethodInfo boundMethod;
                    try
                    {
                        boundMethod = methodDefinition.MakeGenericMethod(typeArgs);
                    }
                    catch (ArgumentException)
                    {
                        // ignores type constraints violations
                        continue;
                    }
                    yield return boundMethod;
                }
            }
        }

        private IEnumerable<Type[]> BindTypeArguments(ArraySegment<Type> typeArguments)
        {
            var unboundArguments = typeArguments.Count > 1
                ? new ArraySegment<Type>(typeArguments.Array, typeArguments.Offset + 1, typeArguments.Count - 1)
                : default(ArraySegment<Type>);

            var nextUnbound = typeArguments.Array[typeArguments.Offset];

            ICollection<Type> typeParameters;
            if (nextUnbound == typeof(TypeArgumentCategory.EntityTypes))
            {
                typeParameters = _entityTypes;
            }
            else if (nextUnbound == typeof(TypeArgumentCategory.EntityTypeCollections))
            {
                typeParameters = _entityTypesCollections;
            }
            else if (nextUnbound == typeof(TypeArgumentCategory.EntityTypeGroupings))
            {
                typeParameters = _entityTypesGroupings;
            }
            else if (nextUnbound == typeof(TypeArgumentCategory.Keys))
            {
                typeParameters = _keyTypes;
            }
            else if (nextUnbound == typeof(TypeArgumentCategory.Properties))
            {
                typeParameters = _propertyTypes;
            }
            else if (nextUnbound == typeof(TypeArgumentCategory.NavigationProperties))
            {
                typeParameters = _navPropTypes;
            }
            else if (nextUnbound == typeof(TypeArgumentCategory.Primitives))
            {
                typeParameters = _primitiveTypes;
            }
            else if (nextUnbound == typeof(TypeArgumentCategory.PrimitiveCollections))
            {
                typeParameters = _primitiveTypesCollections;
            }
            else
            {
                typeParameters = new[] { nextUnbound };
            }

            foreach (var typeParameter in typeParameters)
            {
                var bind = new[] { typeParameter };
                if (unboundArguments.Count == 0)
                {
                    yield return bind;
                }
                else
                {
                    foreach (var boundArguments in BindTypeArguments(unboundArguments))
                    {
                        yield return bind.Concat(boundArguments).ToArray();
                    }
                }
            }
        }

        private TypeInfo GetIdentityMapGetType(IKey key)
            => key.GetWeakReferenceIdentityMapFactory().Invoke().GetType().GetTypeInfo();

        private IEnumerable<Type> GetRuntimeTypes(IEntityType entityType)
        {
            // TODO reuse materializer logic or avoid creating instance at all
            var instance = Activator.CreateInstance(entityType.ClrType);
            var entry = _entityEntryFactory.Create(_stateManager, entityType, instance);

            var concreteEntityType = entityType as EntityType;
            if (concreteEntityType != null)
            {
                yield return concreteEntityType.OriginalValuesFactory(entry).GetType();
                yield return concreteEntityType.RelationshipSnapshotFactory(entry).GetType();
            }

            yield return entityType.GetEmptyShadowValuesFactory()().GetType();

            foreach (var prop in entityType.GetProperties().Where(i => !i.IsShadowProperty))
            {
                yield return prop.GetGetter().GetType();
                yield return prop.GetSetter().GetType();
            }
        }
    }
}
