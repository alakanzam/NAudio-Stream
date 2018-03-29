using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using NAudio.CoreAudioApi;
using System.Linq;

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
        public ObservableCollection<MMDevice> AudioDevices { get; }

        /// <summary>
        /// List of audio loopback devices.
        /// </summary>
        public ObservableCollection<MMDevice> AudioLoopbackDevices { get; }

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
            /// 
            AudioDevices = new ObservableCollection<MMDevice>(GetAudioDevices());
            AudioLoopbackDevices = new ObservableCollection<MMDevice>(GetLoopbackAudioDevices());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get audio devices list.
        /// </summary>
        /// <returns></returns>
        private IList<MMDevice> GetAudioDevices()
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();
            return devices;
        }

        /// <summary>
        /// Get audio loopback devices list.
        /// </summary>
        /// <returns></returns>
        private IList<MMDevice> GetLoopbackAudioDevices()
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            var loopbackAudioDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
            return loopbackAudioDevices;
        }

        #endregion
    }
}