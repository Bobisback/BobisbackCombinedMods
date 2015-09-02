# BobisbackCombinedMods
A Timber and Stone Mod. Game Speed, Idle Settlers, Cheat Menu, Mod loader. http://www.timberandstonegame.com/forum/viewtopic.php?f=22&t=6627

Once you download the repo you need to copy the timber and stone game into the timber and stone folder inside the project. This should line the refrences up for the project. Then just run the timber and stone exe in the project folder after you build the project. When you build the project It should make a plugin.dll in the saves folder.


If you want to make it so the project plays the game when you hit run in visual studio then you need to follow these steps:

1. right click BobisbackConbinedMods project in visual studio
2. click the debug tab
3. under the start external program enter: "path\to\project\BobisbackCombinedMods\BobisbackCombinedMods\Timber and Stone\Timber and Stone.exe"
4. Under working directory enter: "path\to\project\BobisbackCombinedMods\BobisbackCombinedMods\Timber and Stone\"

This will allow you to hit start to start the game with the mod running.

As a side note I currently have a script that copies my BobisbackCombinedMods.dll and then renames it to plugin.dll.
