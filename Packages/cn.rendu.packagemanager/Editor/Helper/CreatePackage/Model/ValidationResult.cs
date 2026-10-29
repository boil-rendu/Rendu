namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 验证结果
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true, ErrorMessage = "" };
        }

        public static ValidationResult Failure(string errorMessage)
        {
            return new ValidationResult { IsValid = false, ErrorMessage = errorMessage };
        }
    }
}
