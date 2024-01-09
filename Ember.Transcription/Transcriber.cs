﻿using Ember.Transcription.RDBMS.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ember.DataSchemaManager.DataSchemas;
using Ember.Transcription.RDBMS.PostgreSql;

namespace Ember.Transcription;

public class Transcriber
{
    public DataSchema Schema { get; set; }
    public DatabaseProviderEnum DatabaseProvider { get; set; }
    public Transcriber(DataSchema Schema)
    {
        this.Schema = Schema;
        this.DatabaseProvider = Schema.DatabaseProvider;
    }
    public String Transcribe()
    {
        // TODO : when transcribing any SQL DB type make a rule function to mkae it more adhesive to that SQL Provider type. 
        // TODO : find out what he meant above. 
        if (DatabaseProvider == DatabaseProviderEnum.SqlServer) return new SqlServerTranscriber().Transcribe(Schema);
        if (DatabaseProvider == DatabaseProviderEnum.PostgreSql) return new PostgreSqlTranscriber().Transcribe(Schema);
        return null!;
    }
}
