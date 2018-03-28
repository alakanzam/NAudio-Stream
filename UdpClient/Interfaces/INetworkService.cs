namespace NAudioClient.Interfaces
{
    public interface INetworkService
    {
        #region Properties

        /// <summary>
        ///     Instance to broadcast sound to server end-point.
        /// </summary>
        System.Net.Sockets.UdpClient Broadcaster { get; set; }
        
        /// <summary>
        /// Instance for listening data in udp connection.
        /// </summary>
        System.Net.Sockets.UdpClient Listener { get; set; }

        #endregion
    }
}