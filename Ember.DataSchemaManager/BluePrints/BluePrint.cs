using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.DataSchemaManager.BluePrints;

public class BluePrint
{
    public String ObjectName { get; set; }
    public String ObjectRename { get; set; }
    public BluePrintAction Action { get; set; }
    public BluePrint()
    {

    }
}

public enum BluePrintAction
{
    Create,
    Alter,
    Drop,
    Query
}
public enum TableBluePrintAlterationAction
{
    CreateColumn,
    AlterColumnName,
    AlterColumnType,
    AddForeignKey,
    RemoveForeignKey,
    AddConstraint,
    RemoveConstraint,
}