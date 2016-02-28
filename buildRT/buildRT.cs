﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;

namespace buildRT
{
    class buildRT
    {
        const int scoreFrames = 1;
        const int eventsThisRun = 128;
        const int bytesPerEvent = 9;
        const double samplesSecond = 44100.0;
        const int maxSamples = 44100 * 180; // 180 seconds max

        public static class GlobalVar
        {
            public static int myUniqueID = 0;
            public static string jobName = "test";
            public static int endTime = 0;
            public static int featureCount = (bytesPerEvent * eventsThisRun);
            public static int[] features = new int[featureCount];
            public static long[] targetWav = new long[maxSamples];
            public static int samples = 0;

            public static double[] runningWave = new double[maxSamples];
            public static long[] calcWav = new long[maxSamples];
            public static long[] diffWav = new long[maxSamples];
            // need start, dur, amp, freq, pan

            public static int[] CMIXstart = new int[eventsThisRun];
            public static int[] CMIXdur = new int[eventsThisRun];
            public static int[] CMIXamp = new int[eventsThisRun];
            public static int[] CMIXfreq = new int[eventsThisRun];
            public static int[] CMIXenv = new int[eventsThisRun];
            public static int[] CMIXplay = new int[eventsThisRun];

            public static long[] frameScore = new long[scoreFrames];
            public static bool wavErr = false;

            public static int myGeneration = 0;
            public static long myScore = 0;
            public static long bestScore = 0;
            public static Random random = new Random(Guid.NewGuid().GetHashCode());
            //            public static Random random = new Random();
            public static int soundPos = 0;
            public static int popMember = 0;
            public static int activeFeatures = 0;
            public static double lowFreq = 8000;
            public static double highFreq = 0;
            public static int lowAmp = 40000;
            public static int highAmp = 0;
            public static int lowFreqW = 8000;
            public static int highFreqW = 0;
            public static int lowAmpW = 40000;
            public static int highAmpW = 0;

            public static int highCtr = 0;
            public static double highAmpNote = 0;
            public static int scoreAll = 0;
            public static int scoreCount = 0;
            public static long worstScore = 0;
            public static int worstNDX = 0;
            public static long worstFrame = 0;
            public static long totalDiff = 0;
            public static long[] potentialDiff = new long[scoreFrames];
            public static int lastLength = 0;
            public static int mostSamples = 0;
            public static bool allFrames = false;
            public static int startFrame = 0;
            public static double[] freqLookup = new double[256 * 256];
            public static int scoreLines = 0;
            public static int es1 = 0;
            public static int es2 = 0;
            public static int es3 = 0;
            public static int es4 = 0;

            public static Stopwatch stopWatch;
            public static TimeSpan ts;
            public static int mutCtr = 0;
        }



        static void Main(string[] args)
        {
            GlobalVar.stopWatch = new Stopwatch();
            GlobalVar.stopWatch.Start();

            double freqInterval = 1.00010092926025;

            GlobalVar.freqLookup[0] = 0.0;
            GlobalVar.freqLookup[1] = 27.5;
            for (int i = 2; i < (6869); i++)
            {
                GlobalVar.freqLookup[i] = GlobalVar.freqLookup[i - 1] * freqInterval;
            }
            for (int i = 6869; i < (65536); i++)
            {
                GlobalVar.freqLookup[i] = (2 * GlobalVar.freqLookup[i - 6868]);
            }


            GlobalVar.targetWav = openWav("/N/dc2/scratch/smccaula/moon/target.wav");


            Random random = new Random();

            if (args.Length > 0)
            {
                GlobalVar.popMember = Convert.ToInt32(args[0]);
            }
            if (args.Length > 1)
            {
                GlobalVar.myUniqueID = Convert.ToInt32(args[1]);
            }
            if (args.Length > 2)
            {
                GlobalVar.mutCtr = Convert.ToInt32(args[2]);
            }
            if (args.Length > 3)
            {
                GlobalVar.jobName = args[3];
            }
            if (args.Length > 4)
            {
                GlobalVar.worstFrame = Convert.ToInt32(args[4]);
            }


            if (PreProcess())
            {
                OutputAllFiles();
            }

        }

