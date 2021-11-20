# YTPMVE

YTPMidiVideoEditor (YTPMVE) is an extension that allows video clips to be synchronized with music automatically, using MIDI to place clips in the same position on a timeline as notes are in a song. Currently, this software is designed specifically to operate in Vegas Pro, although its functionality will likely be expanded as time progresses and it may become more general purpose.


## Dependencies

YTPMVE requires the following to run:

* Vegas Pro
* Python 3 and mido (optional)

## Installation

1. Download a [release](https://github.com/Cantersoft/YTPMVE/releases)
2. (optional) Install Python and its dependencies and make sure they're on your system PATH.
<!---
3. Enter the YTPMVE folder, and

	a. if you use Vegas Pro 14 or higher, delete `Run Script - Vegas 13.cs`.
	
	b. if you use Vegas Pro 13 or lower, delete `Run Script.cs`.
---> 
3. Either:
	
	* Use the `install.bat` script, which requires administrator privileges to copy to folders inside Program Files, like the Script Menu folder
	
	or
	* Copy the entire YTPMVE folder from the zip file and paste it into Vegas Pro's Script Menu folder, which is located at `C:\Program Files\VEGAS\VEGAS Pro XX.0\Script Menu`.

## Usage

**Support for multitrack MIDI has recently been added.** There are still a couple of kinks, so try discarding unnecessary channels from your MIDI file if it's being troublesome.

Once you've prepared your MIDI file, open Vegas Pro and create the same number of tracks as your MIDI file uses (if unsure, create 16), and place an event on each track that corresponds to the content of the MIDI channel you want synchronized with video. Be sure the timeline is entirely clear with the exception of those singular events. 
The events will be duplicated multiple times so that they are in sync with the song. If, for example, you have two tracks with an event on each, the events will be copied
along to the contents of channels 0 and 1 in the MIDI file.

Click Tools > Scripting > YTPMVE > `Run Script` / `Run Script - Vegas 13`, and then select the MIDI file from earlier steps. YTPMVE will then automatically 
synchronize your video clip to the song.

In special situations, some notes may result in indeterminate clip durations. When this happens, you'll get a warning and markers will be added at such points in the
timeline. 

If you get an error message saying "No timecodes found in timestamps.txt!", you may be able to fix it by removing some tracks from your MIDI file and trying again.

Note: YTPMVE does **not** pitch shift audio samples automatically. This would be a pointless feature since it's already standard in digital audio workstations.
[Learn how to generate the audio for a YTPMV](https://youtu.be/RP8MKrwXYKI).
