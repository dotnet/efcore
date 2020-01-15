using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerTimeSpanMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMapping
            = new Dictionary<string, string>
            {
                { nameof(TimeSpan.Hours), "hour" },
                { nameof(TimeSpan.Minutes), "minute" },
                { nameof(TimeSpan.Seconds), "second" },
                { nameof(TimeSpan.Milliseconds), "millisecond" }
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerTimeSpanMemberTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));

            var declaringType = member.DeclaringType;

            if (declaringType == typeof(TimeSpan))
            {
                var memberName = member.Name;

                if (_datePartMapping.TryGetValue(memberName, out var datePart))
                {
                    return _sqlExpressionFactory.Function(
                        "DATEPART",
                        new[] { _sqlExpressionFactory.Fragment(datePart), instance },
                        returnType);
                }

                switch (memberName)
                {
                    case nameof(TimeSpan.TotalSeconds):
                        return _sqlExpressionFactory.Function(
                            "DATEDIFF",
                            new[] { _sqlExpressionFactory.Fragment("second"), _sqlExpressionFactory.Fragment("0"), instance },
                            returnType);

                    case nameof(DateTime.Date):
                        return _sqlExpressionFactory.Function(
                            "CONVERT",
                            new[] { _sqlExpressionFactory.Fragment("date"), instance },
                            returnType,
                            declaringType == typeof(DateTime)
                                ? instance.TypeMapping
                                : _sqlExpressionFactory.FindMapping(typeof(DateTime)));

                    case nameof(DateTime.TimeOfDay):
                        return _sqlExpressionFactory.Convert(instance, returnType);

                    case nameof(DateTime.Now):
                        return _sqlExpressionFactory.Function(
                            declaringType == typeof(DateTime) ? "GETDATE" : "SYSDATETIMEOFFSET",
                            Array.Empty<SqlExpression>(),
                            returnType);

                    case nameof(DateTime.UtcNow):
                        var serverTranslation = _sqlExpressionFactory.Function(
                            declaringType == typeof(DateTime) ? "GETUTCDATE" : "SYSUTCDATETIME",
                            Array.Empty<SqlExpression>(),
                            returnType);

                        return declaringType == typeof(DateTime)
                            ? (SqlExpression)serverTranslation
                            : _sqlExpressionFactory.Convert(serverTranslation, returnType);

                    case nameof(DateTime.Today):
                        return _sqlExpressionFactory.Function(
                            "CONVERT",
                            new SqlExpression[]
                            {
                                _sqlExpressionFactory.Fragment("date"),
                                _sqlExpressionFactory.Function(
                                    "GETDATE",
                                    Array.Empty<SqlExpression>(),
                                    typeof(DateTime))
                            },
                            returnType);
                }
            }

            return null;
        }
    }
}
