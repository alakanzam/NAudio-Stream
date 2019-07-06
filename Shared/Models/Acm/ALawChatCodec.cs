using System;
using NAudio.Codecs;
using NAudio.Wave;
using Shared.Interfaces;

namespace Shared.Models.Acm
{
    public class ALawChatCodec : INetworkChatCodec
    {
        #region Properties

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public string Name => "G.711 a-law";

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public int BitsPerSecond => RecordFormat.SampleRate * 8;

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public WaveFormat RecordFormat => new WaveFormat(8000, 16, 1);

        #endregion

        #region Methods

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] Encode(byte[] data, int offset, int length)
        {
            var encoded = new byte[length / 2];
            var outIndex = 0;
            for (var n = 0; n < length; n += 2)
                encoded[outIndex++] = ALawEncoder.LinearToALawSample(BitConverter.ToInt16(data, offset + n));
            return encoded;
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] Decode(byte[] data, int offset, int length)
        {
            var decoded = new byte[length * 2];
            var outIndex = 0;
            for (var n = 0; n < length; n++)
            {
                var decodedSample = ALawDecoder.ALawToLinearSample(data[n + offset]);
                decoded[outIndex++] = (byte)(decodedSample & 0xFF);
                decoded[outIndex++] = (byte)(decodedSample >> 8);
            }
            return decoded;
        }

        public void Dispose()
        {
            // nothing to do
        }

        public bool IsAvailable => true;

        #endregion
    }
}