# No Outlines
## Mod for VRChat Build 1048

### Description
The cyan outlines around pickupable objects overrender everything in the game, causing disorientation and lessened immersion.  
This mod adds options to toggle off the outline around pickupable objects and the dashed line pointing to them.  

### Example MelonPreferences.cfg
```
[NoOutlines]
Pref_DisableOutlines = false 	# Disables the blue outline around highlighted pickups 
Pref_DisableBeam     = false    # Disables the dashed line pointing towards highlighted pickups
Pref_DebugOutput     = false    # Enables debug output in the MelonLoader console
```

### Dependencies
UIExpansionKit by Knah [link](https://github.com/knah/VRCMods)  
MelonLoader 0.3.0

### Build Info
Place Assembly-CSharp.dll in Libs/  
UIExpansionKit is on the same folder level 

#### Example Folder Hierarchy
* ---Root Folder---  
  * UIExpansionKit  
  * NoOutlines   
    * Libs  
	  * Assembly-CSharp.dll  

### Credits
VRChat Modding Group


#### Soapbox
Avoid closed-source mods.
