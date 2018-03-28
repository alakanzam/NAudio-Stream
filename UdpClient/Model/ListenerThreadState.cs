using System.Net;
using Shared.Interfaces;

namespace NAudioClient.Model
{
    public class ListenerThreadState
    {
        #region Properties

        /// <summary>
        /// End point address.
        /// </summary>
        public IPEndPoint EndPoint { get; }

        /// <summary>
        /// Current codec which is being used.
        /// </summary>
        public INetworkChatCodec Codec { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize listener thread state.
        /// </summary>
        public ListenerThreadState(IPEndPoint endPoint, INetworkChatCodec codec)
        {
            EndPoint = endPoint;
            Codec = codec;
        }

        #endregion
    }
}