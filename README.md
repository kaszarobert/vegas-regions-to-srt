# Sony Vegas scripts for creating subtitles with Regions

If you wish to use Regions to manage subtitles for your project instead of the built-in srt tool, then this is all you need. Create Markers to the desired time periods, type in the subtitle's text & export it to srt easily with these scripts. While using the built-in tool, I ran into problems using non-English letters for the subtitles. For me, it was easier to do the job with Markers. 

## Usage:
1. Download the scripts and copy to your Vegas script folder. For example: c:\Program Files\Sony\Vegas Pro 13.0\Script Menu\
2. In Sony Vegas, go to Tools > Scripting.
3. If you don't see these scripts, click to 'Rescan Script Menu Folder' and do again Step 2.
4. Click to the desired script:

| Script filename                                   | What id does?                                                                                                                                                                                        |
|---------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Export SRT                                        | Create Regions, type in text and this will export you to an *.srt file.                                                                                                                              |
| Import SRT as Regions                             | Import an *.srt file, each subtitle will be a Region                                                                                                                                                 |
| Import SRT as Regions and Tracks                  | Import an *.srt file, each subtitle will be a Region and a Text Event on the timeline.                                                                                                               |
| Import SRT as Regions and Tracks NoRegionDelete   | Import an *.srt file, each subtitle will be a Region and a Text Event on the timeline. Plus, any previous Regions and Markers will remain intact.                                                    |
| Import SRT as Regions and Tracks Q                | Import an *.srt file, each subtitle will be a Region and a Text Event on the timeline. Plus the times will be quantized to frame boundaries.                                                         |
| Import SRT as Regions and Tracks Q NoRegionDelete | Import an *.srt file, each subtitle will be a Region and a Text Event on the timeline. Plus the times will be quantized to frame boundaries and any previous Regions and Markers will remain intact. |
| Import SRT as Regions NoRegionDelete              | Import an *.srt file, each subtitle will be a Region. Plus, any previous Regions and Markers will remain intact.                                                                                     |
| Import SRT as Tracks NoRegionDelete               | Import an *.srt file, each subtitle will be a Text Event on the timeline. This doesn't remove any previous Markers or Regions at all.                                                                |
