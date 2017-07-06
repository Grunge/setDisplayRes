using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace PInvoke.WindowsResolution
{
	// Encapsulates access to the PInvoke functions
	public class Display
	{

        

        public List<DISPLAY_DEVICE> GetDisplayList(bool RetrieveMonitorname = false)
        {
            //todo: EDD_GET_DEVICE_INTERFACE_NAME            
            //const int EDD_GET_DEVICE_INTERFACE_NAME = 0x1;

            List<DISPLAY_DEVICE> displays = new List<DISPLAY_DEVICE>();
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);
            try
            {
                for (uint id = 0; NativeMethods.EnumDisplayDevices(null, id, ref d, 0); id++)
                {
                    if (d.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop))
                    {
                        //call again to get the monitor name (not only the graka name).
                        DISPLAY_DEVICE devWithName = new DISPLAY_DEVICE();
                        devWithName.cb = Marshal.SizeOf(devWithName);

                        NativeMethods.EnumDisplayDevices(d.DeviceName, 0, ref devWithName, 0);
                        //overwrite device string and id, keep the rest!
                        d.DeviceString = devWithName.DeviceString;
                        d.DeviceID = devWithName.DeviceID;

                        displays.Add(d);
                    }//if is display                        
                }//for
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("{0}", ex.ToString()));
            }

            return displays;
        }





		// Return a list of all possible display types for this computer
        public List<DevMode> GetDisplaySettings(string strDevName)
		{
			List<DevMode> modes = new List<DevMode>();
			DevMode devmode = this.DevMode;

			int counter = 0;
			int returnValue = 1;

			// A return value of zero indicates that no more settings are available
			while (returnValue != 0)
			{
				returnValue = GetSettings(strDevName, ref devmode, counter++);

				modes.Add(devmode);
			}

			return modes;
		}
		
		// Return the current display setting
		public int GetCurrentSettings(string strDevName, ref DevMode devmode)
		{
			return GetSettings(strDevName, ref devmode, NativeMethods.ENUM_CURRENT_SETTINGS);
		}



        //todo: CDS_UPDATEREGISTRY

		// Change the settings to the values of the DEVMODE passed
		public string ChangeSettings(string strDevName, DevMode devmode, bool bSetPrimary)
		{
			string errorMessage = "";
            ChangeDisplaySettingsFlags flags = new ChangeDisplaySettingsFlags();
            flags = ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_GLOBAL;
            
            ReturnCodes iRet = NativeMethods.ChangeDisplaySettingsEx(strDevName, ref devmode, IntPtr.Zero, flags, IntPtr.Zero);

            //same again, but with PRIMARY
            if (bSetPrimary && iRet == ReturnCodes.DISP_CHANGE_SUCCESSFUL)
            {
                devmode.dmFields = (int)DEVMODE_Flags.DM_POSITION;
                devmode.dmPositionX = 0;
                devmode.dmPositionY = 0;
                flags = flags | ChangeDisplaySettingsFlags.CDS_SET_PRIMARY;
                iRet = NativeMethods.ChangeDisplaySettingsEx(strDevName, ref devmode, IntPtr.Zero, flags, IntPtr.Zero);
            }

			switch (iRet)
			{
				case ReturnCodes.DISP_CHANGE_SUCCESSFUL:
					break;
				case ReturnCodes.DISP_CHANGE_RESTART:
					errorMessage = "Please restart your system";
					break;
				case ReturnCodes.DISP_CHANGE_FAILED:
					errorMessage = "ChangeDisplaySettigns API failed";
					break;
				case ReturnCodes.DISP_CHANGE_BADDUALVIEW:
					errorMessage = "The settings change was unsuccessful because system is DualView capable.";
					break;
				case ReturnCodes.DISP_CHANGE_BADFLAGS:
					errorMessage = "An invalid set of flags was passed in.";
					break;
				case ReturnCodes.DISP_CHANGE_BADPARAM:
					errorMessage = "An invalid parameter was passed in. This can include an invalid flag or combination of flags.";
					break;
				case ReturnCodes.DISP_CHANGE_NOTUPDATED:
					errorMessage = "Unable to write settings to the registry.";
					break;
				default:
					errorMessage = "Unknown return value from ChangeDisplaySettings API";
					break;
			}
			return errorMessage;
		}
        		
		// Return a properly configured DEVMODE
		public DevMode DevMode
		{
			get
			{
				DevMode devmode = new DevMode();
				devmode.dmDeviceName = new String(new char[32]);
				devmode.dmFormName = new String(new char[32]);
				devmode.dmSize = (short)Marshal.SizeOf(devmode);
				return devmode;
			}
		}

		// call the external function inthe Win32 API
		private int GetSettings(string strDevName, ref DevMode devmode, int iModeNum)
		{
			// helper to wrap EnumDisplaySettings Win32 API
            return NativeMethods.EnumDisplaySettings(strDevName, iModeNum, ref devmode);
		}
	}
}