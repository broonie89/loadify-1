// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SpotifySharp;

namespace SpShellSharp
{
    class ConsoleCommand
    {
        public Func<string[], int> Function { get; private set; }
        public string HelpText { get; private set; }

        public ConsoleCommand(Func<string[], int> aFunction, string aHelpText)
        {
            Function = aFunction;
            HelpText = aHelpText;
        }
    }

    class ConsoleCommandDictionary : IEnumerable<ConsoleCommand>
    {
        Dictionary<string, ConsoleCommand> iCommands = new Dictionary<string, ConsoleCommand>();
        List<string> iOrder = new List<string>();
        Action iDone;

        public ConsoleCommandDictionary(Action aDone)
        {
            iDone = aDone;
        }

        public void Add(string aName, Func<string[], int> aFunction, string aHelpText)
        {
            iCommands.Add(aName, new ConsoleCommand(aFunction, aHelpText));
            iOrder.Add(aName);
        }

        public IEnumerator<ConsoleCommand> GetEnumerator()
        {
            return iCommands.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Execute(string aCommand)
        {
            string[] args = aCommand.Split();
            Execute(args);
        }

        public void Execute(string[] aCommand)
        {
            if (aCommand.Length == 0 || aCommand[0].Trim()=="")
            {
                iDone();
                return;
            }
            ConsoleCommand selectedCommand;
            if (iCommands.TryGetValue(aCommand[0], out selectedCommand))
            {
                if (selectedCommand.Function(aCommand) != 0)
                {
                    iDone();
                }
                return;
            }
            Console.WriteLine("No such command");
            iDone();
        }

        public int CmdHelp(string[] aCommand)
        {
            foreach (string name in iOrder)
            {
                Console.WriteLine("  {0,-20} {1}", name, iCommands[name].HelpText);
            }
            return -1;
        }
    }

    class ConsoleReader : IDisposable, IConsoleReader
    {
        AutoResetEvent iRequestInput;
        AutoResetEvent iProvideInput;
        string iInput;
        bool iStop;
        Thread iThread;

        public WaitHandle InputReady { get { return iProvideInput; } }

        public ConsoleReader()
        {
            iRequestInput = new AutoResetEvent(false);
            iProvideInput = new AutoResetEvent(false);
            iThread = new Thread(Run);
            iThread.IsBackground = true;
            iThread.Start();
        }
        public string GetInput()
        {
            return iInput;
        }
        public void RequestInput(string aPrompt)
        {
            Console.Write(aPrompt);
            iRequestInput.Set();
        }
        public void Stop()
        {
            iStop = true;
            iRequestInput.Set();
        }

        public void Run()
        {
            while (true)
            {
                iRequestInput.WaitOne();
                if (iStop) return;
                iInput = Console.ReadLine();
                iProvideInput.Set();
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string username = args.Length > 0 ? args[0] : null;
            string blob = args.Length > 1 ? args[1] : null;
            string password = null;
            byte[] appkey;

            bool selftest = args.Length > 2 ? args[2] == "selftest" : false;
            try
            {
                appkey = File.ReadAllBytes("spotify_appkey.key");
            }
            catch (IOException)
            {
                Console.WriteLine("Please download your binary app key from Spotify and put it in");
                Console.WriteLine("the working directory as 'spotify_appkey.key'. See here:");
                Console.WriteLine("https://developer.spotify.com/technologies/libspotify/keys/");
                Console.WriteLine("");
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return;
            }

            using (var consoleReader = new ConsoleReader())
            {


                WaitHandle[] handles = new WaitHandle[2];
                AutoResetEvent spotifyEvent;
                handles[0] = spotifyEvent = new AutoResetEvent(false);
                handles[1] = consoleReader.InputReady;

                Console.WriteLine("Using libspotify {0}", Spotify.BuildId());

                if (username == null)
                {
                    Console.Write("Username (press enter to login with stored credentials): ");
                    username = (Console.ReadLine() ?? "").TrimEnd();
                    if (username == "") username = null;
                }

                if (username != null && blob == null)
                {
                    Console.WriteLine("Password: ");
                    // No easy cross-platform way to turn off console echo.
                    // Password will be visible!
                    password = (Console.ReadLine() ?? "").TrimEnd();
                }

                using (SpShell shell = new SpShell(spotifyEvent, username, password, blob, selftest, consoleReader, appkey))
                {
                    //consoleReader.RequestInput("> ");
                    int next_timeout = 0;
                    while (!shell.IsFinished)
                    {
                        int ev = WaitHandle.WaitAny(handles, next_timeout != 0 ? next_timeout : Timeout.Infinite);
                        switch (ev)
                        {
                            case 0:
                            case WaitHandle.WaitTimeout:
                                do
                                {
                                    shell.ProcessEvents(ref next_timeout);
                                } while (next_timeout == 0);
                                if (selftest)
                                {
                                    // TODO: TestProcess
                                }
                                break;
                            case 1:
                                shell.ProcessConsoleInput(consoleReader.GetInput());
                                break;
                        }
                    }
                    Console.WriteLine("Logged out");
                }
                Console.WriteLine("Exiting...");
            }
        }
    }
}
