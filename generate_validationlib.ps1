# Generate complete ValidationLib.ps
$funcs = @"
LINK System

FUNCTION IS_POSITIVE(INT value)[INT] { FLEX result = 0; IF value > 0 { result = 1; }; RETURN result; }
FUNCTION IS_NEGATIVE(INT value)[INT] { FLEX result = 0; IF value < 0 { result = 1; }; RETURN result; }
FUNCTION IS_ZERO(INT value)[INT] { FLEX result = 0; IF value == 0 { result = 1; }; RETURN result; }
FUNCTION IS_NON_NEGATIVE(INT value)[INT] { FLEX result = 0; IF value >= 0 { result = 1; }; RETURN result; }
FUNCTION IS_NON_POSITIVE(INT value)[INT] { FLEX result = 0; IF value <= 0 { result = 1; }; RETURN result; }
FUNCTION IS_DIVISIBLE(INT value, INT divisor)[INT] { FLEX remainder = value % divisor; FLEX result = 0; IF remainder == 0 { result = 1; }; RETURN result; }
FUNCTION IS_MULTIPLE(INT value, INT factor)[INT] { FLEX remainder = value % factor; FLEX result = 0; IF remainder == 0 { result = 1; }; RETURN result; }
FUNCTION IS_POWER_OF_TWO(INT value)[INT] { FLEX result = 0; IF value > 0 { FLEX temp = value; FLEX count = 0; CYCLE temp > 0 { FLEX remainder = temp % 2; IF remainder == 1 { count = count + 1; }; temp = temp / 2; }; IF count == 1 { result = 1; }; }; RETURN result; }
FUNCTION IN_RANGE(INT value, INT min, INT max)[INT] { FLEX result = 0; IF value >= min { IF value <= max { result = 1; }; }; RETURN result; }
FUNCTION VALIDATE_RANGE(INT value, INT min, INT max)[INT] { FLEX result = 0; IF value >= min { IF value <= max { result = 1; }; }; RETURN result; }
FUNCTION VALIDATE_MIN(INT value, INT min)[INT] { FLEX result = 0; IF value >= min { result = 1; }; RETURN result; }
FUNCTION VALIDATE_MAX(INT value, INT max)[INT] { FLEX result = 0; IF value <= max { result = 1; }; RETURN result; }
FUNCTION BETWEEN(INT value, INT min, INT max)[INT] { FLEX result = 0; IF value > min { IF value < max { result = 1; }; }; RETURN result; }
FUNCTION BETWEEN_INCLUSIVE(INT value, INT min, INT max)[INT] { FLEX result = 0; IF value >= min { IF value <= max { result = 1; }; }; RETURN result; }
FUNCTION EQUALS(INT a, INT b)[INT] { FLEX result = 0; IF a == b { result = 1; }; RETURN result; }
FUNCTION NOT_EQUALS(INT a, INT b)[INT] { FLEX result = 0; IF a != b { result = 1; }; RETURN result; }
FUNCTION GREATER_THAN(INT a, INT b)[INT] { FLEX result = 0; IF a > b { result = 1; }; RETURN result; }
FUNCTION LESS_THAN(INT a, INT b)[INT] { FLEX result = 0; IF a < b { result = 1; }; RETURN result; }
FUNCTION VALIDATE_NOT_EMPTY(STRING str)[INT] { FLEX len = str -> Length; FLEX result = 0; IF len > 0 { result = 1; }; RETURN result; }
FUNCTION VALIDATE_LENGTH(STRING str, INT expected)[INT] { FLEX len = str -> Length; FLEX result = 0; IF len == expected { result = 1; }; RETURN result; }
FUNCTION VALIDATE_MIN_LENGTH(STRING str, INT min)[INT] { FLEX len = str -> Length; FLEX result = 0; IF len >= min { result = 1; }; RETURN result; }
FUNCTION VALIDATE_MAX_LENGTH(STRING str, INT max)[INT] { FLEX len = str -> Length; FLEX result = 0; IF len <= max { result = 1; }; RETURN result; }
FUNCTION IS_DIGIT(STRING s)[INT] { FLEX len = s -> Length; IF len == 0 { RETURN 0; }; CYCLE len AS i { FLEX c = s[i]; FLEX is_digit = Char -> IsDigit(c); IF is_digit == 0 { RETURN 0; }; }; RETURN 1; }
FUNCTION IS_ALPHA(STRING s)[INT] { FLEX len = s -> Length; IF len == 0 { RETURN 0; }; CYCLE len AS i { FLEX c = s[i]; FLEX is_letter = Char -> IsLetter(c); IF is_letter == 0 { RETURN 0; }; }; RETURN 1; }
FUNCTION IS_ALPHANUMERIC(STRING s)[INT] { FLEX len = s -> Length; IF len == 0 { RETURN 0; }; CYCLE len AS i { FLEX c = s[i]; FLEX is_letter_or_digit = Char -> IsLetterOrDigit(c); IF is_letter_or_digit == 0 { RETURN 0; }; }; RETURN 1; }
FUNCTION IS_UPPER(STRING s)[INT] { FLEX len = s -> Length; IF len == 0 { RETURN 0; }; CYCLE len AS i { FLEX c = s[i]; FLEX is_upper = Char -> IsUpper(c); IF is_upper == 0 { RETURN 0; }; }; RETURN 1; }
FUNCTION IS_LOWER(STRING s)[INT] { FLEX len = s -> Length; IF len == 0 { RETURN 0; }; CYCLE len AS i { FLEX c = s[i]; FLEX is_lower = Char -> IsLower(c); IF is_lower == 0 { RETURN 0; }; }; RETURN 1; }
FUNCTION IS_WHITESPACE(STRING s)[INT] { FLEX len = s -> Length; IF len == 0 { RETURN 0; }; CYCLE len AS i { FLEX c = s[i]; FLEX is_whitespace = Char -> IsWhiteSpace(c); IF is_whitespace == 0 { RETURN 0; }; }; RETURN 1; }
"@

Set-Content -Path "scripts\stdlib\ValidationLib.ps" -Value $funcs -NoNewline
Write-Host "ValidationLib.ps generated successfully!"
