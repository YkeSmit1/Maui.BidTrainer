namespace Maui.BidTrainer;

public class Utils
{
    public static async Task CopyFileToAppDataDirectory(string filename)
    {
        var filePath = Path.Combine(FileSystem.AppDataDirectory, filename);
        if (File.Exists(filePath))
            return;

        await using Stream inputStream = await FileSystem.OpenAppPackageFileAsync(filename);
        using BinaryReader reader = new BinaryReader(inputStream);
        await using FileStream outputStream = File.Create(filePath);
        await using BinaryWriter writer = new BinaryWriter(outputStream);
        byte[] buffer = new byte[4096];
        int bytesRead;
        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            writer.Write(buffer, 0, bytesRead);
        }
    }
}