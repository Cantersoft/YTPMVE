#YTPMVE
#20240913
from os import path
import tkinter as tk
import tkinter.ttk as ttk
from tkinter.constants import *
import tkinter.filedialog as tf
import YTPMVE
#Mido is a requirement to import YTPMVE, so this line will not be reached if mido is not installed.
import mido

YTPMVE_directory_name = r'%USERPROFILE%\AppData\Local\Temp\YTPMVE'
#Absolute directory is required, because the current directory is not the directory of this script when it is started from a different directory.
YTPMVE_installation_directory_name = r'%PROGRAMFILES%\VEGAS\YTPMVE'

bgcolor = '#d9d9d9'
bgcolor_GO_button = '#009900'
bgcolor_GO_button_pressed = '#006600'
bgcolor_disabled = '#9e9e9e'
fgcolor_disabled = '#cccccc'

font = ("Comic Sans MS", 10)
font_bold = ("Comic Sans MS", 10, "bold")

global root
root = tk.Tk()
root.iconbitmap(path.expandvars(YTPMVE_installation_directory_name + r'\hoof.ico'))
root.protocol('WM_DELETE_WINDOW', root.destroy)
root.geometry("380x380+100+100")
root.resizable(False, False)
root.title("YTPMVE")
root.configure(background=bgcolor)

combo_box_event_track = tk.StringVar()
combo_box_MIDI_channel = tk.StringVar()
spin_box_track_mode = tk.StringVar()
MIDI_filename = tk.StringVar()
checkbox_auto_map_outputs_value = tk.IntVar()
checkbox_flip_y_value = tk.IntVar()
checkbox_flip_x_value = tk.IntVar()
checkbox_legato_video_value = tk.IntVar()
checkbox_legato_audio_value = tk.IntVar()

default_checkbox_flip_y_value = 0
default_checkbox_flip_x_value = 1
default_checkbox_legato_video_value = 1
default_checkbox_legato_audio_value = 0

# Data for comboboxes
try:
	tracks_linear_file = open(path.expandvars(YTPMVE_directory_name + r'\tracks_linear.txt'), 'r')
	tracks_grouped_file = open(path.expandvars(YTPMVE_directory_name + r'\tracks_grouped.txt'), 'r')
	combo_values_event_track_linear = tracks_linear_file.readlines()
	combo_values_event_track_grouped = tracks_grouped_file.readlines()
	if len(combo_values_event_track_linear) == 0:
		YTPMVE.exitScript("No tracks selected.", 1)
	
	tracks_linear_file.close()
	tracks_grouped_file.close()
	combo_values_event_track = combo_values_event_track_linear
except FileNotFoundError:
	combo_values_event_track = ["Track 0", "Track 1", "Track 2", "Track 3", "Track 4", "Track 5", "Track 6", "Track 7", "Track 8", "Track 9", "Track 10", "Track 11", "Track 12", "Track 13", "Track 14", "Track 15"]
	combo_values_event_track_linear = combo_values_event_track
	combo_values_event_track_grouped = combo_values_event_track
combo_values_MIDI_channel = []
spin_values_track_mode = ["linear", "grouped"]

dict_event_tracks = {}
global dict_midi_channels
dict_midi_channels = {}
array_midi_channel_indexes = []
midi_file_imported = False

def file_open_dialogue():
	MIDI_filename_read = tf.askopenfilename(initialdir = "\\", title = "Select MIDI file", filetypes = [('MIDI', '*.mid')])
	if MIDI_filename_read:
		print("read")
		MIDI_filename.set(MIDI_filename_read)
		MIDI_file = mido.MidiFile(MIDI_filename.get(), clip=True)
		global midi_file_imported
		midi_file_imported = True
		set_midi_channel_titles(MIDI_file)
		set_dictionary_to_tracks_length()
		button_GO.configure(state=NORMAL, background=bgcolor_GO_button)
	else:
		print("not read")	
		
def set_midi_channel_titles(MIDI_file):
	#dict_midi_channels dictionary is filled with the channel of each track name.
	global dict_midi_channels
	track_name = "Untitled"
	for msg in MIDI_file:
		if msg.type == "track_name":
			track_name = msg.name
		elif msg.is_cc():#This might grab control changes unrelated to tracks? Not sure, may be an issue later and this should be refined.
			channel = msg.channel
			if channel not in dict_midi_channels:
				dict_midi_channels[channel] = ('Channel {}: {}'.format(channel, track_name))
				track_name = "Untitled"
	global array_midi_channel_indexes
	
	array_midi_channel_indexes = sorted(dict_midi_channels.items())
	dict_midi_channels = dict(array_midi_channel_indexes)
		
	global combo_values_MIDI_channel
	combo_values_MIDI_channel = []
	for channel, track_name in dict_midi_channels.items():
		combo_values_MIDI_channel.append(track_name)
	print(combo_values_MIDI_channel)
	combo_box_MIDI_channel.configure(values=combo_values_MIDI_channel)
	combo_box_MIDI_channel.set(combo_values_MIDI_channel[0])
	on_combo_box_midi_channel_select("<<ComboboxSelected>>")
	
