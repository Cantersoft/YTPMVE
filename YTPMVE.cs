//YTPMVE
//20240529
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using ScriptPortal.Vegas;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using System.Collections;

public class EntryPoint
{
    //Variables for error handling
    public static bool tracks_missing = false;
    public static bool transpositions_missing = false;
    public static bool timestamps_contains_nulls = false;

    //Check whether a Vegas track is empty.
    static bool track_empty(Track track)
    {
        if (track.Events.Count == 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    static void track_event_generate(Track track, string note_start, string note_duration)
    {
        TrackEvent current_event = track.Events[0];
        TrackEvent copied_event = current_event.Copy(track, Timecode.FromPositionString(note_start, RulerFormat.Seconds));
        copied_event.AdjustStartLength(Timecode.FromPositionString(note_start, RulerFormat.Seconds), Timecode.FromPositionString(note_duration, RulerFormat.Seconds), false);
    }
    static void audio_event_generate(Track audio_track, AudioEvent audio_track_source_audio_event, string note_start, string note_duration, int note_tone_offset, bool pitchsemis_supported)
    {
        if (!audio_track.IsAudio()) { return; }
        TrackEvent current_event;

        //Something's not quite right with this logic. An empty audio track results in subsequent audio tracks not getting copied.
        if (track_empty(audio_track))
        {
            return;
        }
        else
        {
            try
            {
                //Select the first event on this audio track.
                current_event = audio_track.Events[0];
            }
            catch { return; }
        }

        //Copy the audio event. MAKE THIS A FUNCTION
        TrackEvent copied_event = current_event.Copy(audio_track, Timecode.FromPositionString(note_start, RulerFormat.Seconds));
        copied_event.AdjustStartLength(Timecode.FromPositionString(note_start, RulerFormat.Seconds), Timecode.FromPositionString(note_duration, RulerFormat.Seconds), false);

        /*PitchSemis NOT SUPPORTED IN VEGAS 14*/
        try
        {
            if (pitchsemis_supported)
            {
                AudioEvent copied_audio_event = (AudioEvent)copied_event;
                //AudioEvent source_audio_event = (AudioEvent)source_audio_events[iterator];
                //This was looking at the wrong event for pitchsemis data. We must somehow get the correct source event, but we can't directly access these vars before they exist, so maybe they'll need to be passed in

                double note_tone = audio_track_source_audio_event.PitchSemis + note_tone_offset;

                //Stupidly, Vegas internally limits pitch within a range of 78 semitones, beyond the UI limit of 48. This logic will keep the note tones within the range.
                if (note_tone < -39 || note_tone > 39)
                {
                    while (note_tone > 39)
                    {
                        note_tone -= 12;
                    }
                    while (note_tone < -39)
                    {
                        note_tone += 12;
                    }
                }

                copied_audio_event.PitchSemis = note_tone;
            }
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            //This happens when Vegas Pro decides to just not work correctly. 
            transpositions_missing = true;
            return;
        }
        /*END PitchSemis NOT SUPPORTED IN VEGAS 14*/
    }

    //Rebound keyframe bounds vertices for flipping
    static void flip_keyframe_x(VideoMotionKeyframe current_video_motion_keyframe)
    {
        current_video_motion_keyframe.ScaleBy(new VideoMotionVertex(-1, 1));
    }
    static void flip_keyframe_y(VideoMotionKeyframe current_video_motion_keyframe)
    {
        current_video_motion_keyframe.ScaleBy(new VideoMotionVertex(1, -1));
    }
    static void flip_keyframe_xy(VideoMotionKeyframe current_video_motion_keyframe)
    {
        current_video_motion_keyframe.ScaleBy(new VideoMotionVertex(-1, -1));
    }

    //SelectEveryOtherEvent.cs + sykhro auto flips
    static void flip_video(Track track, bool flip_x, bool flip_y)
    {
        //Exit function if the track is empty.
        if (track_empty(track)) { return; }
        if (track.IsVideo())
        {
            bool select_this_event = false;
            int sequence_counter = 1;

            for (int i = 0; i < track.Events.Count; i++)
            {
                VideoEvent current_video_event = (VideoEvent)track.Events[i];

                // Assign video vertexes to keyframes.
                VideoMotionKeyframe current_video_motion_keyframe = current_video_event.VideoMotion.Keyframes[0];

                if (flip_x && !flip_y)
                {
                    if (select_this_event)
                    {
                        for (int j = 0; j < current_video_event.VideoMotion.Keyframes.Count; j++)
                        {
                            current_video_motion_keyframe = current_video_event.VideoMotion.Keyframes[j];
                            flip_keyframe_x(current_video_motion_keyframe);
                        }
                    }
                    select_this_event = !select_this_event;
                }

                if (flip_y && !flip_x)
                {
                    if (select_this_event)
                    {
                        for (int j = 0; j < current_video_event.VideoMotion.Keyframes.Count; j++)
                        {
                            current_video_motion_keyframe = current_video_event.VideoMotion.Keyframes[j];
                            flip_keyframe_y(current_video_motion_keyframe);
                        }
                    }
                    select_this_event = !select_this_event;
                }

                if (flip_x && flip_y)
                {
                    for (int j = 0; j < current_video_event.VideoMotion.Keyframes.Count; j++)
                    {
                        current_video_motion_keyframe = current_video_event.VideoMotion.Keyframes[j];
                        // Rolling flips
                        switch (sequence_counter)
                        {
                            case 1:
                                break;
                            case 2:
                                flip_keyframe_x(current_video_motion_keyframe);
                                break;
                            case 3:
                                flip_keyframe_xy(current_video_motion_keyframe);
                                break;
                            case 4:
                                flip_keyframe_y(current_video_motion_keyframe);
                                break;
                            default:
                                break;
                        }
                    }
                    sequence_counter++;
                    if (sequence_counter > 4)
                    {
                        sequence_counter = 1;
                    }
                }
            }
        }
    }
    //Otomad Helper Legato
    public enum Track_legato_type
    {
        STACKING,
        STRETCHING,
        LENGTHENING,
    }
    static void legato(Track track, Track_legato_type track_legato_type, bool legato_video, bool legato_audio)
    {
        //Exit function if the track is empty.
        if (track_empty(track)) { return; }
        if ((legato_video == true && track.IsVideo()) || (legato_audio == true && track.IsAudio()))
        {
            for (int i = 0; i < track.Events.Count - 1; i++)
            {
                // Timecode increaseSpacingTime = null;
                TrackEvent current_event = track.Events[i];
                TrackEvent next_event = track.Events[i + 1];
                if (next_event == null) continue;
                switch (track_legato_type)
                {
                    case Track_legato_type.STACKING:
                        {
                            //current_event.End = this_application.Project.TrackEvents[current_event.Index + 1].Start;
                            break;
                        }
                    case Track_legato_type.STRETCHING:
                        {
                            //bool forceStretch = type == Track_legato_type.STRETCHING;
                            //double rate = current_event.Length.ToMilliseconds() / (next_event.Start - current_event.Start).ToMilliseconds();
                            //current_event.RelativeAdjustPlaybackRate(rate, forceStretch, false);
                            break;
                        }
                    case Track_legato_type.LENGTHENING:
                        {
                            current_event.End = next_event.Start;
                            break;
                        }
                }
            }
        }
    }

    public enum track_mode_types
    {
        linear,
        grouped,
    }

    Vegas this_application;
    public void FromVegas(Vegas vegas)
    {
        this_application = vegas;

        string path = vegas.InstallationDirectory + "\\.." + "\\YTPMVE\\";//Full path to engine files included with the installation of YTPMVE.
        string py_file_path = "\"" + path + "YTPMVE_UI.pyw" + "\"";
        string exe_file_path = "\"" + path + "YTPMVE_UI.exe" + "\"";


        /*CHANGE THIS LINE TO CONTROL WHETHER THE EXECUTABLE OR PYTHON VERSION IS USED*/
        string engine_file_path = exe_file_path;
        /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/


        List<string> engine_process_name = new List<string>();

        if (engine_file_path == py_file_path)
        {
            engine_process_name.AddRange(new[] { "", py_file_path });
        }
        else if (engine_file_path == exe_file_path)
        {
            engine_process_name.AddRange(new[] { "", exe_file_path });
        }
        else
        {
            MessageBox.Show("engine_file_path variable has been set to an invalid value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        //Create YTPMVE AppData directory if it does not exist.
        string temp_path = @"%USERPROFILE%\AppData\Local\Temp\YTPMVE";
        if (!Directory.Exists(Environment.ExpandEnvironmentVariables(temp_path)))
        {
            try
            {
                Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(temp_path));
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show("Please create the directory " + temp_path + ". YTPMVE requires this directory to convert MIDI data and does not have the privleges to create it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        List<string> list_track_names_linear = new List<string>();
        List<string> list_track_names_grouped = new List<string>();

        Track track;
        Track audio_track;
        Track tentative_track;
        //The first event in a track
        TrackEvent event0;

        //This variable keeps track of the number of audio tracks that succeed a video track.
        List<int[]> track_audiovisual_grouping_index = new List<int[]>();
        List<int[]> list_track_names_audio = new List<int[]>();
        int last_video_track_index = -1;
        //TrackEvent current_event;
        //TrackEvent copied_event;

        List<TrackEvent> source_video_events = new List<TrackEvent>();
        List<TrackEvent> source_audio_events = new List<TrackEvent>();

        string[] array_timecodes_src;
        List<String> list_timecodes = new List<String>();
        //HashSet<int> missingTrackIndices = new HashSet<int>();
        int iterator = 0;
        bool pitchsemis_supported = Double.Parse((vegas.Version).Split(' ')[1]) > 14;
        string str_default_event_duration = "0.1";
        string default_text = "[Default Text]";
        string[] errlog = new string[2];
        errlog[0] = default_text;

        //Options (these are initializations-- variables aren't configured from here)
        track_mode_types track_mode = track_mode_types.grouped;

        bool flip_x = true;
        bool flip_y = false;
        bool legato_video = true;
        bool legato_audio = false;

        for (int i = 0; i < this_application.Project.Tracks.Count; i++)
        {
            track = this_application.Project.Tracks[i];

            //Keep track of the indexes of audio tracks.
            list_track_names_audio.Add(new int[] { i, 0 });

            //If the track is empty, don't add it to the index at all.
            if (track_empty(track)) { continue; }
            if (track.IsVideo())
            {
                last_video_track_index = i;
                track_audiovisual_grouping_index.Add(new int[] { i, 0 });//Add video tracks to the array index, so that the script can send timestamp data to the correct video track.

                event0 = track.Events[0];
                source_video_events.Add(event0);

                string track_name = "Track " + Convert.ToString(i);
                string vegas_track_name = track.Name;
                if (!string.IsNullOrEmpty(vegas_track_name))
                {
                    track_name += ": " + vegas_track_name;
                }
                list_track_names_linear.Add(track_name);
                list_track_names_grouped.Add(track_name);
            }
            else if (track.IsAudio())
            {
                string track_name = "Track " + Convert.ToString(i);
                string vegas_track_name = track.Name;
                if (!string.IsNullOrEmpty(vegas_track_name))
                {
                    track_name += ": " + vegas_track_name;
                }
                list_track_names_linear.Add(track_name);

                //Append to an array of source audio events.
                event0 = track.Events[0];
                source_audio_events.Add(event0);
                //Keep track of the indexes of audio tracks. Subtract 1 since this will be used to index source_audio_events, and count begins with 1, whereas index begins with 0.
                list_track_names_audio[i][1] = source_audio_events.Count - 1;

                if (track_audiovisual_grouping_index.Count > 0)
                {
                    //MessageBox.Show("Added literal audio track " + (i).ToString(), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);//DEBUG
                    //Increment the second value (number of succeeding audio tracks) in the last element in track_audiovisual_grouping_index, which is the most recent video track.
                    track_audiovisual_grouping_index[track_audiovisual_grouping_index.Count - 1][1]++;
                }
            }
        }
        //export track labels to be read by YTPMVE_UI
        string file_path_timestamps = Environment.ExpandEnvironmentVariables(temp_path + @"\timestamps.txt");
        string file_path_tracks_linear = Environment.ExpandEnvironmentVariables(temp_path + @"\tracks_linear.txt");
        string file_path_tracks_grouped = Environment.ExpandEnvironmentVariables(temp_path + @"\tracks_grouped.txt");
        using (StreamWriter writer = new StreamWriter(file_path_tracks_linear))
        {
            foreach (string item in list_track_names_linear)
            {
                writer.WriteLine(item);
            }
        }
        using (StreamWriter writer = new StreamWriter(file_path_tracks_grouped))
        {
            foreach (string item in list_track_names_grouped)
            {
                writer.WriteLine(item);
            }
        }

        int last_exit_code;

        //Run the YTPMVE UI/MIDI processing engine
        try
        {
            Process process = System.Diagnostics.Process.Start(engine_process_name[1]);
            process.WaitForExit();
            last_exit_code = process.ExitCode;
        }
        catch (System.Exception error)
        {
            MessageBox.Show("An error occurred while attempting to launch \"" + engine_file_path + "\"! \n\nError: " + error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (last_exit_code != 0)
        {
            try
            {
                errlog = System.IO.File.ReadAllLines(Environment.ExpandEnvironmentVariables(temp_path + @"\errlog.txt"));
                if (errlog.Length == 0)
                {
                    errlog[0] = default_text;
                }
            }
            catch
            {
                MessageBox.Show("Could not read the error log!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            MessageBox.Show("An error occurred during execution of " + engine_file_path + ":\n" + errlog[0], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        //Read the track mode configured in the YTPMVE UI. This should probably be merged with the next section but I'm getting lazy.
        string track_mode_text = System.IO.File.ReadAllText(Environment.ExpandEnvironmentVariables(temp_path + @"\track_mode.txt"));
        switch (track_mode_text)
        {
            case "grouped":
                track_mode = track_mode_types.grouped;
                break;
            case "linear":
                track_mode = track_mode_types.linear;
                break;
            default:
                break;
        }

        //Create a list based on the settings saved from the YTPMVE UI
        string json_track_settings = System.IO.File.ReadAllText(Environment.ExpandEnvironmentVariables(temp_path + @"\settings.json"));
        Dictionary<string, Dictionary<string, object>> dict_track_settings = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json_track_settings);
        List<string[]> track_to_channel = new List<string[]>();
        foreach (var item in dict_track_settings)
        {
            track_to_channel.Add(new string[] { item.Key, item.Value["channel"].ToString() });
        }

        //Read the timestamps file created by the MIDI processing engine
        try
        {
            array_timecodes_src = System.IO.File.ReadAllLines(file_path_timestamps);
        }
        catch
        {
            MessageBox.Show("Timestamps file unreadable or not generated from MIDI!", "Empty Array", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (array_timecodes_src.Length == 0)
        {
            MessageBox.Show("No timecodes found in timestamps.txt!", "Empty Array", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }



        //This region of the code is now much stricter about what data is allowed into the second array. You can put lots of random crap in timestamps.txt now and it will still mostly work.
        //The only thing that hasn't been fixed is that in situations in which the duration value on a line is non-numerical, it gets passed in, and ultimately set to 0. This should be fixed later.
        for (int i = 0; i < array_timecodes_src.Length; i++)
        {
            try
            {
                if (!(array_timecodes_src[i].Contains(",")))
                {
                    continue;//Don't read data from undelimited lines
                }

                string[] current_note = array_timecodes_src[i].Split(',');

                int note_channel = Int32.Parse(current_note[0]);//Channel
                int note_tone_offset = Int32.Parse(current_note[1]);//Semitone offset
                double note_start = Double.Parse(current_note[2]);//Start time
                var note_duration = current_note[3];//Duration	| We don't validate this fourth value, because it might be NULL.

                if (note_duration == "NULL")
                {
                    list_timecodes.Add(note_channel + "," + note_tone_offset + "," + note_start + "," + str_default_event_duration);

                    this_application.Project.Markers.Add(new Marker(Timecode.FromSeconds(note_start), "NULL DURATION"));
                    timestamps_contains_nulls = true;
                }
                else
                {
                    list_timecodes.Add(array_timecodes_src[i]);
                }
            }
            catch
            {
                //Ignore invalid data //Hey, maybe add an "errorMessage" bool and make this an error message option?
            }
        }


        Console.WriteLine("Array contents dumped to the file successfully!");

        if (timestamps_contains_nulls)
        {
            MessageBox.Show("Some notes were overlapping or invalid, and thus their durations could not be determined. Markers have been added at such positions in the timeline.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }



        //Duplicate the clip across the timeline according to the data in the timecodes array.
        foreach (string j in list_timecodes)
        {
            string[] current_note = j.Split(',');//Parse "1,2" into {"1","2"}

            int note_channel = Int32.Parse(current_note[0]);
            int note_tone_offset = Int32.Parse(current_note[1]);
            string note_start = current_note[2];
            string note_duration = current_note[3];

            //Search through the track to channel index
            iterator = 0;

            if (track_mode == track_mode_types.grouped)
            {
                foreach (string[] track_channel in track_to_channel)
                {
                    int track_linear_index = Int32.Parse(track_channel[0]);
                    int track_index = track_audiovisual_grouping_index[track_linear_index][0];
                    //If this track's channel is equal to the MIDI note's channel
                    if (Int32.Parse(track_channel[1]) == note_channel)
                    {

                        //Put an event on this track for this note
                        //Set the current event to the first (video) event on the track with track_audiovisual_grouping_index's index of the timestamp's channel 

                        //Set the track to the one that has the video index of this iteration.
                        try
                        {
                            tentative_track = this_application.Project.Tracks[track_index];
                        }
                        catch
                        {
                            //MessageBox.Show("index outta range", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);//DEBUG
                            //missingTrackIndices.Add(int.Parse(current_note[0]));//remove duplicates
                            tracks_missing = true;
                            continue;
                        }
                        if (!tentative_track.IsVideo())
                        {
                            MessageBox.Show("not a video track", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);//DEBUG
                            continue;
                        }
                        track = tentative_track;

                        //If there are no events, skip this track.
                        if (track_empty(track))
                        {
                            //MessageBox.Show("Track Empty: " + (track_index).ToString(), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);//DEBUG
                            continue;
                        }
                        else
                        {
                            //MessageBox.Show("Attempting to copy video event on track " + (track_audiovisual_grouping_index[track_linear_index][0]).ToString(), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);//DEBUG
                            track_event_generate(track, note_start, note_duration);
                        }

                        //Perform the copy for audio tracks succeeding the video track.
                        for (int k = 1; k <= track_audiovisual_grouping_index[track_linear_index][1]; k++)
                        {
                            //The kth audio track succeeding the video track.
                            //MessageBox.Show((track_audiovisual_grouping_index[track_linear_index][1]).ToString() + " audio tracks succeed video track " + (track_audiovisual_grouping_index[track_linear_index][0]).ToString(), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);//DEBUG
                            audio_track = this_application.Project.Tracks[track_audiovisual_grouping_index[track_linear_index][0] + k];
                            AudioEvent audio_track_source_audio_event = (AudioEvent)audio_track.Events[0];//test
                            try
                            {
                                audio_track_source_audio_event = (AudioEvent)source_audio_events[list_track_names_audio[audio_track.Index][1]];
                            }
                            catch
                            {
                                MessageBox.Show(Convert.ToString(audio_track.Index), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);//DEBUG
                            }
                            //if (!audio_track.IsAudio()) { continue; }//This should never happen, but I might not predict a situation where it would.

                            audio_event_generate(audio_track, audio_track_source_audio_event, note_start, note_duration, note_tone_offset, pitchsemis_supported);
                        }

                    }
                    iterator++;
                }
            }
            else if (track_mode == track_mode_types.linear)
            {
                foreach (string[] track_channel in track_to_channel)
                {
                    int track_linear_index = Int32.Parse(track_channel[0]);
                    int track_index = track_linear_index;
                    //If this track's channel is equal to the MIDI note's channel
                    if (Int32.Parse(track_channel[1]) == note_channel)
                    {

                        //Put an event on this track for this note
                        //Set the current event to the first (video) event on the track with track_audiovisual_grouping_index's index of the timestamp's channel 

                        //Set the track to the one that has the video index of this iteration.
                        try
                        {
                            tentative_track = this_application.Project.Tracks[track_index];
                        }
                        catch
                        {
                            //MessageBox.Show("index outta range", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);//DEBUG
                            //missingTrackIndices.Add(int.Parse(current_note[0]));//remove duplicates
                            tracks_missing = true;
                            continue;
                        }
                        track = tentative_track;

                        if (tentative_track.IsVideo())
                        {
                            //If there are no events, skip this track.
                            if (track_empty(track))
                            {
                                //MessageBox.Show("Track Empty: " + (track_index).ToString(), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);//DEBUG
                                continue;
                            }
                            else
                            {
                                //MessageBox.Show("Attempting to copy video event on track " + (track_audiovisual_grouping_index[track_linear_index][0]).ToString(), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);//DEBUG
                                track_event_generate(track, note_start, note_duration);
                            }
                        }
                        if (tentative_track.IsAudio())
                        {
                            if (track_empty(track))
                            {
                                continue;
                            }
                            else
                            {
                                AudioEvent audio_track_source_audio_event = (AudioEvent)source_audio_events[list_track_names_audio[track.Index][1]];
                                audio_event_generate(track, audio_track_source_audio_event, note_start, note_duration, note_tone_offset, pitchsemis_supported);
                            }
                        }
                    }
                    iterator++;
                }
            }

            //MessageBox.Show((track_audiovisual_grouping_index[note_channel][0]).ToString(), "Warning" , MessageBoxButtons.OK, MessageBoxIcon.Warning);//DEBUG
        }

        //missingTrackIndices = missingTrackIndices.Distinct().ToList();

        //Delete the source events, now that the clips have been synchronized.
        foreach (TrackEvent source_event in source_video_events)
        {
            source_event.Track.Events.Remove(source_event);
        }
        foreach (TrackEvent source_event in source_audio_events)
        {
            source_event.Track.Events.Remove(source_event);
        }


        //In this second loop, all other effects would be applied.
        iterator = -1;
        foreach (var item in dict_track_settings)
        {
            if (track_mode == track_mode_types.grouped)
            {
                iterator = track_audiovisual_grouping_index[Convert.ToInt32(item.Key)][0];
            }
            else if (track_mode == track_mode_types.linear)
            {
                iterator++;
            }

            try
            {
                track = this_application.Project.Tracks[iterator];
            }
            catch
            {
                //missingTrackIndices.Add(int.Parse(current_note[0]));//remove duplicates
                tracks_missing = true;
                continue;
            }

            if (track_empty(track))
            {
                continue;
            }

            var track_settings_key = item.Key;
            var track_settings_values = dict_track_settings[track_settings_key];

            if (Convert.ToInt32(track_settings_values["flip_x"]) == 0)
            {
                flip_x = false;
            }
            else if (Convert.ToInt32(track_settings_values["flip_x"]) == 1)
            {
                flip_x = true;
            }

            if (Convert.ToInt32(track_settings_values["flip_y"]) == 0)
            {
                flip_y = false;
            }
            else if (Convert.ToInt32(track_settings_values["flip_y"]) == 1)
            {
                flip_y = true;
            }

            if (Convert.ToInt32(track_settings_values["legato_video"]) == 0)
            {
                legato_video = false;
            }
            else if (Convert.ToInt32(track_settings_values["legato_video"]) == 1)
            {
                legato_video = true;
            }

            if (Convert.ToInt32(track_settings_values["legato_audio"]) == 0)
            {
                legato_audio = false;
            }
            else if (Convert.ToInt32(track_settings_values["legato_audio"]) == 1)
            {
                legato_audio = true;
            }

            flip_video(track, flip_x, flip_y);
            legato(track, Track_legato_type.LENGTHENING, legato_video, legato_audio);
        }

        if (tracks_missing)
        {
            //MessageBox.Show("Note data from tracks " + string.Join( ", ", missingTrackIndices.ToArray) + " was left out because there are not enough tracks.", "Warning" , MessageBoxButtons.OK, MessageBoxIcon.Warning); //Doesn't work because HashSet is broken
            MessageBox.Show("Note data from some tracks was left out because there are not enough tracks.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        if (transpositions_missing)
        {
            MessageBox.Show("YTPMVE was unable to pitch shift some of the clips, because Vegas Pro's API decided not to be dependable. It's nothing you did. You'll have to fix some of the clips manually. ¯\\_(ツ)_/¯", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        //Delete track and timestamp files.
        foreach (var file in new[] { file_path_tracks_linear, file_path_tracks_grouped, file_path_timestamps })
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}