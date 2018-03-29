using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using GalaSoft.MvvmLight;
using NAudio.CoreAudioApi;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using GalaSoft.MvvmLight.Command;
using NAudio.Wave;
using SoundClient.Enumeration;

namespace SoundClient.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// List of audio devices.
        /// </summary>
        public ObservableCollection<MMDevice> Recorders { get; }

        /// <summary>
        /// Device which is seleected for voice recording purpose.
        /// </summary>
        public MMDevice SelectedRecorder { get; set; }

        /// <summary>
        /// Status of recorder.
        /// </summary>
        private RecorderStatus _recorderStatus;

        /// <summary>
        /// Status of record.
        /// </summary>
        public RecorderStatus RecorderStatus
        {
            get { return _recorderStatus; }
            set { Set(nameof(RecorderStatus), ref _recorderStatus, value); }
        }

        /// <summary>
        /// Instance of recorder which is for voice recording.
        /// </summary>
        private IWaveIn _recorder;

        /// <summary>
        /// Instance for playing sound.
        /// </summary>
        private WaveOut _playback;

        /// <summary>
        /// Playback buffer.
        /// </summary>
        private BufferedWaveProvider _playbackBuffer;

        /// <summary>
        /// Stream to save data to.
        /// </summary>
        private MemoryStream _recordingStream;

        #endregion

        #region Relay commands
        
        /// <summary>
        /// Command which will be triggered when 
        /// </summary>
        public RelayCommand ClickToggleVoiceRecorder { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}

            // Initialize recorder stream
            _recordingStream = new MemoryStream();
            
            // Get available recording devices in computer.
            Recorders = new ObservableCollection<MMDevice>(GetRecorders());

            // Select the first recording device.
            SelectedRecorder = Recorders[0];

            // Relay commands initialization.
            ClickToggleVoiceRecorder = new RelayCommand(ClickTogglerRecorder);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get audio devices list.
        /// </summary>
        /// <returns></returns>
        private IList<MMDevice> GetRecorders()
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();
            return devices;
        }

        /// <summary>
        /// Initialize an instance of recorder.
        /// </summary>
        /// <returns></returns>
        private void ClickTogglerRecorder()
        {
            // Device is recording. Stop it
            if (RecorderStatus == RecorderStatus.Recording)
            {
                if (_recorder != null)
                    _recorder.StopRecording();

                // Change to playback state.
                RecorderStatus = RecorderStatus.Playing;
                
                // Read all recorded data.
                var recordedData = _recordingStream.ToArray();

                // Initialize playback buffer.
                _playbackBuffer = new BufferedWaveProvider(_recorder.WaveFormat);
                _playbackBuffer.BufferLength = recordedData.Length;

                // Initialize wave out device.
                // Close the playback device as it has been intialized.
                if (_playback != null)
                    _playback.Dispose();

                // Initialize playback device.
                _playback = new WaveOut();
                _playback.Init(_playbackBuffer);

                // Play sound.
                _playback.Play();
                
                // Add sound to buffer.
                _playbackBuffer.AddSamples(recordedData, 0, recordedData.Length);
                return;
            }

            // Device is playing the recorded sound.
            if (RecorderStatus == RecorderStatus.Playing)
            {
                // Playback device hasn't been disposed yet.
                if (_playback == null)
                    return;
                
                _playback.Stop();
                _playback.Dispose();
                RecorderStatus = RecorderStatus.Initial;
                return;
            }

            // Initialize recorder as it is not available.
            if (_recorder == null)
            {
                _recorder = new WasapiCapture(SelectedRecorder);
                _recorder.DataAvailable += RecorderOnDataAvailable;
            }

            // Clear the stream.
            _recordingStream = new MemoryStream();

            // Mark recorder status as recording.
            RecorderStatus = RecorderStatus.Recording;
            _recorder.StartRecording();
        }

        /// <summary>
        /// Callback which is raised when data is available.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="waveInEventArgs"></param>
        private void RecorderOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            _recordingStream.Write(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
        }

        #endregion
    }
}