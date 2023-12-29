using DataStructure.Base.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.DataStructure.Base.DatabaseObjects;

public class DatabaseObjectScrapper
{
    public ObservableCollection<String> TableList
    {
        get
        {
            var MigrationTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(Table).IsAssignableFrom(x))
            .Where(x => x.Namespace!.StartsWith($"{nameof(DataStructure)}.{nameof(Migrations)}.{nameof(Migrations.M_02_Tables)}"))
            .Where(x => x.IsInterface == false)
            .OrderBy(x => x.Namespace)
            .ToList();
            return new ObservableCollection<String>(from i in MigrationTypes select i.Name);
        }
    }
    public DatabaseObjectScrapper()
    {
        
    }
}
