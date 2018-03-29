using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using GalaSoft.MvvmLight;
using NAudio.CoreAudioApi;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight.Command;
using NAudio.Wave;
using SoundClient.Enumeration;
using SoundClient.Model;

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
        /// List of playback devices.
        /// </summary>
        public ObservableCollection<MMDevice> PlaybackDevices { get; }

        /// <summary>
        /// Buffer size.
        /// </summary>
        public int BufferSize { get; set; } = 4000000;

        /// <summary>
        /// Device which is selected for voice recording purpose.
        /// </summary>
        public MMDevice SelectedRecorder { get; set; }

        /// <summary>
        /// Device which is selected for voice play.
        /// </summary>
        public MMDevice SelectedPlaybackDevice { get; set; }

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
            set
            {
                Set(nameof(RecorderStatus), ref _recorderStatus, value);
                RaisePropertyChanged(nameof(IsRecorderEnabled));
            }
        }

        /// <summary>
        /// Status of connection.
        /// </summary>
        private ConnectionStatus _connectionStatus;

        /// <summary>
        /// Status of connection.
        /// </summary>
        public ConnectionStatus ConnectionStatus
        {
            get { return _connectionStatus; }
            set { Set(nameof(ConnectionStatus), ref _connectionStatus, value); }
        }

        /// <summary>
        /// List of available roles for the current application.
        /// </summary>
        public ObservableCollection<RoleModel> AvailableRoles { get; }

        /// <summary>
        /// IP Address
        /// </summary>
        public string HostName { get; set; } = "127.0.0.1";

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; } = 9003;

        /// <summary>
        /// Role of current application.
        /// </summary>
        private RoleModel _selectedRole;

        /// <summary>
        /// Application role.
        /// </summary>
        public RoleModel SelectedRole
        {
            get { return _selectedRole; }
            set
            {
                Set(nameof(SelectedRole), ref _selectedRole, value);
                RaisePropertyChanged(nameof(IsRecorderEnabled));
            }
        }

        /// <summary>
        /// Whether recorder is available or not.
        /// </summary>
        public bool IsRecorderEnabled
        {
            get
            {
                if (SelectedRole == null)
                    return false;

                if (SelectedRole.Value == AppRole.Server)
                    return false;

                if (RecorderStatus == RecorderStatus.Recording || RecorderStatus == RecorderStatus.Recording)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Instance of recorder which is for voice recording.
        /// </summary>
        private IWaveIn _recorder;

        /// <summary>
        /// Instance for playing sound.
        /// </summary>
        private WaveOut _playbackDevice;

        /// <summary>
        /// Playback buffer.
        /// </summary>
        private BufferedWaveProvider _playbackBuffer;

        /// <summary>
        /// Stream to save data to.
        /// </summary>
        private MemoryStream _recordingStream;

        /// <summary>
        /// Instance to connet to stream server.
        /// </summary>
        private UdpClient _udpClient;

        /// <summary>
        /// Instance to listen to incoming connection.
        /// </summary>
        private TcpListener _tcpListener;

        /// <summary>
        /// Thread to listen to tcp clients.
        /// </summary>
        private Thread _tcpClientListeningThread;

        /// <summary>
        /// Stream for receiving client data.
        /// </summary>
        private NetworkStream _tcpNetworkStream;

        #endregion

        #region Relay commands

        /// <summary>
        /// Command which will be triggered when record button is clicked.
        /// </summary>
        public RelayCommand ClickRecordRelayCommand { get; private set; }

        /// <summary>
        /// Command which will be triggered when stop button is clicked.
        /// </summary>
        public RelayCommand ClickStopRelayCommand { get; private set; }

        /// <summary>
        /// Command which will be triggered when stop playing back button is clicked.
        /// </summary>
        public RelayCommand ClickStopPlayingBackRelayCommand { get; private set; }

        /// <summary>
        /// Command which will be trigger when connect button is clicked.
        /// </summary>
        public RelayCommand ClickConnectRelayCommand { get; private set; }

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
            PlaybackDevices = new ObservableCollection<MMDevice>(GetPlaybackDevices());

            // Get available roles.
            AvailableRoles = new ObservableCollection<RoleModel>(GetAvailableRoles());

            // By default, application plays as Client.
            SelectedRole = AvailableRoles[0];

            // Select the first recording device.
            SelectedRecorder = Recorders[0];

            // Select the first playback device.
            SelectedPlaybackDevice = PlaybackDevices[0];

            // Relay commands initialization.
            ClickRecordRelayCommand = new RelayCommand(ClickRecord);
            ClickStopRelayCommand = new RelayCommand(ClickStop);
            ClickConnectRelayCommand = new RelayCommand(ClickConnect);
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
        /// Get list of playback devices.
        /// </summary>
        /// <returns></returns>
        private IList<MMDevice> GetPlaybackDevices()
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            var playbackDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
            return playbackDevices;
        }

        /// <summary>
        /// Get the list of available roles.
        /// </summary>
        /// <returns></returns>
        private IList<RoleModel> GetAvailableRoles()
        {
            var availableRoles = new List<RoleModel>();
            availableRoles.Add(new RoleModel(nameof(AppRole.Client), AppRole.Client));
            availableRoles.Add(new RoleModel(nameof(AppRole.Server), AppRole.Server));

            return availableRoles;
        }

        /// <summary>
        /// Initialize an instance of recorder.
        /// </summary>
        /// <returns></returns>
        private void ClickRecord()
        {
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
        /// Event which will be trigged when stop button is clicked.
        /// </summary>
        private void ClickStop()
        {
            if (_recorder != null)
                _recorder.StopRecording();

            // Change to playback state.
            RecorderStatus = RecorderStatus.Initial;

            //// Read all recorded data.
            //var recordedData = _recordingStream.ToArray();



            //// Add sound to buffer.
            //_playbackBuffer.AddSamples(recordedData, 0, recordedData.Length);
            return;

        }

        /// <summary>
        /// Event which is raised when playback device is stopped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="stoppedEventArgs"></param>
        private void PlaybackDeviceOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            RecorderStatus = RecorderStatus.Initial;
        }

        /// <summary>
        /// Event which is triggered when connection button is clicked.
        /// </summary>
        public void ClickConnect()
        {
            if (SelectedRole == null)
                return;

            // End-point initialization.
            IPEndPoint endPoint = null;

            switch (SelectedRole.Value)
            {
                case AppRole.Client:
                    endPoint = new IPEndPoint(IPAddress.Parse(HostName), Port);
                    _udpClient = new UdpClient();
                    _udpClient.Connect(endPoint);
                    break;

                default:
                    endPoint = new IPEndPoint(IPAddress.Any, Port);
                    _tcpListener = new TcpListener(endPoint);
                    _tcpListener.Start();

                    _tcpClientListeningThread = new Thread(ListenToClients);
                    _tcpClientListeningThread.IsBackground = true;
                    _tcpClientListeningThread.Start();

                    // Initialize playback buffer.
                    _playbackBuffer = new BufferedWaveProvider(SelectedPlaybackDevice.AudioClient.MixFormat);
                    //_playbackBuffer = new BufferedWaveProvider(new WaveFormat());
                    _playbackBuffer.BufferLength = BufferSize;

                    // Initialize wave out device.
                    // Close the playback device as it has been intialized.
                    if (_playbackDevice != null)
                        _playbackDevice.Dispose();

                    // Initialize playback device.
                    _playbackDevice = new WaveOut();
                    _playbackDevice.Init(_playbackBuffer);
                    _playbackDevice.PlaybackStopped += PlaybackDeviceOnPlaybackStopped;
                    // Play sound.
                    _playbackDevice.Play();

                    break;
            }

            ConnectionStatus = ConnectionStatus.Connected;
        }

        /// <summary>
        /// Listen to clients.
        /// </summary>
        private void ListenToClients()
        {
            var tcpClient = _tcpListener.AcceptTcpClient();
            _tcpNetworkStream = tcpClient.GetStream();

            // Buffer size.
            var buffer = new byte[BufferSize];

            while (true)
            {
                var iByteRead = _tcpNetworkStream.Read(buffer, 0, BufferSize);
                if (iByteRead < 1)
                    continue;

                _playbackBuffer.ClearBuffer();
                _playbackBuffer.AddSamples(buffer, 0, iByteRead);
            }
        }

        /// <summary>
        /// Callback which is raised when data is available.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="waveInEventArgs"></param>
        private void RecorderOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            //_recordingStream.Write(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
            if (_tcpNetworkStream != null)
            {
                _tcpNetworkStream.Write(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
                _tcpNetworkStream.Flush();
            }
        }

        #endregion
    }
}