using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        string defAvatarPath = @"E:\www\uh1294526\uh1294526.ukrdomen.com\defaultAvatar.jpg";
        Player_En db = new Player_En();

        #region Adds
        public void AddNewAlbum(Singer_Album NewAlbum)
        {
            try
            {
                Singer singer = (from sin in db.Singers where sin.Singer_ID == NewAlbum.Singer.ID select sin).First();
                Directory.CreateDirectory($"{dirPath}\\Singers\\{singer.Name}\\{NewAlbum.Title}");

                if (NewAlbum.Image != null)
                {
                    File.WriteAllBytes($"{dirPath}\\Singers\\{singer.Name}\\{NewAlbum.Title}\\AlbumImage", NewAlbum.Image);
                }
                Album album = new Album()
                {
                    Singer = singer,
                    Title = NewAlbum.Title,
                    ImagePath = NewAlbum.Image == null ? defAvatarPath : $"{dirPath}\\Singers\\{singer.Name}\\{NewAlbum.Title}\\AlbumImage"
                };
                db.Albums.Add(album);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AddNewSinger(Song_Singer NewSinger)
        {
            try
            {
                string singer_Path = dirPath + "\\Singers\\" + NewSinger.Name;
                Directory.CreateDirectory(singer_Path);
                Directory.CreateDirectory(singer_Path + "\\Singles");
                if (NewSinger.Image != null)
                    File.WriteAllBytes(singer_Path + "\\avatar", NewSinger.Image);
                Singer singer = new Singer()
                {
                    Description = NewSinger.Description,
                    Name = NewSinger.Name,
                    ImagePath = NewSinger.Image == null ? defAvatarPath : singer_Path + "\\avatar"
                };
                Album album = new Album() { Title = "Singles", Singer = singer, ImagePath = defAvatarPath };
                db.Albums.Add(album);
                db.Singers.Add(singer);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void AddNewTrack(Song NewSong)
        {
            try
            {
                Album album = (from alb in db.Albums where alb.Album_ID == NewSong.Album_ID select alb).First();
                string song = $"{dirPath}\\Singers\\{album.Singer.Name}\\{album.Title}\\{NewSong.Title}";
                File.WriteAllBytes(song, NewSong.Music);
                Track track = new Track()
                {
                    Singers = ConvertSongToSingerList(NewSong.Singers),
                    Title = NewSong.Title,
                    Album = album,
                    Genre = NewSong.Genre,
                    Path = song,
                    TotalListens = 0,
                    Verification = true,
                    Duration = NewSong.Duration
                };
                db.Tracks.Add(track);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        public SearchResult Search(string searchStr)
        {
            var tracks = db.Tracks.Where(obj => obj.Title.Contains(searchStr));
            var singers = db.Singers.Where(obj => obj.Name.Contains(searchStr));
            var genres = db.Tracks.Where(obj => obj.Genre.Contains(searchStr));
            var albums = db.Albums.Where(obj => obj.Title.Contains(searchStr));
            SearchResult searchResult = new SearchResult()
            {
                Songs = ConvertToListSong(tracks.ToArray()),
                Singers = ConvertToListSong_SingersWAvatar(singers.ToArray()),
                GenreSongs = ConvertToListSong(genres.ToArray()),
                Albums = GetAlbumListWOSongs(albums.ToArray())
            };
            return searchResult;
        }

        public void DownloadFile(byte[] arr)
        {
            File.WriteAllBytes(defAvatarPath, arr);
        }

        public void TempFunc()
        {
            Singer singer = (from t in db.Singers where t.Singer_ID == 1 select t).First();
            singer.ImagePath = defAvatarPath;
            db.SaveChanges();
        }

        #region Convertors
        #region Singer
        Song_Singer ConvertToSong_SingerInfo(Singer singer)
        {
            return new Song_Singer()
            {
                ID = singer.Singer_ID,
                Name = singer.Name,
                Description = singer.Description
            };
        }
        Song_Singer ConvertToSong_SingerFull(Singer singer)
        {
            return new Song_Singer()
            {
                ID = singer.Singer_ID,
                Name = singer.Name,
                Description = singer.Description,
                Image = File.ReadAllBytes(singer.ImagePath),
                Albums = GetAlbumListWSongs(singer.Albums.ToArray())
            };
        }
        Song_Singer ConvertToSingerWAvatar(Singer singer)
        {
            return new Song_Singer()
            {
                ID = singer.Singer_ID,
                Name = singer.Name,
                Description = singer.Description,
                Image = File.ReadAllBytes(singer.ImagePath)
            };
        }
        List<Song_Singer> ConvertToListSong_SingersWAvatar(ICollection<Singer> singes)
        {
            List<Song_Singer> song_Singers = new List<Song_Singer>();
            foreach (var item in singes)
            {
                song_Singers.Add(ConvertToSingerWAvatar(item));
            }
            return song_Singers;
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
        #endregion
        #region Song
        List<Song_Singer> ConvertToListSong_Singers(ICollection<Singer> singes)
        {
            List<Song_Singer> song_Singers = new List<Song_Singer>();
            foreach (var item in singes)
            {
                song_Singers.Add(ConvertToSong_SingerInfo(item));
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
        #region Album
        public Singer_Album GetAlbum(int ID)
        {
            Album album = (from alb in db.Albums where alb.Album_ID == ID select alb).First();
            Singer_Album singer_Album = new Singer_Album()
            {
                ID = album.Album_ID,
                Image = File.ReadAllBytes(album.ImagePath),
                Singer = ConvertToSong_SingerInfo(album.Singer),
                Title = album.Title,
                Songs = ConvertToListSong(album.Tracks)
            };
            return singer_Album;
        }
        List<Singer_Album> GetAlbumListWSongs(int ID)
        {
            var albums = from alb in db.Albums where alb.Singer_Singer_ID == ID select alb;
            List<Singer_Album> lst = new List<Singer_Album>();
            foreach (var item in albums)
            {
                lst.Add(GetAlbum(item.Album_ID));
            }
            return lst;
        }
        List<Singer_Album> GetAlbumListWSongs(Album[] albums)
        {
            List<Singer_Album> lst = new List<Singer_Album>();
            foreach (var item in albums)
            {
                lst.Add(GetAlbum(item.Album_ID));
            }
            return lst;
        }
        Singer_Album GetAlbumWOSongs(int ID)
        {
            Album album = (from alb in db.Albums where alb.Album_ID == ID select alb).First();
            Singer_Album singer_Album = new Singer_Album()
            {
                ID = album.Album_ID,
                Image = File.ReadAllBytes(album.ImagePath),
                Singer = ConvertToSong_SingerInfo(album.Singer),
                Title = album.Title,
            };
            return singer_Album;
        }
        List<Singer_Album> GetAlbumListWOSongs(int ID)
        {
            var albums = from alb in db.Albums where alb.Singer_Singer_ID == ID select alb;
            List<Singer_Album> lst = new List<Singer_Album>();
            foreach (var item in albums)
            {
                lst.Add(GetAlbumWOSongs(item.Album_ID));
            }
            return lst;
        }
        List<Singer_Album> GetAlbumListWOSongs(Album[] albums)
        {
            List<Singer_Album> lst = new List<Singer_Album>();
            foreach (var item in albums)
            {
                lst.Add(GetAlbumWOSongs(item.Album_ID));
            }
            return lst;
        }
        #endregion
        #endregion
        public Stream GetTrackStream(int ID)
        {
            Track track = (from tr in db.Tracks where tr.Track_ID == ID select tr).First();
            Stream st = new FileStream(track.Path, FileMode.Open, FileAccess.Read);
            return st;
        }

        public List<Song_Singer> GetAllSingers()
        {
            try
            {
                var singers = from sin in db.Singers select sin;
                List<Song_Singer> lst = new List<Song_Singer>();
                foreach (var singer in singers)
                {
                    lst.Add(new Song_Singer() { Name = singer.Name, Albums = GetAlbumListWOSongs(singer.Singer_ID), Description = singer.Description, ID = singer.Singer_ID });
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Song_Singer GetSingerFull(int ID)
        {
            var singer = (from sin in db.Singers where sin.Singer_ID == ID select sin).FirstOrDefault();

            return ConvertToSong_SingerFull(singer);
        }

        public Song_Playlist playlist(int ID)
        {
            throw new NotImplementedException();
        }
    }
}