        static void OutputAllFiles()
        {
            double displayPct = 0.00;
//            double framePct = 0.00;
            GlobalVar.myScore = ((2 * GlobalVar.totalDiff) - AlternateScore(0, GlobalVar.samples));

            if (!WriteScoreFile())
                return;
            if (!FeatureScoreFile())
                return;

            GlobalVar.ts = GlobalVar.stopWatch.Elapsed;

            GlobalVar.es3 = GlobalVar.ts.Seconds + (60 * GlobalVar.ts.Minutes)
                + (60 * 60 * GlobalVar.ts.Hours) + (60 * 60 * 24 * GlobalVar.ts.Days);

//            char[] buildChars;
//            buildChars = new char[350000];

//            int nonZero = 0;
//            for (int i = 0; i < GlobalVar.featureCount; i++)
//            {
//                buildChars[i] = (char)GlobalVar.features[i];
//                if (GlobalVar.features[i] > 0) nonZero++;
//            }
            //    Console.WriteLine("cmix  write pop " + GlobalVar.popMember + " " + nonZero);

            String logFile = Convert.ToString(GlobalVar.popMember) + ".log";

            GlobalVar.ts = GlobalVar.stopWatch.Elapsed;
            GlobalVar.es4 = GlobalVar.ts.Seconds + (60 * GlobalVar.ts.Minutes)
                + (60 * 60 * GlobalVar.ts.Hours) + (60 * 60 * 24 * GlobalVar.ts.Days);

            displayPct = (100.00 * GlobalVar.myScore) / (2.0 * GlobalVar.totalDiff);

            string frameDisplay = "";
            for (int fx = 0; fx < scoreFrames; fx++)
            {
                frameDisplay = frameDisplay + fx.ToString() + ":"
                    + ((GlobalVar.frameScore[fx]) / (2 * (GlobalVar.potentialDiff[fx] / 100))).ToString() + ",";
//                + (GlobalVar.potentialDiff[fx] / 100).ToString("##0") + ","
  //              + (GlobalVar.frameScore[fx] / 100).ToString("##0") + ",";
            }

            using (StreamWriter sw = File.AppendText(logFile))
            {
                sw.WriteLine(GlobalVar.popMember + " Pct : " + displayPct.ToString("##0.00") +
                    " Score : " + GlobalVar.myScore.ToString() + " of " + (2 * GlobalVar.totalDiff).ToString()
                    + " Frames : " + frameDisplay
                  //  + " non-Zero : " + nonZero.ToString() 
                    + " Lines : " + GlobalVar.scoreLines.ToString()
                    + " Elapsed : " + GlobalVar.es1.ToString() + "," + GlobalVar.es2.ToString() + "," + GlobalVar.es3.ToString() + "," + GlobalVar.es4.ToString()
                    + " mut : " + GlobalVar.mutCtr.ToString()
                    + " ID : " + GlobalVar.myUniqueID.ToString()
                    + " worst : " + GlobalVar.worstFrame.ToString()
                    );
            }

            // dsm took out 2/22 - does nothing???
//            string bs = new string(buildChars);
//            bs = bs.Substring(0, GlobalVar.featureCount);

 //           string fn = "";
 //           fn = "/N/u/smccaula/Geode/moon/mx" + Convert.ToString(GlobalVar.popMember);
//            File.WriteAllText(fn, bs);
            //            WaitForFile(fn);
        }

        static bool PreProcess()
        {
            // these were set in XML
            //     GlobalVar.myGeneration = 0; not used
            //     GlobalVar.bestScore = 0; not used
            //    GlobalVar.popMember = 0; passed as parameter


            if (!GetExistingCharacteristics(GlobalVar.popMember))
            {
                Console.WriteLine("GetExistingCharacteristics input error");
                return (false);
            }

            for (int i = 0; i < GlobalVar.samples; i++)
            {
                GlobalVar.totalDiff = GlobalVar.totalDiff + Math.Abs(GlobalVar.targetWav[i]);
            }

            for (int fx = 0; fx < scoreFrames; fx++)
            {
                GlobalVar.potentialDiff[fx] = 0;
                int startX = fx * (GlobalVar.samples / scoreFrames);
                int endX = startX + (GlobalVar.samples / scoreFrames);
                for (int sx = startX; sx < endX; sx++)
                {
                    GlobalVar.potentialDiff[fx] = GlobalVar.potentialDiff[fx] + (1 * (Math.Abs(GlobalVar.targetWav[sx])));
                }
            }

            if (!BuildScoreFile())
            {
                Console.WriteLine("BuildScoreFile input error");
                return (false);
            }
            GlobalVar.ts = GlobalVar.stopWatch.Elapsed;

            GlobalVar.es1 = GlobalVar.ts.Seconds + (60 * GlobalVar.ts.Minutes)
                + (60 * 60 * GlobalVar.ts.Hours) + (60 * 60 * 24 * GlobalVar.ts.Days);

            if (!RenderScoreToWav())
            {
                Console.WriteLine("RenderScoreToWav input error");
                return (false);
            }
            GlobalVar.ts = GlobalVar.stopWatch.Elapsed;

            GlobalVar.es2 = GlobalVar.ts.Seconds + (60 * GlobalVar.ts.Minutes)
                + (60 * 60 * GlobalVar.ts.Hours) + (60 * 60 * 24 * GlobalVar.ts.Days);


            return (true);
        }

