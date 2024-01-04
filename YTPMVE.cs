//YTPMVE
//20240104
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using ScriptPortal.Vegas;
using System.Diagnostics;
using System.Reflection;

public class EntryPoint
{

    static void flip_video_x(VideoEvent currentVideoEvent, VideoMotionVertex tl, VideoMotionVertex tr, VideoMotionVertex bl, VideoMotionVertex br)
    {
        // Re-bound them, in order to produce a horizontal flip
        currentVideoEvent.VideoMotion.Keyframes[0].Bounds = new VideoMotionBounds(tr, tl, bl, br);
    }
    static void flip_video_y(VideoEvent currentVideoEvent, VideoMotionVertex tl, VideoMotionVertex tr, VideoMotionVertex bl, VideoMotionVertex br)
    {
        // Re-bound them, in order to produce a vertical flip
        currentVideoEvent.VideoMotion.Keyframes[0].Bounds = new VideoMotionBounds(bl, br, tr, tl);
    }
    static void flip_video_xy(VideoEvent currentVideoEvent, VideoMotionVertex tl, VideoMotionVertex tr, VideoMotionVertex bl, VideoMotionVertex br)
    {
        // Re-bound them, in order to produce a horizontal flip
        currentVideoEvent.VideoMotion.Keyframes[0].Bounds = new VideoMotionBounds(br, bl, tl, tr);
    }

