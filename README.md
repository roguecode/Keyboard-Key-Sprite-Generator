# Keyboard-Key-Sprite-Generator
This is a pretty horridly-thrown-together app to generate sprites for keys of a keyboard, so that I could avoid doing it manually for our game.

You need to provide it with a list of keys to generate. You can edit /Data/Default.txt, or just change the text in the app. Each line represents a key. The format here is [filename],[display text]. If [display text] is missing, then [filename] is used for what to display on the sprite. The output files will be transparent PNG's.

For each key, it will decide between two button templates to use (small/medium) found in /Data/. You can edit those to match your game.

There is a pre-compiled version [here](https://github.com/roguecode/Keyboard-Key-Sprite-Generator/tree/master/Prebuilt)

![Crappy app gif](https://github.com/roguecode/Keyboard-Key-Sprite-Generator/blob/master/Preview.gif?raw=true "Crappy app gif")


