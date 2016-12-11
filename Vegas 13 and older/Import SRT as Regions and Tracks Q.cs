/*
	Imports subtitles from *.srt to Sony Vegas regions and it creates a new Track with the new subtitle Texts.
	Note:
	- any previous Region or Marker will be deleted
	- the imported Regions will be quantized to frame boundaries (they will not remain exactly at the same Timecode as they were originally in the *.srt file). It makes video editing easier.

*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Sony.Vegas;
using System.IO;

public class SrtInfo
{
    private Timecode startTime;
    private Timecode endTime;
    private List<string> subs;
    private double Fps;

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

    private Timecode timeCodeCheck(Timecode t, double _fps)
    {
        double tMs = t.ToMilliseconds();
        //double timeForOneFrame = Math.Round(1 / _fps, 3) * 1000;
        double timeForOneFrame = (1 / _fps) * 1000;

        double currFps = tMs / timeForOneFrame;
        double leftFps, rightFps;

        if (!(currFps % 1 == 0))
        {
            //then it need to be quantized to frame time boundaries
            leftFps = Math.Truncate(currFps);
            rightFps = leftFps + 1;

            leftFps *= timeForOneFrame;
            rightFps *= timeForOneFrame;

            if (Math.Abs(tMs - leftFps) > Math.Abs(tMs - rightFps))
            {
                tMs = rightFps;
            }
            else
            {
                tMs = leftFps;
            }

            //it is now quantized, but we don't need it in milliseconds, we need to return it as a Timecode
            return new Timecode(tMs);
        }

		//everything is perfect, we don't need to do anything. The time fits to frame boundaries
        return t;
    }

    public SrtInfo(List<string> input, double _Fps)
    {
        
        string[] timeStrings = input[1].Split(((string)" ").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        Fps = _Fps;
        startTime = new Timecode(timeStrings[0]);
        endTime = new Timecode(timeStrings[2]);

        startTime = timeCodeCheck(startTime, _Fps);
        endTime = timeCodeCheck(endTime, _Fps);

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
        double fps = vegas.Project.Video.FrameRate;
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
                            subs.Add(new SrtInfo(inputStrings, fps));
                            inputStrings.Clear();
                            isNotEmptySubtitle = false;
                        }
                    }

                    if (isNotEmptySubtitle && inputStrings.Count > 1)
                    {
                        subs.Add(new SrtInfo(inputStrings, fps));
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(String.Format("Error reading the file. Maybe an invalid *.srt file. Error occured at line: {0}\r\nError message: {1}\r\nDetailed error message for debugging: {2}", currentLineIndex, e.Message, e.ToString()));
                    MessageBox.Show(String.Format("There has been an error while importing the subtitles. The subtitles and their timings may not be loaded correctly. Look it up, what could be the problem in your *.srt file around line {0}.\r\nFor example too many empty lines or the timecode format is wrong.", currentLineIndex));
                }
            }

            proj.Regions.Clear();
            proj.Markers.Clear();

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
