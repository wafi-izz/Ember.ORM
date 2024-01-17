using Ember.DataSchemaManager.BluePrints;
using Ember.DataSchemaManager.DataSchemas;
using Ember.Transcription.TranscriptionInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static Ember.DataSchemaManager.SharedFuncctions.Shared;

namespace Ember.Transcription.RDBMS.PostgreSql;

internal class PostgreSqlTableTranscriber : TableTranscriber, ITableTranscriber
{
    public String Transcript { get; set; }
    public TableSchema TableSchema { get; set; }
    public PostgreSqlTableTranscriber(TableSchema TableSchema)
    {
        Transcript = "";
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
        Transcript += $"CREATE TABLE \"{TableBluePrint.TableName}\"(\n";
        foreach ((ColumnBluePrint Column, Int32 Index) in TableBluePrint.ColumnList.Select((Column, Index) => (Column, Index + 1)))
        {
            Transcript += "\t";
            if (Column.Statemant != null)
                Transcript += Column.Statemant;
            else
            {
                Transcript += ColumnHead(Column);
                Transcript += PrimaryKey(Column);
                Transcript += IDENTITY(TableBluePrint.TableName,Column);
                Transcript += ForeignKeySection(TableBluePrint.TableName, Column);
                Transcript += DefaultValue(Column);
                Transcript += NullabilityState(Column);
            }
            Transcript += TableBluePrint.ColumnList.Count > Index ? $",\n" : "\n";
        }
        Transcript += $");\n\n ";
    }
    public String ColumnHead(ColumnBluePrint Column)
    {
        String Length = Column.ColumnDataType["Length"] != null ? Column.ColumnDataType["Length"]!.ToString() : "";
        String DataTypeParameter = Length != "" && Length!.ToLower() != "max" ? $"({Length})" : "";
        return $"{Column.ColumnName} {TranscribeDataType(Column.ColumnDataType["DataTypeName"]!.ToString(),Column.ColumnDataType["DataTypeSQLName"]!.ToString(), DataTypeParameter)} ";
    }
    public String IDENTITY(String TableName, ColumnBluePrint Column)
    {
        return Column.IsIdentity ? $"GENERATED ALWAYS AS IDENTITY ( INCREMENT {Column.Identity["IncrementValue"]} START {Column.Identity["StartValue"]} MINVALUE {Column.MinValue} MAXVALUE {Column.MaxValue} CACHE 1 ) " : "";
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
        return $"CONSTRAINT {ConstraintName} REFERENCES \"{ForeignTable}\" ({ForeignTableColumnName}) {OnUpdate} {OnDelete} ";
    }
    #endregion
    #region Alter
    public void Alter(TableBluePrint TableBluePrint)
    {
        foreach ((ColumnBluePrint Column, Int32 Index) in TableBluePrint.ColumnList.Select((Column, Index) => (Column, Index + 1)))
        {
            if (Column.Action == TableBluePrintAlterationAction.CreateColumn) Transcript += CreateColumn(Column, TableBluePrint.TableName);
            if (Column.Action == TableBluePrintAlterationAction.AlterColumnName) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.AlterColumnType) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.AddForeignKey) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.RemoveForeignKey) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.AddConstraint) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.RemoveConstraint) Transcript += RenameColumn(Column, TableBluePrint.TableName);
        }
    }
    public String CreateColumn(ColumnBluePrint Column, String TableName)
    {
        String Transcript = $"Alter Table \"{TableName}\" \n\tAdd ";
        Transcript += ColumnHead(Column);
        Transcript += PrimaryKey(Column);
        Transcript += IDENTITY(TableName, Column);
        Transcript += ForeignKeySection(TableName, Column);
        Transcript += DefaultValue(Column);
        Transcript += NullabilityState(Column);
        Transcript += ";\n\n";
        return Transcript;
    }
    public String RenameColumn(ColumnBluePrint Column, String TableName)
    {
        return $"Alter Table \"{TableName}\" \n\tRename Column {Column.ColumnName} to {Column.ColumnRename};\n\n";
    }
    public String AlterColumnType(ColumnBluePrint Column, String TableName)
    {
        return "";
    }
    #endregion
    #region Drop
    public void Drop(TableBluePrint TableBluePrint)
    {
        Transcript += DropTableQuery(TableBluePrint);
    }
    public override String DropTableQuery(TableBluePrint TableBluePrint)
    {
        return $"DROP TABLE IF EXISTS \"{TableBluePrint.ObjectName}\";\n\n"; // TODO : must specify witch schema to drop to or create in to.
    }
    #endregion
}
