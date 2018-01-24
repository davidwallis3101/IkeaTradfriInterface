// <copyright file="Program.cs" company="GenieLabs">
// This file is part of HomeGenie Project source code.
//
// HomeGenie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// HomeGenie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with HomeGenie.  If not, see http://www.gnu.org/licenses.
//
// Author: Generoso Martello gene@homegenie.it
// Project Homepage: http://homegenie.it
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using MIG;
using MIG.Config;

namespace TestProject
{
    /// <summary>
    /// Main class
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Main Program
        /// </summary>
        /// <param name="args">command line arguments</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Mig Interface test APP");

            var migService = new MigService();

            // Load the configuration from systemconfig.xml file
            MigServiceConfiguration configuration;

            // Construct an instance of the XmlSerializer with the type
            // of object that is being deserialized.
            XmlSerializer mySerializer = new XmlSerializer(typeof(MigServiceConfiguration));

            // To read the file, create a FileStream.
            FileStream myFileStream = new FileStream("systemconfig.xml", FileMode.Open);

            // Call the Deserialize method and cast to the object type.
            configuration = (MigServiceConfiguration)mySerializer.Deserialize(myFileStream);

            // Set the configuration and start MIG Service
            migService.Configuration = configuration;
            migService.StartService();

            // Get a reference to the test interface
            var interfaceDomain = "HomeAutomation.TradfriInterface";
            var migInterface = migService.GetInterface(interfaceDomain);

            migInterface.Connect();
            Console.WriteLine("Get Modules");
            var interfacemodules = migInterface.GetModules();

            foreach (MIG.InterfaceModule mod in interfacemodules)
            {
                Console.WriteLine($"Module Address: {mod.Address} Module Type: {mod.ModuleType}");
                if (mod.ModuleType.ToString() != "Dimmer")
                {
                    ResponseText resp;
                    resp = (ResponseText)migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + $"/{mod.Address}/Battery.Get"));
                    Console.WriteLine(resp.ResponseValue);
                }

                // var response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + $"/{mod.Address}/Control.On"));
                // System.Threading.Thread.Sleep(1000);
                // response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + $"/{mod.Address}/Control.Colour/eaf6fb"));
                // System.Threading.Thread.Sleep(2000);
                // response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + $"/{mod.Address}/Control.Colour/ebb63e"));
                // System.Threading.Thread.Sleep(2000);
                // response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + $"/{mod.Address}/Control.Colour/f5faf6"));
                // System.Threading.Thread.Sleep(2000);
                // response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + $"/{mod.Address}/Control.Off"));
            }

            Console.WriteLine("\n[Press Enter to Quit]\n");
            Console.ReadLine();
        }
    }
}