        static bool BuildScoreFile()
        {
            // GetExistingCharacteristics should already have turned XML into RTCmix events
            // need to sort all events based on time (any event can have any time)
            // New routine to write header and all events into tracks

            string scoreFile = Convert.ToString(GlobalVar.popMember) + ".sco";
            string wavFile = "/N/dc2/scratch/smccaula/moon/" + Convert.ToString(GlobalVar.popMember) + ".wav";
            //            string wavFile = "/N/u/smccaula/Karst/moon/" + Convert.ToString(GlobalVar.popMember) + ".wav";

            if (File.Exists(scoreFile))
            {
                File.Delete(scoreFile);
            }

            StreamWriter scoreText = new StreamWriter(scoreFile);
            scoreText.WriteLine("set_option(\"audio = off\", \"play = off\", \"clobber = on\")");
            scoreText.WriteLine("rtsetparams(44100, 1)");
            scoreText.WriteLine("rtoutput(\"" + wavFile + "\")");
            scoreText.WriteLine("reset(44100)");
            scoreText.WriteLine("load(\"WAVETABLE\")");
            scoreText.WriteLine("waves = maketable(\"wave\", 5000, \"sine\")");
            //            scoreText.WriteLine("env0 = maketable(\"line\", 5000, 0,0, 1,1, 2,0)");
            for (int envX = 0; envX < 16; envX++)
            {
                for (int envY = 0; envY < 16; envY++)
                {
                    int attackBegin = (envX * 16) + 1;
                    int attackEnd = attackBegin + 1;
                    int decayBegin = attackEnd + 1;
                    int decayEnd = decayBegin + (envY * 16) + 1;
                    int envLabel = envX + (16 * envY);
                    scoreText.WriteLine("env" + envLabel.ToString() +
                    " = maketable(\"line\", 5000, 0,0," + attackBegin + ",1," + attackEnd + ",1," + decayBegin + ",1," + decayEnd + ",0)");
                }
            }

            bool MoreEvents = true;
            int eventX = 0;
            double tempStart = 0.0;
            double tempLength = 0.0;
            double tempOffset = 0.0;
            double tempDur = 0.0;
            double tempFreq = 0.0;
            double tempPan = 0.0;
            double tempAmp = 0.0;
            bool playFeature = false;
            string waveName = "waves";
            string envName = "env1";
            double durIncrement = 0.0;
            double secondsInFrame = 0.0;
  //          double playThreshold = 10000;
            double playThreshold = 0;
            double playCalc = 0;

            GlobalVar.scoreLines = 0;
            //loop through events
            while (MoreEvents)
            {
                waveName = "waves";
                envName = "env1";
                playFeature = true; // test
                GlobalVar.CMIXplay[eventX] = 0;

                tempFreq = GlobalVar.freqLookup[GlobalVar.CMIXfreq[eventX]];
                tempAmp = GlobalVar.CMIXamp[eventX];
                tempDur = GlobalVar.CMIXdur[eventX];

                if (tempAmp > ((256 * 128) - 1))
                {
                    tempAmp = tempAmp - (256 * 128);
         //           GlobalVar.CMIXstart[eventX] = GlobalVar.CMIXstart[eventX] + 256;
                    playFeature = false;
                }

                envName = "env" + GlobalVar.CMIXenv[eventX].ToString();

                tempStart = GlobalVar.CMIXstart[eventX]; // 0-255 samples offset
       //         tempStart = tempStart + 24;//dsm change

                //duration 
                // first find how many cycles fit in a frame
                // come up with an interval that includes whole cycles

                tempLength = (Convert.ToDouble((GlobalVar.samples / samplesSecond))); // length in seconds
                secondsInFrame = tempLength / scoreFrames;

                durIncrement = 1 / samplesSecond;
                tempDur = tempDur * durIncrement;

                tempOffset = (eventX * tempLength) / eventsThisRun;
                tempStart = (tempStart / samplesSecond); // start offset in samples
      //          tempStart = tempStart + tempOffset;

                tempPan = 0;

                if ((tempStart + tempDur) > tempLength)
                    tempDur = tempLength - tempStart;
//                    playFeature = false;  //  don't play past the end of the target file

                if (tempDur < 0.0)
                    tempDur = 0.0;

                if (tempStart < 0.0)
                    tempStart = 0.0;
                
                // DSM
//                if (tempFreq < 40.0)
 //                   tempAmp = 0.0;
  //              if (tempFreq > 440.0)
   //                 tempAmp = 0.0;
//                if (tempDur < 0.05)
//                    tempAmp = 0.0;
//                if (tempAmp < 2000.0)
//                    tempAmp = 0.0;

                playCalc = tempDur * tempAmp;
                if (playCalc < playThreshold)
                    playFeature = false;

                if ((tempAmp == 0) || (tempFreq == 0) || (tempDur == 0))
                    playFeature = false;


                if ((playFeature))
                {
                    GlobalVar.CMIXplay[eventX] = 1;
                    GlobalVar.scoreLines++;
                    scoreText.WriteLine("WAVETABLE("
                        + Convert.ToString(tempStart) + ","
                        + Convert.ToString(tempDur) + "," + envName + "*"
                        + Convert.ToString(tempAmp) + ","
                        + Convert.ToString(tempFreq) + ","
                        + Convert.ToString(tempPan) + "," + waveName + ")"
                        +  "//comments, " + Convert.ToString(tempAmp * tempDur));
                }

                eventX++;
                if (eventX == eventsThisRun) MoreEvents = false;
            }
            scoreText.Close();

            //            if (!WaitForFile(scoreFile))
            //            {
            //                return (false);
            //            }

            return (true);
        }

