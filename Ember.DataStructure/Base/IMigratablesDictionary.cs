using Ember.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.DataStructure.Base;

internal interface IMigratablesDictionary
{
    public Schema Schema { get; set; }
    void Up();
    void Down();
}