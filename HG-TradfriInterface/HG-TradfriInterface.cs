// <copyright file="TradfriInterface.cs" company="Wallis2000">
// This file is part of the Homegenie-BE Ikea Tradfri Interface Project source code.
//
// Author: David Wallis david@wallis2000.co.uk
// Project Homepage: http://github.com/davidwallis3101/TradfriInterface
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MIG.Config;
using NLog;
using Tomidix.CSharpTradFriLibrary;
using Tomidix.CSharpTradFriLibrary.Controllers;
using Tomidix.CSharpTradFriLibrary.Models;

// Your namespace must begin MIG.Interfaces for MIG to be able to load it
namespace MIG.Interfaces.HomeAutomation
{
    /// <summary>
    /// TradfriInterface class
    /// </summary>
    public partial class TradfriInterface : MigInterface
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly Logger Log = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// List containing interface Modules
        /// </summary>
        private readonly List<InterfaceModule> modules;

        private List<TradFriDevice> devices;

        private TradFriCoapConnector gatewayConnection;

        private GatewayController gatewayController;

        /// <summary>
        /// Initializes a new instance of the <see cref="TradfriInterface"/> class.
        /// </summary>
        public TradfriInterface()
        {
            // this.modules = new List<InterfaceModule>();

            // manually add some fake modules
            // var module_1 = new InterfaceModule
            // {
            //     Domain = this.GetDomain(),
            //     Address = "1",
            //     ModuleType = ModuleTypes.Light
            // };

            // var module_2 = new InterfaceModule
            // {
            //     Domain = this.GetDomain(),
            //     Address = "2",
            //     ModuleType = ModuleTypes.Sensor
            // };

            // var module_3 = new InterfaceModule
            // {
            //     Domain = this.GetDomain(),
            //     Address = "3",
            //     ModuleType = ModuleTypes.Sensor
            // };

            // add them to the modules list
            // this.modules.Add(module_1);
            // this.modules.Add(module_2);
            // this.modules.Add(module_3);
        }

        /// <summary>
        /// Event for Interface Modules Changing
        /// </summary>
        public event InterfaceModulesChangedEventHandler InterfaceModulesChanged;

        /// <summary>
        /// Event for Interface Properties Changing
        /// </summary>
        public event InterfacePropertyChangedEventHandler InterfacePropertyChanged;

        /// <summary>
        /// Gets or sets a value indicating whether this interface is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a the interface options
        /// </summary>
        public List<Config.Option> Options { get; set; }

        /// <summary>
        /// Gets a value indicating whether the interface/controller device is connected or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if it is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Called when disposing the Mig Interface
        /// </summary>
        public void Dispose()
        {
            this.Disconnect();
        }

        /// <summary>
        /// Called when setting an interface option?
        /// </summary>
        /// <param name="option">The interface option</param>
        public void OnSetOption(Config.Option option)
        {
            if (this.IsEnabled)
            {
                this.Connect();
            }
        }

