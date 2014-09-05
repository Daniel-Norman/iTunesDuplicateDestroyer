using System;
using iTunesLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace iTunesDuplicateFinder
{
    class iTunesManager
    {
        IITFileOrCDTrack currTrack;
        int deletedCount = 0;

        bool requireSameAlbum = true;
        bool deleteFiles = false;
        bool caseSensitive = false;
        double maximumLengthDifference = 5;
        ProgressBar progressBar;

        public iTunesManager(bool requireSameAlbum, bool deleteFiles, bool caseSensitive, double maximumLengthDifference, ProgressBar progressBar)
        {
            this.requireSameAlbum = requireSameAlbum;
            this.deleteFiles = deleteFiles;
            this.caseSensitive = caseSensitive;
            this.maximumLengthDifference = maximumLengthDifference;
            this.progressBar = progressBar;
        }

        public void RemoveDuplicates()
        {
            // iTunes classes
            iTunesApp itunes = new iTunesApp();
            IITLibraryPlaylist mainLibrary = itunes.LibraryPlaylist;
            IITTrackCollection tracks = mainLibrary.Tracks;

            // working variables
            int numTracks = tracks.Count;
            deletedCount = 0;

            List<IITFileOrCDTrack> trackList = new List<IITFileOrCDTrack>();
            List<List<IITFileOrCDTrack>> albumList = new List<List<IITFileOrCDTrack>>();

            progressBar.Minimum = 0;
            progressBar.Maximum = numTracks * 2;

            while (numTracks > 0)
            {
                // only work with files
                currTrack = tracks[numTracks] as IITFileOrCDTrack;

                // is this a file track?
                if (currTrack != null && currTrack.Kind == ITTrackKind.ITTrackKindFile)
                {
                    trackList.Add(currTrack);
                }

                // progress to the next tack
                numTracks--;
                progressBar.PerformStep();
            }

            IEnumerable<IGrouping<string, IITFileOrCDTrack>> duplicateList = trackList.GroupBy(track => (caseSensitive ? track.Name : track.Name.ToLower()));
            foreach (IGrouping<string, IITFileOrCDTrack> group in duplicateList)
            {
                progressBar.Increment(group.Count());
                if (group.Count() > 1)
                {
                    if (requireSameAlbum) GroupByAlbum(group);
                    else GroupByDuration(group);
                }
            }

            MessageBox.Show("Deleted " + deletedCount + " out of " + tracks.Count + " songs.", "Deletion Complete", MessageBoxButtons.OK);
        }

        //Take in a list of tracks with the same name, and group by album title
        private void GroupByAlbum(IEnumerable<IITFileOrCDTrack> tracks)
        {
            IEnumerable<IGrouping<String, IITFileOrCDTrack>> duplicateList = tracks.GroupBy(track => track.Album);
            foreach (IGrouping<String, IITFileOrCDTrack> group in duplicateList)
            {
                if (group.Count() > 1)
                {
                    GroupByDuration(group);
                }
            }
        }

        //Take in a list of tracks with the same name (and possibly album title), and group by track duration
        private void GroupByDuration(IEnumerable<IITFileOrCDTrack> tracks)
        {
            IEnumerable<IGrouping<double, IITFileOrCDTrack>> duplicateList = tracks.GroupBy(track => Math.Round(track.Duration / maximumLengthDifference));
            foreach (IGrouping<double, IITFileOrCDTrack> group in duplicateList)
            {
                if (group.Count() > 1)
                {
                    RemoveDuplicates(group);
                }
            }
        }

        //Remove duplicate songs, keeping the highest bitrate track
        private void RemoveDuplicates(IEnumerable<IITFileOrCDTrack> enumerableTracks)
        {
            List<IITFileOrCDTrack> tracks = enumerableTracks.ToList();
            double songDurations = 0, songDurationAverage = 0;
            int bestBitrate = tracks[0].BitRate;
            int bestTrackIndex = 0;
            IITFileOrCDTrack bestTrack = tracks[0];
            for (int i = 0; i < tracks.Count; ++i)
            {
                songDurations += tracks[i].Duration;

                if (tracks[i].BitRate > bestBitrate)
                {
                    bestBitrate = tracks[i].BitRate;
                    bestTrackIndex = i;
                }
            }
            songDurationAverage = songDurations / tracks.Count;
            for (int i = 0; i < tracks.Count; ++i)
            {
                if (Math.Abs(tracks[i].Duration - songDurationAverage) > maximumLengthDifference)
                {
                    break;
                }

                if (i != bestTrackIndex)
                {
                    if (deleteFiles)
                    {
                        File.Delete(tracks[i].Location);
                    }
                    tracks[i].Delete();
                    deletedCount++;
                }
            }
        }
    }
}