def set_dictionary_to_tracks_length():
	pass
	array_event_track_titles = []

	global dict_event_tracks
	global combo_values_event_track
	
	dict_event_tracks = {}
	for counter, event_track in enumerate(combo_values_event_track):
		array_event_track_titles.append(event_track)
		
		dict_event_tracks[counter] = {}
		dict_event_tracks[counter]['channel'] = array_midi_channel_indexes[0][0] # Set default channel to first possible channel
		dict_event_tracks[counter]['flip_x'] = default_checkbox_flip_x_value  # Set default value for flip_x
		dict_event_tracks[counter]['flip_y'] = default_checkbox_flip_y_value  # Set default value for flip_y
		dict_event_tracks[counter]['legato_video'] = default_checkbox_legato_video_value  # Set default value for legato video	
		dict_event_tracks[counter]['legato_audio'] = default_checkbox_legato_audio_value  # Set default value for legato audio	
		
	combo_values_event_track = array_event_track_titles	
	combo_box_event_track.configure(values=combo_values_event_track)
	on_combo_box_event_track_select("<<ComboboxSelected>>")	
		
#This will always map from the first event track value. It might eventually be worth setting the mapping based on the currently selected event track.		
def auto_map_outputs():
	if midi_file_imported:
		print(combo_values_MIDI_channel)
		MIDI_channel = combo_box_MIDI_channel.current() #combo_box_MIDI_channel.current()#Midi channel should equal dict_chan_to_name{array_chan_to_chan_index[combo_box_MIDI_channel.current()]}
		local_checkbox_flip_x_value = checkbox_flip_x_value.get()
		local_checkbox_flip_y_value = checkbox_flip_y_value.get()
		local_checkbox_legato_video_value = checkbox_legato_video_value.get()
		local_checkbox_legato_audio_value = checkbox_legato_audio_value.get()
		
		max = len(array_midi_channel_indexes)-1 #len(combo_values_MIDI_channel)-1
		
		if MIDI_channel < 0:
			MIDI_channel = 1
		
		for counter, event_track in enumerate(combo_values_event_track):
			# if counter >= len(combo_values_MIDI_channel):
				# break
			current_MIDI_channel = MIDI_channel+counter

			if current_MIDI_channel > max:
				current_MIDI_channel = max
			if current_MIDI_channel < 0:
				current_MIDI_channel = 0
				
			if counter not in dict_event_tracks:
				dict_event_tracks[counter] = {}	
				
			dict_event_tracks[counter]['channel'] = array_midi_channel_indexes[current_MIDI_channel][0] #combo_values_MIDI_channel[current_MIDI_channel]  
			
			dict_event_tracks[counter]['flip_x'] = local_checkbox_flip_x_value
			dict_event_tracks[counter]['flip_y'] = local_checkbox_flip_y_value
			dict_event_tracks[counter]['legato_video'] = local_checkbox_legato_video_value 
			dict_event_tracks[counter]['legato_audio'] = local_checkbox_legato_audio_value 
			
		combo_box_event_track.set(combo_values_event_track[0])
		on_combo_box_event_track_select("<<ComboboxSelected>>")
	
	
def GO():
	print("dict: ", dict_event_tracks)
	YTPMVE.save_config(dict_event_tracks)
	YTPMVE.save_track_mode(spin_box_track_mode.get())
	YTPMVE.process_midi(MIDI_filename.get())
	
