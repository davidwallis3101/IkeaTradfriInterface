// <copyright file="Enums.Commands.cs" company="Wallis2000">
// This file is part of the Homegenie-BE Ikea Tradfri Interface Project source code.
//
// Author: David Wallis david@wallis2000.co.uk
// Project Homepage: http://github.com/davidwallis3101/TradfriInterface
// </copyright>

namespace MIG.Interfaces.HomeAutomation
{
    internal enum Commands
    {
        NotSet,

        SwitchBinary_Get,
        SwitchBinary_Set,

        SwitchMultilevel_Get,
        SwitchMultilevel_Set,

        MultiInstance_Get,
        MultiInstance_Set,
        MultiInstance_GetCount,

        Battery_Get,

        ManufacturerSpecific_Get,
        NodeInfo_Get,

        SensorBinary_Get,
        SensorMultiLevel_Get,

        Control_On,
        Control_Off,
        Control_Level,
        Control_Toggle,
    }
}