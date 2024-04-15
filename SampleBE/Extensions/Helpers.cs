using Serilog;

namespace SampleBE.Extensions
{
    public static class EnvironmentHelper
    {
        public static string GetValue(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value))
            {
                Log.Error("Warn: Can't find values of " + key + " at environment.");
                throw new Exception($"Can't find values of {key} at environment.");
            }
            return value;
        }
    }
}
