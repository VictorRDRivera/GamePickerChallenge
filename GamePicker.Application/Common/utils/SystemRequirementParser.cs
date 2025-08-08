using System.Text.RegularExpressions;

namespace GamePicker.Application.Common.utils
{
    public static class SystemRequirementParser
    {
        public static bool TryParseMaxRam(string? memoryString, out int ramInGb)
        {
            ramInGb = 0;
            if (string.IsNullOrWhiteSpace(memoryString))
                return false;

            var match = Regex.Match(memoryString, @"(\d+)\s*GB", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var value))
            {
                ramInGb = value;
                return true;
            }

            return false;
        }
    }
}
