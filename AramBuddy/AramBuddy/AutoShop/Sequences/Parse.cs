#region

using AramBuddy.MainCore.Utility.MiscUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace AramBuddy.AutoShop.Sequences
{
    /// <summary>
    ///     A class containing all the methods needed for parsing JSON
    /// </summary>
    internal static class Parse
    {
        /// <summary>
        ///     Tries to parse data into a build.
        /// </summary>
        /// <param name="data">The build JSON</param>
        /// <param name="build">
        ///     The Build class object that will be outputted
        ///     validly if the parse is successful, or null if it is not.
        /// </param>
        /// <returns>True on success, false on failiure</returns>
        public static bool TryParseData(this string data, out Build build)
        {
            try
            {
                // First stage in parsing - getting a dynamic object
                // that we can use to get subobjects
                dynamic parsed = JObject.Parse(data);

                // Get the build array from from the dynamic
                string[] arr = parsed.data.ToObject<string[]>();

                // Create a new build object
                build = new Build { BuildData = arr };
                return true;
            }
            catch (JsonSerializationException ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred in AutoShop on JSON parse:", ex, Logger.LogLevel.Error);

                // Warn the user that AutoShop may not be functioning correctly
                Logger.Send("Exception occurred during AutoShop JSON parse. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
                build = null;
                return false;
            }
        }
    }
}
