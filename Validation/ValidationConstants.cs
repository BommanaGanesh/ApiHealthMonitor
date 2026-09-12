namespace ApiHealthMonitor.Validation;

/// <summary>
/// Constants and validation logic for health check requests.
/// </summary>
public static class ValidationConstants
{
    /// <summary>
    /// Maximum URL length to prevent excessively long URLs.
    /// </summary>
    public const int MaxUrlLength = 2048;

    /// <summary>
    /// Error message for empty or null URL.
    /// </summary>
    public const string ErrorUrlEmpty = "URL cannot be empty.";

    /// <summary>
    /// Error message for URL exceeding maximum length.
    /// </summary>
    public const string ErrorUrlTooLong = "URL cannot exceed 2048 characters.";

    /// <summary>
    /// Error message for malformed URL.
    /// </summary>
    public const string ErrorUrlInvalid = "URL is not a valid absolute URI.";

    /// <summary>
    /// Error message for unsupported URI scheme.
    /// </summary>
    public const string ErrorSchemeNotAllowed = "Only HTTP and HTTPS schemes are allowed.";

    /// <summary>
    /// Error message for private network (SSRF protection).
    /// </summary>
    public const string ErrorPrivateNetworkBlocked = "Requests to private networks are not allowed.";
}
