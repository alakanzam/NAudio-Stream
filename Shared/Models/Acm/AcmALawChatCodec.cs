using NAudio.Wave;

namespace Shared.Models.Acm
{
    public class AcmALawChatCodec : AcmChatCodec
    {
        #region Constructors

        public AcmALawChatCodec()
            : base(new WaveFormat(8000, 16, 1), WaveFormat.CreateALawFormat(8000, 1))
        {
        }

        #endregion

        #region Properties

        public override string Name => "ACM G.711 a-law";

        #endregion
    }
}