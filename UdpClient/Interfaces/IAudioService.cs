using System.IO;
using NAudio.Wave;

namespace NAudioClient.Interfaces
{
    public interface IAudioService
    {
        #region Properties

        /// <summary>
        /// Recorder device.
        /// </summary>
        WaveIn Recorder { get; set; }

        /// <summary>
        /// Playback device.
        /// </summary>
        WaveOut Playback { get; set; }

        /// <summary>
        /// Buffer of playback device.
        /// </summary>
        BufferedWaveProvider PlaybackBuffer { get; set; }

        /// <summary>
        /// Stream which is for voice recording purpose.
        /// </summary>
        MemoryStream RecordingStream { get; set; }

        #endregion
    }
}