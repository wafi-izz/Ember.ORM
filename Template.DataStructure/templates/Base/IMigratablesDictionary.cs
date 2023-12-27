using Ember.DataScheme.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace templates.Base;

internal interface IMigratablesDictionary
{
    public Schema Schema { get; set; }
    void Up();
    void Down();
}