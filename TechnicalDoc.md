# Introduction #

This is the technical spec for YMFAS. It includes information about overall engine design, externally libraries used, etc.

# Platform / Requirements #
YMFAS is aimed at Windows 2000 or higher. It will require DirectX 9, along with other

# Language #
The game/engine will be written in C# on top of the .NET platform, with the possible (though slight) exception of using a scripting language (most likely Lua). The possibility of porting the project to Mono is slim, since the project uses some non-portable libraries (see Graphics, Input)

# Graphics #
## External Libraries ##
We currently plan on using the Ogre3D Rendering Engine (v 1.4.0) and the MOGRE port to C#.

## Features ##
More specific features will be listed in time.

# Input #
## External Libraries ##
YMFAS will only need DirectInput 9.

## Features ##
YMFAS requires a mouse and keyboard. Joystick point is currently not planned. If joystick support becomes necessary, we will consider using XInput as well.

# Networking #
## External Libraries ##
YMFAS uses the [Lidgren](http://code.google.com/p/lidgren-library-network/) networking library.

## Features ##
YMFAS will use a fairly standard server-client architecture for multiplayer games. Matches can support up to 16**players.**

# Physics #
## External Libraries ##
YMFAS will make use of the [Newton Game Dynamics](http://www.newtondynamics.com/) library,  through MogreNewt, a port of OgreNewt, an interface layer between Ogre3D and Newton.

## Features ##