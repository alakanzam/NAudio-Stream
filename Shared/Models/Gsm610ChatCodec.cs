using NAudio.Wave;
using Shared.Models.Acm;

namespace Shared.Models
{
    public class Gsm610ChatCodec : AcmChatCodec
    {
        #region Constructor

        public Gsm610ChatCodec()
            : base(new WaveFormat(8000, 16, 1), new Gsm610WaveFormat())
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public override string Name => "GSM 6.10";

        #endregion
    }
}