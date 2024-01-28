using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace;

public partial class DbContextModel
{
    private string DefaultSchema { get; init; } = "custom";

    partial void Customize()
    {
        RemoveAnnotation("Relational:DefaultSchema");
        AddAnnotation("Relational:DefaultSchema", DefaultSchema);
        RemoveRuntimeAnnotation("Relational:RelationalModel");

        foreach (RuntimeEntityType entityType in ((IModel)this).GetEntityTypes())
        {
            Customize(entityType);

            foreach (var key in entityType.GetDeclaredKeys())
            {
                key.RemoveRuntimeAnnotation(RelationalAnnotationNames.UniqueConstraintMappings);
            }

            foreach (var index in entityType.GetDeclaredIndexes())
            {
                index.RemoveRuntimeAnnotation(RelationalAnnotationNames.TableIndexMappings);
            }

            foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
            {
                foreignKey.RemoveRuntimeAnnotation(RelationalAnnotationNames.ForeignKeyMappings);
            }

            var tableName = entityType.FindAnnotation("Relational:TableName")?.Value as string;
            if (string.IsNullOrEmpty(tableName))
                continue;

            entityType.SetAnnotation("Relational:Schema", DefaultSchema);
        }
    }

    private static void Customize(RuntimeTypeBase entityType)
    {
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.DefaultMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.TableMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.ViewMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.SqlQueryMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.FunctionMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.InsertStoredProcedureMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.DeleteStoredProcedureMappings);
        entityType.RemoveRuntimeAnnotation(RelationalAnnotationNames.UpdateStoredProcedureMappings);

        foreach (var property in entityType.GetDeclaredProperties())
        {
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.DefaultColumnMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.TableColumnMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.ViewColumnMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.SqlQueryColumnMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.FunctionColumnMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.InsertStoredProcedureParameterMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.DeleteStoredProcedureParameterMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.UpdateStoredProcedureParameterMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.InsertStoredProcedureResultColumnMappings);
            property.RemoveRuntimeAnnotation(RelationalAnnotationNames.UpdateStoredProcedureResultColumnMappings);
        }

        foreach (var complexProperty in entityType.GetDeclaredComplexProperties())
        {
            Customize(complexProperty.ComplexType);
        }
    }
}