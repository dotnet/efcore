using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    public class EnumHasFlagTranslator : IExpressionFragmentTranslator
    {
        private static readonly MethodInfo _methodInfo = typeof(Enum).GetRuntimeMethod(nameof(Enum.HasFlag), new[] { typeof(Enum) });

        public virtual Expression Translate(Expression expression)
        {
            var methodCallExpression = expression as MethodCallExpression;

            if (methodCallExpression?.Method == _methodInfo)
            {
                var argument = methodCallExpression.Arguments[0];
                argument = argument.RemoveConvert();

                if (argument.NodeType == ExpressionType.Constant)
                {
                    var argumentValue = ((ConstantExpression)argument).Value;

                    if (argumentValue == null)
                    {
                        throw new ArgumentNullException("flag");
                    }

                    argument = Expression.Constant(argumentValue);
                }

                var unwrappedObjectType = methodCallExpression.Object.Type.UnwrapNullableType();
                var unwrappedArgumentType = argument.Type.UnwrapNullableType();

                if (unwrappedObjectType != unwrappedArgumentType)
                {
                    throw new ArgumentException(string.Format(RelationalStrings.Argument_EnumTypeDoesNotMatch(unwrappedArgumentType, unwrappedObjectType)));
                }

                var unwrappedEnumObjectType = unwrappedObjectType.UnwrapEnumType();
                var unwrappedEnumArgumentType = unwrappedArgumentType.UnwrapEnumType();

                var convertedSource = Expression.Convert(methodCallExpression.Object, unwrappedEnumObjectType);
                var convertedArgument = Expression.Convert(Expression.Convert(argument, unwrappedArgumentType), unwrappedEnumArgumentType);

                return Expression.Equal(
                                        Expression.MakeBinary(
                                            ExpressionType.And,
                                            convertedSource,
                                            convertedArgument)
                                        , convertedArgument);
            }

            return null;
        }
    }
}
