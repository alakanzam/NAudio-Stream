using NAudioClient.Interfaces;

namespace NAudioClient.Services
{
    public class NetworkService : INetworkService
    {
        #region Properties

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public System.Net.Sockets.UdpClient Broadcaster { get; set; }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public System.Net.Sockets.UdpClient Listener { get; set; }

        #endregion
    }
}