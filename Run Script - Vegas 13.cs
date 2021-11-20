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


		try{
			System.Diagnostics.Process.Start("python", pyFilePath).WaitForExit(); // Start the Python script instead and wait for it to finish.
		}
		catch{
			try{
				System.Diagnostics.Process.Start(exeFilePath).WaitForExit();//Or initiate the executable and wait for it to finish.
			}
			catch{
				MessageBox.Show("An error occurred while attempting to launch \"" + pyFilePath + "\" or \"" + exeFilePath + "\"! The file may be missing or named incorrectly.", "Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}	
		}


		string[] arrTimeCodesSource = System.IO.File.ReadAllLines(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Temp\YTPMVE\timestamps.txt"));
		
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
				}
				catch{
					//Don't add source events which do not exist.
				}
			}
			currentEvent = currentVegasApp.Project.Tracks[0].Events[0];
		}	
		catch{
			MessageBox.Show("No events exist in the timeline.", "Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
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
		if(tracksMissing){
			//MessageBox.Show("Note data from tracks " + string.Join( ", ", missingTrackIndices.ToArray) + " was left out because there are not enough tracks.", "Warning" , MessageBoxButtons.OK, MessageBoxIcon.Warning); //Doesn't work because HashSet is broken
			MessageBox.Show("Note data from some tracks was left out because there are not enough tracks.", "Warning" , MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}		
	}
}