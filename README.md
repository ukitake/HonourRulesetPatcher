# HonourRulesetPatcher

Thanks to https://github.com/Norbyte/lslib under the hood. 

Work around the BG3 Custom Rules bugs that prevent you from starting a multiplayer game with Custom Rules: Honour Ruleset + No Single Save reliably.
There are other well documented workarounds that work for up to 4 players, but AFAIK it was impossible to start a multiplayer game with 5+ player characters with Honour Ruleset.

## HOW TO USE IT
1.  Copy one of your savegame folders from `C:\Users\%YOUR_USER%\AppData\Local\Larian Studios\Baldur's Gate 3\PlayerProfiles\Public\Savegames\Story` to Desktop or somewhere else on your disk.
2.  Drag the copy onto HonourRulesetPatcher.exe.
3.  You should see a console window open to begin the patching process and give you feedback about its success or any errors it encounters.
4.  When it finishes, the folder that you dragged should now contain an Honour Ruleset save file.
5.  You can now overwrite the original in the `C:\Users\%YOUR_USER%\AppData\Local\Larian Studios\Baldur's Gate 3\PlayerProfiles\Public\Savegames\Story` directory.
YOU SHOULD BACK UP THE ORIGINAL SAVE SOMEWHERE BEFORE YOU OVERWRITE IT JUST IN CASE SOMETHING WENT WRONG WITH THE PATCHER!
6.  When you load the patched save file the game will warn you that the savegame was tampered with, but you can just ignore that.

## Building
This is building correctly in Visual Studio 2022 but was made in less than a day so mileage may vary.  
