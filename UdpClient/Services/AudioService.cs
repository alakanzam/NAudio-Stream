using System.IO;
using NAudio.Wave;
using NAudioClient.Interfaces;

namespace NAudioClient.Services
{
    public class AudioService : IAudioService
    {
        #region Properties

        /// <summary>
        /// Recorder instance.
        /// </summary>
        public WaveIn Recorder { get; set; }

        /// <summary>
        /// Playback instance.
        /// </summary>
        public WaveOut Playback { get; set; }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public MemoryStream RecordingStream { get; set; }

        /// <summary>
        /// Initialize playback buffer.
        /// </summary>
        public BufferedWaveProvider PlaybackBuffer { get; set; }

        #endregion
    }
}