// <copyright file="HG-TradfriInterface.cs" company="Wallis2000">
// This file is part of the Homegenie-BE Ikea Tradfri Interface Project source code.
//
// Author: David Wallis david@wallis2000.co.uk
// Project Homepage: http://github.com/davidwallis3101/TradfriInterface
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using MIG.Config;
using NLog;
using Tomidix.CSharpTradFriLibrary;
using Tomidix.CSharpTradFriLibrary.Controllers;
using Tomidix.CSharpTradFriLibrary.Models;
using System.Text.RegularExpressions;

// namespaces must begin MIG.Interfaces for MIG to be able to load it
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

        private List<TradFriDevice> devices;

        private TradFriCoapConnector gatewayConnection;

        private GatewayController gatewayController;

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
        /// Gets the Interface Modules
        /// </summary>
        /// <returns>List containing the interfaces modules</returns>
        public List<InterfaceModule> GetModules()
        {
            var modules = new List<InterfaceModule>();

            // Get Devices from the controller
            this.devices = this.LoadAllDevices();

            foreach (var device in this.devices)
            {
                var module = new InterfaceModule
                {
                    Domain = this.GetDomain(),
                    Address = device.ID.ToString()
                };

                switch (device.ApplicationType)
                {
                    case DeviceType.Light:
                        Log.Debug($"Adding Module {device.ApplicationType}");
                        module.Description = "Light";
                        module.ModuleType = ModuleTypes.Dimmer;
                        break;

                    case DeviceType.Remote:
                        Log.Debug($"Adding Module {device.ApplicationType}");
                        module.Description = "Binary Switch";
                        module.ModuleType = ModuleTypes.Switch;
                        break;

                    case DeviceType.Unknown_1:
                        Log.Debug($"Adding Module {device.ApplicationType}");
                        break;

                    case DeviceType.Unknown_2:
                        Log.Debug($"Adding Module {device.ApplicationType}");
                        break;

                    default:
                        Log.Debug($"Adding Module {device.ApplicationType}");
                        module.Description = "Unknown Type";
                        module.ModuleType = ModuleTypes.Generic;
                        break;
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

                if (string.IsNullOrEmpty(this.GetOption("GatewayName").Value))
                {
                    throw new Exception("Gateway name not configured");
                }

                if (string.IsNullOrEmpty(this.GetOption("GatewayAddress").Value))
                {
                    throw new Exception("Gateway address not configured");
                }

                if (string.IsNullOrEmpty(this.GetOption("GatewaySecret").Value))
                {
                    throw new Exception("Gateway secret not configured");
                }

                this.gatewayConnection = new TradFriCoapConnector(
                    this.GetOption("GatewayName").Value,
                    this.GetOption("GatewayAddress").Value,
                    this.GetOption("GatewaySecret").Value);

                this.gatewayConnection.Connect();
                this.gatewayController = new GatewayController(this.gatewayConnection.Client);

                Log.Info("Connected");

                this.IsConnected = true;

                // Console.WriteLine("Loading devices");
                // LoadAllDevices();
                // Console.WriteLine("Loaded devices");
            }

            this.OnInterfaceModulesChanged(this.GetDomain());
            return true;
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
            var returnValue = new ResponseText("OK");

            var raiseEvent = false;

            // TODO use / update this param
            var eventParameter = "Status.Level";

            var eventValue = string.Empty;

            Enum.TryParse(request.Command.Replace(".", "_"), out Commands command);
            var dc = new DeviceController(Convert.ToInt64(request.Address), this.gatewayConnection.Client);

            Log.Debug("Command: {0}", command);

            switch (command)
            {
                case Commands.Control_On:
                    raiseEvent = true;
                    dc.TurnOn();
                    break;

                case Commands.Control_Off:
                    raiseEvent = true;
                    dc.TurnOff();
                    break;

                case Commands.Control_Toggle:
                    throw new NotImplementedException();
                    //raiseEvent = true;
                    //break;

                case Commands.Control_Colour:
                    raiseEvent = true;
                    var colour = request.GetOption(0);

                    // todo parse enum
                    Log.Debug("Setting Colour to {0}", colour);
                    dc.SetColor(colour);
                    break;

                case Commands.Controller_Reboot:
                    this.gatewayController.Reboot();
                    Log.Info("Rebooting controller");
                    break;

                case Commands.Battery_Get:
                    var dev = dc.GetTradFriDevice();
                    returnValue = new ResponseText(dev.Info.Battery.ToString());
                    break;

                default:
                    Log.Error(new ArgumentOutOfRangeException(), "Command [{0}] not recognised", command);
                    break;
            }

            if (raiseEvent)
            {
                this.OnInterfacePropertyChanged(this.GetDomain(), request.Address, "Tradfri Node", eventParameter, eventValue);
            }

            return returnValue;
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

        /// <summary>
        /// Loads the devices from the gateway
        /// </summary>
        /// <returns>List of cref="TradFriDevice" </returns>
        private List<TradFriDevice> LoadAllDevices()
        {
            return this.gatewayController.GetDevices().Select(deviceID => new DeviceController(deviceID, this.gatewayConnection.Client)).Select(dc => dc.GetTradFriDevice()).ToList();
        }
    }
}
