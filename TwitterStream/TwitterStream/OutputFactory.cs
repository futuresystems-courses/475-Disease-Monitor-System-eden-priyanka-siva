using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterStream
{
    public class OutputFactory : IOutputAdapterFactory<string>
    {
        public OutputAdapterBase Create(string StopSignalName, EventShape Shape, CepEventType EventType)
        {
            // we're ignoring the shape as it's only for a Point event but it's required by the interface
            return new Output(StopSignalName, EventType);
        }

        public void Dispose()
        {
        }
    }
}
