using System;
using NAudio.Codecs;
using NAudio.Wave;
using Shared.Interfaces;

namespace Shared.Models.Acm
{
    public class AcmALawChatCodec : AcmChatCodec
    {
        public AcmALawChatCodec()
            : base(new WaveFormat(8000, 16, 1), WaveFormat.CreateALawFormat(8000, 1))
        {
        }

        public override string Name => "ACM G.711 a-law";
    }


    
}