# Sample - Oracle Provider

**NB: This Oracle provider is a sample and is not production quality.**

## Limitations of the sample Oracle provider:

- Only works on full .NET - utilizes official Oracle managed ADO provider.
- Does not fully support Migrations. Only enough has been implemented to enable the runtime functional tests.
- Scaffolding is not supported.
- Update batching is not supported.
- OracleTransientExceptionDetector needs to be updated (impl. copied from SqlServer provider).
- OracleTypeMapper is implemented but likely needs more tuning.
- No Oracle specific features are "lit up".

## What works:

- All query tests passing (aside from a few failing due to various Oracle specific issues).
- All runtime tests passing (aside from those directly related to the limitations above).

## Running the tests

1) Install [Oracle Database 12c Release 2 (12.2.0.1.0) - Standard Edition 2](http://www.oracle.com/technetwork/database/enterprise-edition/downloads/index.html)
    - When installing, ensure to enable pluggable databases - the sample relies on a specific pluggable database.

2) Use a shell to connect via SQLPlus:

    ```
    > sqlplus / as sysdba
    ```

3) Create a pluggable database used to host the EF test databases:

    ```
       CREATE PLUGGABLE DATABASE ef
       ADMIN USER ef_pdb_admin IDENTIFIED BY ef_pdb_admin
       ROLES = (DBA)
       FILE_NAME_CONVERT = ('\pdbseed\', '\pdb_ef\');
    ```

4) Open the pluggable database:

    ```
       ALTER PLUGGABLE DATABASE ef OPEN;
    ```
