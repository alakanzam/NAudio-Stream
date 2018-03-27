using System;
using System.Diagnostics;
using NAudio.Codecs;
using NAudio.Wave;
using Shared.Interfaces;

namespace Shared.Models
{
    public class G722ChatCodec : INetworkChatCodec
    {
        private readonly G722Codec codec;
        private readonly G722CodecState decoderState;
        private readonly G722CodecState encoderState;

        public G722ChatCodec()
        {
            BitsPerSecond = 64000;
            encoderState = new G722CodecState(BitsPerSecond, G722Flags.None);
            decoderState = new G722CodecState(BitsPerSecond, G722Flags.None);
            codec = new G722Codec();
            RecordFormat = new WaveFormat(16000, 1);
        }

        public string Name => "G.722 16kHz";

        public int BitsPerSecond { get; }

        public WaveFormat RecordFormat { get; }

        public byte[] Encode(byte[] data, int offset, int length)
        {
            if (offset != 0)
                throw new ArgumentException("G722 does not yet support non-zero offsets");
            var wb = new WaveBuffer(data);
            var encodedLength = length / 4;
            var outputBuffer = new byte[encodedLength];
            var encoded = codec.Encode(encoderState, outputBuffer, wb.ShortBuffer, length / 2);
            Debug.Assert(encodedLength == encoded);
            return outputBuffer;
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            if (offset != 0)
                throw new ArgumentException("G722 does not yet support non-zero offsets");
            var decodedLength = length * 4;
            var outputBuffer = new byte[decodedLength];
            var wb = new WaveBuffer(outputBuffer);
            var decoded = codec.Decode(decoderState, wb.ShortBuffer, data, length);
            Debug.Assert(decodedLength == decoded * 2); // because decoded is a number of samples
            return outputBuffer;
        }

        public void Dispose()
        {
            // nothing to do
        }

        public bool IsAvailable => true;
    }
}