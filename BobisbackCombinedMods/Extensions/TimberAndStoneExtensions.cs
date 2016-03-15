using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Plugin.Bobisback.CombinedMods.Extensions
{
    public static class TimberAndStoneExtensions
    {
        //public static float Repair(this BuildStructure buildStructure, float workingPace)
        //{
        //    buildStructure.repairProgress += workingPace * buildStructure.craftingDelay * 0.3f;
        //    if (buildStructure.repairProgress < 1f) {
        //        return (float) buildStructure.repairProgress;
        //    }
        //    return 1f;
        //}
        // Only useful before .NET 4

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        public static void CopyTo(this Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}