def on_combo_box_event_track_select(event):
	if midi_file_imported:
		selected_event_track = combo_box_event_track.current()
		if selected_event_track not in dict_event_tracks:
			dict_event_tracks[selected_event_track] = {}
			dict_event_tracks[selected_event_track]['channel'] = 0  # Set default value for combo_box_MIDI_channel
			dict_event_tracks[selected_event_track]['flip_x'] = default_checkbox_flip_x_value  # Set default value for flip_x
			dict_event_tracks[selected_event_track]['flip_y'] = default_checkbox_flip_y_value  # Set default value for flip_y
			dict_event_tracks[selected_event_track]['legato_video'] = default_checkbox_legato_video_value  # Set default value for legato
			dict_event_tracks[selected_event_track]['legato_audio'] = default_checkbox_legato_audio_value  # Set default value for legato
		
		global dict_midi_channels	

		#Try to set the combobox value to the value specified in the event tracks dictionary, but if something invalid is set, then default to a default value
		try:
			#Set the combobox to the key in the midi channels dictionary that matches the value with an index of the event track dictionary's currently selected track's channel key's integer value in an ordered list of the midi channels dictionary
			#dict_midi_channels[list(dict_midi_channels.keys())[int(dict_event_tracks[selected_event_track]['channel'])]]
			combo_box_MIDI_channel.set(dict_midi_channels[int(dict_event_tracks[selected_event_track]['channel'])])
			on_combo_box_midi_channel_select("<<ComboboxSelected>>")
		except:
			combo_box_MIDI_channel.set(combo_values_MIDI_channel[0])
			on_combo_box_midi_channel_select("<<ComboboxSelected>>")
		checkbox_flip_x_value.set(dict_event_tracks[selected_event_track]['flip_x'])
		checkbox_flip_y_value.set(dict_event_tracks[selected_event_track]['flip_y'])
		checkbox_legato_video_value.set(dict_event_tracks[selected_event_track]['legato_video'])
		checkbox_legato_audio_value.set(dict_event_tracks[selected_event_track]['legato_audio'])

def on_combo_box_midi_channel_select(event):
	if midi_file_imported:
		selected_event_track = combo_box_event_track.current()
		if selected_event_track not in dict_event_tracks:
			dict_event_tracks[selected_event_track] = {}
			dict_event_tracks[selected_event_track]['channel'] = 0  # Set default value for combo_box_MIDI_channel
			dict_event_tracks[selected_event_track]['flip_x'] = default_checkbox_flip_x_value  # Set default value for flip_x
			dict_event_tracks[selected_event_track]['flip_y'] = default_checkbox_flip_y_value  # Set default value for flip_y
			dict_event_tracks[selected_event_track]['legato_video'] = default_checkbox_legato_video_value  # Set default value for legato
			dict_event_tracks[selected_event_track]['legato_audio'] = default_checkbox_legato_audio_value  # Set default value for legato

			
		MIDI_channel = array_midi_channel_indexes[combo_box_MIDI_channel.current()][0]
		print("I think that the index of the item you selected (" + str(combo_box_MIDI_channel.current()) + ") says that it is MIDI channel " + str(MIDI_channel))
		dict_event_tracks[selected_event_track]['channel'] = MIDI_channel
	
def checkbox_action(key, value):
	if midi_file_imported:
		print(dict_event_tracks)
		dict_event_tracks[combo_box_event_track.current()][key] = value.get()
	
def spin_box_action():
	global combo_values_event_track
	track_mode_update()
	combo_box_event_track.configure(values=combo_values_event_track)
	combo_box_event_track.set(combo_values_event_track[0])
	
def track_mode_update():
	global combo_values_event_track
	if spin_box_track_mode.get() == spin_values_track_mode[0]:
		combo_values_event_track = combo_values_event_track_linear
		label_event_track.configure(text="Vegas Linear Event Track")
	elif spin_box_track_mode.get() == spin_values_track_mode[1]:
		combo_values_event_track = combo_values_event_track_grouped
		label_event_track.configure(text="Vegas Video Event Track")	
	
class HoverButton(tk.Button):
	def __init__(self, master=None, pressed_color=bgcolor, released_color="SystemButtonFace", **kwargs):
		tk.Button.__init__(self, master, **kwargs)
		self.pressed_color = pressed_color
		self.released_color = released_color
		self.bind("<Enter>", self.on_enter)
		self.bind("<Leave>", self.on_leave)

	def on_enter(self, event):
		if self["state"] == "normal":
			self.config(bg=self.pressed_color)

	def on_leave(self, event):
		if self["state"] == "normal":
			self.config(bg=self.released_color)	

frame_file_input = ttk.Frame(root)
frame_file_input.grid(row=0, column=1, columnspan=2, sticky=EW)

frame_master_options = ttk.Frame(root, borderwidth="2", relief="groove")
frame_master_options.grid(row=1, column=1, columnspan=2, sticky=N)

frame_body_container = ttk.Frame(root)
frame_body_container.grid(row=2, column=1, columnspan=2, pady=5, sticky=EW)

frame_body = ttk.Frame(frame_body_container, borderwidth="4", relief="groove")
frame_body.grid(row=1, column=1, columnspan=2, pady=5, sticky=EW)

frame_event_track = ttk.Frame(frame_body, borderwidth="2", relief="groove")
frame_event_track.grid(row=2, column=1, sticky=N)

