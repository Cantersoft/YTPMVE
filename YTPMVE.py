#YTPMVE
#20250127

import os, sys, subprocess
from os import path
import json
try:
	import mido
	#Make note of MIDI files with key signature issues. This code can remove the key signature handlers, but it also causes other errors.
	#import mido.midifiles.meta
	#del mido.midifiles.meta._META_SPECS[0x59]
	#del mido.midifiles.meta._META_SPEC_BY_TYPE['key_signature']
except ModuleNotFoundError as error:
	exitScript(str(error)+". Module missing.", 1)

# This code checks if the script is running in a bundled EXE or from the normal interpreter
if getattr(sys, 'frozen', False) and hasattr(sys, '_MEIPASS'): 
	print('Running from executable.')
else:
	print('Running from Python script.')



YTPMVE_directory_name = r'%USERPROFILE%\AppData\Local\Temp\YTPMVE'
timestamps_file = open(path.expandvars(YTPMVE_directory_name + r'\timestamps.txt'), 'w')
ErrorLogFile = open(path.expandvars(YTPMVE_directory_name + r'\errlog.txt'), 'w')

try:
	os.makedirs((path.expandvars(YTPMVE_directory_name)))
except FileExistsError:
	pass

def exitScript(errMessage, exitCode,):
	ErrorLogFile.write(errMessage + "\n")
	ErrorLogFile.write(str(exitCode))
	ErrorLogFile.close()
	timestamps_file.close()
	os._exit(exitCode)

def process_midi(MIDI_filename):
	A440_REF = 69			#MIDI index value of A4, the universal reference note.

	counter=0
	current_time=0
	start=0
	MIDI_time=[]

	note_channels=[]		# All note channels (0-15)
	note_tones=[]			# All note tones (0-127)
	note_starts=[]			# All note start times (seconds)
	note_durations=[]		# All note durations (seconds)

	#Open MIDI file
	try:
		MIDI_file=mido.MidiFile(MIDI_filename, clip=True)
	except FileNotFoundError:
		exitScript("No file selected!", 1)
	else:
		print("Opened", MIDI_filename)

	#Process MIDI file
	print("Processing MIDI file.")
	try:
		MIDI_file_list = list(MIDI_file)
		# We must have a 1:1 ratio of note_offs for note_ons. This will create a space in note_durations which will be filled later.
		for i in range(len(MIDI_file_list)):
			note_durations.append("NULL")
			
		for msg in MIDI_file:															   
			current_time=float(msg.time)+current_time
			# Change this so that we don't look at the if statement except the first few times.
			if msg.is_meta:
				start=start+1
				continue
			# Filter messages such as control changes and pitchwheels, which cause issues.
			elif msg.type != "note_on" and msg.type != "note_off":
				continue	
			else:
				MIDI_time.append(current_time)
				# Start of note.
				if msg.type == "note_on" and msg.velocity != 0:
					counter += 1
					note_channels.append(msg.channel)
					note_tones.append(msg.note)
					note_starts.append(current_time)
					
					#Forward note search	
					current_time_2 = current_time
					for counter_2 in range(1, len(MIDI_file_list)-counter):
						msg2 = MIDI_file_list[counter+counter_2]
						current_time_2 = current_time_2 + msg2.time
						#we need to confirm the type has the velocity attribute before checking if the velocity is 0, otherwise an error is thrown
						if msg2.type == "note_off" or msg2.type == "note_on": 
							if msg2.type == "note_off" or (msg2.type == "note_on" and msg2.velocity == 0):
								note_duration = current_time_2 - current_time
								#Avoid zero-duration notes, which may cause crashes.
								if note_duration == 0:
									continue
								note_durations[counter] = note_duration
								break
				
				# End of note
				elif msg.type == "note_off" or msg.velocity == 0:
					#note_durations.append(msg.time)	#I think this was originally used for legato?
					 # Reverse search the note starts list and find the note_on message that was probably linked to this note_off
					for i in range(len(note_starts)-1, -1, -1):							
						if  note_channels[i]==msg.channel and note_tones[i]==msg.note and current_time-note_starts[i]!=0:
							# Matching note found
							list_match=i
							note_durations[list_match]=current_time-note_starts[i]
							break
			
			#Forward search notes. This should probably be an optional feature, but for now it prevents notes from getting stuck on.
			# for i in range(0, len(note_starts), 1):
				# if note_durations[i] == "NULL":
					# try:
						# note_durations[i] = note_durations[i+1]
					# except IndexError:
						# pass
				
	except Exception as error:
		exitScript(str(error)+". MIDI processing failed.", 1)

	print("Writing data.")

	for i, j in enumerate(note_starts):#i becomes a counter, and j becomes the corresponding value in note_starts
		timestamps_file.write(str(note_channels[i])+"|")#Save channel number
		timestamps_file.write(str(note_tones[i] - A440_REF)+"|")#Save semitone offset
		timestamps_file.write(str(note_starts[i])+"|")#Save note start time
		timestamps_file.write(str(note_durations[i])+"\n")#Save note duration
		
	exitScript("none", 0)	
		
def save_config(dictionary):
	# for item in dictionary:
		# print(dictionary[item].get('channel'))

	config_file = open(path.expandvars(YTPMVE_directory_name + r'\settings.json'), 'w')
	config_file.write(json.dumps(dictionary))
	config_file.close()
	
def save_track_mode(track_mode):
	track_mode_file = open(path.expandvars(YTPMVE_directory_name + r'\track_mode.txt'), 'w')
	track_mode_file.write(track_mode)
	track_mode_file.close()