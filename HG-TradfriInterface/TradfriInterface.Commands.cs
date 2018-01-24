namespace MIG.Interfaces.HomeAutomation
{
    public partial class TradfriInterface
    {
        /// <summary>
        /// Enum containing the commands that are permitted via the web interface
        /// </summary>
        private enum Commands
        {
            /// <summary>
            /// No Set Command Example
            /// </summary>
            NotSet,

            /// <summary>
            /// Control.On Example
            /// </summary>
            Control_On,

            /// <summary>
            /// Control.Off Example
            /// </summary>
            Control_Off,

            /// <summary>
            /// Temperature.Get Example
            /// </summary>
            Temperature_Get,

            /// <summary>
            /// Greet.Hello
            /// </summary>
            Greet_Hello
        }
    }
}