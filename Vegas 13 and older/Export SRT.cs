/*
	Saves the Sony Vegas project Regions with their timing and text to a SubRip subtitle file.
    If any of the 2 Regions are overlapping it shows an alert to the user.
*/
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Globalization;
using Sony.Vegas;

public class EntryPoint
{
    Vegas myVegas;

    public void FromVegas(Vegas vegas)
    {
        myVegas = vegas;
        String projName;
        String projFile = myVegas.Project.FilePath;

        // check overlapping Regions
        String overlappingRegions = checkOverlappingRegions();

        if (overlappingRegions != null)
        {
            if (MessageBox.Show(overlappingRegions + "\r\n Do you want to save with overlapped subtitles?", 
                "Warning", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }
        }

        projName = (String.IsNullOrEmpty(projFile))? "Untitled" : Path.GetFileNameWithoutExtension(projFile);

        String exportFile = ShowSaveFileDialog("SubRip (*.srt)|*.srt",
                                                  "Save Regions as Subtitles", projName + "-Regions");

        if (null != exportFile)
        {
            String ext = Path.GetExtension(exportFile);
            if (null != ext)
                ExportRegionsToSRT(exportFile);
        }
    }

    private bool isTimecodeRangesOverlapping(Timecode a_start, Timecode a_end, Timecode b_start, Timecode b_end)
    {
        if (a_start <= b_start && b_start < a_end) return true; // b starts in a
        if (a_start < b_end && b_end <= a_end) return true; // b ends in a
        if (b_start < a_start && a_end < b_end) return true; // a in b
        return false;
    }

    private string checkOverlappingRegions()
    {
        string output = null;
        int regionsCount = myVegas.Project.Regions.Count;
        for (int i = 0; i < regionsCount; i++)
        {
            for (int j = regionsCount - 1; j > i; j--) 
            {
                if (isTimecodeRangesOverlapping(
                    myVegas.Project.Regions[i].Position,
                    myVegas.Project.Regions[i].End,
                    myVegas.Project.Regions[j].Position,
                    myVegas.Project.Regions[j].End))
                {
                    output += String.Format("There are overlaps between Region {0}-{1} and Region {2}-{3}",
                        myVegas.Project.Regions[i].Position.ToString(RulerFormat.Time),
                        myVegas.Project.Regions[i].End.ToString(RulerFormat.Time),
                        myVegas.Project.Regions[j].Position.ToString(RulerFormat.Time),
                        myVegas.Project.Regions[j].End.ToString(RulerFormat.Time));
                }
            }
        }

        return output;
    }

    StreamWriter CreateStreamWriter(String fileName, Encoding encoding)
    {
        FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        StreamWriter sw = new StreamWriter(fs, encoding);
        return sw;
    }

    void ExportRegionsToSRT(String exportFile)
    {
        StreamWriter streamWriter = null;
        try
        {
            streamWriter = CreateStreamWriter(exportFile, System.Text.Encoding.Default);
            int iSubtitle = 0;  //A numeric counter identifying each sequential subtitle
            foreach (Region region in myVegas.Project.Regions)
            {
                StringBuilder tsv = new StringBuilder();
                iSubtitle++;
                tsv.Append(iSubtitle);
                tsv.Append("\r\n");
                var s = region.Position.ToString(RulerFormat.Time);
                if (s.Length > 12)
                {
                    s = s.Substring(0, 12);
                }
                tsv.Append(s);
                tsv.Append(" --> ");
                var s1 = region.End.ToString(RulerFormat.Time);
                if (s1.Length > 12)
                {
                    s1 = s1.Substring(0, 12);
                }
                tsv.Append(s1);
                tsv.Append("\r\n");
                tsv.Append(region.Label);
                tsv.Append("\r\n");
                streamWriter.WriteLine(tsv.ToString());
            }
        }
        finally
        {
            if (null != streamWriter)
                streamWriter.Close();
        }
    }

    String ShowSaveFileDialog(String filter, String title, String defaultFilename)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        if (null == filter)
        {
            filter = "All Files (*.*)|*.*";
        }
        saveFileDialog.Filter = filter;
        if (null != title)
            saveFileDialog.Title = title;
        saveFileDialog.CheckPathExists = true;
        saveFileDialog.AddExtension = true;
        if (null != defaultFilename)
        {
            String initialDir = Path.GetDirectoryName(defaultFilename);
            if (Directory.Exists(initialDir))
            {
                saveFileDialog.InitialDirectory = initialDir;
            }
            saveFileDialog.DefaultExt = Path.GetExtension(defaultFilename);
            saveFileDialog.FileName = Path.GetFileName(defaultFilename);
        }
        if (System.Windows.Forms.DialogResult.OK == saveFileDialog.ShowDialog())
        {
            return Path.GetFullPath(saveFileDialog.FileName);
        }
        else
        {
            return null;
        }
    }
}
