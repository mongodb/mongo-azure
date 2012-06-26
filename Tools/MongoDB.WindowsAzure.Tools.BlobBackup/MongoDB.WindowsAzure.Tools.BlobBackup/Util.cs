/*
 * Copyright 2010-2012 10gen Inc.
 * file : Program.cs
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

namespace MongoDB.WindowsAzure.Tools.BlobBackup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class Util
    {
        /// <summary>
        /// Formats the given number of bytes into human-readable format (e.g. "72.75 KB").
        /// </summary>
        public static String FormatFileSize( double numBytes )
        {
            string[] types = { "bytes", "KB", "MB", "GB", "TB", "PB", "XB", "ZB", "YB" };

            int index = 0;
            if ( numBytes > 0 )
                index = Math.Min( types.Length - 1, (int) ( Math.Log( numBytes ) / Math.Log( 1024 ) ) );
            return String.Format( "{0:0.##}", (double) numBytes / Math.Pow( 1024, index ) ) + " " + types[index];
        }
    }
}
