﻿using System;
using System.Threading.Tasks;
using UsefulProteomicsDatabases;

namespace FlashLFQ
{
    class Driver
    {
        static void Main(string[] args)
        {
            FlashLfqEngine engine = new FlashLfqEngine();

            if (!engine.ReadPeriodicTable())
                return;

            if (!engine.ParseArgs(args))
                return;

            if (!engine.ReadIdentificationsFromTSV())
                return;

            engine.ConstructBinsFromIdentifications();
            
            
            Parallel.For(0, engine.filePaths.Length, 
                new ParallelOptions { MaxDegreeOfParallelism = engine.maxParallelFiles }, 
                fileNumber =>
                {
                    engine.Quantify(fileNumber);
                    GC.Collect();
                }
            );

            if(engine.mbr)
                engine.RetentionTimeCalibrationAndErrorCheckMatchedFeatures();

            engine.QuantifyProteins();

            if (!engine.WriteResults())
                return;

            if (!engine.silent)
                Console.WriteLine("All done");

            if (engine.pause)
                Console.ReadKey();
        }
    }
}
