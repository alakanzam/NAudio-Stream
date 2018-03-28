using NSpeex;

namespace Shared.Models.Speex
{
    public class NarrowBandSpeexCodec : SpeexChatCodec
    {
        #region Constructors

        public NarrowBandSpeexCodec() :
            base(BandMode.Narrow, 8000, "Speex Narrow Band")
        {
        }

        #endregion
    }
}