using System;
using NAudio.Codecs;
using NAudio.Wave;
using Shared.Interfaces;
using Shared.Models.Acm;

namespace Shared.Models.MuLaw
{
    public class AcmMuLawChatCodec : AcmChatCodec
    {

        #region Constructor

        public AcmMuLawChatCodec()
            : base(new WaveFormat(8000, 16, 1), WaveFormat.CreateMuLawFormat(8000, 1))
        {
        }

        #endregion

        #region Properties

        public override string Name => "ACM G.711 mu-law";

        #endregion
    }
}