frame_MIDI_channel = ttk.Frame(frame_body, borderwidth="2", relief="groove")
frame_MIDI_channel.grid(row=2, column=2, sticky=N)

frame_track_mode = ttk.Frame(frame_master_options, borderwidth="2", relief="groove")
frame_track_mode.grid(row=0, column=1, sticky=N)

frame_footer = ttk.Frame(root)
frame_footer.grid(row=3, column=1, columnspan=2, sticky=N, pady=5)

combo_box_event_track = ttk.Combobox(frame_event_track, font=font, textvariable=combo_box_event_track, values=combo_values_event_track)
combo_box_event_track.grid(row=1, pady=5)
combo_box_event_track.bind("<<ComboboxSelected>>", on_combo_box_event_track_select)
combo_box_event_track.set(combo_values_event_track[0])	

combo_box_MIDI_channel = ttk.Combobox(frame_MIDI_channel, font=font, textvariable=combo_box_MIDI_channel, values=combo_values_MIDI_channel)
combo_box_MIDI_channel.grid(row=1, pady=5)
combo_box_MIDI_channel.bind("<<ComboboxSelected>>", on_combo_box_midi_channel_select)

spin_box_track_mode = ttk.Spinbox(frame_track_mode, command=lambda: spin_box_action(), font=font, textvariable=spin_box_track_mode, values=spin_values_track_mode)
spin_box_track_mode.grid(row=1, column=0)

button_auto_map_outputs = HoverButton(frame_body, font=font, text="Auto map outputs", command = auto_map_outputs)
button_auto_map_outputs.grid(row=1, column=1, pady=5, columnspan=2, sticky="N")

button_select_MIDI_file = HoverButton(frame_file_input, font=font, text ="Select MIDI File", command = file_open_dialogue)
button_select_MIDI_file.grid(row=0, column=1, sticky=EW)

button_GO = HoverButton(frame_footer, background=bgcolor_GO_button, pressed_color=bgcolor_GO_button_pressed, released_color=bgcolor_GO_button, font=font_bold, text ="GO!", command = GO)
button_GO.grid(row=5, column=1)
button_GO.configure(state=DISABLED, disabledforeground=fgcolor_disabled, background=bgcolor_disabled)

checkbox_legato_video = tk.Checkbutton(frame_event_track, anchor='w', background=bgcolor, command=lambda: checkbox_action("legato_video", checkbox_legato_video_value), font=font, text="Legato (Video)", variable=checkbox_legato_video_value)
checkbox_legato_video.grid(row=4, sticky = EW)

checkbox_legato_audio = tk.Checkbutton(frame_event_track, anchor='w', background=bgcolor, command=lambda: checkbox_action("legato_audio", checkbox_legato_audio_value), font=font, text="Legato (Audio)", variable=checkbox_legato_audio_value)
checkbox_legato_audio.grid(row=5, sticky = EW)

checkbox_flip_y = tk.Checkbutton(frame_event_track, anchor='w', background=bgcolor, command=lambda: checkbox_action("flip_y", checkbox_flip_y_value), font=font, text="Flip Y", variable=checkbox_flip_y_value)
checkbox_flip_y.grid(row=3, sticky = EW)

checkbox_flip_x = tk.Checkbutton(frame_event_track, anchor='w', background=bgcolor, command=lambda: checkbox_action("flip_x", checkbox_flip_x_value), font=font, text="Flip X", variable=checkbox_flip_x_value)
checkbox_flip_x.grid(row=2, sticky = EW)

label_event_track = tk.Label(frame_event_track, background=bgcolor, font=font_bold, text="Vegas Video Event Track")
label_event_track.grid(row=0, sticky=EW)

label_MIDI_channel = tk.Label(frame_MIDI_channel, background=bgcolor, font=font_bold, text="MIDI Channel")
label_MIDI_channel.grid(row=0, sticky=EW)

label_track_mode = tk.Label(frame_track_mode, background=bgcolor, font=font_bold, text="Track Mode")
label_track_mode.grid(row=0, sticky=EW)

MIDI_filename_entry = tk.Entry(frame_file_input, textvariable = MIDI_filename, font=font)
MIDI_filename_entry.grid(row=0, column=2, columnspan=2, sticky=EW)
	
	
	
#If there are no video tracks above a single audio track...	
if len(combo_values_event_track_grouped) > 0:
	#Default to grouped mode
	spin_box_track_mode.set(spin_values_track_mode[1])
else:
	#Default to linear mode mode
	spin_box_track_mode.set(spin_values_track_mode[0])

spin_box_action()
root.mainloop()