        /// <summary>
        /// Gets the interface modules
        /// </summary>      
        public List<InterfaceModule> GetModules()
        {
            var modules = new List<InterfaceModule>();

            // Get Devices from the controller
            devices = LoadAllDevices(gatewayController, gatewayConnection);

            foreach (var device in devices)
            {
                var module = new InterfaceModule
                {
                    Domain = this.GetDomain(),
                    Address = device.ID.ToString()
                };
                
                if (device.Info.DeviceType.IndexOf(" bulb ", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    Log.Debug($"Adding Module {device.Info.DeviceType}");
                    module.Description = "Light";
                    module.ModuleType = ModuleTypes.Dimmer;
                }
                else if (device.Info.DeviceType.IndexOf(" remote ", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    Log.Debug($"Adding Module {device.Info.DeviceType}");
                    module.Description = "Binary Switch";
                    module.ModuleType = ModuleTypes.Switch;
                }
                else
                {
                    Log.Debug($"Unknown Module Type: {device.Info.DeviceType}");
                    module.Description = "Unknown Type";
                    module.ModuleType = ModuleTypes.Generic;
                }

                modules.Add(module);
            }
            return modules;
        }

        /// <summary>
        /// Returns true if the device has been found in the system
        /// </summary>
        /// <returns>Boolean value indicating if the device is present</returns>
        public bool IsDevicePresent()
        {
            return true;
        }

        /// <summary>
        /// Sample Connect Method
        /// </summary>
        /// <returns>boolean value indicating if the connection was successful</returns>
        public bool Connect()
        {
            if (!this.IsConnected)
            {
                // Log that the interface is loading and display the version being loaded
                Log.Info("Starting Ikea Tradfri Interface, Version {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());

                this.devices = new List<TradFriDevice>();

                if (string.IsNullOrEmpty(this.GetOption("GatewayName").Value)) { throw new Exception("Gateway name not configured"); }
                if (string.IsNullOrEmpty(this.GetOption("GatewayAddress").Value)) { throw new Exception("Gateway address not configured"); }
                if (string.IsNullOrEmpty(this.GetOption("GatewaySecret").Value)) { throw new Exception("Gateway secret not configured"); }

                gatewayConnection = new TradFriCoapConnector(
                    this.GetOption("GatewayName").Value,
                    this.GetOption("GatewayAddress").Value,
                    this.GetOption("GatewaySecret").Value);

                gatewayConnection.Connect();
                gatewayController = new GatewayController(gatewayConnection.Client);

                Log.Info("Connected");

                this.IsConnected = true;

                // Console.WriteLine("Loading devices");
                // LoadAllDevices();
                // Console.WriteLine("Loaded devices");
            }

            this.OnInterfaceModulesChanged(this.GetDomain());
            return true;
        }

        private static List<TradFriDevice> LoadAllDevices(GatewayController gwControler, TradFriCoapConnector gwConn)
        {
            return gwControler.GetDevices().Select(deviceID => new DeviceController(deviceID, gwConn.Client)).Select(dc => dc.GetTradFriDevice()).ToList();
        }

        /// <summary>
        /// Sample Disconnect Method
        /// </summary>
        public void Disconnect()
        {
            if (this.IsConnected)
            {
                // TODO: Perform a disconnection from your 'hardware' here
                this.IsConnected = false;
            }
        }

        /// <summary>
        /// Handles the control of the interface from the Mig / HG web interface
        /// </summary>
        /// <param name="request">request body</param>
        /// <returns>object</returns>
        /// <exception cref="ArgumentOutOfRangeException">Argument out of range</exception>
        public object InterfaceControl(MigInterfaceCommand request)
        {
            var response = new ResponseText("OK"); // default success value

            Commands command;
            Enum.TryParse<Commands>(request.Command.Replace(".", "_"), out command);

            var module = this.modules.Find(m => m.Address.Equals(request.Address));

            if (module != null)
            {
                switch (command)
                {
                    case Commands.Control_On:
                        this.OnInterfacePropertyChanged(this.GetDomain(), request.Address, "Test Interface", "Status.Level", 1);
                        break;
                    case Commands.Control_Off:
                        this.OnInterfacePropertyChanged(this.GetDomain(), request.Address, "Test Interface", "Status.Level", 0);
                        break;
                    case Commands.Temperature_Get:
                        this.OnInterfacePropertyChanged(this.GetDomain(), request.Address, "Test Interface", "Sensor.Temperature", 19.75);
                        break;
                    case Commands.Greet_Hello:
                        this.OnInterfacePropertyChanged(this.GetDomain(), request.Address, "Test Interface", "Sensor.Message", string.Format("Hello {0}", request.GetOption(0)));
                        response = new ResponseText("Hello World!");
                        break;
                    case Commands.NotSet:
                        break;
                    default:
                        Log.Error(new ArgumentOutOfRangeException(), "Command [{0}] not recognised", command);
                        break;
                }
            }
            else
            {
                response = new ResponseText("ERROR: invalid module address");
            }

            return response;
        }

        /// <summary>
        /// Called when the interface mdoules have changed
        /// </summary>
        /// <param name="domain">The domain</param>
        protected virtual void OnInterfaceModulesChanged(string domain)
        {
            if (this.InterfaceModulesChanged != null)
            {
                var args = new InterfaceModulesChangedEventArgs(domain);
                this.InterfaceModulesChanged(this, args);
            }
        }

        /// <summary>
        /// Call this when an interface property has changed to notify MIG
        /// </summary>
        /// <param name="domain">Domain</param>
        /// <param name="source">Source</param>
        /// <param name="description">Description</param>
        /// <param name="propertyPath">Property Path</param>
        /// <param name="propertyValue">Property Value</param>
        protected virtual void OnInterfacePropertyChanged(string domain, string source, string description, string propertyPath, object propertyValue)
        {
            if (this.InterfacePropertyChanged != null)
            {
                var args = new InterfacePropertyChangedEventArgs(domain, source, description, propertyPath, propertyValue);
                this.InterfacePropertyChanged(this, args);
            }
        }
    }
}
