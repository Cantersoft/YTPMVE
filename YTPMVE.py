#YTPMVE
#20240117

import os, sys, subprocess
from os import path

if getattr(sys, 'frozen', False) and hasattr(sys, '_MEIPASS'): # This code checks if the script is running in a bundled EXE or from the normal interpreter
	print('Running from executable.')
else:
	print('Running from Python script.')


try:
	os.makedirs((path.expandvars(r'%USERPROFILE%\AppData\Local\Temp\YTPMVE')))
except FileExistsError:
	pass

YTPMVE_file= open(path.expandvars(r'%USERPROFILE%\AppData\Local\Temp\YTPMVE\timestamps.txt'), 'w')
ErrorLogFile = open(path.expandvars(r'%USERPROFILE%\AppData\Local\Temp\YTPMVE\errlog.txt'), 'w')

def exitScript(errMessage, exitCode,):
	ErrorLogFile.write(errMessage + "\n")
	ErrorLogFile.write(str(exitCode))
	ErrorLogFile.close()
	YTPMVE_file.close()
	os._exit(exitCode)

import tkinter.filedialog as tf
import tkinter

try:
	import mido
	
	#Make note of MIDI files with key signature issues. This code can remove the key signature handlers, but it also causes other errors.
	#import mido.midifiles.meta
	#del mido.midifiles.meta._META_SPECS[0x59]
	#del mido.midifiles.meta._META_SPEC_BY_TYPE['key_signature']
except ModuleNotFoundError as error:
	exitScript(str(error)+". Module missing.", 1)



root = tkinter.Tk()
root.withdraw()
MIDI_filename = tf.askopenfilename(initialdir = "\\", title= "Select MIDI file", filetypes =[('MIDI', '*.mid')])
root.destroy()



try:
	MIDI_file=mido.MidiFile(MIDI_filename, clip=True)
except FileNotFoundError:
	exitScript("No file selected!", 1)
else:
	print("Opened", MIDI_filename)


#if not len(MIDI_file.tracks)-1 == 1 : # reject files with more than one track or zero tracks
#		exitScript("Multiple tracks are present in the MIDI File! This script currently only supports single-track MIDI files.", 1, False)


A440_REF = 69		   #This is the MIDI index value of A4, the universal reference note.

current_time=0
start=0
MIDI_time=[]

note_channels=[]	# All note channels (0-15)
note_tones=[]		# All note tones (0-127)
note_starts=[]		# All note start times (seconds)
note_durations=[]   # All note durations (seconds)

print("Processing MIDI file.")

try:
	for msg in MIDI_file:															   # Change this so that we don't look at the if statement except the first few times
		current_time=float(msg.time)+current_time
		if msg.is_meta:
			start=start+1
			continue
		elif msg.type != "note_on" and msg.type != "note_off":						  # Control changes and pitchwheels really throw this script off.
			continue	
		else:
			MIDI_time.append(current_time)
			if msg.type == "note_on" and msg.velocity != 0:							 # End of note

				note_channels.append(msg.channel)
				note_tones.append(msg.note)
				note_starts.append(current_time)
				note_durations.append("NULL")										   # We must have a 1:1 ratio of note_offs for note_ons. This will create a space in note_durations which will be filled later.

			elif msg.type == "note_off" or msg.velocity == 0:						   # Start of note

				#note_durations.append(msg.time)

				for i in range(len(note_starts)-1, -1, -1):							 # Reverse search the note starts list and find the note_on message that was probably linked to this note_off
					if  note_channels[i]==msg.channel and note_tones[i]==msg.note and current_time-note_starts[i]!=0:	# Matching note found
						list_match=i
						note_durations[list_match]=current_time-note_starts[i]
						break
except Exception as error:
	exitScript(str(error)+". MIDI processing failed.", 1)

print("Writing data.")
		
#print(str(MIDI_time)) #Print all the note starts and lengths, optionally.
#print()				
#print("Note start times, note durations:",str(note_starts))#note start time, note number
#print("Note durations: ",str(note_durations))#note end time

for i, j in enumerate(note_starts):#i becomes a counter, and j becomes the corresponding value in note_starts

	#print(note_starts[i][0])
	#print(note_durations[i])
	YTPMVE_file.write(str(note_channels[i])+",")#Save channel number
	YTPMVE_file.write(str(note_tones[i] - A440_REF)+",")#Save semitone offset
	YTPMVE_file.write(str(note_starts[i])+",")#Save note start time
	YTPMVE_file.write(str(note_durations[i])+"\n")#Save note duration

exitScript("none", 0)