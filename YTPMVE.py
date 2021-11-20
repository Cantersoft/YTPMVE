import os, sys, subprocess
from os import path

isFrozen = None
isPythonPresent = None

if getattr(sys, 'frozen', False) and hasattr(sys, '_MEIPASS'): # This code checks if the script is running in a bundled EXE or from the normal interpreter
    isFrozen = True
    print('Running from bundle')
else:
    isFrozen = False
    print('Running from script')

try:
    subprocess.run(["python", "--version"]) # This checks if python is present
except:
    isPythonPresent = False
    print("Python is not present")
else:
    isPythonPresent = True
    print("Python is present")

try:
    os.makedirs((path.expandvars(r'%USERPROFILE%\AppData\Local\Temp\YTPMVE')))
except FileExistsError:
    pass

YTPMVE_file= open(path.expandvars(r'%USERPROFILE%\AppData\Local\Temp\YTPMVE\timestamps.txt'), 'w')
ErrorLogFile = open(path.expandvars(r'%USERPROFILE%\AppData\Local\Temp\YTPMVE\errlog.txt'), 'w')

def exitScript(errMessage, exitCode, retryWithPythonScript):
    r = True
    r = retryWithPythonScript
    ErrorLogFile.write(errMessage + "\n")
    ErrorLogFile.write(str(exitCode) + "\n")
    ErrorLogFile.write(str(r))
    ErrorLogFile.close()
    YTPMVE_file.close()
    os._exit(exitCode)

import tkinter.filedialog as tf
import tkinter

try:
    import mido
    
    #Removing the key signature handler, which sometimes causes errors.
    import mido.midifiles.meta
    del mido.midifiles.meta._META_SPECS[0x59]
    del mido.midifiles.meta._META_SPEC_BY_TYPE['key_signature']
except ModuleNotFoundError as error:
    exitScript(str(error)+". Module missing.", 1, False)



root = tkinter.Tk()
root.withdraw()
MIDI_filename = tf.askopenfilename(initialdir = "\\", title= "Select MIDI file", filetypes =[('MIDI', '*.mid')])
root.destroy()

try:
    MIDI_file=mido.MidiFile(MIDI_filename)
except FileNotFoundError:
    exitScript("No file selected!", 1, False)
else:
    print("Opened", MIDI_filename)


#if not len(MIDI_file.tracks)-1 == 1 : # reject files with more than one track or zero tracks
#        exitScript("Multiple tracks are present in the MIDI File! This script currently only supports single-track MIDI files.", 1, False)


current_time=0
start=0
MIDI_time=[]

note_channels=[]     # All note channels (0-15) 
note_starts=[]      # All note start times (seconds)
note_durations=[]   # All note durations (seconds)
        
for msg in MIDI_file: # Change this so that we don't look at the if statement except the first few times
    if msg.is_meta:
        start=start+1
        continue
    else:
        print("Message:", msg)
        current_time=float(msg.time)+current_time
        MIDI_time.append(current_time)
        if msg.type == "note_on":
            print('Note Void: ', msg.time)
            print('Note Start Time: ', current_time)

            note_channels.append(msg.channel)
            note_starts.append([current_time, msg.note])
            note_durations.append("NULL") # We must have a 1:1 ratio of note_offs for note_ons. This will create a space in note_durations which will be filled later.

        elif msg.type == "note_off":
            print('Note Duration: ', msg.time)
            print('Note End Time: ', current_time)
            print()

            #note_durations.append(msg.time)

            for i in range(len(note_starts)-1, -1, -1): # Reverse search the note starts list and find the note_on message that was probably linked to this note_off
                print()
                print("   Note start reverse search:", note_starts[i][1])
                if note_starts[i][1]== msg.note:
                    list_match=i
                    note_durations[list_match]=current_time-note_starts[i][0]
                    print()
                    print("   Matching note found in note_starts at index position", list_match)
                    break

        
#print(str(MIDI_time)) #Print all the note starts and lengths, optionally.
#print()                
#print("Note start times, note durations:",str(note_starts))#note start time, note number
#print("Note durations: ",str(note_durations))#note end time

for i, j in enumerate(note_starts):#i becomes a counter, and j becomes the corresponding value in note_starts

    #print(note_starts[i][0])
    #print(note_durations[i])
    YTPMVE_file.write(str(note_channels[i])+",")#Save channel number
    YTPMVE_file.write(str(note_starts[i][0])+",")#Save first argument, note start time
    YTPMVE_file.write(str(note_durations[i])+"\n")#Save second argument, note duration


exitScript("none", 0, False)
