// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Design.Internal
{
    public class RuntimeTypeDiscoverer
    {
        private readonly IModel _model;
        private readonly IStateManager _stateManager;
        private readonly IInternalEntityEntryFactory _entityEntryFactory;

        private static readonly Type[] _primitiveTypes =
        {
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(decimal),
            typeof(float),
            typeof(double),
            typeof(bool),
            typeof(string),
            typeof(char),
            typeof(byte),
            typeof(TimeSpan),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(Guid)
        };

        private static readonly Type[] _emptyTypeArray = new Type[0];

        private List<Type> _propertyTypes;
        private List<Type> _navPropTypes;
        private List<Type> _entityTypes;
        private List<Type> _entityTypesCollections;
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
            _entityTypes = (from e in _model.GetEntityTypes()
                            where e.HasClrType()
                            select e.ClrType).ToList();

            _entityTypesCollections = (from e in _model.GetEntityTypes()
                                       where e.HasClrType()
                                       from collection in new[] { typeof(IEnumerable<>), typeof(IAsyncEnumerable<>), typeof(IOrderedEnumerable<>) }
                                       select collection.MakeGenericType(e.ClrType)).ToList();

            _keyTypes = (from e in _model.GetEntityTypes()
                         from key in e.GetKeys()
                         select key.Properties.Count > 1 ? typeof(object[]) : key.Properties.First().ClrType).Distinct().ToList();

            _propertyTypes = (from e in _model.GetEntityTypes()
                              from p in e.GetProperties()
                              where p.ClrType != null
                              select p.ClrType).Distinct().ToList();

            _navPropTypes = (from e in _model.GetEntityTypes()
                             from p in e.GetNavigations()
                             where p.DeclaringEntityType?.HasClrType() != null
                             select p.DeclaringEntityType.ClrType).Distinct().ToList();

            var members = new List<MemberInfo>();

            if (assemblies != null)
            {
                var methods = from assembly in assemblies
                              from t in assembly.DefinedTypes
                              from m in t.DeclaredMethods
                              where m.GetCustomAttributes<GenericMethodFactory>().Any()
                              from boundMethod in MakeGenericMethods(m)
                              select boundMethod;

                members.AddRange(methods);
            }

            members.AddRange(_entityTypes.Select(t => t.GetTypeInfo()));
            members.AddRange(_entityTypesCollections.Select(t => t.GetTypeInfo()));

            foreach (var entityType in _model.GetEntityTypes()
                .Where(e => e.HasClrType())
                .OfType<EntityType>())
            {
                members.AddRange(GetKeyTypes(entityType));
                members.AddRange(GetSnapshotTypes(entityType));
                members.AddRange(GetPropertyTypes(entityType));
            }

            return members;
        }

        private IEnumerable<MethodInfo> MakeGenericMethods(MethodInfo declaringMethod)
        {
            var usages = declaringMethod.GetCustomAttributes<GenericMethodFactory>();
            foreach (var usage in usages)
            {
                var targetMethod = string.IsNullOrEmpty(usage.MethodName)
                    ? declaringMethod
                    : (usage.TargetType ?? declaringMethod.DeclaringType)
                        .GetTypeInfo()
                        .GetDeclaredMethods(usage.MethodName)
                        .FirstOrDefault(m => m.GetGenericArguments().Length == usage.TypeArguments.Length);

                Debug.Assert(targetMethod != null,
                    "Could not find target method of " + nameof(GenericMethodFactory) +
                    $" on {declaringMethod.DeclaringType?.FullName}.{declaringMethod.Name}. Check the type arguments align");

                foreach (var typeArgs in BindTypeArguments(new ArraySegment<Type>(usage.TypeArguments)).Distinct())
                {
                    yield return targetMethod.MakeGenericMethod(typeArgs);
                }
            }
        }

        private IEnumerable<Type[]> BindTypeArguments(ArraySegment<Type> typeArguments)
        {
            if (typeArguments.Count == 0)
            {
                yield return _emptyTypeArray;
                yield break;
            }

            var unboundArguments = typeArguments.Count > 1 ? new ArraySegment<Type>(typeArguments.Array, 1, typeArguments.Count - 1) : default(ArraySegment<Type>);

            ICollection<Type> typeParameters;
            if (typeArguments.Array[typeArguments.Offset] == typeof(TypeArgumentCategory.EntityTypes))
            {
                typeParameters = _entityTypes;
            }
            else if (typeArguments.Array[typeArguments.Offset] == typeof(TypeArgumentCategory.EntityTypeCollections))
            {
                typeParameters = _entityTypesCollections;
            }
            else if (typeArguments.Array[typeArguments.Offset] == typeof(TypeArgumentCategory.Keys))
            {
                typeParameters = _keyTypes;
            }
            else if (typeArguments.Array[typeArguments.Offset] == typeof(TypeArgumentCategory.Properties))
            {
                typeParameters = _propertyTypes;
            }
            else if (typeArguments.Array[typeArguments.Offset] == typeof(TypeArgumentCategory.NavigationProperties))
            {
                typeParameters = _navPropTypes;
            }
            else if (typeArguments.Array[typeArguments.Offset] == typeof(TypeArgumentCategory.Primitives))
            {
                typeParameters = _primitiveTypes;
            }
            else
            {
                typeParameters = new[] { typeArguments.Array[typeArguments.Offset] };
            }

            foreach (var typeParameter in typeParameters)
            {
                foreach (var boundArguments in BindTypeArguments(unboundArguments))
                {
                    yield return new[] { typeParameter }.Concat(boundArguments).ToArray();
                }
            }
        }

        private IEnumerable<TypeInfo> GetKeyTypes(EntityType entityType)
        {
            foreach (var key in entityType.GetKeys())
            {
                yield return key.GetWeakReferenceIdentityMapFactory().Invoke().GetType().GetTypeInfo();
            }
        }

        private IEnumerable<TypeInfo> GetSnapshotTypes(EntityType entityType)
        {
            // TODO reuse materializer logic or avoid creating instance at all
            var instance = Activator.CreateInstance(entityType.ClrType);
            var entry = _entityEntryFactory.Create(_stateManager, entityType, instance);

            yield return entityType.OriginalValuesFactory(entry).GetType().GetTypeInfo();
            yield return entityType.RelationshipSnapshotFactory(entry).GetType().GetTypeInfo();
            yield return entityType.GetEmptyShadowValuesFactory()().GetType().GetTypeInfo();
        }

        private IEnumerable<TypeInfo> GetPropertyTypes(EntityType entityType)
        {
            foreach (var prop in entityType.GetProperties())
            {
                prop.GetPropertyAccessors();

                if (prop.IsShadowProperty)
                {
                    continue;
                }

                yield return prop.GetGetter().GetType().GetTypeInfo();
                yield return prop.GetSetter().GetType().GetTypeInfo();
            }
        }
    }
}
