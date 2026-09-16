using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace JobApplicationTracker.Pagination;

public class JobApplicationCursorCodec
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public string Encode(JobApplicationCursor cursor)
    {
        var json = JsonSerializer.Serialize(
            cursor,
            SerializerOptions);

        var bytes = Encoding.UTF8.GetBytes(json);

        return WebEncoders.Base64UrlEncode(bytes);
    }

    public bool TryDecode(
        string encodedCursor,
        out JobApplicationCursor? cursor)
    {
        cursor = null;

        if (string.IsNullOrWhiteSpace(encodedCursor))
        {
            return false;
        }

        try
        {
            var bytes = WebEncoders.Base64UrlDecode(
                encodedCursor);

            var json = Encoding.UTF8.GetString(bytes);

            var decodedCursor =
                JsonSerializer.Deserialize<JobApplicationCursor>(
                    json,
                    SerializerOptions);

            if (decodedCursor is null ||
                decodedCursor.Id <= 0 ||
                decodedCursor.AppliedDate == default)
            {
                return false;
            }

            cursor = decodedCursor;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
