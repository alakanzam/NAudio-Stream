﻿using NAudio.Wave;

namespace Shared.Models
{
    public class Gsm610ChatCodec : AcmChatCodec
    {
        public Gsm610ChatCodec()
            : base(new WaveFormat(8000, 16, 1), new Gsm610WaveFormat())
        {
        }

        public override string Name => "GSM 6.10";
    }
}