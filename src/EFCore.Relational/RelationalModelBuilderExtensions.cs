// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="ModelBuilder" />.
    /// </summary>
    public static class RelationalModelBuilderExtensions
    {
        /// <summary>
        ///     Configures a database sequence when targeting a relational database.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <returns> A builder to further configure the sequence. </returns>
        public static SequenceBuilder HasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return new SequenceBuilder(modelBuilder.Model.Relational().GetOrAddSequence(name, schema));
        }

        /// <summary>
        ///     Configures a database sequence when targeting a relational database.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder HasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] Action<SequenceBuilder> builderAction)
            => modelBuilder.HasSequence(name, null, builderAction);

        /// <summary>
        ///     Configures a database sequence when targeting a relational database.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder HasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Action<SequenceBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(HasSequence(modelBuilder, name, schema));

            return modelBuilder;
        }

        /// <summary>
        ///     Configures a database sequence when targeting a relational database.
        /// </summary>
        /// <param name="clrType"> The type of values the sequence will generate. </param>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <returns> A builder to further configure the sequence. </returns>
        public static SequenceBuilder HasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Type clrType,
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var sequence = modelBuilder.Model.Relational().GetOrAddSequence(name, schema);
            sequence.ClrType = clrType;

            return new SequenceBuilder(sequence);
        }

        /// <summary>
        ///     Configures a database sequence when targeting a relational database.
        /// </summary>
        /// <param name="clrType"> The type of values the sequence will generate. </param>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder HasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Type clrType,
            [NotNull] string name,
            [NotNull] Action<SequenceBuilder> builderAction)
            => modelBuilder.HasSequence(clrType, name, null, builderAction);

        /// <summary>
        ///     Configures a database sequence when targeting a relational database.
        /// </summary>
        /// <param name="clrType"> The type of values the sequence will generate. </param>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder HasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Type clrType,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Action<SequenceBuilder> builderAction)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(HasSequence(modelBuilder, clrType, name, schema));

            return modelBuilder;
        }

        /// <summary>
        ///     Configures a database sequence when targeting a relational database.
        /// </summary>
        /// <typeparam name="T"> The type of values the sequence will generate. </typeparam>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <returns> A builder to further configure the sequence. </returns>
        public static SequenceBuilder HasSequence<T>(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var sequence = modelBuilder.Model.Relational().GetOrAddSequence(name, schema);
            sequence.ClrType = typeof(T);

            return new SequenceBuilder(sequence);
        }

        /// <summary>
        ///     Configures a database sequence when targeting a relational database.
        /// </summary>
        /// <typeparam name="T"> The type of values the sequence will generate. </typeparam>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder HasSequence<T>(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] Action<SequenceBuilder> builderAction)
            => modelBuilder.HasSequence<T>(name, null, builderAction);

        /// <summary>
        ///     Configures a database sequence when targeting a relational database.
        /// </summary>
        /// <typeparam name="T"> The type of values the sequence will generate. </typeparam>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder HasSequence<T>(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Action<SequenceBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(HasSequence<T>(modelBuilder, name, schema));

            return modelBuilder;
        }

        /// <summary>
        ///     Configures a database function when targeting a relational database.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="methodInfo"> The methodInfo this dbFunction uses. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static DbFunctionBuilder HasDbFunction(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] MethodInfo methodInfo)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(methodInfo, nameof(methodInfo));

            var dbFunction = modelBuilder.Model.Relational().GetOrAddDbFunction(methodInfo);

            return new DbFunctionBuilder(dbFunction);
        }

        /// <summary>
        ///     Configures a database function when targeting a relational database.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="expression"> The method this dbFunction uses. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static DbFunctionBuilder HasDbFunction<TResult>(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Expression<Func<TResult>> expression)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(expression, nameof(expression));

            var methodInfo = (expression.Body as MethodCallExpression)?.Method;

            if (methodInfo == null)
            {
                throw new ArgumentException(RelationalStrings.DbFunctionExpressionIsNotMethodCall(expression));
            }

            return modelBuilder.HasDbFunction(methodInfo);
        }

        /// <summary>
        ///     Configures a database function when targeting a relational database.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="methodInfo"> The methodInfo this dbFunction uses. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder HasDbFunction(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] MethodInfo methodInfo,
            [NotNull] Action<DbFunctionBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(methodInfo, nameof(methodInfo));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(HasDbFunction(modelBuilder, methodInfo));

            return modelBuilder;
        }

        /// <summary>
        ///     Configures the default schema that database objects should be created in, if no schema
        ///     is explicitly configured.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="schema"> The default schema. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder HasDefaultSchema(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string schema)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(schema, nameof(schema));

            modelBuilder.GetInfrastructure().Relational(ConfigurationSource.Explicit).HasDefaultSchema(schema);

            return modelBuilder;
        }
    }
}
