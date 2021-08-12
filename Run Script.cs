//github.com/Cantersoft/YTPMVE
//08112021

using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using ScriptPortal.Vegas;
using System.Diagnostics;

public class EntryPoint{
    Vegas currentVegasApp;
	public void FromVegas(Vegas vegas){

		string path = vegas.InstallationDirectory + "\\Script Menu\\YTPMVE\\";

		string[] YTPMVEFileNames = {"YTPMVE.py", "YTPMVE.exe"};
		string pyFilePath = path + YTPMVEFileNames[0];
		string exeFilePath = path + YTPMVEFileNames[1];
		
		bool isPythonPresent;
		string[] lines;
		bool retry;

		

		Process p = System.Diagnostics.Process.Start("python", "--version");
		p.WaitForExit(); // Check if Python is present
		if (p.ExitCode != 0) {
			isPythonPresent = false;
		} else {
			isPythonPresent = true;
		}

		Process p2 = System.Diagnostics.Process.Start(exeFilePath);
		p2.WaitForExit(); // Start the bundled Python script and wait for it to finish.
		lines = System.IO.File.ReadAllLines(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Temp\YTPMVE\errlog.txt"));
		retry = bool.Parse(lines[2]);
		if (p2.ExitCode != 0) {
			if (isPythonPresent && retry) {
				try{
					System.Diagnostics.Process.Start("python", pyFilePath).WaitForExit(); // Start the Python script instead and wait for it to finish.
				}
				catch (System.Exception e){
					MessageBox.Show("An error occurred while attempting to launch the script! \n\nError: " + e.Message + "\n\nDetails: \n" + lines[0], "Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				} else {
					MessageBox.Show("An error occurred while attempting to launch the script! \n\nDetails: \n" + lines[0], "Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
			}
		}

		currentVegasApp = vegas;
		TrackEvent currentEvent;
		TrackEvent sourceEvent;		
		string[] arrTimeCodesSource = System.IO.File.ReadAllLines(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Temp\YTPMVE\timestamps.txt"));
		
		List<String> arrTimeCodes = new List<String>();

		String strDefaultEventDuration = "0.1";
		String strCurrentEventStart = "0";
		
		bool timestampsContainsNulls = false;

		//Error handling/avoidance
		if (arrTimeCodesSource.Length == 0){
			MessageBox.Show("No timecodes found in timestamps.txt!", "Empty Array", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;			
		}
		
		if (Convert.ToString(arrTimeCodesSource[0]) == "Error"){
			MessageBox.Show(arrTimeCodesSource[2], arrTimeCodesSource[1], MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}		
		
		//This region of the code is now much stricter about what data is allowed into the second array. You can put lots of random crap in timestamps.txt now and it will still mostly work.
		//The only thing that hasn't been fixed is that in situations in which the second value on a line is non-numerical, it gets passed in, and ultimately set to 0. This should be fixed later.
		for (int i = 0; i < arrTimeCodesSource.Length; i++){
			try{
				Double.Parse(arrTimeCodesSource[i].Split(',')[0]);//Assure the first value is numerical
				if (!(arrTimeCodesSource[i].Contains(","))){
					continue;//Don't read data from undelimited lines
				}
				if (arrTimeCodesSource[i].Split(',')[1] == "NULL"){
					strCurrentEventStart = arrTimeCodesSource[i].Split(',')[0];
					arrTimeCodes.Add(strCurrentEventStart + "," + strDefaultEventDuration);

					currentVegasApp.Project.Markers.Add(new Marker(Timecode.FromSeconds(Double.Parse(strCurrentEventStart)), "NULL DURATION"));
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