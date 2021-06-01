using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace PlayerService
{
    public class PlayerServer : IService1
    {
        string dirPath = @"E:\www\uh1294526\uh1294526.ukrdomen.com";
        string defAvatarPath = @"E:\www\uh1294526\uh1294526.ukrdomen.com\defaultAvatar.jpg";
        string defUserAvatarPath = @"E:\www\uh1294526\uh1294526.ukrdomen.com\defaultAvatar.jpg";
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
                Album album = (from alb in db.Albums where alb.Album_ID == NewSong.Album.ID select alb).First();
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
            var tracks = db.Tracks.Where(x => x.Title.StartsWith(searchStr)).Take(4).ToList();
            if (tracks == null)
                tracks = new List<Track>();
            if (tracks.Count() < 4)
                {
                    var tracks1 = db.Tracks.Where(x => x.Title.Contains(searchStr)).ToList();
                    if (tracks1.Count > 0)
                    {
                        int i = 0;
                        while (tracks.Count < 4)
                        {
                            if (!tracks.Contains(tracks1[i]))
                            {
                                tracks.Add(tracks1[i]);
                            }

                            if (i == tracks1.Count - 1)
                                break;
                            i++;
                        }
                    }
                }
            var singers = db.Singers.Where(obj => obj.Name.StartsWith(searchStr)).Take(4).ToList();
            if (singers == null)
                singers = new List<Singer>();
            if (singers.Count() < 4)
                {
                    var singers1 = db.Singers.Where(x => x.Name.Contains(searchStr)).ToList();
                    if (singers1.Count > 0)
                    {
                        int i = 0;
                        while (singers.Count < 4)
                        {
                            if (!singers.Contains(singers1[i]))
                            {
                                singers.Add(singers1[i]);
                            }

                            if (i == singers1.Count - 1)
                                break;
                            i++;
                        }
                    }
                }
            
            var genres = db.Playlists.Where(obj => obj.Title.StartsWith(searchStr) && obj.Custom == false).Take(4).ToList();
            if (genres == null)
                genres = new List<Playlist>();
            if (genres.Count() < 4)
            {
                var genres1 = db.Playlists.Where(x => x.Title.Contains(searchStr) && x.Custom == false).ToList();
                if (genres1.Count > 0)
                {
                    int i = 0;
                    while (genres.Count < 4)
                    {
                        if (!genres.Contains(genres1[i]))
                        {
                            genres.Add(genres1[i]);
                        }

                        if (i == genres1.Count - 1)
                            break;
                        i++;
                    }
                }
            }
            var albums = db.Albums.Where(obj => obj.Title.StartsWith(searchStr)).Take(4).ToList();
            if (albums == null)
                albums = new List<Album>();
                if (albums.Count() < 4)
                {
                    var albums1 = db.Albums.Where(x => x.Title.Contains(searchStr)).ToList();
                    if (albums1.Count > 0)
                    {
                        int i = 0;
                        while (albums.Count < 4)
                        {
                            if (!albums.Contains(albums1[i]))
                            {
                                albums.Add(albums1[i]);
                            }

                            if (i == albums1.Count - 1)
                                break;
                            i++;
                        }
                    }
                
            }
            SearchResult searchResult = new SearchResult()
            {
                Songs = ConvertToListSong(tracks.ToArray()),
                Singers = ConvertToListSong_SingersWAvatar(singers.ToArray()),
                Playlists = Get_ListSong_Playlist_Prew(genres.ToArray()),
                Albums = GetAlbumListWOSongs(albums.ToArray()),
                Search_Str = searchStr
            };
            return searchResult;
        }
        public void DownloadFile(byte[] arr)
        {
            File.WriteAllBytes(dirPath + "\\Playlists\\Images\\4\\image", arr);
        }
        public Song_Playlist TempFunc()
        {
            //Singer singer = (from t in db.Singers where t.Singer_ID == 1 select t).First();
            //singer.ImagePath = defAvatarPath;
            //db.SaveChanges();
            return new Song_Playlist()
            {
                Songs = ConvertToListSong((from t in db.Playlists where t.Playlist_ID == 1 select t.Tracks).FirstOrDefault())
            };
        }
        Song_Playlist ConvertToSong_Playlist(Playlist playlist)
        {
            return new Song_Playlist()
            {
                ID = playlist.Playlist_ID,
                Creator = ConvertToClient_UserInfo(playlist.Creator_ID),
                Custom = playlist.Custom,
                CreationDate = playlist.CreationDate,
                Image = playlist.ImagePath == null ? null : File.ReadAllBytes(playlist.ImagePath),
                Songs = ConvertToListSong(playlist.Tracks),
                Title = playlist.Title
            };
        }
        List<Song_Playlist> ConvertToListSong_Playlist(ICollection<Playlist> playlists)
        {
            List<Song_Playlist> list = new List<Song_Playlist>();
            foreach (var item in playlists)
            {
                list.Add(ConvertToSong_Playlist(item));
            }
            return list;
        }
        List<Song_Playlist> ConvertToListSong_Playlist(IQueryable<Playlist> playlists)
        {
            List<Song_Playlist> list = new List<Song_Playlist>();
            foreach (var item in playlists)
            {
                list.Add(ConvertToSong_Playlist(item));
            }
            return list;
        }
        Song_Playlist ConvertToSong_PlaylistInfo(Playlist playlist)
        {
            return new Song_Playlist()
            {
                ID = playlist.Playlist_ID,
                Creator = ConvertToClient_UserInfo(playlist.Creator_ID),
                Custom = playlist.Custom,
                CreationDate = playlist.CreationDate,
                Title = playlist.Title
            };
        }
        public List<Song_Playlist> GetPopToday()
        {
            try
            {
                Song_Playlist playlist1 = ConvertToSong_Playlist((from t in db.Playlists where t.Playlist_ID == 5 select t).First());
                Song_Playlist playlist2 = ConvertToSong_Playlist((from t in db.Playlists where t.Playlist_ID == 6 select t).First());
                Song_Playlist playlist3 = ConvertToSong_Playlist((from t in db.Playlists where t.Playlist_ID == 7 select t).First());
                List<Song_Playlist> song_Playlists = new List<Song_Playlist>();
                song_Playlists.Add(playlist1);
                song_Playlists.Add(playlist2);
                song_Playlists.Add(playlist3);
                return song_Playlists;
            }
            catch
            {
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed());
            }
            
        }
        public List<Song_Playlist> GetRockToday()
        {
            try
            {
                Song_Playlist song1 = ConvertToSong_Playlist((from t in db.Playlists where t.Playlist_ID == 2 select t).First());
                Song_Playlist song2 = ConvertToSong_Playlist((from t in db.Playlists where t.Playlist_ID == 3 select t).First());
                Song_Playlist song3 = ConvertToSong_Playlist((from t in db.Playlists where t.Playlist_ID == 4 select t).First());
                Song_Playlist song4 = ConvertToSong_Playlist((from t in db.Playlists where t.Playlist_ID == 10 select t).First());
                List<Song_Playlist> song_Playlists = new List<Song_Playlist>();
                song_Playlists.Add(song1);
                song_Playlists.Add(song2);
                song_Playlists.Add(song3);
                song_Playlists.Add(song4);
                return song_Playlists;
            }
            catch
            {
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed());
            }
        }
        public List<Song_Playlist> GetHouseToday()
        {
            try
            {
                Song_Playlist playlist1 = ConvertToSong_Playlist((from t in db.Playlists where t.Playlist_ID == 8 select t).First());
                Song_Playlist playlist2 = ConvertToSong_Playlist((from t in db.Playlists where t.Playlist_ID == 9 select t).First());
                List<Song_Playlist> song_Playlists = new List<Song_Playlist>();
                song_Playlists.Add(playlist1);
                song_Playlists.Add(playlist2);
                return song_Playlists;
            }
            catch
            {
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed());
            }
        }
        public List<Song_Playlist> GetAllGenres()
        {
            try
            {
                List<Song_Playlist> song_Playlists = new List<Song_Playlist>();
                foreach (var item in (from g in db.Tracks select g.Genre).Distinct())
                {
                    song_Playlists.Add(ConvertToSong_Playlist((from t in db.Playlists where t.Title == item select t).FirstOrDefault()));
                }
               
                return song_Playlists;
            }
            catch
            {
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed());
            }
        }
        public Song_Playlist GetPlaylistByID(int ID)
        {
            try
            {
                return ConvertToSong_Playlist((from t in db.Playlists where t.Playlist_ID == ID select t).First());
            }
            catch
            {
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed());
            }
        }
        public Song_Playlist GetPlaylistInfoByID(int ID)
        {
            try
            {
                return ConvertToSong_PlaylistInfo((from t in db.Playlists where t.Playlist_ID == ID select t).First());
            }
            catch
            {
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed());
            }
        }
        void LinkSimularSongs()
        {
                var track = (from t in db.Tracks select t).ToArray();
                foreach (var item in track)
                {
                    //if (item.SimularTracks == null)
                    //    FindSimularTrack(item);
                    if (item.SimularTracks.Count < 3)
                        FindSimularTrack(item);
                }
        }
        public Song_Playlist AddPlaylist(Song_Playlist new_Playlist)
        {
            try
            {
                Playlist playlist = new Playlist
                {
                    Custom = true,
                    Title = new_Playlist.Title,
                    Creator_ID = new_Playlist.Creator.ID,
                    CreationDate = DateTime.Now
                };
                db.Playlists.Add(playlist);
                User user = (from t in db.Users where t.User_ID == playlist.Creator_ID select t).FirstOrDefault();
                user.Playlists.Add(playlist);
                db.SaveChanges();
                AddPlaylistImage(playlist, new_Playlist.Image);
                return new Song_Playlist { ID = playlist.Playlist_ID, Title = playlist.Title };
            }
            catch
            {
                throw new FaultException<AddPlaylistFailed>(new AddPlaylistFailed { Message = "Load playlist failed!" });
            }
            
        }
        public Song_Playlist AddGenrePlaylist(Song_Playlist new_Playlist)
        {
            try
            {
                Playlist playlist = new Playlist
                {
                    Custom = false,
                    Title = new_Playlist.Title,
                    Creator_ID = 4,
                    CreationDate = DateTime.Now,
                    Tracks = db.Tracks.Where(t => t.Genre == new_Playlist.Title).ToArray()
                    
                };
                db.Playlists.Add(playlist);
                db.SaveChanges();
                AddPlaylistImage(playlist, new_Playlist.Image);
                return new Song_Playlist { ID = playlist.Playlist_ID, Title = playlist.Title };
            }
            catch
            {
                throw new FaultException<AddPlaylistFailed>(new AddPlaylistFailed { Message = "Load playlist failed!" });
            }

        }
        public void tmp(byte[] img)
        {
            User user = (from t in db.Users where t.User_ID == 4 select t).FirstOrDefault();
            List<Track> tracks = new List<Track>();
            var s = (from t in db.Tracks where t.Genre == "House" select t).ToArray();
            Random random = new Random();
            for (int i = 0; i < random.Next(4,7); i++)
            {
                int c = random.Next(0, s.Length);
                if (tracks.Contains(s[c]))
                {
                    i--;
                    continue;
                }
                tracks.Add(s[c]);
            }
            Playlist playlist = new Playlist()
            {
                Title = "Move!",
                CreationDate = DateTime.Now,
                Creator_ID = user.User_ID,
                Custom = false,
                Tracks = tracks
            };
            db.Playlists.Add(playlist);
            db.SaveChanges();
            AddPlaylistImage(playlist, img);
        }
        void AddPlaylistImage(Playlist playlist, byte[] b)
        {
            Directory.CreateDirectory(dirPath + "\\Playlists\\Images\\" + playlist.Playlist_ID);
            if (b != null)
            {
                File.WriteAllBytes(dirPath + "\\Playlists\\Images\\" + playlist.Playlist_ID + "\\image", b);
                playlist.ImagePath = dirPath + "\\Playlists\\Images\\" + playlist.Playlist_ID + "\\image";
                db.SaveChanges();
            }
        }
        void AddPlaylistImage(GenrePlaylist playlist, byte[] b)
        {
            Directory.CreateDirectory(dirPath + "\\Playlists\\Images\\" + playlist.GenrePlaylist_ID);
            if (b != null)
            {
                File.WriteAllBytes(dirPath + "\\Playlists\\Images\\" + playlist.GenrePlaylist_ID + "\\image", b);
                playlist.ImagePath = dirPath + "\\Playlists\\Images\\" + playlist.GenrePlaylist_ID + "\\image";
                db.SaveChanges();
            }
        }
        void FindSimularTrack(Track track)
        {
            var tracks = (from t in db.Tracks where t.Genre == track.Genre select t).ToArray();
            Random random = new Random();
            int i = 0;
            bool flag;
            try
            {
                int max = tracks.Length > 4 ? 2 : tracks.Length - 1;
                while (true)
                {
                    if (i == 3)
                        break;
                    flag = true;
                    Track tmp = tracks[random.Next(0, tracks.Count())];
                    if (track.Track_ID == tmp.Track_ID)
                        continue;
                    foreach (var item in track.SimularTracks)
                    {
                        if (tmp.Track_ID == item.Track_ID)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag == true)
                    {
                        track.SimularTracks.Add(tmp);
                        db.SaveChanges();
                        i++;
                    }
                }
                
            }
            catch
            {
                throw new FaultException<LoginFailed>(new LoginFailed() { Message = "Fail in FindSimularTrack" });
            }
            
        }
        #region Convertors
        #region Singer
        List<Song_Singer> ConvertToListSong_Singers(ICollection<Singer> singes)
        {
            List<Song_Singer> song_Singers = new List<Song_Singer>();
            foreach (var item in singes)
            {
                song_Singers.Add(ConvertToSong_SingerInfo(item));
            }
            return song_Singers;
        }
        List<Song_Singer> ConvertToListSong_Singers(IQueryable<Singer> singes)
        {
            List<Song_Singer> song_Singers = new List<Song_Singer>();
            foreach (var item in singes)
            {
                song_Singers.Add(ConvertToSingerWAvatar(item));
            }
            return song_Singers;
        }
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
                Description = singer.Description == null ? null : singer.Description,
                Image = singer.ImagePath == null?null: File.ReadAllBytes(singer.ImagePath),
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
                Image = singer.ImagePath == null ? null : File.ReadAllBytes(singer.ImagePath)
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
        #endregion
        #region Song
        Song ConvertToSong(Track track)
        {
            return new Song()
            {
                ID = track.Track_ID,
                Genre = track.Genre,
                Title = track.Title,
                TotalListens = track.TotalListens,
                Verification = track.Verification,
                Album = GetAlbumInfo(track.Album_ID),
                Singers = ConvertToListSong_Singers(track.Singers),
                Duration = track.Duration
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
        List<Song> ConvertToListSong(IQueryable<Track> tracks)
        {
            List<Song> songs = new List<Song>();
            foreach (var item in tracks)
            {
                songs.Add(ConvertToSong(item));
            }
            return songs;
        }
        List<Song> ConvertToSortedListSong(IQueryable<Track> tracks)
        {
            List<Song> songs = new List<Song>();
            foreach (var item in tracks)
            {
                songs.Add(ConvertToSong(item));
            }
            songs.Sort();
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
        public Singer_Album GetAlbumInfo(int ID)
        {
            Album album = (from alb in db.Albums where alb.Album_ID == ID select alb).First();
            Singer_Album singer_Album = new Singer_Album()
            {
                ID = album.Album_ID,
                Singer = ConvertToSong_SingerInfo(album.Singer),
                Title = album.Title,
            };
            return singer_Album;
        }
        public byte[] GetAlbumImage(int ID)
        {
            Album album = (from alb in db.Albums where alb.Album_ID == ID select alb).First();
            byte[] img = album.ImagePath == null ? null : File.ReadAllBytes(album.ImagePath);
            return img;
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
                Image = album.ImagePath == null ? null : File.ReadAllBytes(album.ImagePath),
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
        List<Singer_Album> GetAlbumListWOSongs(IQueryable<Album> albums)
        {
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
        #region Playlist
        Song_Playlist Get_Song_Playlist_Info(Playlist playlist)
        {
            return new Song_Playlist() { ID = playlist.Playlist_ID, Title = playlist.Title };
        }
        List<Song_Playlist> Get_ListSong_Playlist_Info(ICollection<Playlist> playlists)
        {
            List<Song_Playlist> song_playlists = new List<Song_Playlist>();
            foreach (var playlist in playlists)
            {
                song_playlists.Add(Get_Song_Playlist_Info(playlist));
            }
            return song_playlists;
        }
        Song_Playlist Get_Song_Playlist_Prew(Playlist playlist)
        {
            return new Song_Playlist() { ID = playlist.Playlist_ID, Title = playlist.Title, Image = playlist.ImagePath==null?null:File.ReadAllBytes(playlist.ImagePath) };
        }
        List<Song_Playlist> Get_ListSong_Playlist_Prew(ICollection<Playlist> playlists)
        {
            List<Song_Playlist> song_playlists = new List<Song_Playlist>();
            foreach (var playlist in playlists)
            {
                song_playlists.Add(Get_Song_Playlist_Prew(playlist));
            }
            return song_playlists;
        }
        List<Song_Playlist> Get_ListSong_Playlist_Full(ICollection<Playlist> playlists)
        {
            List<Song_Playlist> song_playlists = new List<Song_Playlist>();
            foreach (var playlist in playlists)
            {
                song_playlists.Add(Get_Song_Playlist(playlist));
            }
            return song_playlists;
        }
        Song_Playlist Get_Song_Playlist(Playlist playlist)
        {
            return new Song_Playlist() { ID = playlist.Playlist_ID, Title = playlist.Title, CreationDate = playlist.CreationDate, 
                Image = playlist.ImagePath == null ? null : File.ReadAllBytes(playlist.ImagePath), Songs = ConvertToListSong(playlist.Tracks),Custom = playlist.Custom,
                Creator = ConvertToClient_UserInfo(db.Users.Where(t => t.User_ID == playlist.Creator_ID).FirstOrDefault()) };
        }
        #endregion
        #region User
        Client_User ConvertToClient_UserInfo(User user)
        {
            return new Client_User
            {
                ID = user.User_ID,
                NickName = user.NickName,
                Email = user.Email
            };
        }
        Client_User ConvertToClient_UserInfo(int ID)
        {
            User user = (from us in db.Users where us.User_ID == ID select us).FirstOrDefault();
            return new Client_User
            {
                ID = user.User_ID,
                NickName = user.NickName,
                Email = user.Email
            };
        }
        Client_User Get_Contact(User user)
        {
            return new Client_User() { ID = user.User_ID, NickName = user.NickName, Email = user.Email, Image = File.ReadAllBytes(user.ImagePath) };
        }
        List<Client_User> Get_Contacts_List(ICollection<User> users)
        {
            List<Client_User> contacts = new List<Client_User>();
            foreach (var user in users)
            {
                contacts.Add(Get_Contact(user));
            }
            return contacts;
        }
        #endregion
        #endregion
        Stream st;
        public Stream GetTrackStream(int ID)
        {
            Track track = (from tr in db.Tracks where tr.Track_ID == ID select tr).First();
            st = new FileStream(track.Path, FileMode.Open, FileAccess.Read);
            return st;
        }
        public Song_Singer GetSingerFull(int ID)
        {
            var singer = (from sin in db.Singers where sin.Singer_ID == ID select sin).FirstOrDefault();
            Song_Singer song_Singer = ConvertToSong_SingerFull(singer);
            return song_Singer;
        }
        public Song_Playlist playlist(int ID)
        {
            throw new NotImplementedException();
        }
        #region Login && Registration
        public Client_User TryLogin(Login_User login_User)
        {
            User user = (from us in db.Users where us.NickName == login_User.Login || us.Email == login_User.Login select us).FirstOrDefault();

            if (user == null)
                throw new FaultException<LoginFailed>(new LoginFailed() { Message = "User with this login/mail doesn't\nexist. Please, sign up." });
            if(user.Password != login_User.Password)
                throw new FaultException<LoginFailed>(new LoginFailed() { Message = user.Password });

            return new Client_User()
            {
                ID = user.User_ID,
                Email = user.Email,
                Image = user.ImagePath == null ? null : File.ReadAllBytes(user.ImagePath),
                NickName = user.NickName,
                Contacts = Get_Contacts_List(user.Contacts),
                Playlists = Get_ListSong_Playlist_Info(user.Playlists),
                FavoritePlaylists = Get_ListSong_Playlist_Info(user.FavoriteAlbums)
            };
        }

        public Client_User RegNewUser(Login_User login_User)
        {
            User user = (from us in db.Users where us.Email == login_User.Email select us).FirstOrDefault();
            if (user != null)
                throw new FaultException<RegFailed>(new RegFailed() { Message = "This mail already taken. Please try\nwith another one." });
            user = (from us in db.Users where us.NickName == login_User.Login select us).FirstOrDefault();
            if (user != null)
                throw new FaultException<RegFailed>(new RegFailed() { Message = "This login already taken. Please try\nwith another one." });
            user = new User()
            {
                Email = login_User.Email,
                NickName = login_User.Login,
                Password = login_User.Password
            };
            db.Users.Add(user);
            db.SaveChanges();
            CreateFolder(login_User.Login);
            if (login_User.Image != null)
                SaveAvatar(login_User.Login, login_User.Image);

            return TryLogin(login_User);
        }
        void CreateFolder(string login)
        {
            User user = (from us in db.Users where us.NickName == login select us).FirstOrDefault();
            Directory.CreateDirectory(dirPath + "\\Users\\" + user.User_ID);
        }
        void SaveAvatar(string login, byte[] image)
        {
            User user = (from us in db.Users where us.NickName == login select us).FirstOrDefault();
            string img = dirPath + "\\Users\\" + user.User_ID + "\\avatar";
            File.WriteAllBytes(img, image);

            user.ImagePath = img;
            db.SaveChanges();
        }
        public bool EditProfile(Client_User profile)
        {
            User source = (from us in db.Users where us.User_ID == profile.ID select us).FirstOrDefault();
            User user = (from us in db.Users where us.Email == profile.Email && us.User_ID != profile.ID select us).FirstOrDefault();
            if (user != null)
                throw new FaultException<EditFailed>(new EditFailed() { Message = "This mail already taken. Please try\nwith another one." });
            user = (from us in db.Users where us.NickName == profile.NickName && us.User_ID != profile.ID select us).FirstOrDefault();
            if (user != null)
                throw new FaultException<EditFailed>(new EditFailed() { Message = "This login already taken. Please try\nwith another one." });

            source.NickName = profile.NickName;
            source.Email = profile.Email;
            if(profile.Password != null)
                source.Password = profile.Password;
            if(profile.Image != null)
            {
                string img = dirPath + "\\Users\\" + profile.ID + "\\avatar";
                source.ImagePath = img;
                File.WriteAllBytes(img, profile.Image);
            }
            db.SaveChanges();
            return true;
        }
        #endregion
        #region Recover Password
        public bool RecoverPassCodeRequest(Login_User login_User)
        {
            User user = (from us in db.Users where us.NickName == login_User.Login || us.Email == login_User.Login select us).FirstOrDefault();
            if (user == null)
                throw new FaultException<RecoverFailed>(new RecoverFailed() { Message = "User with this login/mail doesn't\nexist." });
            
            VerificationCode ver_code = (from c in db.VerificationCodes where user.User_ID == c.Sender.User_ID && c.IsActive == true select c).FirstOrDefault();
            if (ver_code != null)
            {
                ver_code.IsActive = false;
                db.SaveChanges();
            }
            try { 
            string code = CreateCode();
            ver_code = new VerificationCode() { Code = code, IsActive = true, CreationTime = DateTime.Now, Sender = user };
            db.VerificationCodes.Add(ver_code);
            db.SaveChanges();
            SendMessage(ver_code);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool RecoverPassCodeCheck(Login_User login_User, string code)
        {
            User user = (from us in db.Users where us.NickName == login_User.Login || us.Email == login_User.Login select us).FirstOrDefault();
            if (user == null)
                throw new FaultException<RecoverFailed>(new RecoverFailed() { Message = "User with this login/mail doesn't\nexist." });


            VerificationCode ver_code = (from c in db.VerificationCodes where (user.User_ID == c.Sender.User_ID && c.IsActive == true) select c).FirstOrDefault();
            if (ver_code == null)
                throw new FaultException<RecoverFailed>(new RecoverFailed() { Message = "The code is incorrect.\nPlease, try again." });
            if(ver_code.Code != code)
                throw new FaultException<RecoverFailed>(new RecoverFailed() { Message = "The code is incorrect.\nPlease, try again." });
            ver_code.IsActive = false;
            db.SaveChanges();
            return true;

        }
        public bool ChangePassword(Login_User login_User)
        {
            User user = (from us in db.Users where us.NickName == login_User.Login || us.Email == login_User.Login select us).FirstOrDefault();
            if (user == null)
                throw new FaultException<RecoverFailed>(new RecoverFailed() { Message = "User with this login/mail doesn't\n exist." });
            try
            {
                user.Password = login_User.Password;
                db.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }
           
            return true;
        }
        #endregion
        #region Send ver code
        async void SendMessage(VerificationCode code)
        {


            // отправляем почту с учётной записи на сервере Google
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new NetworkCredential("InTimePlayerMail@gmail.com", "Asr21zfW6bv221H");
            client.EnableSsl = true;

            await client.SendMailAsync(CreateMailMessage(code));
            
        }
        MailMessage CreateMailMessage(VerificationCode code)
        {
            MailMessage mailMsg = new MailMessage();
            mailMsg.From = new MailAddress("SiegeTank@gamil.com");
            mailMsg.To.Add(new MailAddress(code.Sender.Email));
            mailMsg.IsBodyHtml = false;
            mailMsg.Subject = "Registration";
            mailMsg.Body = $"Hello, dear {code.Sender.NickName}, your verification code: {code.Code}";
            return mailMsg;
        }
        string CreateCode()
        {
            int cntNumber = 6;
            string code = "";

            Random random = new Random();
            for (int i = 0; i < cntNumber; i++)
            {
                int digit = random.Next(0, 9);
                code += digit.ToString();
            }
            return code;
        }
        #endregion

        public Song_Playlist GetRecentlyPlayed(int ID)
        {
            var tracks = (from us in db.Users where us.User_ID == ID select us.RecentlyPlayed).FirstOrDefault();

            return new Song_Playlist
            {
                Title = "Recently played",
                Songs = ConvertToListSong(tracks)
            };
        }

        public byte[] GetTrack(int userId,int songID)
        {
            Track track = (from tr in db.Tracks where tr.Track_ID == songID select tr).FirstOrDefault();
            User user = db.Users.Where(u => u.User_ID == userId).FirstOrDefault();
            user.RecentlyPlayed.Add(track);
            track.TotalListens += 1;
            db.SaveChanges();
            return File.ReadAllBytes(track.Path);
        }

        public bool AddPlaylistToFavorite(int userID, int playlistID)
        {
            User user = (from us in db.Users where us.User_ID == userID select us).FirstOrDefault();
            Playlist playlist = (from pl in db.Playlists where pl.Playlist_ID == playlistID select pl).FirstOrDefault();

            user.Playlists.Add(playlist);
            db.SaveChanges();
            return true;
        }

        public List<Song_Playlist> GetUserPlaylistsInfo(int userID)
        {
            return Get_ListSong_Playlist_Info((from t in db.Users where t.User_ID == userID select t.Playlists).FirstOrDefault());
        }

        public bool RemoveFromPlaylistFavorite(int userID, int playlistID)
        {
            User user = (from us in db.Users where us.User_ID == userID select us).FirstOrDefault();
            Playlist playlist = (from pl in db.Playlists where pl.Playlist_ID == playlistID select pl).FirstOrDefault();

            user.FavoriteAlbums.Remove(playlist);
            db.SaveChanges();
            return true;
        }
        public Song_Playlist GetFavoriteTracksPlaylist(int ID)
        {
            User user = (from us in db.Users where us.User_ID == ID select us).FirstOrDefault();
            return new Song_Playlist
            {
                Title = "Favorite tracks",
                Songs = ConvertToListSong(user.FavoriteTracks)
            };
        }

        public bool AddSongToPlaylist(int songID, int playlistID)
        {
            Track track = (from s in db.Tracks where s.Track_ID == songID select s).FirstOrDefault();
            if (track == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "Track not found!" });
            Playlist playlist = (from pl in db.Playlists where pl.Playlist_ID == playlistID select pl).FirstOrDefault();
            if (playlist == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "Playlist not found!" });
            try
            {
                playlist.Tracks.Add(track);
                db.SaveChanges();
            }
            catch (FaultException<LoadPlaylistFailed>)
            {
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message="Cant save to DB!"});
            }

            return true;
        }

        public bool DeletePlaylist(int ID)
        {
            Playlist playlist = (from pl in db.Playlists where pl.Playlist_ID == ID select pl).FirstOrDefault();
            try
            {
                db.Playlists.Remove(playlist);
                db.SaveChanges();
            }
            catch
            {
                return false;
            }
           
            return true;
        }

        public bool AddTrackToFavorite(int userID, int trackID)
        {
            Track track = (from s in db.Tracks where s.Track_ID == trackID select s).FirstOrDefault();
            if (track == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "Track not found!" });
            User user = (from us in db.Users where us.User_ID == userID select us).FirstOrDefault();
            if (user == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "User not found!" });
            try
            {
                user.FavoriteTracks.Add(track);
                db.SaveChanges();
            }
            catch (FaultException<LoadPlaylistFailed>)
            {
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "Cant save to DB!" });
            }
           
            return true;
        }

        public bool AddAlbumToFavorite(int userID, int playlistID)
        {
            User user = (from us in db.Users where us.User_ID == userID select us).FirstOrDefault();
            if (user == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "User not found!" });
            Playlist playlist = (from pl in db.Playlists where pl.Playlist_ID == playlistID select pl).FirstOrDefault();
            if (playlist == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "Playlist not found!" });
            try
            {
                user.FavoriteAlbums.Add(playlist);
                db.SaveChanges();
            }
            catch (FaultException<LoadPlaylistFailed>)
            {
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "Cant save to DB!" });
            }

            return true;
        }

        public bool CloneAlbumToFavoritePlaylist(int userID, int AlbumID)
        {
            Album album = (from alb in db.Albums where alb.Album_ID == AlbumID select alb).FirstOrDefault();
            if (album == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "Album not found!" });
            User user = (from us in db.Users where us.User_ID == userID select us).FirstOrDefault();
            if (user == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "User not found!" });
            try
            {
                Playlist playlist = new Playlist
                {
                    CreationDate = DateTime.Now,
                    Creator_ID = userID,
                    Custom = true,
                    Title = album.Title,
                    Tracks = album.Tracks
                };
                user.FavoriteAlbums.Add(playlist);
                db.Playlists.Add(playlist);
                db.SaveChanges();
                if(album.ImagePath != null)
                {
                    byte[] img = File.ReadAllBytes(album.ImagePath);
                    AddPlaylistImage(playlist, img);
                }
                
            }
            catch (Exception)
            {
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "Clone failed!" });
            }
            
            return true;
        }

        public bool ClonePlaylistToFavoritePlaylist(int userID, int playlistID)
        {
            Playlist sourcePlaylist = (from pl in db.Playlists where pl.Playlist_ID == playlistID select pl).FirstOrDefault();
            if (sourcePlaylist == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "Playlist not found!" });
            User user = (from us in db.Users where us.User_ID == userID select us).FirstOrDefault();
            if (user == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "User not found!" });
            try
            {
                Playlist playlist = new Playlist
                {
                    CreationDate = DateTime.Now,
                    Creator_ID = userID,
                    Custom = true,
                    Title = sourcePlaylist.Title,
                    Tracks = sourcePlaylist.Tracks
                };
                user.FavoriteAlbums.Add(playlist);
                db.Playlists.Add(playlist);
                db.SaveChanges();
                if (sourcePlaylist.ImagePath != null)
                {
                    byte[] img = File.ReadAllBytes(sourcePlaylist.ImagePath);
                    AddPlaylistImage(playlist, img);
                }

            }
            catch (Exception)
            {
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "Clone failed!" });
            }

            return true;
        }

        public bool RemovePlaylist(int playlistID)
        {
            Playlist playlist = (from pl in db.Playlists where pl.Playlist_ID == playlistID select pl).FirstOrDefault();
            if (playlist == null)
                throw new FaultException<DeleteFailed>(new DeleteFailed() { Message = "Playlist not found!" });
            try
            {
                Directory.Delete(dirPath + "\\Playlist\\Images\\" + playlist.Playlist_ID, true);
                db.Playlists.Remove(playlist);
                db.SaveChanges();
            }
            catch
            {
                throw new FaultException<DeleteFailed>(new DeleteFailed() { Message = "Can't delete playlist!" });
            }
            return true;
        }

        public bool RemoveFromTrackFavorite(int userID, int trackID)
        {
            Track track = (from pl in db.Tracks where pl.Track_ID == trackID select pl).FirstOrDefault();
            if (track == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "Track not found!" });
            User user = (from us in db.Users where us.User_ID == userID select us).FirstOrDefault();
            if (user == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "User not found!" });

            try
            {
                user.FavoriteTracks.Remove(track);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public List<Song_Playlist> GetFavoritePlaylists(int userID)
        {
            User user = (from us in db.Users where us.User_ID == userID select us).FirstOrDefault();
            if (user == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "User not found!" });

            return Get_ListSong_Playlist_Full(user.FavoriteAlbums);
        }

        public List<Song_Playlist> GetUserFavoritePlaylistsInfo(int userID)
        {
            User user = (from us in db.Users where us.User_ID == userID select us).FirstOrDefault();
            if (user == null)
                throw new FaultException<LoadPlaylistFailed>(new LoadPlaylistFailed() { Message = "User not found!" });

            return Get_ListSong_Playlist_Info(user.FavoriteAlbums);
        }

        public Song_Playlist Search_GetAllSongs(string searchStr)
        {
            List<Song> songs = ConvertToSortedListSong(db.Tracks.Where(tr => tr.Title.StartsWith(searchStr)));
            songs.AddRange(ConvertToSortedListSong(db.Tracks.Where(x => x.Title.Contains(searchStr))).Where(x => !songs.Any(t => t.ID == x.ID)));
            return new Song_Playlist { Title = $"Search result for songs \"{searchStr}\"", Songs = songs};
        }

        public List<Song_Singer> Search_GetAllSongSingers(string search_str)
        {
            List<Song_Singer> singers = ConvertToListSong_Singers(db.Singers.Where(sin => sin.Name.StartsWith(search_str)));
            singers.AddRange(ConvertToListSong_Singers(db.Singers.Where(sin => sin.Name.Contains(search_str))).Where(x => !singers.Any(t => t.ID == x.ID)));
            return singers;
        }

        public List<Singer_Album> Search_GetAllSingerAlbums(string search_str)
        {
            List<Singer_Album> albums = GetAlbumListWOSongs(db.Albums.Where(sin => sin.Title.StartsWith(search_str)));
            albums.AddRange(GetAlbumListWOSongs(db.Albums.Where(sin => sin.Title.Contains(search_str))).Where(x => !albums.Any(t => t.ID == x.ID)));
            return albums;
        }

        public List<Song_Playlist> Search_GetAllPlaylists(string search_str)
        {
            List<Song_Playlist> playlists = ConvertToListSong_Playlist(db.Playlists.Where(sin => sin.Title.StartsWith(search_str)));
            playlists.AddRange(ConvertToListSong_Playlist(db.Playlists.Where(sin => sin.Title.Contains(search_str) && sin.Custom == false)).Where(x=>!playlists.Any(t=>t.ID==x.ID)));
            return playlists;
        }

        public Song_Playlist EditPlaylist(Song_Playlist new_Playlist)
        {
            var playlist = (from pl in db.Playlists where pl.Playlist_ID == new_Playlist.ID select pl).FirstOrDefault();
            if (playlist == null)
                return null;
            try
            {
                playlist.Title = new_Playlist.Title;
                if (new_Playlist.Image != null)
                {
                    if(playlist.ImagePath != null)
                        File.WriteAllBytes(playlist.ImagePath, new_Playlist.Image);
                    else
                    {
                        AddPlaylistImage(playlist, new_Playlist.Image);
                    }
                }
                db.SaveChanges();
            }
            catch
            {
                return null;
            }
            return new_Playlist;
        }
        public List<Song_Playlist> GetSpecialForYou(int userID)
        {
            var topGenre = (from tg in db.Users where tg.User_ID == userID select tg.RecentlyPlayed.Select(x => x.Genre)).FirstOrDefault().ToArray();
            Random random = new Random();
            Song_Playlist song_Playlist = new Song_Playlist();
            List<Song> lst = new List<Song>();
            for (int i = 0; i < random.Next(15,23); i++)
            {
                string str = topGenre[random.Next(0, topGenre.Count())];
                var list = (from t in db.Tracks where t.Genre == str select t).ToArray();
                Song song = ConvertToSong(list.ElementAt(random.Next(0, list.Count())));

                if(!lst.Contains(song))
                    lst.Add(song);
            }
            song_Playlist.Songs = lst;
            song_Playlist.Creator = ConvertToClient_UserInfo(db.Users.Where(x => x.User_ID == 4).FirstOrDefault());
            song_Playlist.PreLoaded = true;
            List<Song_Playlist> song_Playlists = new List<Song_Playlist> { song_Playlist };
            return song_Playlists;
        }
    }
}
