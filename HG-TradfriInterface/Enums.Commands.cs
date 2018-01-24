// <copyright file="Enums.Commands.cs" company="Wallis2000">
// This file is part of the Homegenie-BE Ikea Tradfri Interface Project source code.
//
// Author: David Wallis david@wallis2000.co.uk
// Project Homepage: http://github.com/davidwallis3101/TradfriInterface
// </copyright>

namespace MIG.Interfaces.HomeAutomation
{
    #pragma warning disable SA1600 // Elements must be documented
    internal enum Commands
    {
        #pragma warning disable SA1602 // Enumeration items must be documented
        NotSet,

        Controller_Reboot,

        Battery_Get,

        //ManufacturerSpecific_Get,
        //NodeInfo_Get,


        Control_On,
        Control_Off,
        Control_Level,
        Control_Toggle,
        Control_Colour

        #pragma warning restore SA1602 // Enumeration items must be documented
    }
}
