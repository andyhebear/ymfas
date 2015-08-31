# Introduction #

Tasks should ideally be in an appropriate section, whenever possible. Priority should be indicated after the task name. If the task is being taken up, it should be _italicized_. Use ~~strikes~~ to indicate completed tasks. Comments can be left on tasks, but should be clearly indicated.

This page is not for bugs. Bugs and other defects should be listed in the issues section of the project site.

# Tasks #

## Graphics ##
  * _Arcball (AB) camera (medium) - Implement an arcball camera which allows for a 3D view at around a specific object at a fixed distance. The AB camera should attach itself to a Mogre SceneNode and, with some sort of Update function, adjust its position to be at the set radius. Ideally, the AB camera will derive from an Ogre camera, so that it can be used seamlessly within Ogre._ - by Mason

  * Model different ships using blender (low)
  * Model projectile weapons (mass-driven and missile) using blender (low)
  * Create engine and plasma weapon effects using mogre's particle effects (low)

## Engine ##
  * Time Management system (high)
    1. _Implement a class which takes in one-time and repeated-interval events and runs them at the specified time. The class should return some sort of id after an event is submitted, so that the user can cancel an event or temporarily pause a repeated event. The system will eventually be integrated into the engine and be used for many critical subsystems (network, input, and physics updates, among others)._ - by Mason
    1. Implement a class which returns managed timers. The timers will be an improvement over native Ogre timers in that they will be automatically update by the engine after each frame. This may be as simple as "registering" a timer with the engine, and this might actually be easier.

## Input ##
  * Scripting engine (low) - Implement a means of allowing user-scripted maneuvers
## Networking ##
  * Game setup (medium) - Implement a game main menu interface for hosting/joining games and a game lobby


## AI ##
  * Design & implement the assisted navigation ("aim where you fly") mode

## Physics ##

## Networking ##
  * Implement connection refusal after game start (medium)

## Miscellaneous ##