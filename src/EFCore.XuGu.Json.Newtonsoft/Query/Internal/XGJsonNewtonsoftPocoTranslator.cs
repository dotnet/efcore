using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Query.Internal
{
    public class XGJsonNewtonsoftPocoTranslator : XGJsonPocoTranslator
    {
        public XGJsonNewtonsoftPocoTranslator(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] ISqlExpressionFactory sqlExpressionFactory)
        : base(typeMappingSource, (XGSqlExpressionFactory)sqlExpressionFactory)
        {
        }

        public override string GetJsonPropertyName(MemberInfo member)
            => member.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName;
    }
}
