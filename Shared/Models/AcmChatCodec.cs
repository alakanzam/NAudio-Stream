using System;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.Compression;
using Shared.Interfaces;

namespace Shared.Models
{
    /// <summary>
    ///     useful base class for deriving any chat codecs that will use ACM for decode and encode
    /// </summary>
    public abstract class AcmChatCodec : INetworkChatCodec
    {
        private readonly WaveFormat encodeFormat;
        private int decodeSourceBytesLeftovers;
        private AcmStream decodeStream;
        private int encodeSourceBytesLeftovers;
        private AcmStream encodeStream;

        protected AcmChatCodec(WaveFormat recordFormat, WaveFormat encodeFormat)
        {
            RecordFormat = recordFormat;
            this.encodeFormat = encodeFormat;
        }

        public WaveFormat RecordFormat { get; }

        public byte[] Encode(byte[] data, int offset, int length)
        {
            if (encodeStream == null)
                encodeStream = new AcmStream(RecordFormat, encodeFormat);
            //Debug.WriteLine(String.Format("Encoding {0} + {1} bytes", length, encodeSourceBytesLeftovers));
            return Convert(encodeStream, data, offset, length, ref encodeSourceBytesLeftovers);
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            if (decodeStream == null)
                decodeStream = new AcmStream(encodeFormat, RecordFormat);
            //Debug.WriteLine(String.Format("Decoding {0} + {1} bytes", data.Length, decodeSourceBytesLeftovers));
            return Convert(decodeStream, data, offset, length, ref decodeSourceBytesLeftovers);
        }

        public abstract string Name { get; }

        public int BitsPerSecond => encodeFormat.AverageBytesPerSecond * 8;

        public void Dispose()
        {
            if (encodeStream != null)
            {
                encodeStream.Dispose();
                encodeStream = null;
            }
            if (decodeStream != null)
            {
                decodeStream.Dispose();
                decodeStream = null;
            }
        }

        public bool IsAvailable
        {
            get
            {
                // determine if this codec is installed on this PC
                var available = true;
                try
                {
                    using (new AcmStream(RecordFormat, encodeFormat))
                    {
                    }
                    using (new AcmStream(encodeFormat, RecordFormat))
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

        private static byte[] Convert(AcmStream conversionStream, byte[] data, int offset, int length,
            ref int sourceBytesLeftovers)
        {
            var bytesInSourceBuffer = length + sourceBytesLeftovers;
            Array.Copy(data, offset, conversionStream.SourceBuffer, sourceBytesLeftovers, length);
            int sourceBytesConverted;
            var bytesConverted = conversionStream.Convert(bytesInSourceBuffer, out sourceBytesConverted);
            sourceBytesLeftovers = bytesInSourceBuffer - sourceBytesConverted;
            if (sourceBytesLeftovers > 0)
                Array.Copy(conversionStream.SourceBuffer, sourceBytesConverted, conversionStream.SourceBuffer, 0,
                    sourceBytesLeftovers);
            var encoded = new byte[bytesConverted];
            Array.Copy(conversionStream.DestBuffer, 0, encoded, 0, bytesConverted);
            return encoded;
        }
    }
}