using System;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.Compression;
using Shared.Interfaces;

namespace Shared.Models.Acm
{
    /// <summary>
    ///     useful base class for deriving any chat codecs that will use ACM for decode and encode
    /// </summary>
    public abstract class AcmChatCodec : INetworkChatCodec
    {
        #region Constructor

        protected AcmChatCodec(WaveFormat recordFormat, WaveFormat encodeFormat)
        {
            RecordFormat = recordFormat;
            _encodeFormat = encodeFormat;
        }

        #endregion

        #region Properties

        private readonly WaveFormat _encodeFormat;

        private int _decodeSourceBytesLeftovers;

        private AcmStream _decodeStream;

        private int _encodeSourceBytesLeftovers;

        private AcmStream _encodeStream;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public WaveFormat RecordFormat { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int BitsPerSecond => _encodeFormat.AverageBytesPerSecond * 8;

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public byte[] Encode(byte[] data, int offset, int length)
        {
            if (_encodeStream == null)
                _encodeStream = new AcmStream(RecordFormat, _encodeFormat);
            //Debug.WriteLine(String.Format("Encoding {0} + {1} bytes", length, encodeSourceBytesLeftovers));
            return Convert(_encodeStream, data, offset, length, ref _encodeSourceBytesLeftovers);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public byte[] Decode(byte[] data, int offset, int length)
        {
            if (_decodeStream == null)
                _decodeStream = new AcmStream(_encodeFormat, RecordFormat);
            //Debug.WriteLine(String.Format("Decoding {0} + {1} bytes", data.Length, decodeSourceBytesLeftovers));
            return Convert(_decodeStream, data, offset, length, ref _decodeSourceBytesLeftovers);
        }

        public void Dispose()
        {
            if (_encodeStream != null)
            {
                _encodeStream.Dispose();
                _encodeStream = null;
            }
            if (_decodeStream != null)
            {
                _decodeStream.Dispose();
                _decodeStream = null;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                // determine if this codec is installed on this PC
                var available = true;
                try
                {
                    using (new AcmStream(RecordFormat, _encodeFormat))
                    {
                    }
                    using (new AcmStream(_encodeFormat, RecordFormat))
                    {
                    }
                }
                catch (MmException)
                {
                    available = false;
                }
                return available;
            }
        }

        /// <summary>
        /// Convert data to this current codec.
        /// </summary>
        /// <param name="conversionStream"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="sourceBytesLeftovers"></param>
        /// <returns></returns>
        private byte[] Convert(AcmStream conversionStream, byte[] data, int offset, int length,
            ref int sourceBytesLeftovers)
        {
            var bytesInSourceBuffer = length + sourceBytesLeftovers;
            Array.Copy(data, offset, conversionStream.SourceBuffer, sourceBytesLeftovers, length);
            var bytesConverted = conversionStream.Convert(bytesInSourceBuffer, out var sourceBytesConverted);
            sourceBytesLeftovers = bytesInSourceBuffer - sourceBytesConverted;
            if (sourceBytesLeftovers > 0)
                Array.Copy(conversionStream.SourceBuffer, sourceBytesConverted, conversionStream.SourceBuffer, 0,
                    sourceBytesLeftovers);
            var encoded = new byte[bytesConverted];
            Array.Copy(conversionStream.DestBuffer, 0, encoded, 0, bytesConverted);
            return encoded;
        }

        #endregion
    }
}