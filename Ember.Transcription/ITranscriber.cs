using Ember.DataSchemaManager.DataSchemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.Transcription;

internal interface ITranscriber
{
    String Transcribe(DataSchema Schema);
}
