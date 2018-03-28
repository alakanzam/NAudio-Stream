using System;
using System.Diagnostics;
using NAudio.Wave;
using NSpeex;
using Shared.Interfaces;

namespace Shared.Models.Speex
{
    public abstract class SpeexChatCodec : INetworkChatCodec
    {
        #region Constructor

        protected SpeexChatCodec(BandMode bandMode, int sampleRate, string description)
        {
            _decoder = new SpeexDecoder(bandMode);
            _encoder = new SpeexEncoder(bandMode);
            RecordFormat = new WaveFormat(sampleRate, 16, 1);
            Name = description;
            _encoderInputBuffer = new WaveBuffer(RecordFormat.AverageBytesPerSecond); // more than enough
        }

        #endregion

        #region Properties

        private readonly SpeexDecoder _decoder;
        private readonly SpeexEncoder _encoder;
        private readonly WaveBuffer _encoderInputBuffer;

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public int BitsPerSecond => -1;

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public WaveFormat RecordFormat { get; }

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
            FeedSamplesIntoEncoderInputBuffer(data, offset, length);
            var samplesToEncode = _encoderInputBuffer.ShortBufferCount;
            if (samplesToEncode % _encoder.FrameSize != 0)
                samplesToEncode -= samplesToEncode % _encoder.FrameSize;
            var outputBufferTemp = new byte[length]; // contains more than enough space
            var bytesWritten = _encoder.Encode(_encoderInputBuffer.ShortBuffer, 0, samplesToEncode, outputBufferTemp, 0,
                length);
            var encoded = new byte[bytesWritten];
            Array.Copy(outputBufferTemp, 0, encoded, 0, bytesWritten);
            ShiftLeftoverSamplesDown(samplesToEncode);
            Debug.WriteLine("NSpeex: In {0} bytes, encoded {1} bytes [enc frame size = {2}]", length, bytesWritten,
                _encoder.FrameSize);
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
            var outputBufferTemp = new byte[length * 320];
            var wb = new WaveBuffer(outputBufferTemp);
            var samplesDecoded = _decoder.Decode(data, offset, length, wb.ShortBuffer, 0, false);
            var bytesDecoded = samplesDecoded * 2;
            var decoded = new byte[bytesDecoded];
            Array.Copy(outputBufferTemp, 0, decoded, 0, bytesDecoded);
            Debug.WriteLine("NSpeex: In {0} bytes, decoded {1} bytes [dec frame size = {2}]", length, bytesDecoded,
                _decoder.FrameSize);
            return decoded;
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public void Dispose()
        {
            // nothing to do
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public bool IsAvailable => true;

        private void ShiftLeftoverSamplesDown(int samplesEncoded)
        {
            var leftoverSamples = _encoderInputBuffer.ShortBufferCount - samplesEncoded;
            Array.Copy(_encoderInputBuffer.ByteBuffer, samplesEncoded * 2, _encoderInputBuffer.ByteBuffer, 0,
                leftoverSamples * 2);
            _encoderInputBuffer.ShortBufferCount = leftoverSamples;
        }

        private void FeedSamplesIntoEncoderInputBuffer(byte[] data, int offset, int length)
        {
            Array.Copy(data, offset, _encoderInputBuffer.ByteBuffer, _encoderInputBuffer.ByteBufferCount, length);
            _encoderInputBuffer.ByteBufferCount += length;
        }

        #endregion
    }
}