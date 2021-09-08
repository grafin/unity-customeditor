# unity-customeditor

**Requires Unity 2019.2 or later**

Inspired by https://github.com/idbrii/unity-vimeditor

Add custom editor to the Unity's External Tools section in Preferences to open files in any editor.

## Features

* Double click files to open them.
* Use any text editor with Unity (even in terminal-based, like vim).
* Generate Visual Studio project for autocompletion with omnisharp.
* Provide custom arguments to editor and/or terminal.

![image](https://user-images.githubusercontent.com/426722/132577216-f6e0ebf8-f370-49e0-af14-3ad911803e2e.png)
![image](https://user-images.githubusercontent.com/426722/132577770-2e4ccd63-f746-4039-9449-39c0d347d151.png)

## Verified

Tested with Unity 2020.3.17f1 on nixos.

## Installation

Add this line to your Packages/manifest.json:

    "com.github.grafin.unity-customeditor": "https://github.com/grafin/unity-customeditor.git",

Select "Custom" from the "External Script Editor" dropdown in "Preferences".
