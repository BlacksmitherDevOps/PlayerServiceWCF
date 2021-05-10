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
        Singer_Album GetAlbum(int ID);
        [OperationContract]
        Singer_Album TempAlbum();

        [OperationContract]
        List<Song_Singer> GetAllSingers();
        [OperationContract]
        Stream GetTrackStream(int ID);
        [OperationContract]
        void AddNewAlbum(Singer_Album NewAlbum);
        [OperationContract]
        void GetFile();

        [OperationContract]
        void DownloadFile(byte[] arr);

        // TODO: Add your service operations here
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class Song
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
        public byte[] Music { get; set; }
        [DataMember]
        public int Album_ID { get; set; }
        [DataMember]
        public TimeSpan Duration { get; set; }
        [DataMember]
        public ICollection<Song_Singer> Singers { get; set; }
        public ICollection<Song> Songs { get; set; }
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
    public class Song_Playlist
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public int Title { get; set; }
        [DataMember]
        public string ImagePath { get; set; }
        [DataMember]
        public DateTime CreationDate { get; set; }
        [DataMember]
        public bool Custom { get; set; }
        [DataMember]
        public ICollection<Song> Songs { get; set; }
    }
}
