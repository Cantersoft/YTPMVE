//YTPMVE
//20230418
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;
using System.Diagnostics;

public class EntryPoint{
    Vegas currentVegasApp;
	public void FromVegas(Vegas vegas){
		
		string path = vegas.InstallationDirectory + "\\Script Menu\\YTPMVE\\";//Full path to all scripts and files included with the installation of YTPMVE.
		string pyFilePath = "\"" + path + "YTPMVE.py" + "\"";
		string exeFilePath = "\"" + path + "YTPMVE.exe" + "\"";
		
		
		/*CHANGE THIS LINE TO CONTROL WHETHER THE EXECUTABLE OR PYTHON VERSION IS USED*/
		string engineFilePath = exeFilePath; 
		/*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/
		
		
		List<string> engineProcessName = new List<string>();
		
		if (engineFilePath == pyFilePath){
			engineProcessName.AddRange(new [] {"", pyFilePath});
		}	
		else if (engineFilePath == exeFilePath){
			engineProcessName.AddRange(new [] {"", exeFilePath});
		}
		else{
			MessageBox.Show("engineFilePath variable has been set to an invalid value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}	

		int lastExitCode;

		try{
			Process process = System.Diagnostics.Process.Start(engineProcessName[1]);
			process.WaitForExit();
			lastExitCode = process.ExitCode;
		}
		catch (System.Exception error){
			MessageBox.Show("An error occurred while attempting to launch \"" + engineFilePath + "\"! \n\nError: " + error.Message, "Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}	
	
		string default_text = "[Default Text]";
		string[] errlog = new string[2];
		errlog[0] = default_text;
		
		try{		
			errlog = System.IO.File.ReadAllLines(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Temp\YTPMVE\errlog.txt"));
			if (errlog.Length == 0){
				errlog[0] = default_text;
			}
		}
		catch{
			MessageBox.Show("Could not read error log.", "Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		if (lastExitCode != 0) {
			MessageBox.Show("An error occurred during execution of \"" + engineFilePath + "\":\n" + errlog[0], "Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		
		string[] arrTimeCodesSource = System.IO.File.ReadAllLines(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Temp\YTPMVE\timestamps.txt"));
		//string[] arrTimeCodesSource = {"1,1,1", "1,2,1"};	

		
		if (arrTimeCodesSource.Length == 0){
			MessageBox.Show("No timecodes found in timestamps.txt!", "Empty Array", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;			
		}

		
		currentVegasApp = vegas;
		TrackEvent currentEvent;
		List<TrackEvent> sourceEvents = new List<TrackEvent>();
		
		List<String> arrTimeCodes = new List<String>();
		//HashSet<int> missingTrackIndices = new HashSet<int>();

		String strDefaultEventDuration = "0.1";
		String strCurrentEventStart = "0";
		
		bool timestampsContainsNulls = false;
		bool tracksMissing = false;
		bool foundFirstEvent = false;
		
		//Options
		
		bool flip_x = true;
		bool flip_y = false;
			
		
		//This region of the code is now much stricter about what data is allowed into the second array. You can put lots of random crap in timestamps.txt now and it will still mostly work.
		//The only thing that hasn't been fixed is that in situations in which the duration value on a line is non-numerical, it gets passed in, and ultimately set to 0. This should be fixed later.
		for (int i = 0; i < arrTimeCodesSource.Length; i++){
			try{		
				if (!(arrTimeCodesSource[i].Contains(","))){
					continue;//Don't read data from undelimited lines
				}
				
				string[] current_note = arrTimeCodesSource[i].Split(',');		

				//Assure indices 0 and 1 are numerical. We don't worry about the third value cause it might be NULL.
				Double.Parse(current_note[0]);
				Double.Parse(current_note[1]);
				
				if (current_note[2] == "NULL"){
					arrTimeCodes.Add(strCurrentEventStart + "," + strDefaultEventDuration);

					currentVegasApp.Project.Markers.Add(new Marker(Timecode.FromSeconds(Double.Parse(current_note[1])), "NULL DURATION"));
					timestampsContainsNulls = true;
				}
				else{
					arrTimeCodes.Add(arrTimeCodesSource[i]);
				}
			}
			catch{
				//Ignore invalid data
			}			
		}

		if(timestampsContainsNulls){
			MessageBox.Show("Some notes were overlapping or invalid, and thus their durations could not be determined. Markers have been added at such positions in the timeline.", "Warning" , MessageBoxButtons.OK, MessageBoxIcon.Warning);		
		}
			
		try{
			for (int i = 0; i < currentVegasApp.Project.Tracks.Count; i++){
				try{
					sourceEvents.Add(currentVegasApp.Project.Tracks[i].Events[0]); //Append to an array of source events.
					if (foundFirstEvent == false){
						currentEvent = currentVegasApp.Project.Tracks[i].Events[0];
						foundFirstEvent = true;
					}	
				}
				catch{
					//Don't add source events which do not exist.
				}
			}
		}	
		catch{
			MessageBox.Show("No events exist in the timeline.", "Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		//Duplicate the clip across the timeline according to the data in the timecodes array.
		foreach (string j in arrTimeCodes){
			string[] current_note = j.Split(',');//Parse "1,2" into {"1","2"}
			
			try{
				currentEvent = currentVegasApp.Project.Tracks[int.Parse(current_note[0])].Events[0];
				var copiedEvent = currentEvent.Copy(currentVegasApp.Project.Tracks[int.Parse(current_note[0])], Timecode.FromPositionString(current_note[1], RulerFormat.Seconds));
				copiedEvent.AdjustStartLength(Timecode.FromPositionString(current_note[1], RulerFormat.Seconds), Timecode.FromPositionString(current_note[2], RulerFormat.Seconds), false);					
			}
			catch{
				//missingTrackIndices.Add(int.Parse(current_note[0]));//remove duplicates
				tracksMissing = true;
			}	
		}
		
		//missingTrackIndices = missingTrackIndices.Distinct().ToList();
		
		for (int i = 0; i < sourceEvents.Count; i++){
			currentVegasApp.Project.Tracks[sourceEvents[i].Track.Index].Events.Remove(sourceEvents[i]);
		}
		
		
		//SelectEveryOtherEvent.cs + sykhro auto flips || In this second loop, all other effects would be applied.	
        foreach (Track track in currentVegasApp.Project.Tracks){
            bool selectThisEvent = false;
            foreach (VideoEvent currentVideoEvent in track.Events){
                if (selectThisEvent){
                    currentVideoEvent.Selected = true;
					
					// Assign video vertexes to keyframes
					VideoMotionVertex tl = currentVideoEvent.VideoMotion.Keyframes[0].TopLeft;
					VideoMotionVertex tr = currentVideoEvent.VideoMotion.Keyframes[0].TopRight;
					VideoMotionVertex bl = currentVideoEvent.VideoMotion.Keyframes[0].BottomLeft;
					VideoMotionVertex br = currentVideoEvent.VideoMotion.Keyframes[0].BottomRight;
					
					if(flip_x){					
						// Re-bound them, in order to produce a horizontal flip.
						currentVideoEvent.VideoMotion.Keyframes[0].Bounds = new VideoMotionBounds(tr, tl, bl, br);
					}
					
					if(flip_y){
						// Re-bound them, in order to produce a vertical flip.
						currentVideoEvent.VideoMotion.Keyframes[0].Bounds = new VideoMotionBounds(bl, br, tr, tl);
					}					
                }
                selectThisEvent = !selectThisEvent;
            }
        }

		
		if(tracksMissing){
			//MessageBox.Show("Note data from tracks " + string.Join( ", ", missingTrackIndices.ToArray) + " was left out because there are not enough tracks.", "Warning" , MessageBoxButtons.OK, MessageBoxIcon.Warning); //Doesn't work because HashSet is broken
			MessageBox.Show("Note data from some tracks was left out because there are not enough tracks.", "Warning" , MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}		
	}
}