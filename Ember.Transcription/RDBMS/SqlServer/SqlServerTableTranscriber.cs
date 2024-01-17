using Ember.DataSchemaManager.BluePrints;
using Ember.DataSchemaManager.DataSchemas;
using Ember.Transcription.TranscriptionInterfaces;
using System.Data.Common;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Principal;

namespace Ember.Transcription.RDBMS.SqlServer;


internal class SqlServerTableTranscriber : TableTranscriber, ITableTranscriber
{
    public String Transcript { get; set; }
    public TableSchema TableSchema { get; set; }
    public SqlServerTableTranscriber(TableSchema TableSchema)
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
        Transcript += $"CREATE TABLE {TableBluePrint.TableName}(\n";
        foreach ((ColumnBluePrint Column, Int32 Index) in TableBluePrint.ColumnList.Select((Column, Index) => (Column, Index + 1)))
        {
            Transcript += "\t";
            if (Column.Statemant != null)
                Transcript += Column.Statemant;
            else
            {
                Transcript += ColumnHead(Column);
                Transcript += PrimaryKey(Column);
                Transcript += IDENTITY(TableBluePrint.TableName, Column);
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
        String DataTypeParameter = Length != "" ? $"({Length})" : "";
        return $"{Column.ColumnName} {TranscribeDataType(Column.ColumnDataType["DataTypeName"]!.ToString(), Column.ColumnDataType["DataTypeSQLName"]!.ToString(), DataTypeParameter)} ";
    }
    public String IDENTITY(String TableName, ColumnBluePrint Column)
    {
        return Column.IsIdentity ? $"IDENTITY({Column.Identity["IncrementValue"]},{Column.Identity["StartValue"]}) CONSTRAINT CHC_{TableName}_IDENTITYrANGE CHECK ({Column.ColumnName} >= {Column.MaxValue} AND {Column.ColumnName} <= {Column.MaxValue})" : "";
    }
    #endregion
    #region Alter
    public void Alter(TableBluePrint TableBluePrint)
    {
        foreach ((ColumnBluePrint Column, Int32 Index) in TableBluePrint.ColumnList.Select((Column, Index) => (Column, Index + 1)))
        {
            if (Column.Action == TableBluePrintAlterationAction.CreateColumn) Transcript += CreateColumn(Column, TableBluePrint.TableName);
            if (Column.Action == TableBluePrintAlterationAction.AlterColumnName) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if (Column.Action == TableBluePrintAlterationAction.AlterColumnType) Transcript += AlterColumnType(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.AddForeignKey) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.RemoveForeignKey) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.AddConstraint) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.RemoveConstraint) Transcript += RenameColumn(Column, TableBluePrint.TableName);
        }
    }
    public String CreateColumn(ColumnBluePrint Column, String TableName)
    {
        String Transcript = $"Alter Table {TableName}\n\tAdd ";
        Transcript += ColumnHead(Column);
        Transcript += PrimaryKey(Column);
        Transcript += IDENTITY(TableName, Column);
        Transcript += ForeignKeySection(TableName, Column);
        Transcript += DefaultValue(Column);
        Transcript += NullabilityState(Column);
        Transcript += "\n\n";
        return Transcript;
    }
    public String RenameColumn(ColumnBluePrint Column, String TableName)
    {
        return $"EXEC sp_rename '{TableName}.{Column.ColumnName}', '{Column.ColumnRename}', 'COLUMN' \n\n";
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
    #endregion
}
