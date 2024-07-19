using NUnit.Framework.Legacy;
using NUnit.Framework;
using Ember.DataSchemaManager.DataSchemas;
using Ember.DataSchemaManager.BluePrints;
using static Ember.DataSchemaManager.DataSchemas.TableSchema;

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
            Table.Integer("DD").ForeignKey().References("ObjectTypeID").On("ObjectTypes").Constraint("some_custom_name");
        });
        Boolean TableBluePrintExists = TableSchema?.TableBluePrintList.Count == 1;
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList.Count > 0; //TODO: Test the ColumnList Thoroughly 
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().Action == BluePrintAction.Create;
        Assert.That(TableBluePrintExists, Is.True);
    }
    [Test]
    public void AlterTest()
    {
        String TableName = "Table1";
        String TableReName = "Table1";
        TableSchema?.Alter(TableName,TableReName, Table => { });
        Boolean TableBluePrintExists = TableSchema?.TableBluePrintList.Count == 1;
        TableBluePrintExists &= TableSchema?.TableBluePrintList.First().ColumnList.Count > 0; //TODO: Test the ColumnList Thoroughly 
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
        //ClassicAssert.AreEqual(8, result);
        Assert.Fail();
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
