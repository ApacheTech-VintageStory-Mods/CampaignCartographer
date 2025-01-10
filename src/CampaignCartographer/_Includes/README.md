# Campaign Cartographer

Adds multiple Cartography related features to the game, such as custom player pins, GPS, auto waypoint markers, and more.

This mod can be installed as a client-side only mod, which will allow you access to most of the features of the mod. If the mod is also installed on the server, extra features will be available to you. These features have been highlighted below.

**SERVER OWNERS:** If you install this mod on your server, it will remain as an optional mod for your clients. They can choose whether or not to have it installed on their client, and it will not cause any issues logging on, if they don't have it installed.

## Features:
  
 - ### **Waypoint Icons and Colours**
 
    The *"add"* and *"edit"* windows for waypoints have been re-written, allowing for new functionality to be added.

     - Added fifty (50) new icons for waypoints.
     - Added seventy (70) new colours for waypoints.
  
 - ### **Waypoint Beacons**
 
    Add existing waypoints as beacons, visible in the world as pillars of light.
    
    Via the settings menu, you can:
    
     - Change the distance at which the beacon beams are visible.
     - Change the distance at which the beacon titles are visible.
     - Choose whether to show the beacon beams as pillars of light.
     - Choose whether to prefix the waypoint title.
     - Choose whether to show the index within the title.
  
 - ### **Waypoint Sharing**
 
    Share waypoints with other players.

     - Share an existing waypoint with a specific player.
     - Share an existing waypoint with everyone on the server.
  
 - ### **Fast Travel Map Overlay**
 
    Display translocators, and teleporters on a separate map layer, with lines connecting the endpoints.

     - Server admins can configure teleporters using a re-written GUI window, by shift-right-clicking a teleporter block.
     - Add a node to the map by ctrl-right-clicking an active teleporter or translocator.
     - Change the title of the node.
     - Change the colour of the marker for the node.
     - Enable/Disable the node.
     - Show/Hide the path between the two endpoints of the node.
     - Change the default colour for all translocators.
     - Change the default colour for all teleporters.
     - Change the default colour for all nodes with errors.
     - Change the default colour for all disabled nodes.
     - Change the opacity for all nodes with errors.
     - Change the opacity for all disabled nodes.
     - Change the width of the line drawn between the nodes.
     - Change the size of the node on the map.
  
 - ### **Manual Waypoint Addition *`(.wp)`***
 
    Quickly and easily add waypoints at your current position, via the chat window. There are over 130 pre-defined waypoints for many different block types, and areas of interest.

     - Add a waypoint at the player's current location, via a chat command. ***`(.wp)`***
     - Add a waypoint to a translocator, within five blocks of the player. ***`(.wptl)`***
     - Add a waypoint to a teleporter block, within five blocks of the player. ***`(.wptp)`***
     - Add a waypoint to a trader, within five blocks of the player. ***`(.wpt)`***
     - Add a waypoint for the block the player is currently targetting. ***`(.wps)`***

 - ### **Automatic Waypoint Addition** 
 
    Make exploration even more rewarding, by documenting your journeys as you travel. From rock strata, to traders, to essential foodstuff; waypoints are added as you interact with the world.

     - Automatically add waypoints for Broken Translocators, as the player steps on them.
     - Automatically add waypoints for Repaired Translocators, as the player travels between them.
     - Automatically add waypoints for Teleporters, as the player travels between them.
     - Automatically add waypoints for Traders, as the player interacts with them.
     - Automatically add waypoints for mine-able Ores, when the player collects surface deposits.
     - Automatically add waypoints for Rock types, when the player collects loose stones.
     - Automatically add waypoints for Berries, Mushrooms, and Resin, when the player harvests them.

 - ### **Centre Map *`(.cm)`***
 
    Allows you to centre the mini-map, and world map on any location you wish.

     - Re-centre the map on the current player. ***`(.cm)`***
     - Re-centre the map on any specific X, Z coordinates. ***`(.cm pos)`***
     - Re-centre the map on any specific online player. ***`(.cm player)`***
     - Re-centre the map on world spawn. ***`(.cm spawn)`***
     - Re-centre the map on a specific waypoint. ***`(.cm wp)`***
     - Re-centre the map on player's spawn point. ***`(.cm home)`*** **(Requires Server Installation)**

 - ### **Global Positioning System *`(/gps)`***
 
    Display and share your current location within the world.
  
     - Display your current XYZ coordinates. ***`(/gps)`***
     - Copy your current XYZ coordinates to clipboard. ***`(/gps copy)`***
     - Send your current XYZ coordinates as a chat message to the current chat group. ***`(/gps chat)`***
     - Whisper your current XYZ coordinates to a single player. Disabled by default. ***`(/gps pm)`***

 - ### **Customisable Player Map Pins *`(.playerPins)`***
 
    Change the colour and scale of player pins on the world map.
 
     - Highlight other player, to distinguish their player pins on the map. ***`(.playerpins highlight add)`***
     - Remove highlight from player. ***`(.playerpins highlight remove)`***
     - Toggle the Player Pins settings window. ***`(.playerPins)`***
         - Change settings for your own player pin.
         - Change settings for the highlighted player pins on the server.
         - Change settings for the player pins of other people on the server.

## Acknowledgements:

Thank you to the following people:

 - **Doombox:** Original creator of the Customisable Player Pins feature.
 - **egocarib:** Huge inspiration for much of the Auto Waypoints feature.
 - **Melchior:** For assistance with ProtoBuf, and overwriting vanilla classes.
 - **Craluminum2413:** Translation into Russian, and Ukrainian.
 - **Aledark:** Translation into French.
 - **Novocain:** Insipiration for some core Gantry features, and the Waypoint Beacons.
 - **Tyron:** For refactoring some of the API to make this mod easier to make.
