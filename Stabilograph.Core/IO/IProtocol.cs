using System.Collections.Generic;

namespace Stabilograph.Core.IO
{
    public interface IProtocol
    {
        float[] ReadWeights();
    }
}