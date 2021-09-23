namespace PixivApi.Models
{
    public record PixivApiAuthInfo(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn);
}