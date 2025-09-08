// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Migrations.Internal
{
    public class XGMigrator : Migrator
    {
        private static readonly Dictionary<Type, Tuple<string, string>> _customMigrationCommands =
            new Dictionary<Type, Tuple<string, string>>
            {
                {
                    typeof(DropPrimaryKeyOperation),
                    new Tuple<string, string>(BeforeDropPrimaryKeyMigrationBegin, BeforeDropPrimaryKeyMigrationEnd)
                },
                {
                    typeof(AddPrimaryKeyOperation),
                    new Tuple<string, string>(AfterAddPrimaryKeyMigrationBegin, AfterAddPrimaryKeyMigrationEnd)
                },
            };

        private readonly IMigrationsAssembly _migrationsAssembly;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly ICurrentDbContext _currentContext;
        private readonly IRelationalCommandDiagnosticsLogger _commandLogger;

        public XGMigrator(
            IMigrationsAssembly migrationsAssembly,
            IHistoryRepository historyRepository,
            IDatabaseCreator databaseCreator,
            IMigrationsSqlGenerator migrationsSqlGenerator,
            IRawSqlCommandBuilder rawSqlCommandBuilder,
            IMigrationCommandExecutor migrationCommandExecutor,
            IRelationalConnection connection,
            ISqlGenerationHelper sqlGenerationHelper,
            ICurrentDbContext currentContext,
            IModelRuntimeInitializer modelRuntimeInitializer,
            IDiagnosticsLogger<DbLoggerCategory.Migrations> logger,
            IRelationalCommandDiagnosticsLogger commandLogger,
            IDatabaseProvider databaseProvider,
            IMigrationsModelDiffer migrationsModelDiffer,
            IDesignTimeModel designTimeModel,
            IDbContextOptions contextOptions,
            IExecutionStrategy executionStrategy)
            : base(
                migrationsAssembly,
                historyRepository,
                databaseCreator,
                migrationsSqlGenerator,
                rawSqlCommandBuilder,
                migrationCommandExecutor,
                connection,
                sqlGenerationHelper,
                currentContext,
                modelRuntimeInitializer,
                logger,
                commandLogger,
                databaseProvider,
                migrationsModelDiffer,
                designTimeModel,
                contextOptions,
                executionStrategy)
        {
            _migrationsAssembly = migrationsAssembly;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
            _currentContext = currentContext;
            _commandLogger = commandLogger;
        }

        protected override IReadOnlyList<MigrationCommand> GenerateUpSql(
            Migration migration,
            MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
        {
            var commands = base.GenerateUpSql(migration, options);

            return options.HasFlag(MigrationsSqlGenerationOptions.Script) &&
                   options.HasFlag(MigrationsSqlGenerationOptions.Idempotent)
                ? commands
                : WrapWithCustomCommands(
                    migration.UpOperations,
                    commands.ToList(),
                    options);
        }

        protected override IReadOnlyList<MigrationCommand> GenerateDownSql(
            Migration migration,
            Migration previousMigration,
            MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
        {
            var commands = base.GenerateDownSql(migration, previousMigration, options);

            return options.HasFlag(MigrationsSqlGenerationOptions.Script) &&
                   options.HasFlag(MigrationsSqlGenerationOptions.Idempotent)
                ? commands
                : WrapWithCustomCommands(
                    migration.DownOperations,
                    commands.ToList(),
                    options);
        }

        public override string GenerateScript(
            string fromMigration = null,
            string toMigration = null,
            MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
        {
            options |= MigrationsSqlGenerationOptions.Script;

            if (!options.HasFlag(MigrationsSqlGenerationOptions.Idempotent))
            {
                return base.GenerateScript(fromMigration, toMigration, options);
            }

            var operations = GetAllMigrationOperations(fromMigration, toMigration);

            var builder = new StringBuilder();

            builder.AppendJoin(string.Empty, GetMigrationCommandTexts(operations, true, options));
            builder.Append(base.GenerateScript(fromMigration, toMigration, options));
            builder.AppendJoin(string.Empty, GetMigrationCommandTexts(operations, false, options));

            return builder.ToString();
        }

        protected virtual List<MigrationOperation> GetAllMigrationOperations(string fromMigration, string toMigration)
        {
            IEnumerable<string> appliedMigrations;
            if (string.IsNullOrEmpty(fromMigration)
                || fromMigration == Migration.InitialDatabase)
            {
                appliedMigrations = Enumerable.Empty<string>();
            }
            else
            {
                var fromMigrationId = _migrationsAssembly.GetMigrationId(fromMigration);
                appliedMigrations = _migrationsAssembly.Migrations
                    .Where(t => string.Compare(t.Key, fromMigrationId, StringComparison.OrdinalIgnoreCase) <= 0)
                    .Select(t => t.Key);
            }

            PopulateMigrations(
                appliedMigrations,
                toMigration,
                out var migratorData);

            return migratorData.AppliedMigrations
                .SelectMany(x => x.UpOperations)
                .Concat(migratorData.RevertedMigrations.SelectMany(x => x.DownOperations))
                .ToList();
        }

        protected virtual IReadOnlyList<MigrationCommand> WrapWithCustomCommands(
            IReadOnlyList<MigrationOperation> migrationOperations,
            IReadOnlyList<MigrationCommand> migrationCommands,
            MigrationsSqlGenerationOptions options)
        {
            var beginCommandTexts = GetMigrationCommandTexts(migrationOperations, true, options);
            var endCommandTexts = GetMigrationCommandTexts(migrationOperations, false, options);

            return new List<MigrationCommand>(
                beginCommandTexts.Select(t => new MigrationCommand(_rawSqlCommandBuilder.Build(t), _currentContext.Context, _commandLogger))
                    .Concat(migrationCommands)
                    .Concat(endCommandTexts.Select(t => new MigrationCommand(_rawSqlCommandBuilder.Build(t), _currentContext.Context, _commandLogger))));
        }

        protected virtual string[] GetMigrationCommandTexts(
            IReadOnlyList<MigrationOperation> migrationOperations,
            bool beginTexts,
            MigrationsSqlGenerationOptions options)
            => GetCustomCommands(migrationOperations)
                .Select(
                    t => PrepareString(
                        beginTexts
                            ? t.Item1
                            : t.Item2,
                        options))
                .ToArray();

        protected virtual IReadOnlyList<Tuple<string, string>> GetCustomCommands(IReadOnlyList<MigrationOperation> migrationOperations)
            => _customMigrationCommands
                .Where(c => migrationOperations.Any(o => c.Key.IsInstanceOfType(o)) && c.Value != null)
                .Select(kvp => kvp.Value)
                .ToList();

        protected virtual string CleanUpScriptSpecificPseudoStatements(string commandText)
        {
            const string temporaryDelimiter = @"//";
            const string defaultDelimiter = @";";
            const string delimiterChangeRegexPatternFormatString = @"[\r\n]*[^\S\r\n]*DELIMITER[^\S\r\n]+{0}[^\S\r\n]*";
            const string delimiterUseRegexPatternFormatString = @"\s*{0}\s*$";

            var temporaryDelimiterRegexPattern = string.Format(
                delimiterChangeRegexPatternFormatString,
                $"(?:{Regex.Escape(temporaryDelimiter)}|{Regex.Escape(defaultDelimiter)})");

            var delimiter = Regex.Match(commandText, temporaryDelimiterRegexPattern, RegexOptions.IgnoreCase);
            if (delimiter.Success)
            {
                commandText = Regex.Replace(commandText, temporaryDelimiterRegexPattern, string.Empty, RegexOptions.IgnoreCase);

                commandText = Regex.Replace(
                    commandText,
                    string.Format(delimiterUseRegexPatternFormatString, temporaryDelimiter),
                    defaultDelimiter,
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }

            return commandText;
        }

        protected virtual string PrepareString(string str, MigrationsSqlGenerationOptions options)
        {
            str = options.HasFlag(MigrationsSqlGenerationOptions.Script)
                ? str
                : CleanUpScriptSpecificPseudoStatements(str);

            str = str
                .Replace("\r", string.Empty)
                .Replace("\n", Environment.NewLine);

            str += options.HasFlag(MigrationsSqlGenerationOptions.Script)
                ? Environment.NewLine + (
                    options.HasFlag(MigrationsSqlGenerationOptions.Idempotent)
                        ? Environment.NewLine
                        : string.Empty)
                : string.Empty;

            return str;
        }

        #region Custom SQL

        private const string BeforeDropPrimaryKeyMigrationBegin = @"DROP PROCEDURE IF EXISTS `POMELO_BEFORE_DROP_PRIMARY_KEY`;
DELIMITER //
CREATE PROCEDURE `POMELO_BEFORE_DROP_PRIMARY_KEY`(IN `SCHEMA_NAME_ARGUMENT` VARCHAR(255), IN `TABLE_NAME_ARGUMENT` VARCHAR(255))
BEGIN
	DECLARE HAS_AUTO_INCREMENT_ID TINYINT(1);
	DECLARE PRIMARY_KEY_COLUMN_NAME VARCHAR(255);
	DECLARE PRIMARY_KEY_TYPE VARCHAR(255);
	DECLARE SQL_EXP VARCHAR(1000);
	SELECT COUNT(*)
		INTO HAS_AUTO_INCREMENT_ID
		FROM `information_schema`.`COLUMNS`
		WHERE `TABLE_SCHEMA` = (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA()))
			AND `TABLE_NAME` = TABLE_NAME_ARGUMENT
			AND `Extra` = 'auto_increment'
			AND `COLUMN_KEY` = 'PRI'
			LIMIT 1;
	IF HAS_AUTO_INCREMENT_ID THEN
		SELECT `COLUMN_TYPE`
			INTO PRIMARY_KEY_TYPE
			FROM `information_schema`.`COLUMNS`
			WHERE `TABLE_SCHEMA` = (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA()))
				AND `TABLE_NAME` = TABLE_NAME_ARGUMENT
				AND `COLUMN_KEY` = 'PRI'
			LIMIT 1;
		SELECT `COLUMN_NAME`
			INTO PRIMARY_KEY_COLUMN_NAME
			FROM `information_schema`.`COLUMNS`
			WHERE `TABLE_SCHEMA` = (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA()))
				AND `TABLE_NAME` = TABLE_NAME_ARGUMENT
				AND `COLUMN_KEY` = 'PRI'
			LIMIT 1;
		SET SQL_EXP = CONCAT('ALTER TABLE `', (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA())), '`.`', TABLE_NAME_ARGUMENT, '` MODIFY COLUMN `', PRIMARY_KEY_COLUMN_NAME, '` ', PRIMARY_KEY_TYPE, ' NOT NULL;');
		SET @SQL_EXP = SQL_EXP;
		PREPARE SQL_EXP_EXECUTE FROM @SQL_EXP;
		EXECUTE SQL_EXP_EXECUTE;
		DEALLOCATE PREPARE SQL_EXP_EXECUTE;
	END IF;
END //
DELIMITER ;";

        private const string BeforeDropPrimaryKeyMigrationEnd = @"DROP PROCEDURE `POMELO_BEFORE_DROP_PRIMARY_KEY`;";

        private const string AfterAddPrimaryKeyMigrationBegin = @"DROP PROCEDURE IF EXISTS `POMELO_AFTER_ADD_PRIMARY_KEY`;
DELIMITER //
CREATE PROCEDURE `POMELO_AFTER_ADD_PRIMARY_KEY`(IN `SCHEMA_NAME_ARGUMENT` VARCHAR(255), IN `TABLE_NAME_ARGUMENT` VARCHAR(255), IN `COLUMN_NAME_ARGUMENT` VARCHAR(255))
BEGIN
	DECLARE HAS_AUTO_INCREMENT_ID INT(11);
	DECLARE PRIMARY_KEY_COLUMN_NAME VARCHAR(255);
	DECLARE PRIMARY_KEY_TYPE VARCHAR(255);
	DECLARE SQL_EXP VARCHAR(1000);
	SELECT COUNT(*)
		INTO HAS_AUTO_INCREMENT_ID
		FROM `information_schema`.`COLUMNS`
		WHERE `TABLE_SCHEMA` = (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA()))
			AND `TABLE_NAME` = TABLE_NAME_ARGUMENT
			AND `COLUMN_NAME` = COLUMN_NAME_ARGUMENT
			AND `COLUMN_TYPE` LIKE '%int%'
			AND `COLUMN_KEY` = 'PRI';
	IF HAS_AUTO_INCREMENT_ID THEN
		SELECT `COLUMN_TYPE`
			INTO PRIMARY_KEY_TYPE
			FROM `information_schema`.`COLUMNS`
			WHERE `TABLE_SCHEMA` = (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA()))
				AND `TABLE_NAME` = TABLE_NAME_ARGUMENT
				AND `COLUMN_NAME` = COLUMN_NAME_ARGUMENT
				AND `COLUMN_TYPE` LIKE '%int%'
				AND `COLUMN_KEY` = 'PRI';
		SELECT `COLUMN_NAME`
			INTO PRIMARY_KEY_COLUMN_NAME
			FROM `information_schema`.`COLUMNS`
			WHERE `TABLE_SCHEMA` = (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA()))
				AND `TABLE_NAME` = TABLE_NAME_ARGUMENT
				AND `COLUMN_NAME` = COLUMN_NAME_ARGUMENT
				AND `COLUMN_TYPE` LIKE '%int%'
				AND `COLUMN_KEY` = 'PRI';
		SET SQL_EXP = CONCAT('ALTER TABLE `', (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA())), '`.`', TABLE_NAME_ARGUMENT, '` MODIFY COLUMN `', PRIMARY_KEY_COLUMN_NAME, '` ', PRIMARY_KEY_TYPE, ' NOT NULL AUTO_INCREMENT;');
		SET @SQL_EXP = SQL_EXP;
		PREPARE SQL_EXP_EXECUTE FROM @SQL_EXP;
		EXECUTE SQL_EXP_EXECUTE;
		DEALLOCATE PREPARE SQL_EXP_EXECUTE;
	END IF;
END //
DELIMITER ;";

        private const string AfterAddPrimaryKeyMigrationEnd = @"DROP PROCEDURE `POMELO_AFTER_ADD_PRIMARY_KEY`;";

        #endregion Custom SQL
    }
}
