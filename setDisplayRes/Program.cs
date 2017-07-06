using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PInvoke.WindowsResolution;
using System.Threading;// Thread.Sleep, for the exit sleep timer...
using System.Configuration;//ConfigurationManager


//log4net should use config from app
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace setDisplayRes
{

    /// <summary>
    /// Yes, this is the easiest way to set the display resolution (19.06.2017).
    /// Windows ;-(
    /// https://msdn.microsoft.com/en-us/library/aa719104(v=vs.71).aspx
    /// </summary>
    class Program
    {

        // Create a logger for use in this class
        static log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            Console.WriteLine(" ------------------------------------------------------ ");
            Console.WriteLine(" ------------    set Display Resolution   ------------- ");
            Console.WriteLine(" ------------------------------------------------------ ");
            //Console.WriteLine("Welcome to the extra complicated windows display resolution setter.");
            Console.WriteLine(" Possible Arguments:                                      ");
            Console.WriteLine(" -set : sets the Resolutions defined in the config.     ");
            Console.WriteLine(" -list : lists all Displays                             ");
            Console.WriteLine(" -listmodes : lists modes of a Displays                 ");
            Console.WriteLine();

            bool bSet = false;
            bool bListDisplays = false;
            bool bListDisplayModes = false;
            string strArguments = "";
            foreach (string arg in args)
            {               
                if (strArguments != "")
                {
                    strArguments += " ";
                }
                strArguments += arg;

                string strLowerArg = arg.ToLower();
                Console.WriteLine("check Arguments: " + strLowerArg);

                if (strLowerArg == "-list")
                {
                    bListDisplays = true;
                }else if (strLowerArg == "-listmodes")
                {
                    bListDisplayModes = true;
                }
                else if (strLowerArg == "-set")
                {
                    bSet = true;
                }
                else
                {                   
                    string strWrongArgMsg = " UNKNOWN ARGUMENT: " + arg;
                    Console.WriteLine(strWrongArgMsg);
                    _log.Error(strWrongArgMsg);

                    //exit delay animation
                    ExitWithDelay(3000);

                    return; //exit
                }
                
            }//foreach arg

            _log.Debug("Program Start, Arguments: " + strArguments);

            if (bListDisplays)
            {
                _log.Debug("list displays");

               //get all Displays
                Display display = new Display();
                List<DISPLAY_DEVICE> displays = display.GetDisplayList();

                Console.WriteLine();
                foreach (DISPLAY_DEVICE dev in displays)
                {
                    String strDevice = "Name: '" + dev.DeviceName + "' String: '" + dev.DeviceString + "' Id: '" + dev.DeviceID + "'.";
                    Console.WriteLine("Device Found: " + strDevice);                                           
                }
                
            }
            else if (bListDisplayModes)
            {
                _log.Debug("list modes of a display");

                Console.WriteLine("Enter a DeviceName to process:");
                string strDeviceName = Console.ReadLine();

                //get all Modes
                Display display = new Display();
                List<DevMode> modes = display.GetDisplaySettings(strDeviceName);

                foreach (DevMode mode in modes)
                {
                    String strMode = "Width: '" + mode.dmPelsWidth + "' Height: '" + mode.dmPelsHeight + "' Frequency: '" + mode.dmDisplayFrequency.ToString() + "'.";
                    Console.WriteLine(strMode);
                }

            }
            else if (bSet)
            {
                _log.Info("Set DisplayModus to Extend");
                //okay, first we set the display to extende to change both/all displays
                NativeMethods.SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, ChangeDisplayConfigFlags.SDC_TOPOLOGY_EXTEND | ChangeDisplayConfigFlags.SDC_APPLY);
                _log.Debug("adjust resolutions");
                changeDisplaySettings();

                bool bDuplicateMode = System.Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings.Get("setWindowModeDuplicated"));
                bool bDuplicateModeSetRes = System.Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings.Get("setWindowModeDuplicatedChangeRes"));
                if (bDuplicateMode)
                {
                    //now we set the resolution again, but in clone modus.
                    _log.Info("Set DisplayModus to Duplicate");
                    NativeMethods.SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, ChangeDisplayConfigFlags.SDC_TOPOLOGY_CLONE | ChangeDisplayConfigFlags.SDC_APPLY);

                    if (bDuplicateModeSetRes)
                    {
                        _log.Debug("adjust resolution");
                        changeDisplaySettings(false);
                    }
                }
            }
            else
            {
                //nothing to do.
            }

            _log.Debug("Program End");

            //exit delay animation
            ExitWithDelay(6000);
        }//main()


        /// <summary>
        /// Dont set primary when in Duplicate Modus. Any Monitor (so usualy the wrong one) can be Primary after.
        /// Best to set Prmiary it in Extend Modus only.
        /// </summary>
        /// <param name="bUsePrimarySettings"></param>
        static private void changeDisplaySettings(bool bUsePrimarySettings = true)
        {
            //get Config (fails when config is wrong)
            _log.Debug("read config, Start - fails when config is wrong");

            //get MkDirServers from config file (fails when config is wrong)
            DisplayConfig config = (DisplayConfig)ConfigurationManager.GetSection("DisplaySettings");

            if (config != null)
            {
                _log.Debug("DisplaySettings loaded.");
            }
            else
            {
                String strError = "Worker, failed to load DisplaySettings section. -> check config file.";
                _log.Fatal(strError);
                throw new Exception(strError);
            }

            List<DisplayElement> configdisplays = config.GetAllDisplays();
            foreach (DisplayElement displ in configdisplays)
            {
                _log.Debug("setting found: " + displ.ToString());
            }

            _log.Debug("read config, End");


            //get all Displays from Windows 
            Display display = new Display();
            List<DISPLAY_DEVICE> windowsdisplays = display.GetDisplayList();

            //all windows installed displays
            foreach (DISPLAY_DEVICE dev in windowsdisplays)
            {
                //display defined in configuration
                foreach (DisplayElement configDisplay in configdisplays)
                {
                    //check name or devicestring
                    bool bMatchingName = false;
                    if (configDisplay.name != "")
                    {
                        //use device name
                        if (configDisplay.name == "*" || configDisplay.name.ToLower() == dev.DeviceName.ToLower())
                        {
                            bMatchingName = true;
                        }
                    }
                    else if (configDisplay.devicestring != "")
                    {
                        //use device string (usualy the real name)
                        if (configDisplay.devicestring.ToLower() == dev.DeviceString.ToLower())
                        {
                            bMatchingName = true;
                        }                        
                    }
                    else if (configDisplay.deviceid != "")
                    {
                        //use device string (usualy the real name)
                        if (dev.DeviceID.ToLower().Contains(configDisplay.deviceid.ToLower()))
                        {
                            bMatchingName = true;
                        }
                    }
                    else
                    {
                        throw new Exception("must define name, devicestring or deviceid");                    
                    }

                    //found?
                    if (bMatchingName)
                    {
                        //found
                        _log.Debug("matching device found, name: '" + dev.DeviceName + "' string: '" + dev.DeviceString + "'.");

                        //get resolutions
                        List<DevMode> settings = display.GetDisplaySettings(dev.DeviceName);

                        bool bResFound = false;
                        DevMode selectedMode = new DevMode();
                        foreach (DevMode mode in settings)
                        {
                            if (mode.dmPelsHeight == configDisplay.height && mode.dmPelsWidth == configDisplay.width)
                            {
                                _log.Debug("matching resolution found: " + dev.DeviceName);

                                //optional
                                if (configDisplay.freqhz > 0)
                                {
                                    if (mode.dmDisplayFrequency == configDisplay.freqhz)
                                    {
                                        _log.Debug("matching frequency found: " + dev.DeviceName);
                                        //bingo
                                        bResFound = true;
                                        selectedMode = mode;
                                        break;
                                    }
                                }
                                else
                                {
                                    //ignore freq
                                    //bingo
                                    bResFound = true;
                                    selectedMode = mode;
                                    break;
                                }
                            }
                        }//foreach mode of this display

                        if (bResFound)
                        {
                            _log.Debug("settings found for device" + dev.DeviceName);
                            //set resolution
                            if (bResFound)
                            {
                                bool bSetAsPrimary = false;
                                if(bUsePrimarySettings)
                                {
                                    bSetAsPrimary = configDisplay.primary;
                                }

                                _log.Info("Change Settings, name: '" + dev.DeviceName + "', id: '" + dev.DeviceID + "'. primary: '" + bSetAsPrimary.ToString() + "' Res: " + selectedMode.dmPelsWidth.ToString() + "x" + selectedMode.dmPelsHeight.ToString() + " " + selectedMode.dmDisplayFrequency + "Hz");
                                string strError = display.ChangeSettings(dev.DeviceName, selectedMode, bSetAsPrimary);

                                if (strError == "")
                                {
                                    _log.Debug("Changed Settings successful.");
                                }
                                else
                                {
                                    _log.Error("Changed Settings with Error: " + strError);
                                }
                            }
                            else
                            {
                                Console.WriteLine("could not find the Resolution in the possible Modes.");
                            }
                        }
                        else
                        {
                            _log.Debug("no matching settings found for device, name: '" + dev.DeviceName + "', id: '" + dev.DeviceID + "'.");
                        }
                    }//if match
                }//foreach config entry                    
            }//foreach windows device        
        }

        static private void ExitWithDelay(long a_lDelayMs)
        {
            //exit delay animation            
            if (a_lDelayMs > 1000)
            {

                Console.WriteLine("");

                // chose one of the exit animations...

                //AnimateTheDotsLine(a_lDelayMs); //shit
                AnimateTheSecondsCounter(a_lDelayMs, "Exit in: '"); // okay   

                // coming soon in this theater: multiline animations ! ;-)               
            }
        }

        static private void AnimateTheSecondsCounter(long a_lDelayMs, string a_strText)
        {
            // Close the Application with delay
            Console.Write(a_strText);
            // Countdown...
            for (int i = System.Convert.ToInt32(a_lDelayMs / 1000); i > 0; i--)
            {
                String strText = i.ToString() + "'.";

                Console.Write(strText);

                Thread.Sleep(1000);
                Console.Write(new string('\b', strText.Length));
            }
        }//AnimateTheCounter
                
    }//class
}//ns
