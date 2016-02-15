using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Threading;

namespace launchRT
{
    class launchRT
    {

        public static class GlobalVar
        {
            public static int savedGeneration = 0;
            public static long savedScore = 0;
        }


        static void Main(string[] args)
        {
            string jobName = "test";
            int runSeconds = 60;
            int maxConcurrent = 4;
            int startValue = 0;
            bool keepGoing = true;
            bool keepWaiting = false;
            int myUniqueID = 0;
            int elapsedSeconds = 0;
            Random random = new Random(Guid.NewGuid().GetHashCode());
//            Random random = new Random();
            Process[] getProcess = new Process[512];
            int launchCtr = 0;
            int runLoop = 1;

            if (args.Length > 0) // first arg should be mandatory - job name
            {
                jobName = args[0];
            }

            if (args.Length > 1)
            {
                maxConcurrent = Convert.ToInt32(args[1]);
            }

            if (args.Length > 2)
            {
                runSeconds = Convert.ToInt32(args[2]);
            }

            if (args.Length > 3)
            {
                runLoop = Convert.ToInt32(args[3]);
            }

            if (args.Length > 4)
            {
                startValue = Convert.ToInt32(args[4]);
            }

            if (String.IsNullOrEmpty(jobName))
            {
                Console.WriteLine("missing job parameter - cancelling");
                return;
            }

            if (maxConcurrent < 1)
            {
                Console.WriteLine("missing or invalid concurrent jobs parameter - cancelling");
                return;
            }

            if (runSeconds < 1)
            {
                Console.WriteLine("missing or invalid run time parameter - cancelling");
                return;
            }

            myUniqueID = random.Next(1, 32000);

            Console.WriteLine("starting process ID : " + myUniqueID.ToString());

           for (int i = 0; i < maxConcurrent; i++)
                {
                getProcess[i] = new Process();

                getProcess[i].StartInfo.CreateNoWindow = true;
                getProcess[i].StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                getProcess[i].StartInfo.FileName = "loopRT.exe";
                getProcess[i].StartInfo.Arguments = " " + jobName + " " + Convert.ToString(runLoop) + " " +
                    Convert.ToString(myUniqueID) + " " + Convert.ToString(startValue);
                getProcess[i].Start();

                System.Threading.Thread.Sleep(5); // offset them to get unique random seeds
            }
            launchCtr = maxConcurrent;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            TimeSpan ts = stopWatch.Elapsed;

//            Console.WriteLine("Cycles, Seconds, Generation, TopScore, Goal, Pct");

            while (keepGoing)
            {

                ts = stopWatch.Elapsed;
                elapsedSeconds = ts.Seconds + (60 * ts.Minutes) + (60 * 60 * ts.Hours) + (60 * 60 * 24 * ts.Days);

                if (elapsedSeconds >= (runSeconds))
                {
                    Console.WriteLine("...time expired, shutting down...");
                    keepGoing = false;
                }
                for (int i = 0; i < maxConcurrent; i++)
                {
                    if (getProcess[i].HasExited)
                    {
                        getProcess[i].Dispose();

                        if (keepGoing)
                        {
                            getProcess[i] = new Process();

                            getProcess[i].StartInfo.CreateNoWindow = true;
                            getProcess[i].StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                            getProcess[i].StartInfo.FileName = "loopRT.exe";
                            getProcess[i].StartInfo.Arguments = " " + jobName + " " + Convert.ToString(runLoop) + " " +
                                Convert.ToString(myUniqueID) + " " + Convert.ToString(startValue);
                            getProcess[i].Start();
                            launchCtr++;
                        }
                    }
                }
                int sleepRand = random.Next(1000, 5000);
                System.Threading.Thread.Sleep(sleepRand); // spread the processes out

//                System.Threading.Thread.Sleep(5); 

            }

            // wait for launched processes to finish

            keepWaiting = true;

            while (keepWaiting)
            {
                keepWaiting = false;
                for (int i = 0; i < maxConcurrent; i++)
                {
                    try
                    {
                        if (!getProcess[i].HasExited)
                            keepWaiting = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message.ToString());
                    }

                }
            }

            Console.WriteLine("done.");

        }
    }
}
