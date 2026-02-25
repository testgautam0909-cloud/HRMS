namespace HRMS.Shared.Helpers;

public static class StringHelper
{
    public static string GenerateEmployeeCode(int year, int sequence)
    {
        return $"EMP-{year}-{sequence:D4}";
    }

    public static string MaskBankAccount(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 4)
            return "****";
        return new string('*', accountNumber.Length - 4) + accountNumber[^4..];
    }

    public static string GenerateRandomPassword(int length = 12)
    {
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        var allChars = uppercase + lowercase + digits + special;

        var random = new Random();
        var password = new char[length];
        password[0] = uppercase[random.Next(uppercase.Length)];
        password[1] = lowercase[random.Next(lowercase.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        for (int i = 4; i < length; i++)
            password[i] = allChars[random.Next(allChars.Length)];

        return new string(password.OrderBy(_ => random.Next()).ToArray());
    }
}
