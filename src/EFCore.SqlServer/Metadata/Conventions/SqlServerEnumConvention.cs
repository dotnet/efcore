// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that creates check constraint for Enum column in a model.
    /// </summary>
    public class SqlServerEnumConvention : IModelFinalizedConvention
    {
        private IInClauseGenerator[] _inClauseGenerator;

        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerEnumConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public SqlServerEnumConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
            _inClauseGenerator = new IInClauseGenerator[]
                {
                    new InClauseGenerator<sbyte>(),
                    new InClauseGenerator<int>(),
                    new InClauseGenerator<long>(),
                    new InClauseGenerator<short>(),
                    new InClauseGenerator<byte>(),
                    new InClauseGenerator<ulong>(),
                    new InClauseGenerator<uint>(),
                    new InClauseGenerator<ushort>()
                };
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach( var property in entityType.GetDeclaredProperties())
                {
                    if(property?.PropertyInfo?.PropertyType.IsEnum ?? false)
                    {
                        var columnType = (property.FindTypeMapping()?.Converter ?? property.GetValueConverter())?.ProviderClrType;
                        bool isStringColumn = columnType == typeof(string);
                        bool isIntColumn = IsColumnTypeInt(columnType);
                        if(!isStringColumn && !isIntColumn)
                            continue;

                        StringBuilder sql = new StringBuilder($"CHECK ({property.Name} IN(");
                        if(isStringColumn)
                        {
                            var enumNames = property.PropertyInfo.PropertyType.GetEnumNames();
                            if(enumNames.Length <= 0)
                                continue;
                            foreach (var item in enumNames)
                            {
                                sql.Append($"'{item}', ");
                            }
                        }
                        else
                        {
                            var enumValues = Enum.GetValues(property.PropertyInfo.PropertyType);
                            if(enumValues.Length <= 0)
                                continue;
                            var inClause = GenerateInClause(property, columnType);
                            if(string.IsNullOrEmpty(inClause.ToString()))
                                continue;
                            sql.Append(inClause);
                        }
                        sql.Remove(sql.Length - 2, 2);
                        sql.Append("))");
                        string constraintName = $"CK_{entityType.GetTableName()}_{property.GetColumnName()}_Enum_Constraint";
                        entityType.AddCheckConstraint(constraintName, sql.ToString());
                    }
                }
            }
        }
        private StringBuilder GenerateInClause(IConventionProperty property, Type columnType)
        {
            var sql = new StringBuilder();
            foreach (var item in _inClauseGenerator)
            {
                if(item.Type == property.PropertyInfo.PropertyType.GetEnumUnderlyingType())
                {
                    sql = item.GenerateInClause(property);
                }
            }
            return sql;
        }

        private bool IsColumnTypeInt(Type columnType)
        {
            bool isInt = false;
            foreach (var item in _inClauseGenerator)
            {
                if(item.Type == columnType)
                {
                    isInt = true;
                }
            }
            return isInt;
        }

        interface IInClauseGenerator
        {
            Type Type{ get; }
            StringBuilder GenerateInClause(IConventionProperty property);
        }

        class InClauseGenerator<T> : IInClauseGenerator
        {
            public Type Type => typeof(T);

            public StringBuilder GenerateInClause(IConventionProperty property)
            {
                StringBuilder sql = new StringBuilder();
                var enumValues = Enum.GetValues(property.PropertyInfo.PropertyType);
                foreach (T item in enumValues)
                {
                    sql.Append($"{item}, ");
                }
                return sql;
            }
        }
    }
}
