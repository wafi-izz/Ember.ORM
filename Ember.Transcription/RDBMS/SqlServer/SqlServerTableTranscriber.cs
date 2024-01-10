using Ember.DataSchemaManager.BluePrints;
using Ember.DataSchemaManager.DataSchemas;
using Ember.Transcription.TranscriptionInterfaces;
using System.Data.Common;
using System.Security.Principal;

namespace Ember.Transcription.RDBMS.SqlServer;


internal class SqlServerTableTranscriber : TableTranscriber, ITableTranscriber
{
    public String TransScript { get; set; }
    public TableSchema TableSchema { get; set; }
    public SqlServerTableTranscriber(TableSchema TableSchema)
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
                TransScript += IDENTITY(Column);
                TransScript += ForeignKeySection(TableBluePrint.TableName, Column);
                TransScript += DefaultValue(Column);
                TransScript += NullabilityState(Column);
            }
            TransScript += TableBluePrint.Columns.Count > Index ? $",\n" : "\n";
        }
        TransScript += $");\n\n ";
    }
    public String IDENTITY(ColumnBluePrint Column)
    {
        return Column.IsIdentity ? $"IDENTITY({Column.Identity["IncrementValue"]},{Column.Identity["StartValue"]}) CONSTRAINT CHC_ValueRange CHECK (ValueColumn >= {Column.MaxValue} AND ValueColumn <= {Column.MaxValue})" : "";
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
        TransScript += DropTableQuery(TableBluePrint);
    }
    #endregion
}
