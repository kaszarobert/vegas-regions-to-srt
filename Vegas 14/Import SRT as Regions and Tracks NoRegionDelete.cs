/*
	Imports subtitles from *.srt to Sony Vegas regions and it creates a new Track with the new subtitle Texts.
	Note:
	- the imported Regions will not be quantized to frame boundaries (they will remain at the same Timecode as they were originally in the *.srt file)

*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ScriptPortal.Vegas;
using System.IO;

public class SrtInfo
{
    private Timecode startTime;
    private Timecode endTime;
    private List<string> subs;

    public Timecode getStartTime()
    {
        return startTime;
    }

    public Timecode getEndTime()
    {
        return endTime;
    }

    public Timecode getEndMinusStartTime()
    {
        return (endTime - startTime);
    }

    public string getText()
    {
        string s = "";
        int i;

        for(i=0; i<subs.Count; ++i)
        {
            s += subs[i];
            if (i != subs.Count - 1)
                s += "\r\n";
        }

       return s;
    }

    public SrtInfo(List<string> input)
    {
        string[] timeStrings = input[1].Split(((string)" ").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        startTime = new Timecode(timeStrings[0]);
        endTime = new Timecode(timeStrings[2]);

        subs = new List<string>();
        for (int subtitles = 2; subtitles < input.Count; ++subtitles)
        {
            subs.Add(input[subtitles]);
        }
    }

}

public class EntryPoint
{
    Vegas myVegas;

    public void FromVegas(Vegas vegas)
    {
        myVegas = vegas;
        Project proj = vegas.Project;
        List<SrtInfo> subs = new List<SrtInfo>();
        string s = "";
        int currentLineIndex = 0;

        //load all the lines from the *.srt to a linked list
        OpenFileDialog fileDialog = new OpenFileDialog();
        fileDialog.CheckFileExists = true;
        if (fileDialog.ShowDialog() == DialogResult.OK)
        {
            // read in the file            
            List<string> inputStrings = new List<string>();
            using (StreamReader sr = new StreamReader(fileDialog.FileName, Encoding.Default))
            {
                try
                {
                    bool isNotEmptySubtitle = false;

                    while ((s = sr.ReadLine()) != null)
                    {
                        currentLineIndex++;
                        if (s.Length != 0)
                        {
                            inputStrings.Add(s);
                            isNotEmptySubtitle = true;
                        }
                        else if (inputStrings.Count > 1)
                        {
                            subs.Add(new SrtInfo(inputStrings));
                            inputStrings.Clear();
                            isNotEmptySubtitle = false;
                        }
                    }

                    if (isNotEmptySubtitle && inputStrings.Count > 1)
                    {
                        subs.Add(new SrtInfo(inputStrings));
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(String.Format("Error reading the file. Maybe an invalid *.srt file. Error occured at line: {0}\r\nError message: {1}\r\nDetailed error message for debugging: {2}", currentLineIndex, e.Message, e.ToString()));
                    MessageBox.Show(String.Format("There has been an error while importing the subtitles. The subtitles and their timings may not be loaded correctly. Look it up, what could be the problem in your *.srt file around line {0}.\r\nFor example too many empty lines or the timecode format is wrong.", currentLineIndex));
                }
            }

            //a new Track with TrackEvents
            VideoTrack track = new VideoTrack();
            proj.Tracks.Add(track);

            foreach (SrtInfo x in subs)
            {
                try
                {
                    if (x.getStartTime().ToMilliseconds() == x.getEndTime().ToMilliseconds())
                    {
                        throw new Exception(String.Format("Error! Some subtitles have the same timecodes for the times they should appear and disappear. Check the subtitle at {0} -> {1}, because it may not have been loaded at the correct timecode!\r\nAlso, it may have loaded as a Marker instead of a Region.", x.getStartTime().ToString(), x.getEndTime().ToString()));
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                
                proj.Regions.Add(new Region(x.getStartTime(), x.getEndMinusStartTime(), x.getText()));
                AddTextEvent(myVegas, track, x.getStartTime(), x.getEndMinusStartTime());
            }
            
        }
    }

    void AddTextEvent(Vegas vegas, VideoTrack track, Timecode start, Timecode length)
    {
        // find the text generator plug-in
        //MessageBox.Show(vegas.Generators.GetChildByName("Sony Titles & Text").ToString());
        PlugInNode plugIn = vegas.Generators.GetChildByName("(Legacy) Text");
        // create a media object with the generator plug-in
        Media media = new Media(plugIn);
        // set the generator preset
        media.Generator.Preset = "centered";
        // add the video event
        VideoEvent videoEvent = track.AddVideoEvent(start, length);
        // add the take using the generated video stream
        Take take = videoEvent.AddTake(media.GetVideoStreamByIndex(0));
        //return videoEvent;
    }
}
