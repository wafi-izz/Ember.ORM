using Ember.DataScheme.BluePrints;
using Ember.DataScheme.Schemas;
using Ember.Transcription.TranscriptionInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Ember.Transcription.RDBMS.SqlServer;


public class SqlServerTableTranscriber : ITableTranscriber
{
    public String TransScript { get; set; }
    private TableSchema TableSchema { get; set; }
    public SqlServerTableTranscriber(TableSchema TableSchema)
    {
        TransScript = "";
        this.TableSchema = TableSchema;
        List<TableBluePrint> CreateList = TableSchema.TableBluePrintList.Where(x => x.Action == BluePrintAction.Create).ToList();
        List<TableBluePrint> AlterList = TableSchema.TableBluePrintList.Where(x => x.Action == BluePrintAction.Alter).ToList();
        List<TableBluePrint> DropList = TableSchema.TableBluePrintList.Where(x => x.Action == BluePrintAction.Drop).ToList();
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
        Int32 Index = 1;
        TransScript += $"CREATE TABLE {TableBluePrint.TableName}(\n";
        foreach (var Column in TableBluePrint.Columns)
        {
            if (Column.Statemant != null)
                TransScript += $"\t{Column.Statemant} ,\n";
            else
            {
                TransScript += $"\t{ColumnHead(Column)} ";
                TransScript += Column.IsPrimaryKey ? "PRIMARY KEY " : "";
                TransScript += Column.IsIdentity ? $"IDENTITY({Column.Identity["IncrementValue"]},{Column.Identity["StartValue"]}) " : "";
                TransScript += Column.IsForeignKey ? $"{ForeignKeySection(TableBluePrint.TableName, Column)} " : "";
                TransScript += Column.Default != null ? $"DEFAULT('{Column.Default}') " : "";
                TransScript += Column.Nullable ? $"NULL " : "NOT NULL ";
            }
            TransScript += TableBluePrint.Columns.Count > Index ? $",\n" : "\n";
            Index++;
        }
        TransScript += $");\n\n ";
    }
    public String ColumnHead(ColumnBluePrint Column)
    {
        var DataTypeParameter = Column.ColumnDataType["Length"] != null ? $"({Column.ColumnDataType["Length"]})" : "";
        return $"{Column.ColumnName} {Column.ColumnDataType["DataTypeSQLName"]}{DataTypeParameter}";
    }
    public String ForeignKeySection(String TableName, ColumnBluePrint Column)
    {
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
        return $"CONSTRAINT {ConstraintName} FOREIGN KEY ({ColumnName}) REFERENCES {ForeignTable} ({ForeignTableColumnName}) {OnUpdate} {OnDelete}";
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
        TransScript += $"DROP TABLE IF EXISTS {TableBluePrint.ObjectName};\n\n"; // TODO : must specify witch schema to drop to or create in to.
    }
    #endregion
}
