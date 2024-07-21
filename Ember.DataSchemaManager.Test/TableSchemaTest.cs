using NUnit.Framework.Legacy;
using NUnit.Framework;
using Ember.DataSchemaManager.DataSchemas;
using Ember.DataSchemaManager.BluePrints;
using static Ember.DataSchemaManager.DataSchemas.TableSchema;
using Newtonsoft.Json.Linq;

namespace Ember.DataSchemaManager.Test;

[TestFixture]
public class TableSchemaTest
{
    private TableSchema? TableSchema;
    [SetUp]
    public void setup()
    {
        TableSchema = new TableSchema();
    }
    [Test]
    public void CreateTest()
    {
        String TableName = "Table1";
        TableSchema?.Create(TableName, Table =>
        {
            Table.Integer("ObjectTypeID").PrimaryKey().Identity().Min(1).Max(100);
            Table.String("ObjectName",500);
        });
        Boolean TableBluePrintExists = TableSchema?.TableBluePrintList.Count == 1;
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList.Count > 0;
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList.First().ColumnName == "ObjectTypeID";
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList.First().ColumnDataType["DataTypeName"]!.ToString() == "Integer";
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList[1].ColumnName == "ObjectName";
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList[1].ColumnDataType["DataTypeName"]!.ToString() == "String";
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().Action == BluePrintAction.Create;
        Assert.That(TableBluePrintExists, Is.True);
    }
    [Test]
    public void AlterTest()
    {
        String TableName = "Table1";
        String TableReName = "Table1";
        TableSchema?.Alter(TableName,TableReName, Table =>
        {
            Table.AlterColumn("ObjectTypeID").Rename("NameColumnName");
            Table.AlterColumn("ObjectTypeID").AddConstraint("1=1", "ccc");
        });
        Boolean TableBluePrintExists = TableSchema?.TableBluePrintList.Count == 1;
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList.Count > 0;
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList.First().ColumnRename == "NameColumnName";
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList.First().Action == TableBluePrintAlterationAction.AlterColumnName;
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList[1].ColumnName == "ObjectTypeID";
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList[1].Action == TableBluePrintAlterationAction.AddConstraint;
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList[1].ConstraintName == "ccc";
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().Action == BluePrintAction.Alter;
        Assert.That(TableBluePrintExists, Is.True);
    }
    [Test]
    public void DropTest()
    {
        String TableName = "Table1";
        TableSchema?.Drop(TableName);
        Boolean TableBluePrintExists = TableSchema?.TableBluePrintList.Count == 1;
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ObjectName == TableName;
        TableBluePrintExists &=  TableSchema?.TableBluePrintList.First().Action == BluePrintAction.Drop;
        Assert.That(TableBluePrintExists, Is.True);
    }
}


public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public int Subtract(int a, int b)
    {
        return a - b;
    }
}
[TestFixture]
public class CalculatorTests
{
    private Calculator calculator;

    [SetUp]
    public void Setup()
    {
        // Arrange: Initialize the Calculator instance
        calculator = new Calculator();
    }

    [Test]
    public void Add_ValidNumbers_ReturnsCorrectSum()
    {
        // Act: Perform the operation
        int result = calculator.Add(5, 3);

        // Assert: Verify the result
        ClassicAssert.AreEqual(8, result);
    }

    [Test]
    public void Subtract_ValidNumbers_ReturnsCorrectDifference()
    {
        // Act: Perform the operation
        int result = calculator.Subtract(10, 4);

        // Assert: Verify the result
        ClassicAssert.AreEqual(6, result);
    }
}