        static bool RenderScoreToWav()
        {
            Process scoreProcess = new Process();
            String scoreFile = Convert.ToString(GlobalVar.popMember) + ".sco";
            String logFile = Convert.ToString(GlobalVar.popMember) + ".log";

            string shellFile = Convert.ToString(GlobalVar.popMember) + ".sh";

            if (File.Exists(shellFile))
            {
                File.Delete(shellFile);
            }

            StreamWriter shellText = new StreamWriter(shellFile);
            shellText.WriteLine("./RTcmix/bin/CMIX -D plug:null -Q < " + scoreFile + " > " + logFile);
            //            shellText.WriteLine("./RTcmix/bin/CMIX -Q -D plug:null < " + scoreFile);
            shellText.Close();

            //            if (!WaitForFile(shellFile))
            //            {
            //                return (false);
            //            }

            scoreProcess = new Process();

            scoreProcess.StartInfo.CreateNoWindow = true;
            scoreProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            scoreProcess.StartInfo.UseShellExecute = true;

            scoreProcess.StartInfo.FileName = "/bin/bash";
            scoreProcess.StartInfo.Arguments = shellFile;

            scoreProcess.Start();
            scoreProcess.WaitForExit();
            scoreProcess.Dispose();

            //            if (!WaitForFile("/N/u/smccaula/Karst/moon/" + Convert.ToString(GlobalVar.popMember) + ".wav"))
            //if (!WaitForFile("/N/dc2/scratch/smccaula/moon/" + Convert.ToString(GlobalVar.popMember) + ".wav"))
            //          {
            //            return (false);
            //      }

            //            GlobalVar.calcWav = openWav("/N/u/smccaula/Karst/moon/" + Convert.ToString(GlobalVar.popMember) + ".wav");
            GlobalVar.calcWav = openWav("/N/dc2/scratch/smccaula/moon/" + Convert.ToString(GlobalVar.popMember) + ".wav");

            if (GlobalVar.wavErr)
            {
                return (false);
            }

            return (true);
        }


