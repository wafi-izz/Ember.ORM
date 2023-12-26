using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ember.DataScheme.Schemas.TableSchema;

namespace Ember.DataScheme.Schemas;

public class Schema
{
    public TableSchema TableSchema { get; set; }
    public Schema(ObservableCollection<String> TableList)
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
