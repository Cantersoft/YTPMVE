import os
from os import path

try:
    os.makedirs((path.expandvars(r'%USERPROFILE%/AppData/Local/Temp/YTPMVE/')))
except FileExistsError:
    pass

YTPMVE_file= open(path.expandvars(r'%USERPROFILE%/AppData/Local/Temp/YTPMVE/timestamps.txt'), 'w')

import tkinter.filedialog as tf
import tkinter

try:
    import mido
except ModuleNotFoundError as error:
    print("Error"+"\n")
    print("ModuleNotFoundError"+"\n")
    print(str(error)+". Module missing. Install mido with the command \"pip install mido\" in the command prompt and try again.")
    YTPMVE_file.close()
    input("Press Enter to continue...")
    os._exit(1)


root = tkinter.Tk()
root.withdraw()
MIDI_filename = tf.askopenfilename(initialdir = "/",title= "Select MIDI file", filetypes =[('MIDI', '*.mid')])
print("Opened", MIDI_filename)
root.destroy()

MIDI_file=mido.MidiFile(MIDI_filename)

if not len(MIDI_file.tracks)-1 == 1 : # reject files with more than one track or zero tracks
    print("Error"+"\n")
    print("TrackError"+"\n")
    print("An error occurred because there are too many tracks in the MIDI file you selected. Currently, this script only supports 1 track.")
    YTPMVE_file.close()
    input("Press Enter to continue...")
    os._exit(1)

    
current_time=0
start=0
MIDI_time=[]

note_starts=[] # All note start times (seconds)
note_durations=[] # All note durations (seconds)
        
for msg in MIDI_file: # Change this so that we don't look at the if statement except the first few times
    if msg.is_meta:
        start=start+1
        continue
    else:
        #print("Message:", msg)
        current_time=float(msg.time)+current_time
        MIDI_time.append(current_time)
        if msg.type == "note_on":
           print('Note Void: ', msg.time)
           print('Note Start Time: ', current_time)

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
    YTPMVE_file.write(str(note_starts[i][0])+",")#Save first argument, note start time
    YTPMVE_file.write(str(note_durations[i])+"\n")#Save second argument, note duration

YTPMVE_file.close()
