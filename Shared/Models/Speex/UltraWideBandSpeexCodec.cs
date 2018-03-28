using NSpeex;

namespace Shared.Models.Speex
{
    public class UltraWideBandSpeexCodec : SpeexChatCodec
    {
        #region Constructor

        public UltraWideBandSpeexCodec() :
            base(BandMode.UltraWide, 32000, "Speex Ultra Wide Band (32kHz)")
        {
        }

        #endregion
    }
}