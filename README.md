# setDisplayRes
can list windows monitors and there possible resolutions, sets mode to extended, sets resolutions to display and then the same in clone mode

This tool should be run in cmd.exe for convinience.

Possible Arguments:
 -set : sets the Resoultions defined in the config.
 -list : lists all Displays
 -listmodes : lists modes of a Display (enter name)
 
 Config Example:
 <DisplaySettings>
		<Displays>
			<Display name="\\.\DISPLAY1" setres="true" width="1280" height="1024" freqhz="60"/>
			<Display name="\\.\DISPLAY2" setres="true" width="1280" height="1024" freqhz="60"/>
		</Displays>
	</DisplaySettings>
  
  
  In "set" mode it will do:
  - set the windows mode to "extended"
  - set resolution for both monitors (monitor and hardware scaler with emulated monitor)
  - set the windows mode to "clone"
  - set resolution (only one monitor in windows) again to be sure all is like expected.
     
