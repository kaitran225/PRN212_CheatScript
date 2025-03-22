using System.Text.RegularExpressions;

namespace PerfumeRepository.Utils
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates if each word starts with a capital letter or digits 1-9
        /// </summary>
        public static bool ValidateWordCapitalization(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
                
            var words = text.Split(' ');
            foreach (var word in words)
            {
                if (word.Length > 0 && !char.IsUpper(word[0]) && !(char.IsDigit(word[0]) && word[0] != '0'))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Checks if text contains any of the specified special characters
        /// </summary>
        public static bool ContainsSpecialCharacters(string text, string specialChars = "$%^@")
        {
            if (string.IsNullOrEmpty(text))
                return false;
                
            return Regex.IsMatch(text, $"[{Regex.Escape(specialChars)}]");
        }
        
        /// <summary>
        /// Validates text length is within specified range
        /// </summary>
        public static bool ValidateLength(string text, int minLength, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return false;
                
            return text.Length >= minLength && text.Length <= maxLength;
        }
    }
} 