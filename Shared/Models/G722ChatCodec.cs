using System;
using System.Diagnostics;
using NAudio.Codecs;
using NAudio.Wave;
using Shared.Interfaces;

namespace Shared.Models
{
    public class G722ChatCodec : INetworkChatCodec
    {
        #region Properties

        /// <summary>
        /// Codec instance.
        /// </summary>
        private readonly G722Codec _codec;

        /// <summary>
        /// State of decoder.
        /// </summary>
        private readonly G722CodecState _decoderState;

        /// <summary>
        /// State of encoder.
        /// </summary>
        private readonly G722CodecState _encoderState;

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public string Name => "G.722 16kHz";

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public int BitsPerSecond { get; }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public WaveFormat RecordFormat { get; }

        #endregion

        #region Constructor

        public G722ChatCodec()
        {
            BitsPerSecond = 64000;
            _encoderState = new G722CodecState(BitsPerSecond, G722Flags.None);
            _decoderState = new G722CodecState(BitsPerSecond, G722Flags.None);
            _codec = new G722Codec();
            RecordFormat = new WaveFormat(16000, 1);
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public byte[] Encode(byte[] data, int offset, int length)
        {
            if (offset != 0)
                throw new ArgumentException("G722 does not yet support non-zero offsets");
            var wb = new WaveBuffer(data);
            var encodedLength = length / 4;
            var outputBuffer = new byte[encodedLength];
            var encoded = _codec.Encode(_encoderState, outputBuffer, wb.ShortBuffer, length / 2);
            Debug.Assert(encodedLength == encoded);
            return outputBuffer;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public byte[] Decode(byte[] data, int offset, int length)
        {
            if (offset != 0)
                throw new ArgumentException("G722 does not yet support non-zero offsets");
            var decodedLength = length * 4;
            var outputBuffer = new byte[decodedLength];
            var wb = new WaveBuffer(outputBuffer);
            var decoded = _codec.Decode(_decoderState, wb.ShortBuffer, data, length);

#if DEBUG
            Debug.Assert(decodedLength == decoded * 2); // because decoded is a number of samples
#endif
            return outputBuffer;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public void Dispose()
        {
            // nothing to do
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public bool IsAvailable => true;

        #endregion
    }
}