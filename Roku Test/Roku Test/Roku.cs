using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronIO;

namespace Roku_Test
{
    public class Roku
    {

        RokuEventArgs rokuEventArgs;
        List<sAppInfo> appList;
        List<sAppInfo> completeAppList = new List<sAppInfo>();
        int listSize;

        string baseRequest = "";
        string requestBody = "";
        string description = "";
        static int numRokus;
        string processorIPAddress = "";

        #region Roku IP Connection -- Done
        HttpClient rokuHttpClient;
        #endregion

        #region Roku XML Parser -- Done
        XmlReader rokuXMLReader;
        #endregion

        /// <summary>
        /// SIMPL+ can only execute the default constructor. If you have variables that require initialization, please
        /// use an Initialize method
        /// </summary>
        /// 

        //Completed
        #region Instantiation
        public Roku(string Host, int Port, string Description, string ProcessorIPAddress)
        {
            try
            {
                // IP Address, Port Number -- Username && Password??
                appList = new List<sAppInfo>(); //Instantiate App List
                rokuEventArgs = new RokuEventArgs();

                if (Port == 0)
                {
                    Port = 8060;
                }

                baseRequest = string.Format(@"http://{0}:{1}/", Host, Port);
                description = Description;
                rokuEventArgs.rokuName = description;
                rokuHttpClient = new HttpClient();
                rokuHttpClient.HostAddress = Host;
                rokuHttpClient.Port = Port;
                processorIPAddress = ProcessorIPAddress;
                //TODO: Define the necessary information for this Roku device
                numRokus++;
                ClearAppList();
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error Instantiating Roku, Reason: {0}", e.Message);
            }
        }

