# YTPMVE

YTPMidiVideoExtension (YTPMVE) is a scripting tool that synchronizes clips to music automatically, using MIDI files. Currently, this software is designed and provided only as a Vegas Pro extension, although its functionality may be expanded in the future to make it more accessible.


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
	* Copy `YTPME.cs` to Vegas Pro's Script Menu folder, which is located at `C:\Program Files\VEGAS\VEGAS Pro XX.0\Script Menu`.
		* If you are using Vegas Pro version 12 or earlier, then in `YTPMVE.cs`, change the namespace `ScriptPortal.Vegas` to `Sony.Vegas`.
	* Copy `YTPMVE.py` and `YTPMVE.exe` to `C:\Program Files\VEGAS\YTPMVE\`.

## Usage

Open Vegas Pro, and create 16 video tracks. Place a video event on each track that corresponds by index to the MIDI channel whose content you wish the video event to be synchronized with. Each audio track will be associated with the first video track that precedes it, so if your video has audio, do not move the audio track it occupies above the track containing the video it is associated with. Be sure the timeline is entirely clear with the exception of these events. 
The events will be duplicated along the timeline so that they are synchronized with the song. If, for example, you place a video clip on video track 1, it will be copied
along to the contents of channel 0 in the MIDI file. If you place a clip on video track 2, it will be copied to the contents of channel 1.

Click Tools > Scripting > `YTPMVE`, and then select the MIDI file from earlier steps. YTPMVE will then automatically synchronize your video clip to the song.

In special situations, some notes may result in indeterminate clip durations. When this happens, you'll get a warning and markers will be added at such points in the
timeline. 

If the clip generation fails or results in a high number of errors, try removing unnecessary channels and especially long notes from your MIDI file.

## Engine Configuration
By default, YTPMVE uses a compiled executable as its engine. In order to run YTPMVE's engine as its Python source code, change the variable `engineFilePath` in YTPMVE.cs

## Video Flipping

In order to configure video flipping or to disable it, change the boolean variables "flip_x" and "flip_y" in `YTPMVE.cs`.

## Pitch Shifting

YTPMVE pitch shifts audio samples automatically, but note that Vegas Pro is not a Digital Audio Workstation (DAW). For more realistic and better-sounding YTPMV audio, see the FL Studio tutorial below.
[Learn how to generate YTPMV audio](https://youtu.be/RP8MKrwXYKI).
