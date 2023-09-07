// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

public class JsonTypesCustomMappingSqlServerTest : JsonTypesSqlServerTestBase
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => base.OnConfiguring(optionsBuilder.ReplaceService<IRelationalTypeMappingSource, TestSqlServerTypeMappingSource>());

    private class TestSqlServerTypeMappingSource(
            TypeMappingSourceDependencies dependencies,
            RelationalTypeMappingSourceDependencies relationalDependencies)
        : SqlServerTypeMappingSource(dependencies, relationalDependencies)
    {
        protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var mapping = base.FindMapping(in mappingInfo);

            if ((mapping == null
                    || (mappingInfo.CoreTypeMappingInfo.ElementTypeMapping != null
                        && mapping.ElementTypeMapping == null))
                && mappingInfo.ClrType != null
                && mappingInfo.ClrType != typeof(string))
            {
                var elementClrType = TryGetElementType(mappingInfo.ClrType, typeof(IEnumerable<>))!;

                mapping = CustomFindCollectionMapping(
                    mappingInfo, mappingInfo.ClrType,
                    null,
                    mappingInfo.CoreTypeMappingInfo.ElementTypeMapping ?? FindMapping(elementClrType));
            }

            return mapping;
        }

        protected override RelationalTypeMapping? FindCollectionMapping(
            RelationalTypeMappingInfo info,
            Type modelType,
            Type? providerType,
            CoreTypeMapping? elementMapping)
            => null;

        private RelationalTypeMapping? CustomFindCollectionMapping(
            RelationalTypeMappingInfo info,
            Type modelType,
            Type? providerType,
            CoreTypeMapping? elementMapping)
        {
            if (TryFindJsonCollectionMapping(
                    info.CoreTypeMappingInfo, modelType, providerType, ref elementMapping, out var collectionReaderWriter))
            {
                var elementType = TryGetElementType(modelType, typeof(IEnumerable<>))!;
                var comparer = (ValueComparer?)Activator.CreateInstance(
                    IsNullableValueType(elementType)
                        ? typeof(NullableValueTypeListComparer<>).MakeGenericType(UnwrapNullableType(elementType))
                        : typeof(ListComparer<>).MakeGenericType(elementMapping!.Comparer.Type),
                    elementMapping!.Comparer);

                return (RelationalTypeMapping)FindMapping(
                        info.WithConverter(
                            // Note that the converter info is only used temporarily here and never creates an instance.
                            new ValueConverterInfo(modelType, typeof(string), _ => null!)))!
                    .Clone(
                        (ValueConverter)Activator.CreateInstance(
                            typeof(CollectionToJsonStringConverter<>).MakeGenericType(elementType), collectionReaderWriter!)!,
                        comparer,
                        comparer,
                        elementMapping,
                        collectionReaderWriter);
            }

            return null;
        }

        private static Type? TryGetElementType(Type type, Type interfaceOrBaseType)
        {
            if (type.IsGenericTypeDefinition)
            {
                return null;
            }

            var types = GetGenericTypeImplementations(type, interfaceOrBaseType);

            Type? singleImplementation = null;
            foreach (var implementation in types)
            {
                if (singleImplementation == null)
                {
                    singleImplementation = implementation;
                }
                else
                {
                    singleImplementation = null;
                    break;
                }
            }

            return singleImplementation?.GenericTypeArguments.FirstOrDefault();
        }

        private static IEnumerable<Type> GetGenericTypeImplementations(Type type, Type interfaceOrBaseType)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericTypeDefinition)
            {
                var baseTypes = interfaceOrBaseType.GetTypeInfo().IsInterface
                    ? typeInfo.ImplementedInterfaces
                    : GetBaseTypes(type);
                foreach (var baseType in baseTypes)
                {
                    if (baseType.IsGenericType
                        && baseType.GetGenericTypeDefinition() == interfaceOrBaseType)
                    {
                        yield return baseType;
                    }
                }

                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == interfaceOrBaseType)
                {
                    yield return type;
                }
            }
        }

        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            var currentType = type.BaseType;

            while (currentType != null)
            {
                yield return currentType;

                currentType = currentType.BaseType;
            }
        }

        private static Type UnwrapNullableType(Type type)
            => Nullable.GetUnderlyingType(type) ?? type;

        private static bool IsNullableValueType(Type type)
            => type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

    }
}
