using System;
using NAudio.Wave;
using Shared.Interfaces;

namespace Shared.Models
{
    public class UncompressedPcmChatCodec : INetworkChatCodec
    {
        #region Constructor

        public UncompressedPcmChatCodec()
        {
            RecordFormat = new WaveFormat(8000, 16, 1);
        }

        #endregion

        #region Properties

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public string Name => "PCM 8kHz 16 bit uncompressed";

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public WaveFormat RecordFormat { get; }

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] Encode(byte[] data, int offset, int length)
        {
            var encoded = new byte[length];
            Array.Copy(data, offset, encoded, 0, length);
            return encoded;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] Decode(byte[] data, int offset, int length)
        {
            var decoded = new byte[length];
            Array.Copy(data, offset, decoded, 0, length);
            return decoded;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public int BitsPerSecond => RecordFormat.AverageBytesPerSecond * 8;

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public bool IsAvailable => true;

        #endregion





    }
}