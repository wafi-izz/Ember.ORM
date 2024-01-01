using Ember.DataSchemaManager.DataSchemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ember.DataSchemaManager.Dictionaries;

public interface IMigratablesDictionary
{
    void Up();
    void Down();
}