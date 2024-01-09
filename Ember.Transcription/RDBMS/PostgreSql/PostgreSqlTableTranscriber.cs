using Ember.DataSchemaManager.BluePrints;
using Ember.DataSchemaManager.DataSchemas;
using Ember.Transcription.TranscriptionInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Ember.Transcription.RDBMS.PostgreSql;

internal class PostgreSqlTableTranscriber : TableTranscriber, ITableTranscriber
{
    public String TransScript { get; set; }
    public TableSchema TableSchema { get; set; }
    public PostgreSqlTableTranscriber(TableSchema TableSchema)
    {
        TransScript = "";
        this.TableSchema = TableSchema;
        Transcribe();
    }
    public void Transcribe()
    {
        foreach (TableBluePrint TableBluePrint in TableSchema.TableBluePrintList)
        {
            if (TableBluePrint.Action == BluePrintAction.Create) Create(TableBluePrint);
            if (TableBluePrint.Action == BluePrintAction.Alter) Alter(TableBluePrint);
            if (TableBluePrint.Action == BluePrintAction.Drop) Drop(TableBluePrint);
        }
    }
    #region Create
    public void Create(TableBluePrint TableBluePrint)
    {
        TransScript += $"CREATE TABLE {TableBluePrint.TableName}(\n\t";
        foreach ((ColumnBluePrint Column, Int32 Index) in TableBluePrint.Columns.Select((Column, Index) => (Column, Index + 1)))
        {
            if (Column.Statemant != null)
                TransScript += Column.Statemant;
            else
            {
                TransScript += ColumnHead(Column);
                TransScript += PrimaryKey(Column);
                TransScript += Column.IsIdentity ? $"IDENTITY({Column.Identity["IncrementValue"]},{Column.Identity["StartValue"]}) " : "";
                TransScript += ForeignKeySection(TableBluePrint.TableName, Column);
                TransScript += DefaultValue(Column);
                TransScript += NullabilityState(Column);
            }
            TransScript += TableBluePrint.Columns.Count > Index ? $",\n" : "\n";
        }
        TransScript += $");\n\n ";
    }
    public override String ForeignKeySection(String TableName, ColumnBluePrint Column)
    {
        if (!Column.IsForeignKey)
            return "";

        String ForeignTable = Column.ForeignKeyArguments["ForeignTable"]!.GetValue<String>();
        String ForeignTableColumnName = Column.ForeignKeyArguments["ForeignTableColumnName"]!.GetValue<String>();
        String ColumnName = Column.ColumnName;
        //on delete
        String OnDelete = "";
        if (Column.ForeignKeyArguments["OnDelete"] != null)
            OnDelete = $"ON DELETE {Column.ForeignKeyArguments["OnDelete"]}";

        //on update
        String OnUpdate = "";
        if (Column.ForeignKeyArguments["OnUpdate"] != null)
            OnUpdate = $"ON UPDATE {Column.ForeignKeyArguments["OnUpdate"]}";

        JsonNode? CustomConstraintName = Column.ForeignKeyArguments["CustomConstraintName"];
        String ConstraintName = CustomConstraintName != null ? CustomConstraintName.GetValue<String>() : $"FK_{TableName}_{ForeignTable}_{ColumnName}_{ForeignTableColumnName} ";
        return $"CONSTRAINT {ConstraintName} REFERENCES {ForeignTable} ({ForeignTableColumnName}) {OnUpdate} {OnDelete} ";
    }
    #endregion
    #region Alter
    public void Alter(TableBluePrint TableBluePrint)
    {

    }
    #endregion
    #region Drop
    public void Drop(TableBluePrint TableBluePrint)
    {
    }
    #endregion
}
