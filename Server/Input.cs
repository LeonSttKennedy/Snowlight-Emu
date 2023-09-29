using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Snowlight.Config;
using Snowlight.Game.Misc;

namespace Snowlight
{
    public static class Input
    {
        /// <summary>
        /// Uses invoking thread (usually main thread) as input listener.
        /// </summary>
        public static void Listen()
        {
            while (Program.Alive)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    Console.Write("$" + Environment.UserName.ToLower() + "@snowlight> ");
                    string Input = Console.ReadLine();

                    if (Input.Length > 0)
                    {
                        ProcessInput(Input.Split(' '));
                    }
                }
            }
        }

        /// <summary>
        /// Handles command line input.
        /// </summary>
        /// <param name="Args">Arguments split by space character</param>
        public static void ProcessInput(string[] Args)
        {
            switch (Args[0].ToLower())
            {
                case "shutdown":

                    string TimeString = DateTime.Now.AddMinutes(15.0).ToShortTimeString();

                    if (Args.Length == 1 || Args[1] == "cancel")
                    {
                        if (ShutdownCommandWorker.Shutdown)
                        {
                            ShutdownCommandWorker.Shutdown = false;
                            ShutdownCommandWorker.TenMinToCloseAdvise = false;
                            Output.WriteLine("You had canceled scheduled a server shutdown successfully!", OutputLevel.Warning);
                            return;
                        }
                        else
                        {
                            Output.WriteLine("The correct use of the command is: \"shutdown <hour>\".", OutputLevel.Warning);
                            return;
                        }
                    }

                    if (Args[1].Contains(":"))
                    {
                        TimeString = Args[1];
                    }
                    else
                    {
                        Output.WriteLine("Please verify if the entered time has a \":\" separator.", OutputLevel.Warning);
                        return;
                    }

                    
                    if ((TimeSpan.Parse(TimeString) - TimeSpan.Parse(DateTime.Now.ToShortTimeString())).TotalMinutes <= 5.0)
                    {
                        Output.WriteLine("Please enter a time greater than 5 minutes from the current time.", OutputLevel.Warning);
                    }
                    else
                    {
                        ShutdownCommandWorker.Shutdown = true;
                        ShutdownCommandWorker.CloseHour = TimeSpan.Parse(TimeString);
                        Output.WriteLine("You had scheduled a server shutdown successfully! (At the time: " + TimeString + ")", OutputLevel.Warning);
                    }

                    return;

                case "delay": // Used to delay startup after a restart to allow current instance to shut down safely

                    int Delay = 5000;

                    if (Args.Length > 1)
                    {
                        int.TryParse(Args[1], out Delay);
                    }

                    Thread.Sleep(Delay);
                    return;

                case "restart":

                    Process.Start(Environment.CurrentDirectory + "\\Snowlight.exe", "\"delay 2600\"");
                    Program.Stop();
                    return;

                case "crash":

                    Environment.FailFast(string.Empty);
                    return;

                case "stop":

                    Program.Stop();
                    return;

                case "cls":

                    Output.ClearStream();
                    break;

                case "help":

                    Output.WriteLine("The following commands are available:", OutputLevel.Warning);
                    Output.WriteLine("delay <seconds>, restart, crash, shutdown, stop, cls, help", OutputLevel.Warning);
                    break;

                default:

                    Output.WriteLine("'" + Args[0].ToLower() + "' is not recognized as a command or internal operation.", OutputLevel.Warning);
                    break;
            }
        }
    }
}
