using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support

namespace Roku_Test
{
    public class ControlSystem : CrestronControlSystem
    {
        Roku testRoku;
        
        public ControlSystem()
            : base()
        {
           
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(ControlSystem_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControlSystem_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(ControlSystem_ControllerEthernetEventHandler);
                CrestronConsole.AddNewConsoleCommand(RokuRequest, "MakeRequest", "Make a Request to the Roku Device", ConsoleAccessLevelEnum.AccessAdministrator);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        public void RokuRequest(string Request)
        {
            if (testRoku != null)
            {
                switch (Request.ToUpper())
                {
                    case "APPS":
                        {
                            testRoku.MakeRequest(Roku.eRokuRequest.GetAppList);
                            break;
                        }
                    case "UP":
                        {
                            testRoku.MakeRequest(Roku.eRokuRequest.KeyPress_Key, Roku.eRokuKeyCommand.Down);
                            break;
                        }
                    case "DOWN":
                        {
                            testRoku.MakeRequest(Roku.eRokuRequest.KeyPress_Key, Roku.eRokuKeyCommand.Up);
                            break;
                        }
                    case "HOME":
                        {
                            testRoku.MakeRequest(Roku.eRokuRequest.KeyPress_Key, Roku.eRokuKeyCommand.Home);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        public override void InitializeSystem()
        {
            try
            {
                testRoku = new Roku("10.64.57.140", 8060, "Test Rokue");
                testRoku.onResponseProcessed += new EventHandler<RokuEventArgs>(testRokue_onResponseProcessed);

            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        void testRokue_onResponseProcessed(object sender, RokuEventArgs e)
        {
            switch (e.requestMade)
            {
                case eRokuRequest.GetActiveApp:
                    foreach (var app in e.applicationList)
                    {
                        CrestronConsole.PrintLine(app.appName);
                    }
                    break;
                default:
                    break;
            }
        }

        void ControlSystem_ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }

        void ControlSystem_ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }

        }

        void ControlSystem_ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }

        }
    }
}