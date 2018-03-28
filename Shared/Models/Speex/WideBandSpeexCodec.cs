using NSpeex;

namespace Shared.Models.Speex
{
    public class WideBandSpeexCodec : SpeexChatCodec
    {
        #region Constructor

        public WideBandSpeexCodec() :
            base(BandMode.Wide, 16000, "Speex Wide Band (16kHz)")
        {
        }

        #endregion
    }
}