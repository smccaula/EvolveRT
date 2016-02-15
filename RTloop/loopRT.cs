using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Net;
using System.Net.Http;

namespace loopRT
{
    class loopRT
    {
        const int scoreFrames = 128;
        const long possibleScore = 38592310639;
        public static class GlobalVar
        {
            public static string URL = "http://156.56.32.86:8080/glog/services/";
            public static HttpClient client = new HttpClient();
            public static HttpResponseMessage response = null;

            public static int popCount = 0;
            public static int popIndex = 0;
            public static string jobName = "test";
            public static int myGeneration = 0;
            public static long myScore = 0;
            public static Random random = new Random(Guid.NewGuid().GetHashCode());
//            public static Random random = new Random();
            public static int[,] features = new int[120000, 21];
            public static int featureCount = 0;
            public static int launchGeneration = 0;
            public static int myUniqueID = 0;
            public static long bestScore = -999999999999999;
            public static int MutPer100Members = 100;
            public static int normalMut = 1250;
            public static int alternateMut = 6250;
            public static int xoverType = 0;
            public static int parentDist = 50;
            public static long[,] frameScore = new long[scoreFrames, 21];
            public static int mutCtr = 0;
            public static int loopCtr = 0;
            public static int startLoop = 0;
            public static int[] worstFrameNDX = new int [21];
            public static long[] worstFrameVal = new long[21];

        }

        static void Main(string[] args)
        {
            // initialize local variables

            int myPopNumber = 0;
            int runLoop = 1;

            // get input parameters for job name and iteration count

            if (args.Length > 0) // first arg should be mandatory - job name
            {
                GlobalVar.jobName = args[0];
            }

            if (args.Length > 1)
            {
                runLoop = Convert.ToInt32(args[1]);
                GlobalVar.startLoop = runLoop;
            }

            if (args.Length > 2)
            {
                GlobalVar.myUniqueID = Convert.ToInt32(args[2]);
            }

            if (args.Length > 3)
            {
                GlobalVar.launchGeneration = Convert.ToInt32(args[3]);
            }

//            GlobalVar.client.BaseAddress = new Uri(GlobalVar.URL);
//            GlobalVar.response = GlobalVar.client.GetAsync("ping").Result;  

 //           System.Threading.Thread.Sleep(0); // give the server a break

            GlobalVar.popCount = 10000;
            GlobalVar.popIndex = 0;

            // move feature count to an xml file
            GlobalVar.featureCount = 32678;

            runLoop = 200;
            int loopIncrement = 50;

//            int loopIncrement = GlobalVar.popCount / runLoop;

//            if ((GlobalVar.random.Next(0, 100) < 50))
  //          {
    //            loopIncrement = -1 * loopIncrement;
    //            GlobalVar.launchGeneration = GlobalVar.popCount -
    //                (loopIncrement - GlobalVar.launchGeneration);
    //        }


            int loopRand = 0;

            while (runLoop > 0)
            {

                GlobalVar.myGeneration = 1;
                GlobalVar.myScore = 1;
                GlobalVar.mutCtr = 0;

                    if (GlobalVar.loopCtr > 0)
//                        myPopNumber = myPopNumber + (GlobalVar.popCount / GlobalVar.startLoop);
                        myPopNumber = myPopNumber + (loopIncrement);
                    else
                        myPopNumber = GlobalVar.launchGeneration;

                if (GlobalVar.myGeneration.Equals(0))
                {
                    PopulateEmptyMember(myPopNumber);
                }
                else
                {
                    GetExistingScores(myPopNumber, 0);
                }

                GlobalVar.MutPer100Members = 
                    (GlobalVar.random.Next(GlobalVar.normalMut, GlobalVar.alternateMut));
//                GlobalVar.parentDist = GlobalVar.random.Next(0, GlobalVar.parentDist) + 1;

                if (!GlobalVar.myGeneration.Equals(0))
                {
                    NextGenerationValues(myPopNumber);
                }

                if (!GlobalVar.myGeneration.Equals(0))
                {
                    char[] buildChars;
                    buildChars = new char[350000];

                    for (int i = 0; i < GlobalVar.featureCount; i++)
                    {
                        buildChars[i] = (char)GlobalVar.features[i, 0];
                    }

                    string bs = new string(buildChars);
                    bs = bs.Substring(0, GlobalVar.featureCount);

                    string fn = "";
                    fn = "/N/u/smccaula/Geode/moon/mx" + Convert.ToString(myPopNumber);
                    File.WriteAllText(fn, bs);
                }

                // call the user processing job in the series...

                Process userProcess = new Process();

                userProcess.StartInfo.CreateNoWindow = true;
                userProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                userProcess.StartInfo.FileName = "buildRT.exe";
                userProcess.StartInfo.Arguments = Convert.ToString(myPopNumber) + " " +
                    Convert.ToString(GlobalVar.myUniqueID) + " " + Convert.ToString(GlobalVar.mutCtr)
                    + " " + GlobalVar.jobName + " " + GlobalVar.worstFrameNDX[0];
                userProcess.Start();
                userProcess.WaitForExit();
                userProcess.Dispose();

                runLoop--;
                GlobalVar.loopCtr++;
     //           for (int loopX = 0; loopX < GlobalVar.launchGeneration; loopX++)
     //           {
     //               loopRand = GlobalVar.random.Next(100, 1000);
     //           }
     //           System.Threading.Thread.Sleep(loopRand); // spread the processes out

                //            Console.WriteLine("loop");

            }
    //        for (int loopX = 0; loopX < GlobalVar.launchGeneration; loopX++)
    //        {
    //            loopRand = GlobalVar.random.Next(10000, 30000);
    //        }
    //        System.Threading.Thread.Sleep(loopRand); // spread the processes out

        }

