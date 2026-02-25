namespace HRMS.Shared.Constants;

public static class AppConstants
{
    public const int DEFAULT_PAGE_SIZE = 20;
    public const int MAX_PAGE_SIZE = 100;
    public const int MAX_FILE_SIZE_MB = 10;
    public const int MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;
    public const int MAX_MESSAGE_LENGTH = 4000;
    public const int MAX_GROUP_MEMBERS = 100;
    public const int MESSAGE_RETRACT_MINUTES = 60;
    public const int MAX_LOGIN_ATTEMPTS = 5;
    public const int LOCKOUT_DURATION_MINUTES = 15;
    public const string EMPLOYEE_CODE_PREFIX = "EMP";
    public const string CORRELATION_ID_HEADER = "X-Correlation-Id";
    public const string SALARY_SLIP_FOLDER = "hrms/{0}/salary-slips/{1}/{2}";
    public const string DOCUMENT_FOLDER = "hrms/{0}/{1}";
}
