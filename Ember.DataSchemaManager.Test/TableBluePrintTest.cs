using Ember.DataSchemaManager.BluePrints;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.DataSchemaManager.Test;
[TestFixture]
internal class TableBluePrintTest
{
    private TableBluePrint? TableBluePrint { get; set; }
    [SetUp]
    public void Setup()
    {
        TableBluePrint = new TableBluePrint();
    }
    [Test]
    public void ColumnChain()
    {
        Assert.Fail();
        //Assert.Pass();
    }

}

