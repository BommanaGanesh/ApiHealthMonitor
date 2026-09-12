using ApiHealthMonitor.Validation;

namespace ApiHealthMonitor.Services;

/// <summary>
/// Validates URLs for correctness and security concerns.
/// </summary>
public static class UrlValidator
{
    /// <summary>
    /// Validates the provided URL for basic correctness and optional security concerns.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="blockPrivateNetworks">If true, reject requests to private IP ranges.</param>
    /// <returns>An error message if validation fails; null if validation passes.</returns>
    public static string? ValidateUrl(string? url, bool blockPrivateNetworks = false)
    {
        // Check for null or empty
        if (string.IsNullOrWhiteSpace(url))
        {
            return ValidationConstants.ErrorUrlEmpty;
        }

        // Check URL length
        if (url.Length > ValidationConstants.MaxUrlLength)
        {
            return ValidationConstants.ErrorUrlTooLong;
        }

        // Try to parse as absolute URI
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return ValidationConstants.ErrorUrlInvalid;
        }

        // Only allow HTTP and HTTPS schemes
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return ValidationConstants.ErrorSchemeNotAllowed;
        }

        // Optional: check for private networks (SSRF protection)
        if (blockPrivateNetworks && IsPrivateNetwork(uri))
        {
            return ValidationConstants.ErrorPrivateNetworkBlocked;
        }

        return null;
    }

    /// <summary>
    /// Determines if a URI points to a private network address.
    /// This includes localhost, private IP ranges, and link-local addresses.
    /// </summary>
    private static bool IsPrivateNetwork(Uri uri)
    {
        var host = uri.Host.ToLowerInvariant();

        // Check for localhost
        if (host is "localhost" or "127.0.0.1" or "::1" or "[::1]")
        {
            return true;
        }

        // Attempt to parse as IP address
        if (System.Net.IPAddress.TryParse(host, out var ipAddress))
        {
            return IsPrivateIpAddress(ipAddress);
        }

        return false;
    }

    /// <summary>
    /// Determines if an IP address is in a private range.
    /// </summary>
    private static bool IsPrivateIpAddress(System.Net.IPAddress ip)
    {
        // IPv4 private ranges
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();
            return bytes[0] switch
            {
                10 => true,                        // 10.0.0.0/8
                172 when bytes[1] is >= 16 and <= 31 => true,  // 172.16.0.0/12
                192 when bytes[1] == 168 => true,  // 192.168.0.0/16
                127 => true,                       // 127.0.0.0/8 (loopback)
                169 when bytes[1] == 254 => true,  // 169.254.0.0/16 (link-local)
                _ => false
            };
        }

        // IPv6 private ranges
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            // ::1 (loopback), fc00::/7 (unique local), fe80::/10 (link-local)
            if (System.Net.IPAddress.IsLoopback(ip))
                return true;

            var bytes = ip.GetAddressBytes();
            // Unique local addresses (fc00::/7)
            if ((bytes[0] & 0xFE) == 0xFC)
                return true;

            // Link-local addresses (fe80::/10)
            if (bytes[0] == 0xFE && (bytes[1] & 0xC0) == 0x80)
                return true;

            return false;
        }

        return false;
    }
}
