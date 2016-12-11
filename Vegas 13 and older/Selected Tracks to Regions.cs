/* 
	This will place a Region at each Event on the selected Tracks (can be 1 or multiple)
*/
using System;
using System.Windows.Forms;
using Sony.Vegas;

class EntryPoint
{
	public void FromVegas(Vegas vegas)
	{
		foreach (Track track in vegas.Project.Tracks)
		{
			// only the selected tracks
			if (!track.Selected) continue;

			foreach (TrackEvent trackEvent in track.Events)
			{
				Region region = new Region(trackEvent.Start, trackEvent.Length, trackEvent.ActiveTake.Name);
				try
				{
					vegas.Project.Regions.Add(region);
				}
				catch (Exception e)
				{
					MessageBox.Show(String.Format("Couldn't place Region at {0}\nThe error message: {1}", trackEvent.Start.ToString(), e.Message));
				}
			}
		}
	}
}