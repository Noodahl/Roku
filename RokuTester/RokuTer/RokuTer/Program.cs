using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Crestron.SimplSharp.Net.Http;

namespace RokuTer
{
    class Program
    {        
        static void Main(string[] args)
        {
            ReadFile tester;
            Console.WriteLine("Application Starting, press Enter to Continue");

            tester = new ReadFile();
            Console.ReadLine();

            tester.RequestIcon();

            Console.WriteLine("Request Complete! Press Enter to close Application");
            Console.ReadLine();
        }

       
    }

    public class ReadFile
    {
        HttpClient testClient = new HttpClient();
        FileStream writeToFile;
        public void RequestIcon()
        {
            byte[] responseArray;

            responseArray = testClient.GetBytes("http://172.22.253.234:8060/query/icon/12");

            //responseArray = testClient.

            using (writeToFile = new FileStream(Directory.GetCurrentDirectory() + @"\testIcon.jpeg", FileMode.Create))
            {
                if (responseArray != null)
                {
                    writeToFile.Write(responseArray, 0, responseArray.Length);
                }
            }

        }
    }
}
