using System.Collections.Generic;
using System.Diagnostics;

namespace Stabilograph.Protocol
{
    public interface IChannelReader
    {
        List<float> ReadWeights();
    }

    public class ChannelReader : IChannelReader
    {
        private readonly Protocol _protocol;

        public ChannelReader(Protocol protocol)
        {
            _protocol = protocol;
            protocol.Initialize();
        }

        public List<float> ReadWeights()
        {
            return ReadWeights(_protocol.ReadChannels());
        }

        public List<float> ReadWeights(int[] channels)
        {
            var values = new List<float>();

            for (var index = 0; index < channels.Length/2; index ++)
            {
                values.Add(channels[index*2] - channels[index*2 + 1]);
            }

            Debug.WriteLine("Channels combined " + string.Join(", ", values));

            return values;
        }
    }
}