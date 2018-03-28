using NAudio.Wave;
using Shared.Models.Acm;

namespace Shared.Models
{
    /// <summary>
    ///     DSP Group TrueSpeech codec, using ACM
    ///     n.b. Windows XP came with a TrueSpeech codec built in
    ///     - looks like Windows 7 doesn't
    /// </summary>
    public class TrueSpeechChatCodec : AcmChatCodec
    {
        #region Constructor

        public TrueSpeechChatCodec()
            : base(new WaveFormat(8000, 16, 1), new TrueSpeechWaveFormat())
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public override string Name => "DSP Group TrueSpeech";

        #endregion
    }
}