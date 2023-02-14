using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage
{
    internal class SqlServerHierarchyIdTypeMapping : RelationalTypeMapping
    {
        private static readonly MethodInfo _getSqlBytes
            = typeof(SqlDataReader).GetRuntimeMethod(nameof(SqlDataReader.GetSqlBytes), new[] { typeof(int) });

        private static readonly MethodInfo _parseHierarchyId
            = typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.Parse), new[] { typeof(string) });

        private static readonly SqlServerHierarchyIdValueConverter _valueConverter = new SqlServerHierarchyIdValueConverter();

        private static Action<DbParameter, SqlDbType> _sqlDbTypeSetter;
        private static Action<DbParameter, string> _udtTypeNameSetter;

        public SqlServerHierarchyIdTypeMapping(string storeType, Type clrType)
            : base(CreateRelationalTypeMappingParameters(storeType, clrType))
        {
        }

        private static RelationalTypeMappingParameters CreateRelationalTypeMappingParameters(string storeType, Type clrType)
        {
            return new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    clrType: clrType,
                    converter: null //this gets the generatecodeliteral to run
                ),
                storeType);
        }

        // needed to implement Clone
        protected SqlServerHierarchyIdTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        {
            return new SqlServerHierarchyIdTypeMapping(parameters);
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            var type = parameter.GetType();
            LazyInitializer.EnsureInitialized(ref _sqlDbTypeSetter, () => CreateSqlDbTypeAccessor(type));
            LazyInitializer.EnsureInitialized(ref _udtTypeNameSetter, () => CreateUdtTypeNameAccessor(type));

            if (parameter.Value == DBNull.Value)
            {
                parameter.Value = SqlBytes.Null;
            }

            _sqlDbTypeSetter(parameter, SqlDbType.Udt);
            _udtTypeNameSetter(parameter, StoreType);
        }

        public override MethodInfo GetDataReaderMethod()
        {
            return _getSqlBytes;
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            return Expression.Call(
                _parseHierarchyId,
                Expression.Constant(value.ToString(), typeof(string))
            );
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            //this appears to only be called when using the update-database
            //command, and the value is already a hierarchyid
            return $"'{value}'";
        }

        public override DbParameter CreateParameter(DbCommand command, string name, object value, bool? nullable = null, ParameterDirection direction = ParameterDirection.Input)
        {
            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = name;

            if (Converter != null)
            {
                value = Converter.ConvertToProvider(value);
            }

            parameter.Value = value is null
                ? DBNull.Value
                : _valueConverter.ConvertToProvider(value);

            if (nullable.HasValue)
            {
                parameter.IsNullable = nullable.Value;
            }

            ConfigureParameter(parameter);

            return parameter;
        }

        public override Expression CustomizeDataReaderExpression(Expression expression)
        {
            if (expression.Type != _valueConverter.ProviderClrType)
            {
                expression = Expression.Convert(expression, _valueConverter.ProviderClrType);
            }

            return ReplacingExpressionVisitor.Replace(
                _valueConverter.ConvertFromProviderExpression.Parameters.Single(),
                expression,
                _valueConverter.ConvertFromProviderExpression.Body);
        }

        private static Action<DbParameter, SqlDbType> CreateSqlDbTypeAccessor(Type paramType)
        {
            var paramParam = Expression.Parameter(typeof(DbParameter), "parameter");
            var valueParam = Expression.Parameter(typeof(SqlDbType), "value");

            return Expression.Lambda<Action<DbParameter, SqlDbType>>(
                Expression.Call(
                    Expression.Convert(paramParam, paramType),
                    paramType.GetProperty("SqlDbType").SetMethod,
                    valueParam),
                paramParam,
                valueParam).Compile();
        }

        private static Action<DbParameter, string> CreateUdtTypeNameAccessor(Type paramType)
        {
            var paramParam = Expression.Parameter(typeof(DbParameter), "parameter");
            var valueParam = Expression.Parameter(typeof(string), "value");

            return Expression.Lambda<Action<DbParameter, string>>(
                Expression.Call(
                    Expression.Convert(paramParam, paramType),
                    paramType.GetProperty("UdtTypeName").SetMethod,
                    valueParam),
                paramParam,
                valueParam).Compile();
        }
    }
}
