using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace PlayerService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class PlayerServer : IService1
    {
        string dirPath = @"E:\www\uh1294526\uh1294526.ukrdomen.com";
        Player_En db = new Player_En();


        public void AddNewAlbum(Singer_Album NewAlbum)
        {
            Singer singer = (from sin in db.Singers where sin.Singer_ID == NewAlbum.Singer.ID select sin).First();
            Directory.CreateDirectory($"{dirPath}\\Singers\\{singer.Name}\\{NewAlbum.Title}");
            string image = $"{dirPath}\\Singers\\{singer.Name}\\{NewAlbum.Title}\\AlbumImage";
            File.WriteAllBytes(image, NewAlbum.Image);
            Album album = new Album()
            {
                Singer = singer,
                Title = NewAlbum.Title,
                ImagePath = image
            };
            db.Albums.Add(album);
            db.SaveChanges();
        }
        List<Singer> ConvertSongToSingerList(ICollection<Song_Singer> song_Singers)
        {
            List<Singer> singers = new List<Singer>();
            foreach (var song_Singer in song_Singers)
            {
                singers.Add((from sin in db.Singers where sin.Singer_ID == song_Singer.ID select sin).First());
            }
            return singers;
        }
        public void AddNewSinger(Song_Singer NewSinger)
        {
            Singer singer = new Singer()
            {
                Description = NewSinger.Description,
                Name = NewSinger.Name
            };
            Directory.CreateDirectory(dirPath + "\\Singers\\" + singer.Name);
            db.Singers.Add(singer);
            db.SaveChanges();
        }

        public void AddNewTrack(Song NewSong)
        {
            Album album = (from al in db.Albums where al.Album_ID == NewSong.Album_ID select al).First();
            string song = $"{dirPath}\\Singers\\{NewSong.Singers.First().Name}\\{album.Title}\\{NewSong.Title}";
            File.WriteAllBytes(song, NewSong.Music);
            Track track = new Track()
            {
                Singers = ConvertSongToSingerList(NewSong.Singers),
                Title = NewSong.Title,
                Album = album,
                Genre = NewSong.Genre,
                Path = song,
                TotalListens = 0,
                Verification = true
            };
            db.Tracks.Add(track);
            db.SaveChanges();
        }

        public List<string> BogdanLox()
        {
            List<string> lst = new List<string>();
            DirectoryInfo di = new DirectoryInfo(@"E:\www\uh1294526\uh1294526.ukrdomen.com\TestDirectory");
            foreach (var file in di.GetFiles())
            {
                lst.Add(file.Name);
            }
            return lst;
        }

        public void DownloadFile(byte[] arr)
        {
            File.WriteAllBytes(@"E:\www\uh1294526\uh1294526.ukrdomen.com\TestDirectory\UploadedFiles\test.php",arr);
        }

        public string GetData(int value)
        {
            return Environment.CurrentDirectory;
        }

        public byte[] GetFile()
        {
            return File.ReadAllBytes(@"E:\www\uh1294526\uh1294526.ukrdomen.com\TestDirectory\UploadedFiles\test.php");
        }
        List<Song> tmpConvert(ICollection<Track> tracks)
        {
            List<Song> songs = new List<Song>();
            foreach (var item in tracks)
            {
                songs.Add(new Song() { ID = item.Track_ID, Title = item.Title });
            }
            return songs;
        }
        public Singer_Album TempAlbum()
        {
            Album al = (from t in db.Albums where t.Album_ID == 3 select t).First();
            Singer_Album album = new Singer_Album()
            {
                ID = 3,
                Songs = tmpConvert(al.Tracks)

            };
            return album;
        }
        #region Convertors
        Song_Singer ConvertToSinger(Singer singer)
        {
            return new Song_Singer()
            {
                ID = singer.Singer_ID,
                Name = singer.Name
            };
        }
        List<Song_Singer> ConvertToListSong_Singers(ICollection<Singer> singes)
        {
            List<Song_Singer> song_Singers = new List<Song_Singer>();
            foreach (var item in singes)
            {
                song_Singers.Add(ConvertToSinger(item));
            }
            return song_Singers;
        }
        Song ConvertToSong(Track track)
        {
            return new Song()
            {
                ID = track.Track_ID,
                Genre = track.Genre,
                Title = track.Title,
                TotalListens = track.TotalListens,
                Verification = track.Verification,
                Album_ID = track.Album_ID,
                Singers = ConvertToListSong_Singers(track.Singers)
            };
        }
        List<Song> ConvertToListSong(ICollection<Track> tracks)
        {
            List<Song> songs = new List<Song>();
            foreach (var item in tracks)
            {
                songs.Add(ConvertToSong(item));
            }
            return songs;
        }
        #endregion
        public Singer_Album GetAlbum(int ID)
        {
            Album album = (from alb in db.Albums where alb.Album_ID == ID select alb).First();
            Singer_Album singer_Album = new Singer_Album()
            {
                ID = album.Album_ID,
                Image = File.ReadAllBytes(album.ImagePath),
                Singer = ConvertToSinger(album.Singer),
                Title = album.Title,
                Songs = ConvertToListSong(album.Tracks)
            };
            return singer_Album;
        }

        public Stream GetTrackStream(int ID)
        {
            Track track = (from tr in db.Tracks where tr.Track_ID == ID select tr).First();
            Stream st = new FileStream(track.Path, FileMode.Open, FileAccess.Read);
            return st;
        }
    }
}
