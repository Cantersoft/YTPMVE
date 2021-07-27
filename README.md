# YTPMVE

YTPMidiVideoEditor (YTPMVE) is a collection of scripts that allows video clips to be synchronized with music automatically, using MIDI to place clips in the same position on a timeline as notes are in a song. Currently, this software is designed specifically to automate the process of creating YTPMVs in Vegas Pro, although its functionality will likely be expanded as time progresses and it may become more general purpose.


## Dependencies

YTPMVE requires the following to run:

* Vegas Pro
* Python 3 and mido

## Installation

1. Download the repository, either via the green button on the top of the page or with a git clone
2. Install the dependencies and make sure they're on your system PATH.
<!---
3. Enter the YTPMVE folder, and

	a. if you use Vegas Pro 14 or higher, delete `Run Script - Vegas 13.cs`.
	
	b. if you use Vegas Pro 13 or lower, delete `Run Script.cs`.
---> 
3. Copy the entire YTPMVE folder and paste it inside Vegas Pro's Script Menu folder, which is located at `C:\Program Files\VEGAS\VEGAS Pro XX.0\Script Menu`.

## Usage

**YTPMVE currently only supports single-track (type 0) MIDI files.** Support for multitrack MIDI will eventually be added, but for now you'll need to remove all but 
one of the tracks if you want to use multitrack MIDI files. You can accomplish this easily in a DAW.

Once you've prepared your MIDI file, open Vegas Pro and place an event on the timeline. Be sure the timeline is entirely clear with the exception of that one event. 
The event will be duplicated multiple times so that it is in sync with the song. If, for example, you have two tracks with an event on each, both events will be copied
along to the contents of the MIDI file! The fun has been doubled!

Click Tools > Scripting > YTPMVE > `Run Script` / `Run Script - Vegas 13`, and then select the MIDI file from earlier steps. YTPMVE will then automatically 
synchronize your video clip to the song.

In special situations, some notes may result in indeterminate clip durations. When this happens, you'll get a warning and markers will be added at such points in the
timeline. 

Note: YTPMVE does **not** pitch shift audio samples automatically. This would be a pointless feature since it's already standard in digital audio workstations.
