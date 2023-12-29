using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ember.DataSchemaManager.DataSchemas.TableSchema;

namespace Ember.DataSchemaManager.DataSchemas;

public class DataSchema
{
    public ObservableCollection<Schema> SchemaList { get; set; }
    

    public TableSchema TableSchema { get; set; }
    public DataSchema(ObservableCollection<String> TableList)
    {
        TableSchema = new TableSchema(TableList);
    }
    public void Create(String TableName, TableBluePrintCallBack TableBluePrint)
    {
        TableSchema.Create(TableName, TableBluePrint);
    }
    public void DropTable(String TableName)
    {
        TableSchema.Drop(TableName);
    }
}
