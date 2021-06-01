using NAudio.Wave.SampleProviders;
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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        void AddNewSinger(Song_Singer NewSinger);

        [OperationContract]
        void AddNewTrack(Song NewSong);

        [OperationContract]
        void AddNewAlbum(Singer_Album NewAlbum);

        [OperationContract]
        [FaultContract(typeof(EditFailed))]
        bool EditProfile(Client_User profile);

        [OperationContract]
        [FaultContract(typeof(LoginFailed))]
        Client_User TryLogin(Login_User login_User);

        [OperationContract]
        [FaultContract(typeof(RegFailed))]
        Client_User RegNewUser(Login_User login_User);

        [OperationContract]
        [FaultContract(typeof(RecoverFailed))]
        bool RecoverPassCodeRequest(Login_User login_User);

        [OperationContract]
        [FaultContract(typeof(RecoverFailed))]
        bool RecoverPassCodeCheck(Login_User login_User, string code);

        [OperationContract]
        [FaultContract(typeof(RecoverFailed))]
        bool ChangePassword(Login_User login_User);

        [OperationContract]
        Singer_Album GetAlbum(int ID);

        [OperationContract]
        byte[] GetTrack(int userID, int songID);
        [OperationContract]
        byte[] GetAlbumImage(int ID);

        [OperationContract]
        Song_Playlist GetRecentlyPlayed(int ID);

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        Song_Playlist GetPlaylistByID(int ID);

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        Song_Playlist GetPlaylistInfoByID(int ID);

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        Song_Playlist GetFavoriteTracksPlaylist(int userID);

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        List<Song_Playlist> GetFavoritePlaylists(int userID);


        [OperationContract]
        Song_Singer GetSingerFull(int ID);

        [OperationContract]
        bool AddPlaylistToFavorite(int userID, int playlistID);

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        bool AddTrackToFavorite(int userID, int trackID);

        [OperationContract]
        bool AddAlbumToFavorite(int userID, int playlistID);

        [OperationContract]
        bool CloneAlbumToFavoritePlaylist(int userID, int albumID);

        [OperationContract]
        bool ClonePlaylistToFavoritePlaylist(int userID, int playlistID);

        [OperationContract]
        bool RemoveFromPlaylistFavorite(int userID, int playlistID);

        [OperationContract]
        bool RemoveFromTrackFavorite(int userID, int trackID);

        [OperationContract]
        [FaultContract(typeof(DeleteFailed))]
        bool RemovePlaylist(int playlistID); 

         [OperationContract]
        List<Song_Playlist> GetUserPlaylistsInfo(int userID);

        [OperationContract]
        List<Song_Playlist> GetUserFavoritePlaylistsInfo(int userID);

        [OperationContract]
        List<Song_Singer> GetAllSingers();

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        List<Song_Playlist> GetRockToday();

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        List<Song_Playlist> GetPopToday();

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        List<Song_Playlist> GetHouseToday();

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        List<Song_Playlist> GetAllGenres();

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        List<Song_Playlist> GetSpecialForYou(int userID);

        [OperationContract]
        Stream GetTrackStream(int ID);

        [OperationContract]
        [FaultContract(typeof(LoginFailed))]
        Song_Playlist TempFunc();

        [OperationContract]
        [FaultContract(typeof(AddPlaylistFailed))]
        Song_Playlist AddPlaylist(Song_Playlist new_Playlist);

        [OperationContract]
        [FaultContract(typeof(AddPlaylistFailed))]
        Song_Playlist EditPlaylist(Song_Playlist new_Playlist);

        [OperationContract]
        [FaultContract(typeof(AddPlaylistFailed))]
        Song_Playlist AddGenrePlaylist(Song_Playlist new_Playlist);

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        Song_Playlist Search_GetAllSongs(string search_str);

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        List<Song_Singer> Search_GetAllSongSingers(string search_str);

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        List<Singer_Album> Search_GetAllSingerAlbums(string search_str);

        [OperationContract]
        [FaultContract(typeof(LoadPlaylistFailed))]
        List<Song_Playlist> Search_GetAllPlaylists(string search_str);

        [OperationContract]
        bool AddSongToPlaylist(int songID, int playlistID);

        [OperationContract]
        bool DeletePlaylist(int ID);

        [OperationContract]
        void tmp(byte[] img);

        [OperationContract]
        SearchResult Search(string searchStr);

        [OperationContract]
        Song_Playlist playlist(int ID);
    }

    [DataContract]
    public class SearchResult
    {
        [DataMember]
        public List<Song> Songs { get; set; }
        [DataMember]
        public List<Song_Singer> Singers { get; set; }
        [DataMember]
        public List<Singer_Album> Albums { get; set; }
        [DataMember]
        public List<Song_Playlist> Playlists { get; set; }
        [DataMember]
        public string Search_Str { get; set; }
    }
    
    [DataContract]
    public class Song : IComparable<Song>
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public int Path { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Genre { get; set; }
        [DataMember]
        public int TotalListens { get; set; }
        [DataMember]
        public bool Verification { get; set; }
        [DataMember]
        public byte[] Image { get; set; }
        [DataMember]
        public byte[] Music { get; set; }
        [DataMember]
        public Singer_Album Album { get; set; }
        [DataMember]
        public TimeSpan Duration { get; set; }
        [DataMember]
        public ICollection<Song_Singer> Singers { get; set; }
        [DataMember]
        public ICollection<Song> Songs { get; set; }

        public int CompareTo(Song other)
        {
            if (other == null)
                return 1;

            else
                return this.TotalListens.CompareTo(other.TotalListens);
        }
    }
    [DataContract]
    public class Song_Singer
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public byte[] Image { get; set; }
        [DataMember]
        public string ImagePath { get; set; }
        [DataMember]
        public ICollection<Singer_Album> Albums { get; set; }
    }

    [DataContract]
    public class Song_Playlist
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string ImagePath { get; set; }
        [DataMember]
        public byte[] Image { get; set; }
        [DataMember]
        public List<Song_Singer> Singers { get; set; }
        [DataMember]
        public Client_User Creator { get; set; }
        [DataMember]
        public DateTime CreationDate { get; set; }
        [DataMember]
        public bool Custom { get; set; }
        [DataMember]
        public bool PreLoaded { get; set; }
        [DataMember]
        public ICollection<Song> Songs { get; set; }
    }

    [DataContract]
    public class Singer_Album
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string ImagePath { get; set; }
        [DataMember]
        public byte[] Image { get; set; }
        [DataMember]
        public ICollection<Song> Songs { get; set; }
        [DataMember]
        public Song_Singer Singer { get; set; }
    }

    [DataContract]
    public class Client_User
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string NickName { get; set; }
        [DataMember]
        public string Password { get; set; }    
        [DataMember]
        public string ImagePath { get; set; }
        [DataMember]
        public byte[] Image { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public ICollection<Client_User> Contacts { get; set; }
        [DataMember]
        public ICollection<Song_Playlist> Playlists { get; set; }
        [DataMember]
        public ICollection<Song_Playlist> FavoritePlaylists { get; set; }

    }
    [DataContract]
    public class Login_User
    {
        public Login_User()
        {

        }
        public Login_User(string login, string email, string password, byte[] image)
        {
            Login = login;
            Email = email;
            Image = image;
            Password = password;
        }
        [DataMember]
        public string Login { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public byte[] Image { get; set; }
        [DataMember]
        public string Password { get; set; }
    }

    [DataContract]
    public class LoginFailed
    {
        string message;

        [DataMember]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
    [DataContract]
    public class LoadPlaylistFailed
    {
        string message = "Load Playlist Failed";

        [DataMember]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
    [DataContract]
    public class AddPlaylistFailed
    {
        string message = "Load Playlist Failed";

        [DataMember]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
    public class DeleteFailed
    {
        string message = "Delete failed";

        [DataMember]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
    [DataContract]
    public class RegFailed
    {
        string message;

        [DataMember]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
    [DataContract]
    public class EditFailed
    {
        string message;

        [DataMember]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
    [DataContract]
    public class RecoverFailed
    {
        string message;

        [DataMember]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
