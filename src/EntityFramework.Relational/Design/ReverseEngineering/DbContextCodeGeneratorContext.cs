// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class DbContextCodeGeneratorContext : ModelCodeGeneratorContext
    {
        private readonly IModel _model;
        private readonly string _namespaceName;
        private readonly string _className;
        private readonly string _connectionString;

        private Dictionary<IEntityType, string> _entityTypeToClassNameMap = new Dictionary<IEntityType, string>();

        public DbContextCodeGeneratorContext(
            IModel model, string namespaceName,
            string className, string connectionString)
        {
            _model = model;
            _namespaceName = namespaceName;
            _className = className;
            _connectionString = connectionString;
            InitializeEntityTypeNames();
        }

        private void InitializeEntityTypeNames()
        {
            foreach(var entityType in _model.EntityTypes)
            {
                _entityTypeToClassNameMap[entityType] =
                    CSharpUtilities.Instance.GenerateCSharpIdentifier(
                        entityType.SimpleName, _entityTypeToClassNameMap.Values);
            }
        }

        public override string ClassName
        {
            get
            {
                return _className;
            }
        }

        public override string ClassNamespace
        {
            get
            {
                return _namespaceName;
            }
        }

        public virtual string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        public Dictionary<IEntityType, string> EntityTypeToClassNameMap
        {
            get
            {
                return _entityTypeToClassNameMap;
            }
        }

        public override void GenerateCommentHeader(IndentedStringBuilder sb)
        {
            sb.AppendLine("//");
            sb.Append("// Generated using Connection String: ");
            sb.AppendLine(ConnectionString);
            sb.AppendLine("//");
            sb.AppendLine();
        }

        public override void GenerateProperties(IndentedStringBuilder sb)
        {
            foreach (var entityType in OrderedEntityTypes())
            {
                sb.Append("public virtual DbSet<");
                sb.Append(_entityTypeToClassNameMap[entityType]);
                sb.Append("> ");
                sb.Append(_entityTypeToClassNameMap[entityType]);
                sb.AppendLine(" { get; set; }");
            }

            if (_model.EntityTypes.Any())
            {
                sb.AppendLine();
            }
        }

        public override void GenerateMethods(IndentedStringBuilder sb)
        {
            GenerateOnConfiguringCode(sb);
            GenerateOnModelCreatingCode(sb);
        }

        public virtual void GenerateOnConfiguringCode(IndentedStringBuilder sb)
        {
            sb.AppendLine("protected override void OnConfiguring(DbContextOptions options)");
            sb.AppendLine("{");
            using (sb.Indent())
            {
                sb.Append("options.UseSqlServer(");
                sb.Append(CSharpUtilities.Instance.GenerateVerbatimStringLiteral(ConnectionString));
                sb.AppendLine(");");
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }

        public virtual void GenerateOnModelCreatingCode(IndentedStringBuilder sb)
        {
            sb.AppendLine("protected override void OnModelCreating(ModelBuilder modelBuilder)");
            sb.AppendLine("{");
            using (sb.Indent())
            {
                foreach (var entityType in OrderedEntityTypes())
                {
                    sb.Append("modelBuilder.Entity<");
                    sb.Append(_entityTypeToClassNameMap[entityType]);
                    sb.Append(">(");
                    GenerateEntityConfiguration(sb, entityType);
                    sb.AppendLine(");");
                }
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }

        public virtual void GenerateEntityConfiguration(IndentedStringBuilder sb, IEntityType entityType)
        {
            sb.AppendLine("entity =>");
            sb.AppendLine("{");
            using (sb.Indent())
            {
                var key = entityType.TryGetPrimaryKey();
                if (key != null && key.Properties.Count > 0)
                {
                    GenerateEntityKeyConfiguration(sb, key);
                }
                GenerateForeignKeysConfiguration(sb, entityType);
            }
            sb.AppendLine("}");
        }

        public virtual void GenerateEntityKeyConfiguration(IndentedStringBuilder sb, IKey key)
        {
            sb.Append("entity.Key( e => ");
            sb.Append(ModelUtilities.Instance
                .GenerateLambdaToKey(key.Properties, PrimaryKeyPropertyOrder, "e"));
            sb.AppendLine(" );");
        }

        public abstract void GenerateForeignKeysConfiguration(IndentedStringBuilder sb, IEntityType entityType);


        //
        // helper methods
        //
        public abstract int PrimaryKeyPropertyOrder(IProperty property);

        public virtual IEnumerable<IEntityType> OrderedEntityTypes()
        {
            return _model.EntityTypes.OrderBy(e => _entityTypeToClassNameMap[e]);
        }
    }
}