namespace PluginExasol.API.Utility
{
    public static partial class Utility
    {
        public static string GetSafeName(string unsafeName, char escapeChar = '"')
        {
            unsafeName = unsafeName.Trim(escapeChar);
            return $"{escapeChar}{unsafeName.Replace("'", "''")}{escapeChar}";
        }
    }
}