        static long DeltaScore(int startX, int endX)
        {
            int targetUp = 1;
            int calcUp = 1;

            long runningScore = 0;
            for (int i = startX; i < endX; i++)
            {
                if (GlobalVar.targetWav[i] < GlobalVar.targetWav[i - 1]) targetUp = 0;
                if (GlobalVar.calcWav[i] < GlobalVar.calcWav[i - 1]) calcUp = 0;
                if (targetUp.Equals(calcUp))
                    runningScore = runningScore + (Math.Abs((GlobalVar.targetWav[i] - GlobalVar.targetWav[i - 1]) -
                        (GlobalVar.calcWav[i] - GlobalVar.calcWav[i - 1])));
                if (!targetUp.Equals(calcUp))
                    runningScore = runningScore + (Math.Abs(GlobalVar.targetWav[i] - GlobalVar.targetWav[i - 1]))
                        + Math.Abs(GlobalVar.calcWav[i] - GlobalVar.calcWav[i - 1]);
            }
            return (runningScore);
        }

        static long AlternateScore(int startX, int endX)
        {
            long runningScore = 0;

            for (int i = startX; i < endX; i++)
            {
                runningScore = runningScore + (Math.Abs(GlobalVar.targetWav[i] - GlobalVar.calcWav[i]));
            }

            return (runningScore);
        }

        static long ScoreFeature(long startX, long endX)
        {
            long runningScore = 0;
            long sampleVariance = 0;

//            if (endX > GlobalVar.samples)
//                endX = GlobalVar.samples;

            for (long i = startX; i < endX; i++)
            {
                sampleVariance = Math.Abs(GlobalVar.targetWav[i] - GlobalVar.calcWav[i]);
                runningScore = runningScore + ((Math.Abs(GlobalVar.targetWav[i]) - sampleVariance));
//                runningScore = runningScore + (Math.Abs(GlobalVar.calcWav[i])/2);
            }

            return (runningScore);
        }

        static bool WriteScoreFile()
        {
            string fn = "/N/u/smccaula/Geode/moon/sx" + Convert.ToString(GlobalVar.popMember);
            BinaryWriter scoreFile = new BinaryWriter(File.Open(fn, FileMode.Create));

            for (int fx = 0; fx < scoreFrames; fx++)
            {
                int startX = fx * (GlobalVar.samples / scoreFrames);
                int endX = startX + (GlobalVar.samples / scoreFrames);
                GlobalVar.frameScore[fx] = (2 * GlobalVar.potentialDiff[fx]) - AlternateScore(startX, endX);
                scoreFile.Write(Convert.ToInt64(GlobalVar.frameScore[fx]));
            }
            scoreFile.Close();
            return (true);
        }

        static bool FeatureScoreFile()
        {

            string fn = "/N/u/smccaula/Geode/moon/fx" + Convert.ToString(GlobalVar.popMember);
            BinaryWriter scoreFile = new BinaryWriter(File.Open(fn, FileMode.Create));
            long tempStart = 0;
            long tempEnd = 0;

            long eventInterval = (GlobalVar.samples / eventsThisRun);
            long tempScore = 0;
            double calcStart = 0.0;

            for (int eventX = 0; eventX < eventsThisRun; eventX++)
            {
                tempScore = -1;
                calcStart = GlobalVar.samples / eventsThisRun;
                calcStart = eventX * calcStart;
                tempStart = Convert.ToInt64(calcStart);
                tempStart = 0;  // dsm no offset this run
                tempStart = tempStart + GlobalVar.CMIXstart[eventX]; // event start in samples
                if (tempStart < 0) tempStart = 0;
                tempEnd = tempStart + GlobalVar.CMIXdur[eventX];
                if (tempEnd > GlobalVar.samples) tempEnd = GlobalVar.samples;

                if (tempEnd.Equals(tempStart))
                    tempEnd = tempStart + 1;

                if (GlobalVar.CMIXplay[eventX] > 0)
                {
                    tempScore = 0;
                    for (long i = tempStart; i < tempEnd; i++)
                    { 
                        tempScore = (2 * Math.Abs(GlobalVar.targetWav[i])); // larger score is better
                        tempScore = tempScore - (Math.Abs(GlobalVar.targetWav[i] - GlobalVar.calcWav[i]));
                    }
//                    tempScore = tempScore / (tempEnd - tempStart); // average over length
                }

                scoreFile.Write(Convert.ToInt64(tempScore));
            }
            scoreFile.Close();
            return (true);
        }