        static int AltMemberToProcess()
        {
            int randomPopNumber = 0;
            bool foundIt = false;

            GlobalVar.myGeneration = 1;
            GlobalVar.myScore = 1;


            string paramStr = "";


            while (!foundIt)
            {
                randomPopNumber = GlobalVar.random.Next(1, 51);
//                paramStr = "getvalue/asb_" + Convert.ToString(randomPopNumber);
                paramStr = "getbusy/" + Convert.ToString(randomPopNumber);
                          GlobalVar.response = GlobalVar.client.GetAsync(paramStr).Result;  
                        if (!GlobalVar.response.IsSuccessStatusCode)
                        {
                            foundIt = true;
                        }
            }
  //          paramStr = "setvalue/asb_" + Convert.ToString(randomPopNumber) + "/*/60";
            paramStr = "setbusy/" + Convert.ToString(randomPopNumber) + "/20";
            GlobalVar.response = GlobalVar.client.GetAsync(paramStr).Result;
            return randomPopNumber;
        }

        static void PopulateEmptyMember(int popMember)
        {
            int newIntValue = 0;
            int featureIntMin = 0;
            int featureIntMax = 0;
            char[] buildChars;
            buildChars = new char[350000];
            string fn = "";

            GlobalVar.myScore = 0;
            GlobalVar.featureCount = 65536;

            featureIntMin = 0;
            featureIntMax = 255;



            for (int i = 0; i < GlobalVar.featureCount; i++)
            {
                newIntValue = GlobalVar.random.Next(featureIntMin, featureIntMax);
                GlobalVar.features[i, 0] = newIntValue;
                if ((GlobalVar.random.Next(0, 800) < 700))
                    GlobalVar.features[i, 0] = 0; // dsm experiment start with mostly zero
            }

            for (int i = 0; i < GlobalVar.featureCount; i++)
            {
                buildChars[i] = (char)GlobalVar.features[i, 0];
            }
            string bs = new string(buildChars);
            bs = bs.Substring(0, GlobalVar.featureCount);

            fn = "/N/u/smccaula/Geode/moon/mx" + Convert.ToString(popMember);
            File.WriteAllText(fn, bs);

        }

