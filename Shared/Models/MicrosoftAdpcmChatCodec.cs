using NAudio.Wave;
using Shared.Models.Acm;

namespace Shared.Models
{
    public class MicrosoftAdpcmChatCodec : AcmChatCodec
    {
        #region Constructor

        public MicrosoftAdpcmChatCodec()
            : base(new WaveFormat(8000, 16, 1), new AdpcmWaveFormat(8000, 1))
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string Name => "Microsoft ADPCM";

        #endregion
    }
}