        static bool GetExistingCharacteristics(int popMember)
        {
            char[] buildChars;
            buildChars = new char[350000];
            string featureString = "";
            string fn = "";

            try
            {
                fn = "/N/u/smccaula/Geode/moon/mx" + Convert.ToString(popMember);

                featureString = File.ReadAllText(fn);
                buildChars = featureString.ToCharArray();
                Array.Resize(ref buildChars, GlobalVar.featureCount);

//                int nonZero = 0;
                for (int i = 0; i < GlobalVar.featureCount; i++)
                {
                    GlobalVar.features[i] = buildChars[i];
                    if (GlobalVar.features[i] > 255)
                        GlobalVar.features[i] = 255;
                    if (GlobalVar.features[i] < 0)
                        GlobalVar.features[i] = 0;
  //                  if (GlobalVar.features[i] > 0)
    //                    nonZero++;
                }
                //        Console.WriteLine("cmix  read pop " + GlobalVar.popMember + " " + nonZero);

                AssignToParamaters();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return (false);
            }
            return (true);
        }

        static void AssignToParamaters()
        {
            int CMIXSize = bytesPerEvent;

            for (int i = 0; i < eventsThisRun; i++)
            {
                GlobalVar.CMIXamp[i] = GlobalVar.features[0 + (i * CMIXSize)] + (256 * GlobalVar.features[1 + (i * CMIXSize)]);
                GlobalVar.CMIXfreq[i] = GlobalVar.features[2 + (i * CMIXSize)] + (256 * GlobalVar.features[3 + (i * CMIXSize)]);
                GlobalVar.CMIXenv[i] = GlobalVar.features[4 + (i * CMIXSize)];
                GlobalVar.CMIXdur[i] = GlobalVar.features[5 + (i * CMIXSize)] + (256 * GlobalVar.features[6 + (i * CMIXSize)]);
                GlobalVar.CMIXstart[i] = GlobalVar.features[7 + (i * CMIXSize)] + (256 * GlobalVar.features[8 + (i * CMIXSize)]);
                //                GlobalVar.CMIXstart[i] = GlobalVar.features[6 + (i * CMIXSize)] + (256 * GlobalVar.features[7 + (i * CMIXSize)]);
            }
        }

        static bool WaitForFile(string fullPath)
        {
            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {
                    using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 100))
                    {
                        fs.ReadByte();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (numTries > 10)
                    {
                        return false;
                    }

            //        System.Threading.Thread.Sleep(25);
                }
            }

            return true;
        }

        static long bytesToInteger(byte firstByte, byte secondByte)
        {
            long r = 0;
            // convert two bytes to one short (little endian)
            long s = (Convert.ToInt32(secondByte) * 256) + Convert.ToInt32(firstByte);
            // convert to range from -1 to (just below) 1

            r = s;

            if (s > ((128 * 256) - 1))
            {
                r = s - (256 * 256);
            }

            return r;
        }

        // Returns left and right double arrays. 'right' will be null if sound is mono.
        static long[] openWav(string filename)
        {
            byte[] wav = File.ReadAllBytes(filename);
            long[] wavArray = new long[maxSamples];

            // Determine if mono or stereo
            int channels = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

            // Get past all the other sub chunks to get to the data subchunk:
            int pos = 12;   // First Subchunk ID from 12 to 16

            // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
            while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
            {
                pos += 4;
                int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            //      pos += 8;

            pos += 4;
            int wavSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
            pos += 4;

            GlobalVar.soundPos = pos;

            // Pos is now positioned to start of actual sound data.
            if (GlobalVar.samples < 1)
            {
                GlobalVar.samples = (wavSize / 2);     // more accurate, get actual chunk size
                GlobalVar.endTime = (Convert.ToInt32((GlobalVar.samples / samplesSecond) * 1000)) / 1;
            }
            int genSamples = wavSize / 2;


            wavArray = new long[GlobalVar.samples];

            // Write to double array/s:
            int i = 0;

            // pos++;

            int retSize = Math.Min(genSamples, GlobalVar.samples);
            while (i < (retSize))
            {
                wavArray[i] = bytesToInteger(wav[pos], wav[pos + 1]);
                pos += 2;
                i++;
            }
            return wavArray;
        }



    }
}

