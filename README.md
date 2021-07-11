<h2 align="right"><a href="https://www.youtube.com/c/Cantersoft"> <img height="110" src="https://yt3.ggpht.com/ytc/AAUvwngMs9rvEkOIVDYBmO_IpNmT6V0j1wEe1H8qwT1z=s176-c-k-c0x00ffffff-no-rj" alt="Cantersoft"> <br> Cantersoft</a></h2>


# YTPMVE

Hi! Here are the instructions for installation and usage of YTPMVE. 

YTPMidiVideoEditor (YTPMVE) is a technology that allows video clips to be synchronized with music automatically, using MIDI to place clips in the same position 
on a timeline as notes are in a song. Currently, this software is designed specifically to automate the process of creating YTPMVs in Vegas Pro,
although its functionality will likely be expanded as time progresses and it may become more general purpose.

[Here](https://youtu.be/gGs7wNIjXos) is an example of what can be accomplished using this software.

If the concept of YTPMV is unfamiliar to you, watch [this video, which explains it in detail](https://www.youtube.com/watch?v=B7UvyuOkg2E).


## Dependencies

YTPMVE requires the following to run:

* Vegas Pro
* Python 3 & mido [optional]

## Installation

1. Download and extract the *.zip from [this repository](https://github.com/Cantersoft/YTPMVE).
2. Enter the `\YTPMVE` subfolder, and

	a. if you use Vegas Pro 14 or higher, delete `YTPMVE v0.112 - V13.cs`.
	
	b. if you use Vegas Pro 13 or lower, delete `YTPMVE v0.112.cs`.
	
	c. finally, if you have Python 3 installed, install the mido module via pip;
	  ```
    pip install mido
	  ```
  	* The executable file is superfluous if Python and mido are installed, and you can safely delete it.
	* Note: the executable file sometimes doesn't work properly. Try installing Python and mido, and deleting the executable if performance is poor.
3. Copy the YTPMVE folder and paste it inside Vegas Pro's Script Menu folder, which should look something like `C:\Program Files\VEGAS\VEGAS Pro XX.0\Script Menu`.

## Auto-generate Your YTPMVs!

After installation, you're about ready to fire this thing up! 

You'll first need to get a MIDI file of the song of your choice. You can easily find one by searching Google or a site such as [Midiworld](https://www.midiworld.com/),
or you can create your own in a DAW.

**YTPMVE currently only supports single-track (type 0) MIDI files.** Support for multitrack MIDI will eventually be added, but for now you'll need to remove all but 
one of the tracks if you want to use multitrack MIDI files. You can accomplish this easily in a DAW.

Once you've prepared your MIDI file, open Vegas Pro and place an event on the timeline. Be sure the timeline is entirely clear with the exception of that one event. 
The event will be duplicated multiple times so that it is in sync with the song. If, for example, you have two tracks with an event on each, both events will be copied
along to the contents of the MIDI file! The fun has been doubled!

Click Tools > Scripting > YTPMVE > [whatever option in here **doesn't** end with "Py"], and then select the MIDI file from earlier steps. YTPMVE will then automatically 
synchronize your video clip to the song.

In special situations, some notes may result in indeterminate clip durations. When this happens, you'll get a warning and markers will be added at such points in the
timeline. 

Note: YTPMVE does **not** pitch shift audio samples automatically. This would be a pointless feature since it's already standard in digital audio workstations.
[Learn how to generate the audio for a YTPMV](https://youtu.be/RP8MKrwXYKI).
