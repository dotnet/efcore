using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Update
{
    public static class MongoDbUpdateEntryExtensions
    {
        public static WriteModel<TEntity> ToMongoDbWriteModel<TEntity>([NotNull] this IUpdateEntry updateEntry)
        {
            Check.NotNull(updateEntry, nameof(updateEntry));
            if (!typeof(TEntity).GetTypeInfo().IsAssignableFrom(updateEntry.EntityType.ClrType))
            {
                throw new InvalidOperationException($"Entity must derive from {nameof(TEntity)}.");
            }
            if (updateEntry.EntityState != EntityState.Added && 
                updateEntry.EntityState != EntityState.Modified &&
                updateEntry.EntityState != EntityState.Deleted)
            {
                throw new InvalidOperationException($"Entity state must be Added, Modified, or Deleted.");
            }

            WriteModel<TEntity> writeModel;
            switch (updateEntry.EntityState)
            {
                case EntityState.Added:
                    writeModel = ToInsertModel<TEntity>(updateEntry as InternalEntityEntry);
                    break;
                case EntityState.Modified:
                    writeModel = new ReplaceOneModel<TEntity>(
                        GetIdFilter<TEntity>(updateEntry),
                        (TEntity)updateEntry.ToEntityEntry().Entity);
                    break;
                default:
                    writeModel = new DeleteOneModel<TEntity>(GetIdFilter<TEntity>(updateEntry));
                    break;
            }
            return writeModel;
        }

        private static InsertOneModel<TEntity> ToInsertModel<TEntity>(InternalEntityEntry updateEntry)
        {
            IEnumerable<IProperty> temporaryProperties = updateEntry.EntityType
                .GetProperties()
                .Where(updateEntry.HasTemporaryValue);
            foreach (IProperty property in temporaryProperties)
            {
                //for some reason, DiscardStoreGeneratedValues doesn't appear to work correctly, so we have to
                //forcibly clear the property and tell the StateManager that the property no longer has a value
                updateEntry.SetOriginalValue(property, property.ClrType.GetDefaultValue());
                updateEntry.SetPropertyModified(property, changeState: false, isModified: false);
            }
            return new InsertOneModel<TEntity>((TEntity)updateEntry.Entity);
        }

        private static FilterDefinition<TEntity> GetIdFilter<TEntity>(IUpdateEntry updateEntry)
        {
            IList<FilterDefinition<TEntity>> filterDefinitions = updateEntry.EntityType
                .FindPrimaryKey()
                .Properties
                .Select(property => GetPropertyFilterDefinition<TEntity>(property, updateEntry.GetCurrentValue(property)))
                .DefaultIfEmpty(Builders<TEntity>.Filter.Empty)
                .ToList();
            return filterDefinitions.Count > 1
                ? Builders<TEntity>.Filter.And(filterDefinitions)
                : filterDefinitions[index: 0];
        }

        private static FilterDefinition<TEntity> GetPropertyFilterDefinition<TEntity>(IPropertyBase property, object propertyValue)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), name: "entity");
            LambdaExpression lambdaExpression = Expression.Lambda(
                Expression.MakeMemberAccess(parameterExpression, property.PropertyInfo),
                parameterExpression);
            return (FilterDefinition<TEntity>)typeof(FilterDefinitionBuilder<TEntity>)
                .GetTypeInfo()
                .GetDeclaredMethods(nameof(FilterDefinitionBuilder<TEntity>.Eq))
                .First(methodInfo => methodInfo.GetParameters().Length == 2 && 
                    methodInfo.GetParameters()[0].ParameterType.GetTypeInfo().IsGenericType &&
                    methodInfo.GetParameters()[0].ParameterType.GetTypeInfo().GetGenericTypeDefinition() == typeof(Expression<>))
                .GetGenericMethodDefinition()
                .MakeGenericMethod(property.ClrType)
                .Invoke(Builders<TEntity>.Filter, new[] { lambdaExpression, propertyValue });
        }
    }
}