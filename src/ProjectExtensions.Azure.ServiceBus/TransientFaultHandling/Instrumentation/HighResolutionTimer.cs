//=======================================================================================
// Transient Fault Handling Framework for SQL Azure, Storage, Service Bus & Cache
//
// This sample is supplemental to the technical guidance published on the Windows Azure
// Customer Advisory Team blog at http://windowsazurecat.com/. 
//
//=======================================================================================
// Copyright © 2011 Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=======================================================================================
namespace Microsoft.AzureCAT.Samples.TransientFaultHandling.Instrumentation
{
    #region Using statements
    using System;
    using System.Runtime.InteropServices;
    #endregion

    /// <summary>
    /// Provides the implementation of a high-resolution timer.
    /// </summary>
    public sealed class HighResolutionTimer
    {
        #region Private members
        private readonly static HighResolutionTimer singleton = new HighResolutionTimer();

        private static readonly bool isHighResolution;
        private static readonly long frequency = 0;
        private static readonly double tickFrequency;
        
        private const long TicksPerMillisecond = 0x2710L;
        private const string KernelLib = "Kernel32.dll";

        [DllImport(KernelLib)]
        private static extern int QueryPerformanceCounter(ref long count);
        [DllImport(KernelLib)]
        private static extern int QueryPerformanceFrequency(ref long frequency);
        #endregion

        #region Constructor
        static HighResolutionTimer()
        {
            // Query the high-resolution timer only if it is supported.
            // A returned frequency of 1000 typically indicates that it is not
            // supported and is emulated by the OS using the same value that is
            // returned by Environment.TickCount.
            // A return value of 0 indicates that the performance counter is
            // not supported.
            int returnVal = QueryPerformanceFrequency(ref frequency);

            if (returnVal != 0 && frequency != 1000)
            {
                // The performance counter is supported.
                isHighResolution = true;
                tickFrequency = 10000000.0;
                tickFrequency /= (double)frequency;
            }
            else
            {
                // The performance counter is not supported. Use Environment.TickCount instead.
                frequency = 10000000;
                tickFrequency = 1.0;
                isHighResolution = false;
            }
        } 
        #endregion

        #region Public methods
        /// <summary>
        /// Returns the frequency of the high-resolution timer, if one exists.
        /// </summary>
        public long Frequency
        {
            get { return frequency; }
        }

        /// <summary>
        /// Returns the current value of the high-resolution timer. 
        /// </summary>
        public long TickCount
        {
            get
            {
                Int64 tickCount = 0;

                if (isHighResolution)
                {
                    // Get the value here if the counter is supported.
                    QueryPerformanceCounter(ref tickCount);
                    return tickCount;
                }
                else
                {
                    // Otherwise, use Environment.TickCount.
                    return (long)Environment.TickCount;
                }
            }
        }

        /// <summary>
        /// Returns a singleton instance of the <see cref="HighResolutionTimer"/> object.
        /// </summary>
        public static HighResolutionTimer Current
        {
            get { return singleton; }
        }

        /// <summary>
        /// Returns the current value of the high-resolution timer. 
        /// </summary>
        public static long CurrentTickCount
        {
            get { return singleton.TickCount; }
        }

        /// <summary>
        /// Gets the total elapsed time (in milliseconds) measured by taking into account the specified start timer value. 
        /// </summary>
        /// <param name="startTicks">The start timer value.</param>
        /// <returns>The total elapsed time in milliseconds.</returns>
        public long GetElapsedMilliseconds(long startTicks)
        {
            return GetElapsedDateTimeTicks(startTicks) / TicksPerMillisecond;
        } 
        #endregion

        #region Private methods
        /// <summary>
        /// Gets the number of ticks that represent the date and time measured by taking into account the specified start timer value.
        /// </summary>
        /// <param name="startTicks">The start timer value.</param>
        /// <returns>The calculated the number of ticks.</returns>
        private long GetElapsedDateTimeTicks(long startTicks)
        {
            long rawElapsedTicks = TickCount - startTicks;

            if (isHighResolution)
            {
                double dateTimeTicks = rawElapsedTicks;
                dateTimeTicks *= tickFrequency;

                return (long)dateTimeTicks;
            }

            return rawElapsedTicks;
        } 
        #endregion
    }
}
