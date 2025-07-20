namespace Maui.BidTrainer;

public static class Utils
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

    public static FormattedString FormatTextWithSymbols(string text)
    {
        var formatted = new FormattedString();
        int lastIndex = 0;
    
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] != '♥' && text[i] != '♦') continue;
            if (i > lastIndex)
            {
                formatted.Spans.Add(new Span { Text = text.Substring(lastIndex, i - lastIndex), TextColor = GetStandardTextColor() });
            }
            
            formatted.Spans.Add(new Span { Text = text[i].ToString(), TextColor = Colors.Red });
            lastIndex = i + 1;
        }
    
        if (lastIndex < text.Length)
        {
            formatted.Spans.Add(new Span { Text = text[lastIndex..], TextColor = GetStandardTextColor() });
        }
    
        return formatted;

        Color GetStandardTextColor()
        {
            return Application.Current?.RequestedTheme == AppTheme.Light ? Colors.Black : Colors.White;
        }
    }
}