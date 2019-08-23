﻿using System;
using System.Text;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.CrestronXml;
using Newtonsoft.Json;
// For Basic SIMPL# Classes

namespace RokuIP
{
    public class Roku
    {

        RokuEventArgs rokuEventArgs;
        List<sAppInfo> appList;
        int listSize;
 
        string baseRequest = "";
        string requestBody = "";
        string description = "";
        static int numRokus;
        
        #region Roku IP Connection -- Done
        HttpClient rokuHttpClient;
        string hostAddress;
        int portNumber;
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
        public Roku(string Host, int Port, string Description)
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
                rokuHttpClient = new HttpClient();
                rokuHttpClient.HostAddress = Host;
                rokuHttpClient.Port = Port;                
                //TODO: Define the necessary information for this Roku device
                numRokus++;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error Instantiating Roku, Reason: {0}", e.Message);
            }
        }

        #region Prepare Get Request 
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
                    case eRokuRequest.KeyPress_Down:
                        requestBody = "keydown/key";
                        break;
                    case eRokuRequest.KeyPress_Up:
                        requestBody = "keyup/key";
                        break;
                    case eRokuRequest.KeyPress_Key:
                        requestBody = "keypress/";
                        break;
                    case eRokuRequest.Launch_App:
                        requestBody = "launch/";
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
            try
            {
                response = rokuHttpClient.Get(Request);
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
            rokuXMLReader = new XmlReader(Response);
            bool getAppIcons = false;
            switch (RequestMade)
            {
                case eRokuRequest.GetAppList:
                    {
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
                        if (listSize != appList.Count)
                        {
                            getAppIcons = true;
                            rokuEventArgs.applicationList = appList;
                            listSize = appList.Count;
                        }
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
                if (onResponseProcessed != null)
                {
                    onResponseProcessed(this, rokuEventArgs);
                }
            }
        }

        private void UpdateIcons()
        {
            //TODO: Move to a Thread

            foreach (var app in appList)
            {
                rokuHttpClient.Get(baseRequest + "query/apps/icon/" + app.appId);
            }
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