namespace Bevelop.VSClient.Services
{
    public interface IZipper
    {
        byte[] Zip(string text);
        string Unzip(byte[] bytes);
    }
}