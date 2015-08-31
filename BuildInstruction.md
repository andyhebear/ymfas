# Dependencies #

The following libraries are required to build and run YMFAS:

  * Ogre3d 1.4.0 + Mogre 0.2.0 SDK (at least)
  * Newton Game Dynamics SDK
  * DirectInput SDK

# Build Prereqs #

## Ogre\MOGRE\MogreNewt ##
  1. Make sure that your `PATH` environment variable includes the installation path of the Ogre SDK. This is usually C:\OgreSDK.
  1. Copy the Newton dll and file (located at `NewtonSDKPath`\sdk\dll) into the OgreSDK bin folders (`OgreSDKPath`\bin\debug and `OgreSDKPath`\bin\release)
  1. The YMFAS bin\Debug folder has a configuration file called Plugins.cfg. In this file, change the Plugins folder to `OgreSDKPath`\bin\debug, and do similarly for the release version.

## References ##
  1. The trunk does not (currently) include project files, for ease of development. However, you need to add the following references to compile:
    * Mogre.dll
    * MogreNewt.dll
    * Microsoft.DirectX
    * Microsoft.DirectX.DirectInput
    * System.Windows.Forms