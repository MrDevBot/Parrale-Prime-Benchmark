using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

class Program
{
    static int Main(string[] Args)
    {   
        DateTime StartTime = System.DateTime.UtcNow;

        //Console.ReadKey();

        //List<Int32> PrimeList = new List<Int32>();
        //Lists are not thread-safe, this causes a race condition and can lead to some strange errors, use ConcurrentBags
        ConcurrentBag<Int32> PrimeList = new ConcurrentBag<Int32>();


        //Caveat, SampleSize<int> and TotalCore<int> NEED to be devisible by eachother, otherwise numbers will be skipped.
        Int32 SampleSize = 10000000;
        //Int32 TotalCore = Environment.ProcessorCount + 4; // org: +8 
        Int32 TotalCore = 10;
        Int32 BatchSize = SampleSize / TotalCore; // e.g. 1/12th of the total
        Int32 TargetThreadSize = RoundUp(TotalCore); //round current core count to closest 10
        //Int32 CurrentCore = TotalCore;
        Int32 CurrentCore = TargetThreadSize;
        Int16 CurrentThreads = 0;

        while (CurrentCore > 0)
        {
            Int32 _LocalCore = CurrentCore;
            var thread = new Thread(() =>   
              {
                  CurrentThreads++;
                  Int32 StartSize = BatchSize * (_LocalCore - 1);
                  Int32 StopSize = StartSize + BatchSize;

                  Console.WriteLine("CORE " + _LocalCore + " " + StartSize + " / " + StopSize);

                  int _Current = StartSize;

                  while(_Current < StopSize)
                  {
                      //Console.WriteLine("EVAL " + _Current);
                      if(IsPrime(_Current))
                      {
                          //Console.WriteLine("PRIME " + _Current);
                          PrimeList.Add(_Current);
                      }

                      //Console.WriteLine("EVAL " + _Current);
                      _Current++;
                  }
                  CurrentThreads--;
              });

            //so we can still use the machine, lets set the priority to lowest. (anything above this will affect the machines usability / responsivness)
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();

            CurrentCore--;
        }

        //wait till all threads have exited.
        while (CurrentThreads > 0) { }

        DateTime StopTime = System.DateTime.UtcNow;
        TimeSpan ProcTime = StopTime.Subtract(StartTime);

        Console.WriteLine("Processing Time: " + ProcTime.TotalMilliseconds);
        Console.ReadKey();

        return 0;
    }

    public static int RoundUp(int Value)
    {
        return 10 * ((Value + 9) / 10);
    }

    /*
    public static int LowestCommonMult(int Number1, int Number2)
    {
        //return Number * Math.Ceiling(Math.Pow(10, (Number - 1)) / Number);
        int num1, num2;
        if (Number1 > Number2)
        {
            num1 = Number1; num2 = Number2;
        }
        else
        {
            num1 = Number2; num2 = Number1;
        }

        for (int i = 1; i < num2; i++)
        {
            int mult = num1 * i;
            if (mult % num2 == 0)
            {
                return mult;
            }
        }
        return num1 * num2;
    }
    */
    public static bool IsPrime(int Number)
    {
        if (Number % 2 == 0) return false;
        if (Number <= 1) return false;
        if (Number == 2) return true;
        
        var boundary = (int)Math.Floor(Math.Sqrt(Number));

        for (int i = 3; i <= boundary; i += 2)
            if (Number % i == 0)
                return false;

        return true;
    }
}