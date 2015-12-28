using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterStream
{
    public class InputFactory : ITypedInputAdapterFactory<InputConfig>
    {
        public InputAdapterBase Create<PayLoadType>(InputConfig config, EventShape shape)
        {
            return new Input(config);
        }
        public void Dispose()
        {

        }
    }
}
