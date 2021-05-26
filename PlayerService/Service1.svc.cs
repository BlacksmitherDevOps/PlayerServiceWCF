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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
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
            var tracks = (db.Tracks.Where(obj => obj.Title.Contains(searchStr))).Take(4);
            var singers = db.Singers.Where(obj => obj.Name.Contains(searchStr)).Take(4);
            var genres = db.Tracks.Where(obj => obj.Genre.Contains(searchStr)).Take(4);
            var albums = db.Albums.Where(obj => obj.Title.Contains(searchStr)).Take(4);
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
                Image = File.ReadAllBytes(playlist.ImagePath),
                Songs = ConvertToListSong(playlist.Tracks),
                Title = playlist.Title
            };
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
        public bool AddPlaylist(Song_Playlist new_Playlist)
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
                db.SaveChanges();
                AddPlaylist(playlist, new_Playlist.Image);
            }
            catch
            {
                throw new FaultException<AddPlaylistFailed>(new AddPlaylistFailed { Message = "Load playlist failed!" });
            }
            return true;
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
            AddPlaylist(playlist, img);
        }
        void AddPlaylist(Playlist playlist, byte[] b)
        {
            Directory.CreateDirectory(dirPath + "\\Playlists\\Images\\" + playlist.Playlist_ID);
            File.WriteAllBytes(dirPath + "\\Playlists\\Images\\" + playlist.Playlist_ID + "\\image", b);
            playlist.ImagePath = dirPath + "\\Playlists\\Images\\" + playlist.Playlist_ID + "\\image";
            db.SaveChanges();
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
        Song_Playlist Get_Song_Playlist(Playlist playlist)
        {
            return new Song_Playlist() { ID = playlist.Playlist_ID, Title = playlist.Title, CreationDate = playlist.CreationDate, Image = File.ReadAllBytes(playlist.ImagePath), Songs = ConvertToListSong(playlist.Tracks),Custom = playlist.Custom };
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

        public List<Song> GetRecentlyPlayed(int ID)
        {
            var tracks = (from us in db.Users where us.User_ID == ID select us.RecentlyPlayed).FirstOrDefault();
            if (tracks == null)
                return new List<Song>();
            return ConvertToListSong(tracks);
        }

        public object GetTrack(int ID, TimeSpan skipspan, TimeSpan takespan)
        {
            Track track = (from tr in db.Tracks where tr.Track_ID == ID select tr).First();
            var file = new AudioFileReader(track.Path);
            var trimmed = new OffsetSampleProvider(file);
            trimmed.SkipOver = TimeSpan.FromSeconds(15);
            trimmed.Take = TimeSpan.FromSeconds(10);
            trimmed.ToWaveProvider();
            return trimmed;
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

            user.Playlists.Remove(playlist);
            db.SaveChanges();
            return true;
        }
        public Song_Playlist GetFavoritePlaylist(int ID)
        {
            User user = (from us in db.Users where us.User_ID == ID select us).FirstOrDefault();
            return null;
        }

    }
}
