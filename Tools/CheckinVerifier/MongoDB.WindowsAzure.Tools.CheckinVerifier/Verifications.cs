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
    using System.Xml.Linq;
    using System.IO;

    public class Verifier
    {
        //===========================================
        //
        // PATH CONFIGURATION
        //
        //===========================================

        static class BaseProject
        {
            public const string DeployFolder = "src\\MongoDB.WindowsAzure.Deploy";
            public const string SetupFolder = "Setup";
        }

        static class SampleProject
        {
            public const string DeployFolder = "src\\SampleApplications\\MvcMovieSample\\MongoDB.WindowsAzure.Sample.Deploy";
            public const string SetupFolder = "Setup";
        }

        static int numTestsPassed = 0;
        static int numTestsFailed = 0;

        //===========================================
        //
        // METHODS
        //
        //===========================================

        /// <summary>
        /// Verifies the different files and returns if all were sucessful.
        /// </summary>
        public static bool RunVerifications()
        {

            // Ensure running in the right directory.
            if (!Directory.Exists(BaseProject.DeployFolder) || !Directory.Exists(SampleProject.DeployFolder))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: This tool must be run in the top level of the mongo-azure directory.");
                return false;
            }

            //
            // Step 1: Verify ServiceDefinition between Source/AzureDeploy and Sample/SampleAzureDeploy.
            //
            {
                var file = "ServiceDefinition.csdef";
                var baseServiceDefinition = Path.Combine(BaseProject.DeployFolder, file);
                var sampleServiceDefinition = Path.Combine(SampleProject.DeployFolder, file);
                PrintResult(file, VerifyElementMatches(baseServiceDefinition, sampleServiceDefinition, "MongoDB.WindowsAzure.MongoDBRole"));
            }

            //
            // Step 2: Verify the local ServiceConfiguration between Source/AzureDeploy and Sample/SampleAzureDeploy.
            //
            {
                var file = "ServiceConfiguration.Local.cscfg";
                var baseDoc = Path.Combine(BaseProject.DeployFolder, file);
                var sampleDoc = Path.Combine(SampleProject.DeployFolder, file);
                PrintResult(file, VerifyElementMatches(baseDoc, sampleDoc, "MongoDB.WindowsAzure.MongoDBRole"));
            }

            //
            // Step 3: Verify the cloud ServiceConfiguration between the two setup dirs.
            //
            {
                var basePath = Path.Combine(BaseProject.SetupFolder, "ServiceConfiguration.Cloud.cscfg.core");
                var samplePath = Path.Combine(SampleProject.SetupFolder, "ServiceConfiguration.Cloud.cscfg.sample");
                PrintResult("ServiceConfiguration.Cloud.cscfg", VerifyElementMatches(basePath, samplePath, "MongoDB.WindowsAzure.MongoDBRole"));
            }

            // Print results.
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine("\nResults:");
            Console.WriteLine(String.Format("\t{0, 2} passed", numTestsPassed));
            Console.WriteLine(String.Format("\t{0, 2} failed\n", numTestsFailed));

            return (numTestsFailed == 0);
        }

        /// <summary>
        /// Verifies that both the document in basePath and samplePath have an element with a "name" attribute of elementName, and that these two elements are identical.
        /// </summary>
        static bool VerifyElementMatches(string basePath, string samplePath, string elementName)
        {
            if (!CheckForFileExistance(basePath) || !CheckForFileExistance(samplePath))
                return false;

            XElement baseDoc = XElement.Load(basePath);
            XElement sampleDoc = XElement.Load(samplePath);

            try
            {
                var baseElement = baseDoc.Elements().First(element => element.Attribute("name").Value == elementName);
                var sampleElement = sampleDoc.Elements().First(element => element.Attribute("name").Value == elementName);

                return (baseElement.ToString().Equals(sampleElement.ToString()));
            }
            catch (InvalidOperationException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: Element \"" + elementName + "\" does not exist for this test:");
                return false;
            }
        }

        /// <summary>
        /// Returns whether a file at the given path exists; writes an error before returning if it does not.
        /// </summary>
        static bool CheckForFileExistance(string path)
        {
            if (File.Exists(path))
                return true;
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: File \"" + path + "\" does not exist for this test:");
                return false;
            }
        }

        /// <summary>
        /// Prints the result of the given test nicely to the console.
        /// </summary>
        static void PrintResult(string test, bool result)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(String.Format("Verifying {0, -60}", test + ".."));

            // Print the result.            
            if (result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[ OK ]");
                numTestsPassed++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ FAIL ]");
                numTestsFailed++;
            }

            Console.WriteLine();
        }
    }
}
