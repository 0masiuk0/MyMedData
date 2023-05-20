using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using NTwain;
using NTwain.Data;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace MyMedData.Classes
{
	internal class Scanner : IDisposable
	{
		ImageCodecInfo _tiffCodecInfo;
		TwainSession _twain;
		bool _stopScan;
		bool _loadingCaps;
		IntPtr _ownerWindowHandle;
		Window _ownerWindow;
		DataSource dataSourse;

		public Scanner(Window ownerWindow)
		{
			foreach (var enc in ImageCodecInfo.GetImageEncoders())
			{
				if (enc.MimeType == "image/tiff") { _tiffCodecInfo = enc; break; }
			}
			_ownerWindow = ownerWindow;
			_ownerWindowHandle = new WindowInteropHelper(ownerWindow).Handle;

			if (SettingsManager.AppSettings["scanner_name"].Value is string srcName)
			{
				DataSource? source = GetSourceList().FirstOrDefault(s => s.Name == srcName, null);
				if (source != null && source.Open() == ReturnCode.Success)
					dataSourse = source;
				else
					throw new Exception("Не удается открыть сессию со сканером.");

				SetupTwain();
			}
			else
			{
				throw new Exception("Сканер не выбран");
			}
		}

		private void SetupTwain()
		{
			var appId = TWIdentity.CreateFromAssembly(DataGroups.Image, Assembly.GetEntryAssembly());
			_twain = new TwainSession(appId);
			_twain.StateChanged += (s, e) =>
			{
				PlatformInfo.Current.Log.Info("State changed to " + _twain.State + " on thread " + Thread.CurrentThread.ManagedThreadId);
			};
			_twain.TransferError += (s, e) =>
			{
				PlatformInfo.Current.Log.Info("Got xfer error on thread " + Thread.CurrentThread.ManagedThreadId);
			};
			_twain.DataTransferred += (s, e) =>
			{
				PlatformInfo.Current.Log.Info("Transferred data event on thread " + Thread.CurrentThread.ManagedThreadId);

				// example on getting ext image info
				var infos = e.GetExtImageInfo(ExtendedImageInfo.Camera).Where(it => it.ReturnCode == ReturnCode.Success);
				foreach (var it in infos)
				{
					var values = it.ReadValues();
					PlatformInfo.Current.Log.Info(string.Format("{0} = {1}", it.InfoID, values.FirstOrDefault()));
					break;
				}

				// handle image data
				if (e.NativeData != IntPtr.Zero)
				{
					var stream = e.GetNativeImageStream();
					if (stream != null)
					{
						_image = System.Drawing.Image.FromStream(stream);
					}
				}
				else if (!string.IsNullOrEmpty(e.FileDataPath))
				{
					_image = new Bitmap(e.FileDataPath);
				}

				if (_image != null)
					OnTransferComplete();
			};
			_twain.SourceDisabled += (s, e) =>
			{
				PlatformInfo.Current.Log.Info("Source disabled event on thread " + Thread.CurrentThread.ManagedThreadId);
				OnTwainSorseDisabled();
				LoadSourceCaps();
			};
			_twain.TransferReady += (s, e) =>
			{
				PlatformInfo.Current.Log.Info("Transferr ready event on thread " + Thread.CurrentThread.ManagedThreadId);
				e.CancelAll = _stopScan;
			};

			// either set sync context and don't worry about threads during events,
			// or don't and use control.invoke during the events yourself
			PlatformInfo.Current.Log.Info("Setup thread = " + Thread.CurrentThread.ManagedThreadId);
			_twain.SynchronizationContext = SynchronizationContext.Current;
			if (_twain.State < 3)
			{
				// use this for internal msg loop
				_twain.Open();
				// use this to hook into current app loop
				//_twain.Open(new WindowsFormsMessageLoopHook(this.Handle));
			}

		}		

		public ReturnCode StartScan()
		{
			_image = null;

			if (_twain.State == 4)
			{
				//_twain.CurrentSource.CapXferCount.Set(4);

				_stopScan = false;

				if (_twain.CurrentSource.Capabilities.CapUIControllable.IsSupported)//.SupportedCaps.Contains(CapabilityId.CapUIControllable))
				{
					// hide scanner ui if possible
					return _twain.CurrentSource.Enable(SourceEnableMode.NoUI, false, _ownerWindowHandle);
				}
				else
				{
					return _twain.CurrentSource.Enable(SourceEnableMode.ShowUI, true, _ownerWindowHandle);
				}
			}
			throw new Exception("Сканер не готов.");
		}

		public void StopScan()
		{
			_stopScan = true;
		}

		private void LoadSourceCaps()
		{
			var src = _twain.CurrentSource;
			_loadingCaps = true;

			//var test = src.SupportedCaps;

			if (src.Capabilities.ICapPixelType.IsSupported)
			{
				LoadDepth(src.Capabilities.ICapPixelType);
			}
			if (src.Capabilities.ICapXResolution.IsSupported && src.Capabilities.ICapYResolution.IsSupported)
			{
				LoadDPI(src.Capabilities.ICapXResolution);
			}
			// TODO: find out if this is how duplex works or also needs the other option
			if (src.Capabilities.CapDuplexEnabled.IsSupported)
			{
				LoadDuplex(src.Capabilities.CapDuplexEnabled);
			}
			if (src.Capabilities.ICapSupportedSizes.IsSupported)
			{
				LoadPaperSize(src.Capabilities.ICapSupportedSizes);
			}
			_loadingCaps = false;
		}

		private List<SupportedSize> _supportedSizes = new List<SupportedSize>();
		public List<SupportedSize> SupportedSizes
		{
			get { return _supportedSizes; }
		}		

		private void LoadPaperSize(ICapWrapper<SupportedSize> cap)
		{
			_supportedSizes = cap.GetValues().ToList();
		}

		public bool UsesDuplex { get; private set; } = false;
		private void LoadDuplex(ICapWrapper<BoolType> cap)
		{
			UsesDuplex = cap.GetCurrent() == BoolType.True;
		}

		public List<TWFix32> SupportedDPI { get; private set;} = new List<TWFix32>();
		private void LoadDPI(ICapWrapper<TWFix32> cap)
		{
			// only allow dpi of certain values for those source that lists everything
			SupportedDPI = cap.GetValues().Where(dpi => (dpi % 50) == 0).ToList();
		}

		public List<PixelType> SupportedPixelTypes { get; private set; }
		private void LoadDepth(ICapWrapper<PixelType> cap)
		{
			SupportedPixelTypes = cap.GetValues().ToList();
		}

		SupportedSize size;
		public SupportedSize Size
		{
			get => size;
			set
			{
				if (!_loadingCaps && _twain.State == 4)
				{
					size = value;
					_twain.CurrentSource.Capabilities.ICapSupportedSizes.SetValue(size);
				}
			}
		}

		PixelType _depth;
		public PixelType Depth
		{
			get => _depth;
			set
			{
				if (!_loadingCaps && _twain.State == 4)
				{
					_depth = value;
					_twain.CurrentSource.Capabilities.ICapPixelType.SetValue(_depth);
				}
			}
		}

		TWFix32 _DPI;
		public TWFix32 DPI
		{
			get { return _DPI; }
			set
			{
				if (!_loadingCaps && _twain.State == 4)
				{
					_DPI = value;
					_twain.CurrentSource.Capabilities.ICapXResolution.SetValue(_DPI);
					_twain.CurrentSource.Capabilities.ICapYResolution.SetValue(_DPI);
				}
			}
		}

		bool _duplex;
		public bool Duplex
		{
			get => _duplex;
			set
			{
				if (!_loadingCaps && _twain.State == 4)
				{
					_duplex = value;
					_twain.CurrentSource.Capabilities.CapDuplexEnabled.SetValue(_duplex ? BoolType.True : BoolType.False);
				}
			}
		}

		//private void ScanWithUI(object sender, EventArgs e)
		//{
		//	_twain.CurrentSource.Enable(SourceEnableMode.ShowUIOnly, true, this.Handle);
		//}

		private IEnumerable<DataSource?> GetSourceList()
		{
			if (_twain.State >= 3)
			{
				foreach (DataSource? dataSource in _twain)
					yield return dataSource;
			}
			else
				yield break;
		}

		private void CleanupTwain()
		{
			if (_twain.State == 4)
			{
				_twain.CurrentSource.Close();
			}
			if (_twain.State == 3)
			{
				_twain.Close();
			}

			if (_twain.State > 2)
			{
				// normal close down didn't work, do hard kill
				_twain.ForceStepDown(2);
			}
		}

		System.Drawing.Image _image;
		public System.Drawing.Image Image
		{
			get => _image;
			private set => _image = value;
		}

		public event EventHandler TwainSourseWasDisabled;

		private void OnTwainSorseDisabled()
		{
			TwainSourseWasDisabled?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler AscuisitionComplete;

		private void OnTransferComplete()
		{
			AscuisitionComplete?.Invoke(this, EventArgs.Empty);
		}

		public void Dispose()
		{
			if (_twain.State > 4)
				GC.SuppressFinalize(this);
			else
				CleanupTwain();
		}
	}
}