        static void GetExistingCharacteristics(int popMember, int parent)
        {
            char[] buildChars;
            buildChars = new char[350000];
            string featureString = "";
            string fn = "";
            int frameSize = (GlobalVar.featureCount / scoreFrames);
            int thisFrame = 0;
            int frameNDX = 0;
            try
            {
                fn = "/N/u/smccaula/Geode/moon/mx" + Convert.ToString(popMember);
                if (File.Exists(fn))
                {
                    featureString = File.ReadAllText(fn);
                    buildChars = featureString.ToCharArray();
                }

                Array.Resize(ref buildChars, GlobalVar.featureCount);
                for (int i = 0; i < GlobalVar.featureCount; i++)
                {
                    GlobalVar.features[i, parent] = buildChars[i];

                    if (GlobalVar.features[i, parent] > 255)
                        GlobalVar.features[i, parent] = 255;
                    if (GlobalVar.features[i, parent] < 0)
                        GlobalVar.features[i, parent] = 0;
                    if (thisFrame.Equals(GlobalVar.worstFrameNDX[parent]))
                    {
                        GlobalVar.features[i, parent] = 0; 
                    }
               //     if ((GlobalVar.random.Next(0, 100) < 50) && (parent.Equals(0)))
               //         GlobalVar.features[i, parent] = 0; // dsm temporary
                    frameNDX++;
                    if (frameNDX > frameSize)
                    {
                        frameNDX = 0;
                        thisFrame++;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void GetExistingScores(int popMember, int parent)
        {
            string fn = "/N/u/smccaula/Geode/moon/sx" + Convert.ToString(popMember);
            try
            {
                BinaryReader scoreFile = new BinaryReader(File.OpenRead(fn));

                for (int fx = 0; fx < scoreFrames; fx++)
                {
                    GlobalVar.frameScore[fx, parent] = scoreFile.ReadInt64();
                }
                scoreFile.Close();
            }
            catch
            {
                for (int fx = 0; fx < scoreFrames; fx++)
                {
                    GlobalVar.frameScore[fx, parent] = GlobalVar.frameScore[fx, 0] - 1;
                }
            }

            for (int fx = 0; fx < (scoreFrames - 1); fx++)
            {
                GlobalVar.frameScore[fx, parent] = GlobalVar.frameScore[fx, parent] + (GlobalVar.frameScore[fx + 1, parent] / 2);
            }
            GlobalVar.worstFrameNDX[parent] = 0;
            GlobalVar.worstFrameVal[parent] = GlobalVar.frameScore[GlobalVar.worstFrameNDX[parent],parent];

            for (int fx = 0; fx < scoreFrames; fx++)
            {
                if (GlobalVar.frameScore[fx,parent] < GlobalVar.worstFrameVal[parent])
                {
                    GlobalVar.worstFrameNDX[parent] = fx;
                    GlobalVar.worstFrameVal[parent] = GlobalVar.frameScore[fx, parent];
                }
            }
            GlobalVar.frameScore[GlobalVar.worstFrameNDX[parent], parent] = 0;

            GetExistingCharacteristics(popMember, parent);
        }

        static void NextGenerationValues(int popMember)
        {
            int[] mutateArray = new int[8] { 1, 2, 4, 8, 16, 32, 64, 128 };
            int mutatePosition = 0;
            int mutateValue = 0;
            bool noMutation = false;
            int parentIndex = 0;
            int nextNeighbor = 0;
            int xP1 = 0;
            int xP2 = 0;
            int parentX = 1;
            int neighborhood = GlobalVar.random.Next(1, (GlobalVar.parentDist + 1));
            nextNeighbor = popMember;
            int sameParent = 0;

            for (int nx = 0; nx < 5; nx++)
            {
                if ((nextNeighbor - neighborhood) < 1)
                    nextNeighbor = GlobalVar.popCount;
                nextNeighbor = nextNeighbor - neighborhood;
                GetExistingScores(nextNeighbor, parentX);
                parentX++;
            }

            nextNeighbor = popMember;
            for (int nx = 0; nx < 5; nx++)
            {
                nextNeighbor = nextNeighbor + neighborhood;
                if (nextNeighbor >= GlobalVar.popCount)
                    nextNeighbor = (nextNeighbor - GlobalVar.popCount) + 1;
                GetExistingScores(nextNeighbor, parentX);
                parentX++;
            }

            long bestScore = 0;
            long P1Score = 0;
            long P2Score = 0;
            int fIndex = 0;
            for (int i = 0; i < scoreFrames; i++)
            {
                // get parents based on feature/frame

                xP1 = 0;
                bestScore = GlobalVar.frameScore[i, xP1];
                //                if (bestScore < 0) bestScore = 0;
                for (int px = 1; px < 11; px++)
                {
                    if ((GlobalVar.frameScore[i, px] >= GlobalVar.frameScore[i, xP1]))
                    {
                        xP1 = px;
                        bestScore = GlobalVar.frameScore[i, xP1];
                        P1Score = bestScore;
                    }
                }

                xP2 = 0;
                bestScore = GlobalVar.frameScore[i, xP2];
                //                if (bestScore < 0) bestScore = 0;
                for (int px = 6; px < 21; px++)
                {
                    if ((GlobalVar.frameScore[i, px] >= GlobalVar.frameScore[i, xP2]))
                    {
                        xP2 = px;
                        bestScore = GlobalVar.frameScore[i, xP2];
                        P2Score = bestScore;
                    }
                }
                if (xP1.Equals(xP2))
                    sameParent++;

                int frameSize = (GlobalVar.featureCount / scoreFrames);

                GlobalVar.xoverType = 0; // 0 is random, 1 is xover
                if ((GlobalVar.random.Next(0, 100) < 50))
              //      if ((GlobalVar.random.Next(0, 100) < 80))
                        GlobalVar.xoverType = 1; // 
                int xoverLoc = GlobalVar.random.Next(0, frameSize);

                int xPsave = xP1;
                if ((GlobalVar.random.Next(0, 100) < 50))
                {
                    xP1 = xP2;
                    xP2 = xPsave;
                }

                for (int fx = 0; fx < frameSize; fx++)
                {
                    if (fIndex < GlobalVar.featureCount)
                    {
                        parentIndex = xP1;
                        if ((GlobalVar.xoverType.Equals(0)) && (GlobalVar.random.Next(0, 100) < 50)) // - random no crossover
                            parentIndex = xP2;
                        if ((!GlobalVar.xoverType.Equals(0)) && (fx > xoverLoc)) // - crossover
                            parentIndex = xP2;

//                        if (P2Score > P1Score) // dsm
//                            parentIndex = xP2;

                        GlobalVar.features[fIndex, 0] = GlobalVar.features[fIndex, parentIndex];
                        fIndex++;
                    }
                }
            }

            for (int i = 0; i < GlobalVar.featureCount; i++)
            {
                if (GlobalVar.random.Next(0, (GlobalVar.featureCount * 100)) < GlobalVar.MutPer100Members)
                {
                    GlobalVar.mutCtr++;
                    mutatePosition = GlobalVar.random.Next(0, 8);
                    mutateValue = mutateArray[mutatePosition];
                    if (noMutation) mutateValue = 0;
                    if (GlobalVar.random.Next(0, 100) < 50)
                        mutateValue = -1 * mutateValue;
                    GlobalVar.features[i, 0] = GlobalVar.features[i, 0] + mutateValue;
                    if (GlobalVar.features[i, 0] < 0)
                        GlobalVar.features[i, 0] = GlobalVar.features[i, 0] - (2 * mutateValue);
                    if (GlobalVar.features[i, 0] > 255)
                        GlobalVar.features[i, 0] = GlobalVar.features[i, 0] - (2 * mutateValue);
                }
            }
        }

    }
}
