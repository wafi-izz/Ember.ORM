using Ember.DataStructure.Base;
using Ember.DataStructure.Base.DatabaseObjects;
using Ember.Transcription;
using Ember.DataScheme.Schemas;
using System;
using System.Linq;

namespace Ember.DataStructure;

public class Init
{
    public Init()
    {
        
    }
    public static String Main()
    {
        DatabaseObjectScrapper Scrapper = new DatabaseObjectScrapper();
        Schema Schema = new Schema(Scrapper.TableList);

        var MigrationTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(IMigratablesDictionary).IsAssignableFrom(x))
            .Where(x => x.Namespace!.StartsWith($"{nameof(Ember)}.{nameof(DataStructure)}.{nameof(Migrations)}"))
            .Where(x => x.IsInterface == false)
            .OrderBy(x => x.Namespace)
            .ToList();
        foreach (var type in MigrationTypes)
        {
            IMigratablesDictionary Migration = (IMigratablesDictionary)Activator.CreateInstance(type)!;
            var t = type.GetType().Name;
            Migration.Schema = Schema;
            Migration.Down();
            Migration.Up();
        }
        Transcriber Transcriber = new Transcriber(Schema, SqlTypeEnum.SqlServer);
        String GeneratedScript = Transcriber.Transcribe();
        return GeneratedScript; //"a generated database script ... hopefully"
    }
}
