using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharp.CrestronIO;

namespace Roku_Test
{
    public class ControlSystem : CrestronControlSystem
    {
        Roku testRoku;
        BitConverter bitConverter;
        
        Ts1542 testPanel;

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
                ErrorLog.Notice("Application Directory is: {0}", Directory.GetApplicationDirectory());
                ErrorLog.Notice("Application Root Directory is: {0}", Directory.GetApplicationRootDirectory());
                
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        public void RokuRequest(string Request)
        {
            CrestronConsole.PrintLine(Request);

            if (testRoku != null)
            {
                if (Request.ToUpper().Contains("LAUNCH"))
                {
                    string[] pieces = Request.Split(' ');
                    CrestronConsole.PrintLine(Request);
                    testRoku.MakeRequest(Roku.eRokuRequest.Launch_App, pieces[1]);
                }
                else
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
        }

        public override void InitializeSystem()
        {
            try
            {

                testRoku = new Roku("172.22.253.234", 8060, "Test Rokue", CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0));
                testRoku.onResponseProcessed += new EventHandler<RokuEventArgs>(testRokue_onResponseProcessed);

                testPanel = new Ts1542(0x03, this);

                if (testPanel.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Notice("Error Registering Panel, Reason: {0}", testPanel.RegistrationFailureReason);
                }
                else
                {
                    testPanel.LoadSmartObjects(string.Format(@"{0}\Test UI.sgd", Directory.GetApplicationDirectory()));
                    foreach (var so in testPanel.SmartObjects)
                    {
                        so.Value.SigChange += new SmartObjectSigChangeEventHandler(Value_SigChange);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        void Value_SigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            CrestronConsole.PrintLine("Button {0} on {1} pressed", args.Sig.Name, args.SmartObjectArgs.ID);
        }

        void testRokue_onResponseProcessed(object sender, RokuEventArgs e)
        {
            int offset;
            switch (e.requestMade)
            {
                case eRokuRequest.GetAppList:
                    testPanel.SmartObjects[2].UShortInput["Set Number of Items"].UShortValue = (ushort)e.applicationList.Count;
                    ErrorLog.Notice("Number of Apps is: {0}", e.applicationList.Count);
                    foreach (var app in e.applicationList)
                    {
                        offset = (e.applicationList.IndexOf(app) * 2) + 1;
                        CrestronConsole.PrintLine(string.Format("App Name: {0} Image URL: {1} -- {2}", app.appName, app.appIcon, app.appId));
                        testPanel.SmartObjects[2].StringInput[string.Format("text-o{0}", offset)].StringValue = app.appName;
                        testPanel.SmartObjects[2].StringInput[string.Format("text-o{0}",offset + 1)].StringValue = app.appIcon;
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