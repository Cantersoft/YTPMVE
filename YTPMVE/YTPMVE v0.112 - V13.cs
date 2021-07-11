//YTPMVE v0.110 | © Matthew Hansen
//04282021
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;


public class EntryPoint
{
    Vegas currentVegasApp;
	public void FromVegas(Vegas vegas)
	{
		
		string[] YTPMVEFileNames = {"YTPMVE v0.110 Py.py", "YTPMVE v0.110 Py.exe"};
		
		try{
			System.Diagnostics.Process.Start(vegas.InstallationDirectory + "\\Script Menu\\YTPMVE\\" + YTPMVEFileNames[0]).WaitForExit();//Initiate the Python script and wait for it to finish.
		}
		catch{
			try{
				System.Diagnostics.Process.Start(vegas.InstallationDirectory + "\\Script Menu\\YTPMVE\\" + YTPMVEFileNames[1]).WaitForExit();//Or initiate the executable and wait for it to finish.
			}
			catch{
				MessageBox.Show("An error occurred while attempting to launch \"" + YTPMVEFileNames[0] + "\" or \"" + YTPMVEFileNames[1] + "\"! The file may be missing or named incorrectly.", "Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}	

		}
		
		string path = vegas.InstallationDirectory;
		
		currentVegasApp = vegas;
		TrackEvent currentEvent;
		TrackEvent sourceEvent;		
		string[] arrTimeCodesSource = System.IO.File.ReadAllLines(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Temp\YTPMVE\timestamps.txt"));
		
		List<String> arrTimeCodes = new List<String>();

		String strDefaultEventDuration = "0.1";
		String strCurrentEventStart = "0";
		
		bool timestampsContainsNulls = false;

		if (Convert.ToString(arrTimeCodesSource[0]) == "TrackError"){
			MessageBox.Show("An error occurred because there are " + Convert.ToString(arrTimeCodesSource[1]) + " tracks in the MIDI file you selected. Currently, this script only supports 1 track. Pls forgiv.", "Track Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}		
		
		for (int i = 0; i < arrTimeCodesSource.Length; i++){
			if (arrTimeCodesSource[i].Contains("NULL")){
				strCurrentEventStart = arrTimeCodesSource[i].Split(',')[0];
				arrTimeCodes.Add(strCurrentEventStart + "," + strDefaultEventDuration);

				currentVegasApp.Project.Markers.Add(new Marker(Timecode.FromSeconds(Double.Parse(strCurrentEventStart)), "NULL DURATION"));
				timestampsContainsNulls = true;
			}
			else{
				arrTimeCodes.Add(arrTimeCodesSource[i]);
			}
		}

		if(timestampsContainsNulls){
			MessageBox.Show("Some notes were overlapping or invalid, and thus their durations could not be determined. Markers have been added at such positions in the timeline.", "Warning" , MessageBoxButtons.OK, MessageBoxIcon.Warning);		
		}
		
		
		
		for (int i = 0; i < currentVegasApp.Project.Tracks.Count; i++){
			try{
				sourceEvent = currentVegasApp.Project.Tracks[i].Events[0];
				currentEvent = currentVegasApp.Project.Tracks[i].Events[0];
			}
			catch{
				continue;
			}
			foreach (string j in arrTimeCodes){
				string[] current_note = j.Split(',');//Parse "1,2" into {"1","2"}
				var copiedEvent = currentEvent.Copy(currentVegasApp.Project.Tracks[i], Timecode.FromPositionString(current_note[0], RulerFormat.Seconds));
				copiedEvent.AdjustStartLength(Timecode.FromPositionString(current_note[0], RulerFormat.Seconds), Timecode.FromPositionString(current_note[1], RulerFormat.Seconds), false);
			}
			currentVegasApp.Project.Tracks[i].Events.Remove(sourceEvent);
		}		
	}			
}
