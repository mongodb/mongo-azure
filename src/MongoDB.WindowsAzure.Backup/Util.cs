/*
 * Copyright 2010-2013 10gen Inc.
 * file : Util.cs
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace MongoDB.WindowsAzure.Backup
{
    using System;
    using System.Text;

    static class Util
    {
        /// <summary>
        /// Formats the given number of bytes into human-readable format (e.g. "72.75 KB").
        /// <param name="numBytes">The number of bytes to format</param>
        /// </summary>
        public static String FormatFileSize(double numBytes)
        {
            string[] types = { "bytes", "KB", "MB", "GB", "TB", "PB", "XB", "ZB", "YB" };

            int index = 0;
            if (numBytes > 0)
            {
                index = Math.Min(types.Length - 1, (int) (Math.Log(numBytes) / Math.Log(1024)));
            }
            
            return String.Format("{0:0.##}", (double) numBytes / Math.Pow(1024, index)) + " " + types[index];
        }
    }
}
