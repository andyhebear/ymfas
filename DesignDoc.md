# Introduction #

A multiplayer 3-D first-person space pilot simulation game.  Includes accurate inertial physics with a computer-assisted "aim where you fly" mode and customized maneuver scripting.

# Details #
## Game Setup ##
Y.M.F.A.S. is a multiplayer-only game, except for a single player tutorial / practice mode.  The main menu will contain buttons to host or join a game over UDP.

After hosting or joining a game, a player enters the game **lobby**, where players may chat  and organize teams until each of them has indicated that they are ready to begin the game.  At this point, the game host may start the match.

## Environment / Maps ##
## Game Modes ##
### Deathmatch ###
A free-for-all where each pilot may shoot down any other pilot to earn points.
### Team Deathmatch ###
Similar to deathmatch, but pits two teams (organized in the **lobby**) against each other.  Friendly fire is enabled, and shooting down a friendly ship will lose a pilot points.
### Capture the Flag ###
Two opposing teams have bases (with some automated defense turrets) at opposite ends of a small map.  Pilots try to maneuver into the enemy base, steal the flag (which will have a decent collision radius), and bring it back to base before the enemy team does the same.  The flag can be dropped by the flag carrier, and will automatically drop if the flag carrier dies.  If a team picks up its own dropped flag, then it is returned to base.  If a team picks up a dropped enemy flag, then the new carrier has an opportunity to capture it.  Flags are fairly massive, slowing the carrier's acceleration and maneuvering.

Flags can only be captured by a team when its flag is in its base.  Points are awarded for successful flag capture.

### King of the Asteroid ###


### Convoy Defense ###
A slow-moving freight convoy is moving from some point A to some point B on the map.  Players are divided into teams: one team must protect the convoy until it reachees its destination while annother attempts to assault it.
## Ships ##
### Propulsion Systems ###
### Weapon Systems ###

Each weapon type included in YMFAS fulfills a specific role in taking out a target's defenses.  Weapons have varying rates of fire, damage to armor/shields respectively, and transmission rates through shielding.

**Mass Driven**

Mass driven weapons are solid projectiles.  They pass directly through shields uninhibited, dealing medium damage to hull in addition to knocking the impacted ship about  realistically (based on momentum, etc).  They have a relatively slow rate of fire (on the
order of one shot every 1-2 seconds) but travel at high velocity.  Medium power consumption.

**Beam Weapons**

Beam weapons are continuous and burst-fired.  They do low damage to shields and medium damage to armor, with approximately a 75% transmission rate through shielding.  Low-medium power consumption.

**Plasma Weapons**

Plasma weapons are pulse-fired at a medium rate.  They do high damage to shields and medium damage to armor but can barely penetrate shields so long as they are still up.  Medium-high power consumption.

**Missile Systems**

**Point Defense Weapons**


### Defense Systems ###
### Power Systems ###
  * All the ship's systems share a main power supply.
  * Pilots control what percentage of power is directed towards weaponry, propulsion, and shielding.
  * Power to shielding can be routed  in each of the 6 +/- X,Y, and Z directions.
  * Each ship has a power transfer rate which is higher the lower its class (interceptors are most adept at re-routing power)
  * By default, the ship has enough power to operate all systems at 100% capacity, with an extra 20% power in reserve.  The effects of overloading different systems is described above.
### Classes ###
**INTERCEPTOR**

**CORVETTE**

**CRUISER**

**BATTLESHIP**