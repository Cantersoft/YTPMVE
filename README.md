# YTPMVE

YTPMidiVideoEditor (YTPMVE) is a collection of scripts that synchronizes clips with music automatically, using MIDI. Currently, this software is designed and provided only as a Vegas Pro extension, although its functionality may be expanded in the future to make it more accessible.


## Dependencies

YTPMVE requires the following to run:

* Vegas Pro
* Python 3 and mido (optional)

## Installation

1. Download a [release](https://github.com/Cantersoft/YTPMVE/releases).
2. (optional) Install Python and its dependencies and make sure they're on your system PATH.
<!---
3. Enter the YTPMVE folder, and

	a. if you use Vegas Pro 14 or higher, delete `Run Script - Vegas 13.cs`.
	
	b. if you use Vegas Pro 13 or lower, delete `Run Script.cs`.
---> 
3. Either:
	
	* Use the `install.bat` script, which requires administrator privileges to copy to folders inside Program Files, like the Script Menu folder.
	
	or
	* Copy the entire YTPMVE folder from the zip file and paste it into Vegas Pro's Script Menu folder, which is located at `C:\Program Files\VEGAS\VEGAS Pro XX.0\Script Menu`.

## Usage

**Support for multitrack MIDI has recently been added.** There are still a couple of kinks, so try discarding unnecessary channels from your MIDI file if it's being troublesome.

Open Vegas Pro and create the same number of tracks as your MIDI file uses (if unsure, create 16), and place an event on each track that corresponds to the content of the MIDI channel you want synchronized with video. Be sure the timeline is entirely clear with the exception of those singular events. 
The events will be duplicated along the timeline so that they are in sync with the song. If, for example, you place a video clip on track 1, it will be copied
along to the contents of channel 0 in the MIDI file. If you place a clip on track 2, it will be copied to the contents of channel 1.

Click Tools > Scripting > YTPMVE > `Run Script` / `Run Script - Vegas 13`, and then select the MIDI file from earlier steps. YTPMVE will then automatically 
synchronize your video clip to the song.

In special situations, some notes may result in indeterminate clip durations. When this happens, you'll get a warning and markers will be added at such points in the
timeline. 

If you get an error message saying "No timecodes found in timestamps.txt!", you may be able to fix it by removing some tracks from your MIDI file and trying again.

Note: YTPMVE does **not** pitch shift audio samples automatically. This would be a pointless feature since it's already standard in digital audio workstations.
[Learn how to generate the audio for a YTPMV](https://youtu.be/RP8MKrwXYKI).
