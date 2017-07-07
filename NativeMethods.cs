using System;
using System.Runtime.InteropServices;

namespace PInvoke.WindowsResolution
{
	// Encapsulate the magic numbers for the return value in an enumeration
	public enum ReturnCodes : int
	{
		DISP_CHANGE_SUCCESSFUL = 0,
		DISP_CHANGE_BADDUALVIEW = -6,
		DISP_CHANGE_BADFLAGS = -4,
		DISP_CHANGE_BADMODE = -2,
		DISP_CHANGE_BADPARAM = -5,
		DISP_CHANGE_FAILED = -1,
		DISP_CHANGE_NOTUPDATED = -3,
		DISP_CHANGE_RESTART = 1
	}

	// To see how the DEVMODE struct was translated from the unmanaged to the managed see the Task 2 Declarations section

	// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/gdi/prntspol_8nle.asp
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct DevMode
	{
		// The MarshallAs attribute is covered in the Background section of the article
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmDeviceName;

		public short dmSpecVersion;
		public short dmDriverVersion;
		public short dmSize;
		public short dmDriverExtra;
		public int dmFields;
		public int dmPositionX;
		public int dmPositionY;
		public int dmDisplayOrientation;
		public int dmDisplayFixedOutput;
		public short dmColor;
		public short dmDuplex;
		public short dmYResolution;
		public short dmTTOption;
		public short dmCollate;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmFormName;

		public short dmLogPixels;
		public short dmBitsPerPel;
		public int dmPelsWidth;
		public int dmPelsHeight;
		public int dmDisplayFlags;
		public int dmDisplayFrequency;
		public int dmICMMethod;
		public int dmICMIntent;
		public int dmMediaType;
		public int dmDitherType;
		public int dmReserved1;
		public int dmReserved2;
		public int dmPanningWidth;
		public int dmPanningHeight;

		public override string ToString()
		{
			return dmPelsWidth.ToString() + " x " + dmPelsHeight.ToString();
		}


		public string[] GetInfoArray()
		{
			string[] items = new string[5];

			items[0] = dmDeviceName;
			items[1] = dmPelsWidth.ToString();
			items[2] = dmPelsHeight.ToString();
			items[3] = dmDisplayFrequency.ToString();
			items[4] = dmBitsPerPel.ToString();

			return items;
		}
	}


    //Display Listening
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DISPLAY_DEVICE
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        [MarshalAs(UnmanagedType.U4)]
        public DisplayDeviceStateFlags StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    [Flags()]
    public enum DEVMODE_Flags : int
    {
        DM_BITSPERPEL = 0x40000,
        DM_DISPLAYFLAGS = 0x200000,
        DM_DISPLAYFREQUENCY = 0x400000,
        DM_PELSHEIGHT = 0x100000,
        DM_PELSWIDTH = 0x80000,
        DM_POSITION = 0x20
    }

    [Flags()]
    public enum DisplayDeviceStateFlags : int
    {
        /// <summary>The device is part of the desktop.</summary>
        AttachedToDesktop = 0x1,
        MultiDriver = 0x2,
        /// <summary>The device is part of the desktop.</summary>
        PrimaryDevice = 0x4,
        /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
        MirroringDriver = 0x8,
        /// <summary>The device is VGA compatible.</summary>
        VGACompatible = 0x10,
        /// <summary>The device is removable; it cannot be the primary display.</summary>
        Removable = 0x20,
        /// <summary>The device has more display modes than its output devices support.</summary>
        ModesPruned = 0x8000000,
        Remote = 0x4000000,
        Disconnect = 0x2000000
    }

    [Flags()]
    public enum ChangeDisplaySettingsFlags : uint
    {
        CDS_NONE = 0,
        CDS_UPDATEREGISTRY = 0x00000001,
        CDS_TEST = 0x00000002,
        CDS_FULLSCREEN = 0x00000004,
        CDS_GLOBAL = 0x00000008,
        CDS_SET_PRIMARY = 0x00000010,
        CDS_VIDEOPARAMETERS = 0x00000020,
        CDS_ENABLE_UNSAFE_MODES = 0x00000100,
        CDS_DISABLE_UNSAFE_MODES = 0x00000200,
        CDS_RESET = 0x40000000,
        CDS_RESET_EX = 0x20000000,
        CDS_NORESET = 0x10000000
    }

    [Flags()]
    public enum ChangeDisplayConfigFlags : uint
    {
        SDC_TOPOLOGY_INTERNAL = 0x00000001,
        SDC_TOPOLOGY_CLONE = 0x00000002,
        SDC_TOPOLOGY_EXTEND = 0x00000004,
        SDC_TOPOLOGY_EXTERNAL = 0x00000008,
        SDC_APPLY = 0x00000080,
    }

	class NativeMethods
	{
		// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/gdi/devcons_84oj.asp
		[DllImport("user32.dll", CharSet = CharSet.Ansi)]
		public static extern int EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DevMode lpDevMode);

		// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/gdi/devcons_7gz7.asp
        [DllImport("user32.dll", CharSet = CharSet.Ansi)] //CallingConvention = CallingConvention.Cdecl
        public static extern ReturnCodes ChangeDisplaySettingsEx(string lpszDeviceName, ref DevMode lpDevMode, IntPtr hwnd, ChangeDisplaySettingsFlags dwFlags, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettingsEx(int lpszDeviceName, int lpDevMode, int hwnd, int dwFlags, int lParam);


        [DllImport("user32.dll")]
        // A signature for ChangeDisplaySettingsEx with a DEVMODE struct as the second parameter won't allow you to pass in IntPtr.Zero, so create an overload
        public static extern int ChangeDisplaySettingsEx(string lpszDeviceName, IntPtr lpDevMode, IntPtr hwnd, ChangeDisplaySettingsFlags dwflags, IntPtr lParam);





        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern ReturnCodes ChangeDisplaySettings(ref DevMode lpDevMode, int dwFlags);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);


        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern long SetDisplayConfig(uint numPathArrayElements, IntPtr pathArray, uint numModeArrayElements, IntPtr modeArray, ChangeDisplayConfigFlags flags);


		public const int ENUM_CURRENT_SETTINGS = -1;
	}
}
