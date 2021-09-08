# unity-customeditor

**Requires Unity 2019.2 or later**

Inspired by https://github.com/idbrii/unity-vimeditor

Add custom editor to the Unity's External Tools section in Preferences to open files in any editor.

## Features

* Double click files to open them.
* Use any text editor with Unity (even in terminal-based, like vim).
* Generate Visual Studio project for autocompletion with omnisharp.
* Provide custom arguments to editor and/or terminal.

## Verified

Tested with Unity 2020.3.17f1 on nixos.

## Installation

Add this line to your Packages/manifest.json:

    "com.github.grafin.unity-customeditor": "https://github.com/github/unity-customeditor.git",

Win: Edit > Preferences > External Tools
Mac: Unity > Preferences > External Tools

Then select "Custom" from the "External Script Editor" dropdown.
