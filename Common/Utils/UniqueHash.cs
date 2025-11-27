namespace Common.Utils;

public class UniqueHash
{
    public static string Generate()
    {
        return Guid.NewGuid().ToString();
    }

    public static bool IsGuid(string guid)
    {
        return Guid.TryParse(guid, out _);
    }
}
