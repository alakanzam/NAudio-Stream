using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using UdpSoundClient.Enumeration;

namespace UdpSoundClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Constructor

        /// <summary>
        ///     Initialize view model with settings.
        /// </summary>
        public MainViewModel()
        {
            _mediaCapture = new MediaCapture();

            // Relay commands loading.
            LoadMediaCaptureRelayCommand = new RelayCommand(LoadMediaCaptureCommand);
            RecordAudioRelayCommand = new RelayCommand(RecordAudioCommand);
            StopRecordingRelayCommand = new RelayCommand<MediaElement>(StopCapture);
            IntializeUdpConnectionRelayCommand = new RelayCommand(IntializeUdpConnection);
        }

        #endregion

        #region Relay commands

        /// <summary>
        ///     Command to load media capture command.
        /// </summary>
        public RelayCommand LoadMediaCaptureRelayCommand { get; }

        /// <summary>
        /// Command to capture audio.
        /// </summary>
        public RelayCommand RecordAudioRelayCommand { get; }

        /// <summary>
        /// Command to stop voice recording progress.
        /// </summary>
        public RelayCommand<MediaElement> StopRecordingRelayCommand { get; }

        /// <summary>
        /// Command to initialize udp connection.
        /// </summary>
        public RelayCommand IntializeUdpConnectionRelayCommand { get; }

        /// <summary>
        /// Udp client instance to broadcast signal to.
        /// </summary>
        private UdpClient _udpClient;

        public string HostName { get; set; } = "192.168.1.12";

        public string Port { get; set; } = "9003";

        private bool _bIsConnectionBroadcasted;

        /// <summary>
        /// Check whether connection has been broadcasted or not.
        /// </summary>
        public bool IsConnectionBroadcasted
        {
            get { return _bIsConnectionBroadcasted; }
            private set { Set(nameof(IsConnectionBroadcasted), ref _bIsConnectionBroadcasted, value); }
        }

        private const int MaxUdpPackageSize = 512;


        #endregion

        #region Methods

        /// <summary>
        ///     Load media capture command.
        /// </summary>
        private async void LoadMediaCaptureCommand()
        {
            var settings = new MediaCaptureInitializationSettings();
            settings.StreamingCaptureMode = StreamingCaptureMode.Audio;
            settings.MediaCategory = MediaCategory.Other;
            settings.AudioProcessing = AudioProcessing.Default;



            IsAudioCaptureInitialized = true;
            await _mediaCapture.InitializeAsync(settings);
        }

        /// <summary>
        /// Record audio command.
        /// </summary>
        private async void RecordAudioCommand()
        {
            try
            {
                // Initialize recording stream.
                _recordingStream = new InMemoryRandomAccessStream();

                Debug.WriteLine("Starting record");
                var recordProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Auto);
                await _mediaCapture.StartRecordToStreamAsync(recordProfile, _recordingStream);


                Debug.WriteLine("Start Record successful");
                Status = RecorderStatus.Recording;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to capture audio");
            }
        }

        /// <summary>
        /// Stop voice recording.
        /// </summary>
        private async void StopCapture(MediaElement mediaElement)
        {
            if (Status != RecorderStatus.Recording)
                return;

            Debug.WriteLine("Stopping recording");
            await _mediaCapture.StopRecordAsync();
            Debug.WriteLine("Stop recording successful");
            Status = RecorderStatus.Initial;

            _recordingStream.Seek(0);
            //mediaElement.AutoPlay = true;
            //mediaElement.SetSource(_recordingStream, "");
            //mediaElement.Play();

            //var buffer = new Windows.Storage.Streams.Buffer(4000000);
            //await _recordingStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);

            //var bytes = buffer.ToArray();

            var clientDatagramSocket = new Windows.Networking.Sockets.DatagramSocket();
            var hostName = new Windows.Networking.HostName(HostName);
            await clientDatagramSocket.ConnectAsync(hostName, Port);
            using (var serverDatagramSocket = new Windows.Networking.Sockets.DatagramSocket())
            {
                using (Stream outputStream = (await serverDatagramSocket.GetOutputStreamAsync(hostName, Port)).AsStreamForWrite())
                {
                    var buffer = new Windows.Storage.Streams.Buffer(MaxUdpPackageSize);
                    while (true)
                    {
                        var retrievedBuffer = _recordingStream.ReadAsync(buffer, MaxUdpPackageSize, InputStreamOptions.None).GetResults();
                        if (retrievedBuffer == null || retrievedBuffer.Length < 1)
                            break;

                        outputStream.Write(retrievedBuffer.ToArray(), 0, (int) retrievedBuffer.Length);
                        outputStream.Flush();
                    }
                }
            }
        }

        /// <summary>
        /// Connect client to server.
        /// </summary>
        private async void IntializeUdpConnection()
        {
            //_mediaCapture.
            IsConnectionBroadcasted = true;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Instance of media capture device.
        /// </summary>
        private readonly MediaCapture _mediaCapture;

        /// <summary>
        ///     Whether audio capture instance has been initialized or not.
        /// </summary>
        private bool _bIsAudioCaptureInitialized;

        /// <summary>
        /// Stream which is for storing recording data.
        /// </summary>
        private IRandomAccessStream _recordingStream;

        private RecorderStatus _status;

        public RecorderStatus Status
        {
            get => _status;
            private set
            {
                Set(nameof(Status), ref _status, value);
                RaisePropertyChanged(nameof(IsAbleToRecordAudio));
                RaisePropertyChanged(nameof(IsAbleToStopRecordingAudio));
            }
        }

        /// <summary>
        ///     Whether audio capture instance has been initialized or not.
        /// </summary>
        public bool IsAudioCaptureInitialized
        {
            get => _bIsAudioCaptureInitialized;
            private set
            {
                Set(nameof(IsAudioCaptureInitialized), ref _bIsAudioCaptureInitialized, value);
                RaisePropertyChanged(nameof(IsAbleToRecordAudio));
            }
        }

        /// <summary>
        /// Whether device can record audio or not.
        /// </summary>
        public bool IsAbleToRecordAudio
        {
            get
            {
                // Audio hasn't been initialized.
                if (!IsAudioCaptureInitialized)
                    return false;

                // Device is recording.
                if (Status == RecorderStatus.Recording)
                    return false;

                return true;
            }
        }

        public bool IsAbleToStopRecordingAudio
        {
            get
            {
                // Audio is not being recorded.
                if (Status != RecorderStatus.Recording)
                    return false;

                return true;
            }
        }

        #endregion
    }
}