    //SelectEveryOtherEvent.cs + sykhro auto flips
    static void flip_video(Track track, bool flip_x, bool flip_y)
    {
        if (track.IsVideo())
        {
            bool selectThisEvent = false;
            int sequence_counter = 1;

            for (int i = 0; i < track.Events.Count; i++)
            {
                VideoEvent currentVideoEvent = (VideoEvent)track.Events[i];

                // Assign video vertexes to keyframes
                VideoMotionVertex tl = currentVideoEvent.VideoMotion.Keyframes[0].TopLeft;
                VideoMotionVertex tr = currentVideoEvent.VideoMotion.Keyframes[0].TopRight;
                VideoMotionVertex bl = currentVideoEvent.VideoMotion.Keyframes[0].BottomLeft;
                VideoMotionVertex br = currentVideoEvent.VideoMotion.Keyframes[0].BottomRight;

                if (flip_x && !flip_y)
                {
                    if (selectThisEvent)
                    {
                        flip_video_x(currentVideoEvent, tl, tr, bl, br);
                    }
                    selectThisEvent = !selectThisEvent;
                }

                if (flip_y && !flip_x)
                {
                    if (selectThisEvent)
                    {
                        flip_video_y(currentVideoEvent, tl, tr, bl, br);
                    }
                    selectThisEvent = !selectThisEvent;
                }

                if (flip_x && flip_y)
                {
                    // Rolling flips
                    switch (sequence_counter)
                    {
                        case 1:
                            //
                            break;
                        case 2:
                            flip_video_x(currentVideoEvent, tl, tr, bl, br);
                            break;
                        case 3:
                            flip_video_y(currentVideoEvent, tl, tr, bl, br);
                            break;
                        case 4:
                            flip_video_xy(currentVideoEvent, tl, tr, bl, br);
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
    //Otomad Helper Legato
    public enum Track_legato_type
    {
        STACKING,
        STRETCHING,
        LENGTHENING,
    }
    static void legato(Track track, Track_legato_type track_legato_type, bool legato_video, bool legato_audio)
    {
        if ((legato_video==true && track.IsVideo()) || (legato_audio == true && track.IsAudio()))
        {
            for (int i = 0; i < track.Events.Count - 1; i++)
            {
                // Timecode increaseSpacingTime = null;
                TrackEvent currentEvent = track.Events[i];
                TrackEvent nextEvent = track.Events[i + 1];
                if (nextEvent == null) continue;
                switch (track_legato_type)
                {
                    case Track_legato_type.STACKING:
                        {
                            //currentEvent.End = currentVegasApp.Project.TrackEvents[currentEvent.Index + 1].Start;
                            break;
                        }
                    case Track_legato_type.STRETCHING:
                        {
                            //bool forceStretch = type == Track_legato_type.STRETCHING;
                            //double rate = currentEvent.Length.ToMilliseconds() / (nextEvent.Start - currentEvent.Start).ToMilliseconds();
                            //currentEvent.RelativeAdjustPlaybackRate(rate, forceStretch, false);
                            break;
                        }
                    case Track_legato_type.LENGTHENING:
                        {
                            currentEvent.End = nextEvent.Start;
                            break;
                        }
                }
            }
        }
    }

Vegas currentVegasApp;
    public void FromVegas(Vegas vegas)
    {

        string path = vegas.InstallationDirectory + "\\.." + "\\YTPMVE\\";//Full path to engine files included with the installation of YTPMVE.
        string pyFilePath = "\"" + path + "YTPMVE.py" + "\"";
        string exeFilePath = "\"" + path + "YTPMVE.exe" + "\"";


        /*CHANGE THIS LINE TO CONTROL WHETHER THE EXECUTABLE OR PYTHON VERSION IS USED*/
        string engineFilePath = exeFilePath;
        /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/


        List<string> engineProcessName = new List<string>();

        if (engineFilePath == pyFilePath)
        {
            engineProcessName.AddRange(new[] { "", pyFilePath });
        }
        else if (engineFilePath == exeFilePath)
        {
            engineProcessName.AddRange(new[] { "", exeFilePath });
        }
        else
        {
            MessageBox.Show("engineFilePath variable has been set to an invalid value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        int lastExitCode;

        try
        {
            Process process = System.Diagnostics.Process.Start(engineProcessName[1]);
            process.WaitForExit();
            lastExitCode = process.ExitCode;
        }
        catch (System.Exception error)
        {
            MessageBox.Show("An error occurred while attempting to launch \"" + engineFilePath + "\"! \n\nError: " + error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string default_text = "[Default Text]";
        string[] errlog = new string[2];
        errlog[0] = default_text;

        try
        {
            errlog = System.IO.File.ReadAllLines(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Temp\YTPMVE\errlog.txt"));
            if (errlog.Length == 0)
            {
                errlog[0] = default_text;
            }
        }
        catch
        {
            MessageBox.Show("Could not read error log.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (lastExitCode != 0)
        {
            MessageBox.Show("An error occurred during execution of \"" + engineFilePath + "\":\n" + errlog[0], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }


        string[] arrTimeCodesSource = System.IO.File.ReadAllLines(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Temp\YTPMVE\timestamps.txt"));
        //string[] arrTimeCodesSource = {"1,1,1", "1,2,1"};	


        if (arrTimeCodesSource.Length == 0)
        {
            MessageBox.Show("No timecodes found in timestamps.txt!", "Empty Array", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }


        currentVegasApp = vegas;
        List<int[]> arrTrackIndex = new List<int[]>();
        TrackEvent currentEvent;
        TrackEvent copiedEvent;

        List<TrackEvent> source_events = new List<TrackEvent>();
        List<TrackEvent> source_audio_events = new List<TrackEvent>();

        List<String> arrTimeCodes = new List<String>();
        //HashSet<int> missingTrackIndices = new HashSet<int>();

        String strDefaultEventDuration = "0.1";

        bool timestampsContainsNulls = false;
        bool tracksMissing = false;

        //Options

        bool flip_x = true;
        bool flip_y = false;
        bool legato_video = true;
        bool legato_audio = false;


        //This region of the code is now much stricter about what data is allowed into the second array. You can put lots of random crap in timestamps.txt now and it will still mostly work.
        //The only thing that hasn't been fixed is that in situations in which the duration value on a line is non-numerical, it gets passed in, and ultimately set to 0. This should be fixed later.
        for (int i = 0; i < arrTimeCodesSource.Length; i++)
        {
            try
            {
                if (!(arrTimeCodesSource[i].Contains(",")))
                {
                    continue;//Don't read data from undelimited lines
                }

                string[] current_note = arrTimeCodesSource[i].Split(',');

                int note_track = Int32.Parse(current_note[0]);//Channel
                int note_tone_offset = Int32.Parse(current_note[1]);//Semitone offset
                double note_start = Double.Parse(current_note[2]);//Start time
                var note_duration = current_note[3];//Duration	| We don't validate this fourth value, because it might be NULL.

                if (note_duration == "NULL")
                {
                    arrTimeCodes.Add(note_track + "," + note_tone_offset + "," + note_start + "," + strDefaultEventDuration);

                    currentVegasApp.Project.Markers.Add(new Marker(Timecode.FromSeconds(note_start), "NULL DURATION"));
                    timestampsContainsNulls = true;
                }
                else
                {
                    arrTimeCodes.Add(arrTimeCodesSource[i]);
                }
            }
            catch
            {
                //Ignore invalid data //Hey, maybe add an "errorMessage" bool and make this an error message option?
            }
        }


        string filePath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Temp\YTPMVE\timecodes.txt");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (string item in arrTimeCodes)
            {
                writer.WriteLine(item);
            }
        }


        Console.WriteLine("Array contents dumped to the file successfully!");

        if (timestampsContainsNulls)
        {
            MessageBox.Show("Some notes were overlapping or invalid, and thus their durations could not be determined. Markers have been added at such positions in the timeline.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        try
        {
            for (int i = 0; i < currentVegasApp.Project.Tracks.Count; i++)
            {

                if (currentVegasApp.Project.Tracks[i].IsVideo())
                {
                    arrTrackIndex.Add(new int[] { i, 0 });//Add video tracks to the array index, so that the script can send timestamp data to the correct video track.
                }
                else if (currentVegasApp.Project.Tracks[i].IsAudio())
                {
                    if (arrTrackIndex.Count > 0)
                    {
                        arrTrackIndex[arrTrackIndex.Count - 1][1]++; //Increment value 2 in the last element in arrTrackIndex
                    }
                    source_audio_events.Add(currentVegasApp.Project.Tracks[i].Events[0]); //Append to an array of source audio events.
                    /*
					for (j = arrTrackIndex.Count - 1; j >= 0; j--){
						if (currentVegasApp.Project.Tracks[j].IsVideo()){
							arrTrackIndex[j][1]++;
							break;
						}	
						
					}	
					*/
                }

                try
                {
                    currentEvent = currentVegasApp.Project.Tracks[i].Events[0];
                    source_events.Add(currentEvent); //Append to an array of source events.   
                }
                catch
                {
                    //Don't add source events which do not exist.
                }
            }
        }
        catch
        {
            MessageBox.Show("No events exist in the timeline.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        //Duplicate the clip across the timeline according to the data in the timecodes array.
        foreach (string j in arrTimeCodes)
        {
            string[] current_note = j.Split(',');//Parse "1,2" into {"1","2"}

            int note_track = Int32.Parse(current_note[0]);
            int note_tone_offset = Int32.Parse(current_note[1]);
            string note_start = current_note[2];
            string note_duration = current_note[3];

            try
            {
                //MessageBox.Show((arrTrackIndex[note_track][0]).ToString(), "Warning" , MessageBoxButtons.OK, MessageBoxIcon.Warning);//DEBUG

                currentEvent = currentVegasApp.Project.Tracks[(arrTrackIndex[note_track][0])].Events[0];//Set the current event to the first event on the track with arrTrackIndex's index of the timestamp's channel 
                copiedEvent = currentEvent.Copy(currentVegasApp.Project.Tracks[(arrTrackIndex[note_track][0])], Timecode.FromPositionString(note_start, RulerFormat.Seconds));
                copiedEvent.AdjustStartLength(Timecode.FromPositionString(note_start, RulerFormat.Seconds), Timecode.FromPositionString(note_duration, RulerFormat.Seconds), false);

                for (int k = 1; k <= arrTrackIndex[note_track][1]; k++)
                {
                    currentEvent = currentVegasApp.Project.Tracks[(arrTrackIndex[note_track][0]) + k].Events[0];
                    copiedEvent = currentEvent.Copy(currentVegasApp.Project.Tracks[(arrTrackIndex[note_track][0]) + k], Timecode.FromPositionString(note_start, RulerFormat.Seconds));
                    copiedEvent.AdjustStartLength(Timecode.FromPositionString(note_start, RulerFormat.Seconds), Timecode.FromPositionString(note_duration, RulerFormat.Seconds), false);
                    /*PitchSemis NOT SUPPORTED IN VEGAS 14*/
                    AudioEvent copied_audio_event = (AudioEvent)copiedEvent;
                    AudioEvent source_audio_event = (AudioEvent)source_audio_events[note_track];

                    double note_tone = source_audio_event.PitchSemis + note_tone_offset;

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
                    /*END PitchSemis NOT SUPPORTED IN VEGAS 14*/
                }
            }
            catch
            {
                //missingTrackIndices.Add(int.Parse(current_note[0]));//remove duplicates
                tracksMissing = true;
            }
        }

        //missingTrackIndices = missingTrackIndices.Distinct().ToList();

        //Delete the source events, now that the clips have been synchronized.
        for (int i = 0; i < source_events.Count; i++)
        {
            currentVegasApp.Project.Tracks[source_events[i].Track.Index].Events.Remove(source_events[i]);
        }


        // In this second loop, all other effects would be applied.	
        foreach (Track track in currentVegasApp.Project.Tracks)
        {
            flip_video(track, flip_x, flip_y);
            legato(track, Track_legato_type.LENGTHENING, legato_video, legato_audio);
        }

        if (tracksMissing)
        {
            //MessageBox.Show("Note data from tracks " + string.Join( ", ", missingTrackIndices.ToArray) + " was left out because there are not enough tracks.", "Warning" , MessageBoxButtons.OK, MessageBoxIcon.Warning); //Doesn't work because HashSet is broken
            MessageBox.Show("Note data from some tracks was left out because there are not enough tracks.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}