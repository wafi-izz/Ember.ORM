using Ember.DataSchemaManager.DataSchemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.DataStructure.Base;

internal interface IMigratablesDictionary
{
    public DataSchema Schema { get; set; }
    void Up();
    void Down();
}