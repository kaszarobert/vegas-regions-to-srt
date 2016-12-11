/*
	Saves the Sony Vegas project Regions with their timing and text to a SubRip subtitle file.

*/
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Globalization;
using ScriptPortal.Vegas;

public class EntryPoint
{
    Vegas myVegas;

    public void FromVegas(Vegas vegas) {
        myVegas = vegas;
 
        String projName;

        String projFile = myVegas.Project.FilePath;
        if (String.IsNullOrEmpty(projFile)) {
            projName = "Untitled";
        } else  {
            projName = Path.GetFileNameWithoutExtension(projFile);
        }

        String exportFile = ShowSaveFileDialog("SubRip (*.srt)|*.srt", 
                                                  "Save Regions as Subtitles", projName + "-Regions");
	
        if (null != exportFile) {
            String ext = Path.GetExtension(exportFile);
 			if (null != ext)
                ExportRegionsToSRT(exportFile);
        }
    }
        
    StreamWriter CreateStreamWriter(String fileName, Encoding encoding) {
        FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        StreamWriter sw = new StreamWriter(fs, encoding);
        return sw;
    }

	void ExportRegionsToSRT(String exportFile) {
        StreamWriter streamWriter = null;
        try {
            streamWriter = CreateStreamWriter(exportFile, System.Text.Encoding.Default);
            int iSubtitle = 0;  //A numeric counter identifying each sequential subtitle
            foreach (Region region in myVegas.Project.Regions) {
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
        } finally {
            if (null != streamWriter)
                streamWriter.Close();
        }        
    }

    String ShowSaveFileDialog(String filter, String title, String defaultFilename) {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        if (null == filter) {
            filter = "All Files (*.*)|*.*";
        }
        saveFileDialog.Filter = filter;
        if (null != title)
            saveFileDialog.Title = title;
        saveFileDialog.CheckPathExists = true;
        saveFileDialog.AddExtension = true;
        if (null != defaultFilename) {
            String initialDir = Path.GetDirectoryName(defaultFilename);
            if (Directory.Exists(initialDir)) {
                saveFileDialog.InitialDirectory = initialDir;
            }
            saveFileDialog.DefaultExt = Path.GetExtension(defaultFilename);
            saveFileDialog.FileName = Path.GetFileName(defaultFilename);
        }
        if (System.Windows.Forms.DialogResult.OK == saveFileDialog.ShowDialog()) {
            return Path.GetFullPath(saveFileDialog.FileName);
        } else {
            return null;
        }
    }
}
