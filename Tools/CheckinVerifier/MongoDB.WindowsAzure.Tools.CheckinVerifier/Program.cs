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

namespace MongoDB.WindowsAzure.Tools.CheckinVerifier
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.IO;
    using System.Xml.Linq;

    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {

            // Back up the original console color.
            var startingForegroundColor = Console.ForegroundColor;
            var startingBackgroundColor = Console.BackgroundColor;

            bool success = false;

            try
            {
                Console.BackgroundColor = ConsoleColor.White;
                Verifier.RunVerifications();
                success = true;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Verification failed with {0}", e.Message);
                success = false;
            }
            finally
            {
                Console.ForegroundColor = startingForegroundColor;
                Console.BackgroundColor = startingBackgroundColor;
            }

            if (!success)
            {
                Environment.Exit(-1);
            }
        }


    }
}
