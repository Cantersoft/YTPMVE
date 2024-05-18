# YTPMVE
YTPMidiVideoExtension (YTPMVE) is a scripting tool that automatically synchronizes clips to music patterns by MIDI data. Currently, this software is designed and provided only as a Vegas Pro extension. Due to the mediocrity and inaccessibility of Vegas Pro, there are plans to support DaVinci Resolve... at some point.


## Dependencies
YTPMVE requires the following to run:

* Vegas Pro
* Python 3 and mido (optional)

## Installation
1. Download a [release](https://github.com/Cantersoft/YTPMVE/releases) (the *.zip in the "Assets" dropdown).
2. (optional) Install Python and its dependencies as administrator and make sure they're on your system PATH.
3. Either:
	
	* Use the `install.bat` script, which requires administrator privileges to copy to folders inside Program Files, like the Script Menu folder.
	
	or
	* Copy `YTPMVE.cs`, Newtonsoft.Json.dll, and YTPMVE.cs.config to `C:\Program Files\VEGAS\VEGAS Pro XX.0\Script Menu`.
		* If you are using Vegas Pro version 12 or earlier, then in `YTPMVE.cs`, change the namespace `ScriptPortal.Vegas` to `Sony.Vegas`.
		* If you are using Vegas Pro version 15 or earlier, then in `YTPMVE.cs`, comment out the "PitchSemis NOT SUPPORTED IN VEGAS" region.
	* Copy `YTPMVE.py`, `YTPMVE_UI.py`, `hoof.ico` and `YTPMVE_UI.exe` to `C:\Program Files\VEGAS\YTPMVE\`.

## Usage
Open Vegas Pro, and place any number of clips on the timeline, each on separate tracks. For each audio clip, open `Properties` and use the `Pitch Change` value to tune it to A4.

Click Tools > Scripting > YTPMVE_VEGAS > `YTPMVE`, and then select a MIDI file. Use the dropdown menus to link Vegas tracks to MIDI channels. Use `Auto map outputs` to sequentially configure all Vegas tracks. In grouped mode, each audio track will be associated with the first video track that precedes it. In linear mode, each literal track can be independently assigned a MIDI channel, regardless of whether it is video or audio.

Press `GO!`

In special situations, some notes may result in indeterminate clip durations. When this happens, you'll get a warning and markers will be added at such points in the timeline. 

If the clip generation fails or results in a high number of errors, try removing unnecessary channels and especially long notes from your MIDI file.

## Engine Configuration
By default, YTPMVE uses a compiled executable as its engine. In order to run YTPMVE's engine as its Python source code, change the variable `engineFilePath` in `YTPMVE.cs`.

Note that Vegas Pro is not a very good tool for audio production. For more realistic and better-sounding YTPMV audio, see the following FL Studio tutorial.
[Learn how to generate YTPMV audio](https://youtu.be/RP8MKrwXYKI).
