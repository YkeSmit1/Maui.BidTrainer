﻿using System.Globalization;

namespace Maui.BidTrainer.Converters;

public class LessonToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) 
            return string.Empty;
        var filePath = Path.Combine("theories", $"lesson{(int)value}.txt");
        if (!FileSystem.AppPackageFileExistsAsync(filePath).Result)
            return string.Empty;
        using var reader = new StreamReader(FileSystem.OpenAppPackageFileAsync(filePath).Result);
        return reader.ReadToEnd();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}