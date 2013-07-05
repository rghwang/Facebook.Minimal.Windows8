﻿using System;
using Windows.UI.Xaml.Data;

namespace Facebook.Minimal.Windows8.Common
{
    /// <summary>
    /// true를 false로, false를 true로 변환하는 값 변환기입니다.
    /// </summary>
    public sealed class BooleanNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }
    }
}