        public void ClearAppList()
        {
            try
            {
                foreach (var file in Directory.GetFiles(string.Format(@"HTML\{0}", description)))
                {
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Notice("Error Updating Roku {}'s Applications, Reason: {0}", e.Message);
            }
        }
        #endregion

        #region Control roku
        
        #endregion

        #region Prepare body of Request
        //Prepare the body of the request for the Roku
        public void MakeRequest(eRokuRequest RequestToMake)
        {
            //TODO: Make a request to the Roku
            requestBody = "";
            try
            {
                switch (RequestToMake)
                {
                    case eRokuRequest.GetAppList:
                        {
                            requestBody = "query/apps";
                            break;
                        }
                    case eRokuRequest.GetActiveApp:
                        {
                            requestBody = "query/active-app";
                            break;
                        }
                    case eRokuRequest.GetIconByApp:
                        requestBody = "query/icon/";
                        break;
                    case eRokuRequest.Install_App:
                        requestBody = "install/";
                        break;
                    default:
                        break;
                }

                if (requestBody != "")
                {
                    SendRequest(baseRequest + requestBody, RequestToMake);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Notice("Error Sending Request, Reason: {0}", e.Message);
            }

        }

        public void MakeRequest(eRokuRequest RequestToMake, string AppID)
        {
            //TODO: Make a request to the Roku
            requestBody = "";
            try
            {
                switch (RequestToMake)
                {                   
                    case eRokuRequest.Launch_App:
                        requestBody = "launch/";
                        requestBody += AppID;
                        break;
                    default:
                        break;
                }

                if (requestBody != "")
                {
                    CrestronConsole.PrintLine(requestBody);
                    SendRequest(baseRequest + requestBody, RequestToMake);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Notice("Error Sending Request, Reason: {0}", e.Message);
            }

        }

        //Send Remote Control Command
        public void MakeRequest(eRokuRequest RequestToMake, eRokuKeyCommand KeyCommand)
        {
            //TODO: Make a request to the Roku
            requestBody = "";
            try
            {
                switch (RequestToMake)
                {
                    case eRokuRequest.KeyPress_Key:
                        requestBody = "keypress/" + KeyCommand.ToString();
                        ErrorLog.Notice("Command Set to: {0}", requestBody);
                        break;
                    case eRokuRequest.KeyPress_Down:
                        {
                            requestBody = "keypdown/" + KeyCommand.ToString();
                            break;
                        }
                    case eRokuRequest.KeyPress_Up:
                        {
                            requestBody = "keypdown/" + KeyCommand.ToString();
                            break;
                        }
                    default:
                        break;
                }

                if (requestBody != "")
                {
                    SendRequest(baseRequest + requestBody, RequestToMake);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Notice("Error Sending Request, Reason: {0}", e.Message);
            }

        }
        #endregion

        #region Send To Roku
        //Send Command to Roku
        private void SendRequest(string Request, eRokuRequest RequestMade)
        {
            //TODO: Connect To Roku
            string response = "";
            CrestronConsole.PrintLine("Request to be sent = {0}", Request);
            try
            {
                switch (RequestMade)
                {
                    case eRokuRequest.GetAppList:
                        response = rokuHttpClient.Get(Request, Encoding.ASCII);
                       
                        break;
                    case eRokuRequest.GetActiveApp:
                        break;
                    case eRokuRequest.GetIconByApp:
                        break;
                    case eRokuRequest.KeyPress_Down:
                        response = rokuHttpClient.Post(Request, new byte[]{});
                        break;
                    case eRokuRequest.KeyPress_Up:
                        response = rokuHttpClient.Post(Request, new byte[] { });
                        break;
                    case eRokuRequest.KeyPress_Key:
                        response = rokuHttpClient.Post(Request, new byte[] { });
                        break;
                    case eRokuRequest.Launch_App:
                        rokuHttpClient.Post(Request, new byte[] { });
                        break;
                    case eRokuRequest.Install_App:
                        break;
                    default:
                        break;
                }
                
                if (response.Length != 0)
                {
                    ProcessResponse(response, RequestMade);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Notice("Error Sending Request {0} to Roku", e.Message);
            }
        }
        #endregion

        #region Process Response
        //Process Response from Roku
        private void ProcessResponse(string Response, eRokuRequest RequestMade)
        {
            //CrestronConsole.PrintLine("Processing Response: {0}", Response);
            rokuXMLReader = new XmlReader(Response);
            bool getAppIcons = false;
            switch (RequestMade)
            {
                case eRokuRequest.GetAppList:
                    {
                        while (rokuXMLReader.Read())
                        {
                            //CrestronConsole.PrintLine("1");
                            switch (rokuXMLReader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    //CrestronConsole.PrintLine("2");
                                    if (rokuXMLReader.HasAttributes)
                                    {
                                        //CrestronConsole.PrintLine("3");
                                        while (rokuXMLReader.MoveToNextAttribute())
                                        {
                                            //CrestronConsole.PrintLine("Attrinbute is {0}", rokuXMLReader.Name);
                                            if (rokuXMLReader.Name.ToLower() == "id")
                                            {
                                                sAppInfo appInfo = new sAppInfo();
                                                appInfo.appId = rokuXMLReader.Value;
                                                appInfo.appName = rokuXMLReader.ReadString();
                                                if (!appList.Contains(appInfo))
                                                {
                                                    appList.Add(appInfo);
                                                }                                                
                                                ErrorLog.Notice("App: {0} with ID: {1} added to List", appInfo.appName, appInfo.appId);
                                                //Get Icon after the List has been generated
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        //if (listSize != appList.Count)
                        //{
                            getAppIcons = true;
                            rokuEventArgs.applicationList = appList;
                            listSize = appList.Count;
                        //}
                        break;
                    }
                case eRokuRequest.GetActiveApp:
                    while (rokuXMLReader.Read())
                    {
                        switch (rokuXMLReader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (rokuXMLReader.HasAttributes)
                                {
                                    while (rokuXMLReader.MoveToNextAttribute())
                                    {
                                        if (rokuXMLReader.Name.ToLower() == "app id")
                                        {
                                            sAppInfo appInfo = new sAppInfo();
                                            appInfo.appId = rokuXMLReader.Value;
                                            appInfo.appName = rokuXMLReader.ReadString();
                                            appList.Add(appInfo);
                                            ErrorLog.Notice("App: {0} with ID: {1} added to List", appInfo.appName, appInfo.appId);
                                            //Get Icon after the List has been generated
                                        }
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
            if (getAppIcons)
            {
                //TODO: Request App Icon List
               UpdateIcons();
            }
        }
       
        private void UpdateIcons()
        {
            //TODO: Move to a Thread
            byte[] iconData;
            int appIndex;
            string iconFileName = "";
            sAppInfo tempAppInfo;
            try
            {
                if (appList.Count != 0)
                {
                    foreach (var app in appList)
                    {
                        tempAppInfo = app;
                        appIndex = appList.IndexOf(app);
                        iconData = rokuHttpClient.GetBytes(baseRequest + "query/icon/" + app.appId);
                        iconFileName = writeToFile(iconData, app.appName);
                        if (iconFileName != "")
                        {
                            //CrestronConsole.PrintLine("File Name = {0}", iconFileName);
                            tempAppInfo.appIcon = iconFileName;
                            completeAppList.Add(tempAppInfo);
                            //CrestronConsole.PrintLine("App {0} - {1} - {2} added at Index {3}", tempAppInfo.appName, tempAppInfo.appId, tempAppInfo.appIcon, appIndex);
                        }                        
                    }
                    rokuEventArgs.requestMade = Roku_Test.eRokuRequest.GetAppList;
                    rokuEventArgs.applicationList = completeAppList;

                    if (onResponseProcessed != null)
                    {
                        onResponseProcessed(this, rokuEventArgs);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.Notice("Error requesting App Icon, reason: {0}", e.Message);
            }            
        }

        private string writeToFile(byte[] Content, string IconName)
        {
            FileStream createIcon;
            string fileName = "";
            try
            {
                fileName = string.Format(@"\HTML\{0}\{1}.png", this.description, IconName);
                //recallFileName = string.Format(@"\\NVRAM\{0}.png", IconName);

                if (!File.Exists(fileName))
                {
                    using (createIcon = new FileStream(fileName, FileMode.CreateNew))
                    {
                        createIcon.Write(Content, 0, Content.Length);
                        return string.Format(@"http://{0}\{1}\{2}.png", processorIPAddress,this.description, IconName);
                    }
                }                              
            }
            catch (Exception e)
            {
                ErrorLog.Notice("Error Writing to file, Reason: {0}", e.Message);
            }

            return "";
        }
        #endregion

        #region Enumerations

        public enum eRokuRequest
        {
            //TODO: List of Requests which can be made to the Roku Device
            GetAppList,
            GetActiveApp,
            GetIconByApp,
            KeyPress_Down,
            KeyPress_Up,
            KeyPress_Key,
            Launch_App,
            Install_App
        }

        public enum eRokuKeyCommand
        {
            Home,
            Rev,
            Fwd,
            Play,
            Select,
            Left,
            Right,
            Down,
            Up,
            Back,
            InstantReplay,
            Info,
            Backspace,
            Search,
            Enter
        }

        #endregion

        #region Event
        public event EventHandler<RokuEventArgs> onResponseProcessed;
        #endregion
    }

    public class RokuEventArgs : EventArgs
    {
        public string rokuName { get; set; }
        public List<sAppInfo> applicationList { get; set; }
        public eRokuRequest requestMade { get; set; }
    }

    public enum eRokuRequest
    {
        //TODO: List of Requests which can be made to the Roku Device
        GetAppList,
        GetActiveApp,
        GetIconByApp,
        KeyPress_Down,
        KeyPress_Up,
        KeyPress_Key,
        Launch_App,
        Install_App
    }

    #region Structore

    public struct sAppInfo
    {
        public string appName { get; set; }
        public string appIcon { get; set; }
        public string appId { get; set; }
    }

    #endregion
}