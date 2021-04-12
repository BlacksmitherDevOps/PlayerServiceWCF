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
    public class Service1 : IService1
    {
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
        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public byte[] GetFile()
        {
            return File.ReadAllBytes(@"E:\www\uh1294526\uh1294526.ukrdomen.com\TestDirectory\UploadedFiles\test.php");
        }
        
    }
}
