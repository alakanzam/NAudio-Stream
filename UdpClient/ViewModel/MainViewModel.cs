using System;
using GalaSoft.MvvmLight;
using NAudio.Wave;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Shared.Interfaces;
using Shared.Models;
using System.Linq;
using System.Windows.Threading;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Command;
using UdpClient.Enumerations;
using UdpClient.Interfaces;

namespace UdpClient.ViewModel
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
        public ObservableCollection<WaveInCapabilities> AudioDevices { get; private set; }

        /// <summary>
        /// List of audio codecs.
        /// </summary>
        public ObservableCollection<INetworkChatCodec> AudioCodecs { get; private set; }

        /// <summary>
        /// Device which is being selected.
        /// </summary>
        public WaveInCapabilities SelectedAudioDevice { get; set; }

        /// <summary>
        /// Current selected audio codecs
        /// </summary>
        public INetworkChatCodec SelectedAudioCodec { get; set; }

        /// <summary>
        /// IP address.
        /// </summary>
        public string IPAddress { get; set; } = "localhost";

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; } = 9003;

        /// <summary>
        /// Status of current application.
        /// </summary>
        private ApplicationStatus _applicationStatus;

        /// <summary>
        /// Status of current application.
        /// </summary>
        public ApplicationStatus Status
        {
            get { return _applicationStatus; }
            set
            {
                Set(nameof(ApplicationStatus), ref _applicationStatus, value);
                RaisePropertyChanged(nameof(IsRecordButtonAvailable));
                RaisePropertyChanged(nameof(IsStopRecordingButtonAvailable));
                RaisePropertyChanged(nameof(IsConnectButtonAvailable));
            }
        }

        /// <summary>
        /// Service which holds instance of recorder and playback device.
        /// </summary>
        private readonly IAudioService _audioService;

        /// <summary>
        /// Whether connect button is available or not.
        /// </summary>
        public bool IsConnectButtonAvailable
        {
            get
            {
                if (Status == ApplicationStatus.ConnectedUdp)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Whether record button is available on screen or not.
        /// </summary>
        public bool IsRecordButtonAvailable
        {
            get
            {
                if (Status == ApplicationStatus.Initial)
                    return false;

                if (Status == ApplicationStatus.Recording)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Whether stop recording button is available or not.
        /// </summary>
        public bool IsStopRecordingButtonAvailable
        {
            get
            {
                if (Status == ApplicationStatus.Initial)
                    return false;

                if (Status == ApplicationStatus.ConnectedUdp)
                    return false;

                return true;
            }
        }
        #endregion

        #region Relay commands

        /// <summary>
        /// Command which should be triggered when record button is clicked.
        /// </summary>
        public RelayCommand ClickRecordRelayCommand { get; private set; }

        /// <summary>
        /// Command which should be triggered when stop button is clicked.
        /// </summary>
        public RelayCommand ClickStopRelayCommand { get; private set; }

        /// <summary>
        /// Command which should be trigger when connect button is clicked.
        /// </summary>
        public RelayCommand ClickConnectRelayCommand { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            // Get list of audio device.
            AudioDevices = GetAudioDevices();
            AudioCodecs = GetAudioCodecs();

            // Initialize audio service.
            _audioService = ServiceLocator.Current.GetInstance<IAudioService>();

            // Relay command initialization.
            ClickRecordRelayCommand = new RelayCommand(ClickRecord);
            ClickStopRelayCommand = new RelayCommand(ClickStop);
            ClickConnectRelayCommand = new RelayCommand(ClickConnect);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get list of audio devices.
        /// </summary>
        private ObservableCollection<WaveInCapabilities> GetAudioDevices()
        {
            var audioDevices = new List<WaveInCapabilities>();
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                var capabilities = WaveIn.GetCapabilities(n);
                audioDevices.Add(capabilities);
            }

            // Select the first item.
            SelectedAudioDevice = audioDevices[0];
            return new ObservableCollection<WaveInCapabilities>(audioDevices);
        }

        /// <summary>
        /// Get list of audio codecs.
        /// </summary>
        private ObservableCollection<INetworkChatCodec> GetAudioCodecs()
        {
            var codecs = new List<INetworkChatCodec>() {new AcmALawChatCodec(), new ALawChatCodec(), new G722ChatCodec(), new Gsm610ChatCodec(), new MicrosoftAdpcmChatCodec(), new AcmMuLawChatCodec(),
                new NarrowBandSpeexCodec(), new WideBandSpeexCodec(), new UltraWideBandSpeexCodec(), new TrueSpeechChatCodec(), new UncompressedPcmChatCodec() };

            var sorted = from codec in codecs
                         where codec.IsAvailable
                         orderby codec.BitsPerSecond ascending
                         select codec;

            foreach (var codec in sorted)
            {
                var bitRate = codec.BitsPerSecond == -1 ? "VBR" : String.Format("{0:0.#}kbps", codec.BitsPerSecond / 1000.0);
                var text = String.Format("{0} ({1})", codec.Name, bitRate);
            }

            // Select the first code.
            SelectedAudioCodec = sorted.FirstOrDefault();
            return new ObservableCollection<INetworkChatCodec>(codecs);
        }

        /// <summary>
        /// Event which is triggered when record button is clicked.
        /// </summary>
        private void ClickRecord()
        {
            var recorder = new WaveIn();
            recorder.BufferMilliseconds = 50;
            recorder.DeviceNumber = AudioDevices.IndexOf(SelectedAudioDevice);
            recorder.WaveFormat = SelectedAudioCodec.RecordFormat;
            recorder.DataAvailable += OnSoundBeingRecorded;

            // Initialize buffer.
            _audioService.PlaybackBuffer = new BufferedWaveProvider(SelectedAudioCodec.RecordFormat);

            // Initialize playback device.
            var playback = new WaveOut();
            playback.Init(_audioService.PlaybackBuffer);
            _audioService.Playback = playback;

            // Clear the recording stream.
            _audioService.RecordingStream = new MemoryStream();
            _audioService.Recorder = recorder;

            Status = ApplicationStatus.Recording;

            // Start sound recording.
            recorder.StartRecording();
        }

        /// <summary>
        /// Event which is raised when user clicks on stop button.
        /// </summary>
        public void ClickStop()
        {
            // Stop recording.
            _audioService.Recorder.StopRecording();

            _audioService.Playback.Play();

            // Read all data from stream
            var data = _audioService.RecordingStream.ToArray();
            _audioService.PlaybackBuffer.BufferLength = data.Length;
            _audioService.PlaybackBuffer.AddSamples(data, 0, data.Length);

            // Stop recording.
            Status = ApplicationStatus.ConnectedUdp;
        }

        /// <summary>
        /// Event which will be raised when sound is being recorded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="waveInEventArgs"></param>
        private async void OnSoundBeingRecorded(object sender, WaveInEventArgs waveInEventArgs)
        {
            await _audioService.RecordingStream.WriteAsync(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
        }

        /// <summary>
        /// Event which is trigger when connect button is clicked.
        /// </summary>
        private void ClickConnect()
        {
            Status = ApplicationStatus.ConnectedUdp;
        }

        #endregion
    }
}