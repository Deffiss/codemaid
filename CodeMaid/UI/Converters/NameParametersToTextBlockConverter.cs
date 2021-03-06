﻿#region CodeMaid is Copyright 2007-2015 Steve Cadwallader.

// CodeMaid is free software: you can redistribute it and/or modify it under the terms of the GNU
// Lesser General Public License version 3 as published by the Free Software Foundation.
//
// CodeMaid is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details <http://www.gnu.org/licenses/>.

#endregion CodeMaid is Copyright 2007-2015 Steve Cadwallader.

using Microsoft.VisualStudio.PlatformUI;
using SteveCadwallader.CodeMaid.Helpers;
using SteveCadwallader.CodeMaid.Model.CodeItems;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace SteveCadwallader.CodeMaid.UI.Converters
{
    /// <summary>
    /// Converts a code item into a single TextBlock object containing its name and optionally its parameters.
    /// </summary>
    public class NameParametersToTextBlockConverter : IMultiValueConverter
    {
        #region Fields

        /// <summary>
        /// A default instance of the <see cref="NameParametersToTextBlockConverter" />.
        /// </summary>
        public static NameParametersToTextBlockConverter Default = new NameParametersToTextBlockConverter();

        /// <summary>
        /// An instance of the <see cref="NameParametersToTextBlockConverter" /> that includes parameters.
        /// </summary>
        public static NameParametersToTextBlockConverter WithParameters = new NameParametersToTextBlockConverter { IncludeParameters = true };

        /// <summary>
        /// An instance of the <see cref="NameParametersToTextBlockConverter" /> for parent items.
        /// </summary>
        public static NameParametersToTextBlockConverter Parent = new NameParametersToTextBlockConverter
        {
            FontSize = 14,
            FontStyle = FontStyles.Normal,
            FontWeight = FontWeights.SemiBold
        };

        /// <summary>
        /// An instance of the <see cref="NameParametersToTextBlockConverter" /> for parent items with parameters.
        /// </summary>
        public static NameParametersToTextBlockConverter ParentWithParams = new NameParametersToTextBlockConverter
        {
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            FontStyle = FontStyles.Normal,
            IncludeParameters = true
        };

        private static readonly SolidColorBrush BrushTypeRun = new SolidColorBrush(Color.FromRgb(0x77, 0x77, 0x77));

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets a flag indicating if parameters should be included.
        /// </summary>
        public bool IncludeParameters { get; set; }

        /// <summary>
        /// Gets or sets the size of the font.
        /// </summary>
        /// <value>
        /// The size of the font.
        /// </value>
        public int FontSize { get; set; }

        /// <summary>
        /// Gets or sets the font weight.
        /// </summary>
        /// <value>
        /// The font weight.
        /// </value>
        public FontWeight FontWeight { get; set; }

        /// <summary>
        /// Gets or sets the font style.
        /// </summary>
        /// <value>
        /// The font style.
        /// </value>
        public FontStyle FontStyle { get; set; }

        #endregion Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="NameParametersToTextBlockConverter"/> class.
        /// </summary>
        public NameParametersToTextBlockConverter()
        {
            FontWeight = FontWeights.Normal;
            FontSize = 12;
            FontStyle = FontStyles.Normal;
        }

        #region Implementation of IValueConverter

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="values">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var codeItem = values[0] as ICodeItem;
            if (codeItem == null) return null;

            var highlightedText = values[1] as string;

            var textBlock = new TextBlock { FontSize = FontSize };
            // if we have a filter perform searching of it for highlighting.
            if (!string.IsNullOrEmpty(highlightedText))
            {
                var lastIndexOf = 0;

                while (lastIndexOf >= 0)
                {
                    var indexOf = codeItem.Name.IndexOf(highlightedText, lastIndexOf, StringComparison.InvariantCultureIgnoreCase);
                    var commonPart = codeItem.Name.Substring(lastIndexOf,
                        indexOf >= 0 ? indexOf - lastIndexOf : codeItem.Name.Length - lastIndexOf);

                    if (commonPart.Length > 0)
                    {
                        var run = CreateRun(commonPart);
                        textBlock.Inlines.Add(run);
                    }

                    if (indexOf >= 0)
                    {
                        var highlightedPart = codeItem.Name.Substring(indexOf, highlightedText.Length);
                        var highlightedRun = CreateHighlightedRun(highlightedPart);
                        textBlock.Inlines.Add(highlightedRun);
                    }

                    lastIndexOf = indexOf >= 0 ? indexOf + highlightedText.Length : -1;
                }
            }
            // else just display the name of the item
            else
            {
                textBlock.Inlines.Add(codeItem.Name);
            }

            var codeItemWithParams = codeItem as ICodeItemParameters;
            // if our item has parameters display them with italic font style
            if (codeItemWithParams != null && IncludeParameters)
            {
                var opener = GetOpeningString(codeItemWithParams);
                if (opener != null)
                {
                    textBlock.Inlines.Add(CreateItalicRun(opener));
                }

                bool isFirst = true;

                try
                {
                    foreach (var param in codeItemWithParams.Parameters)
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            textBlock.Inlines.Add(CreateItalicRun(", "));
                        }

                        try
                        {
                            textBlock.Inlines.Add(CreateTypeRun(TypeFormatHelper.Format(param.Type.AsString) + " "));
                            textBlock.Inlines.Add(CreateItalicRun(param.Name));
                        }
                        catch (Exception)
                        {
                            textBlock.Inlines.Add(CreateItalicRun("?"));
                        }
                    }
                }
                catch (Exception)
                {
                    textBlock.Inlines.Add(CreateItalicRun("?"));
                }

                var closer = GetClosingString(codeItemWithParams);
                if (closer != null)
                {
                    textBlock.Inlines.Add(CreateItalicRun(closer));
                }
            }

            return textBlock;
        }

        ///// <summary>
        ///// Converts a value.
        ///// </summary>
        ///// <param name="value">The value that is produced by the binding target.</param>
        ///// <param name="targetType">The type to convert to.</param>
        ///// <param name="parameter">The converter parameter to use.</param>
        ///// <param name="culture">The culture to use in the converter.</param>
        ///// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Implementation of IValueConverter

        #region Methods

        /// <summary>
        /// Creates an inline run based on the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The created run.</returns>
        private Run CreateRun(string text)
        {
            // Get VS theme color to make runs look well on all themes
            var foreground = VSColorTheme
                .GetThemedColor(TreeViewColors.BackgroundTextBrushKey);

            var run = new Run(text)
            {
                FontSize = FontSize,
                FontStyle = FontStyle,
                FontWeight = FontWeight,
                BaselineAlignment = BaselineAlignment.Baseline,
                Foreground = new SolidColorBrush(new Color { A = foreground.A, B = foreground.B, G = foreground.G, R = foreground.R })
            };

            return run;
        }

        /// <summary>
        /// Creates a highlighted inline run based on the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Highlighted inline run.</returns>
        private Run CreateHighlightedRun(string text)
        {
            // Get VS theme color to make runs look well on all themes
            var foreground = VSColorTheme
                .GetThemedColor(TreeViewColors.HighlightedSpanTextBrushKey);
            var background = VSColorTheme
                .GetThemedColor(TreeViewColors.HighlightedSpanBrushKey);

            var run = new Run(text)
            {
                FontSize = FontSize,
                FontStyle = FontStyle,
                FontWeight = FontWeight,
                Foreground = new SolidColorBrush(new Color { A = foreground.A, B = foreground.B, G = foreground.G, R = foreground.R }),
                Background = new SolidColorBrush(new Color { A = background.A, B = background.B, G = background.G, R = background.R }),
                BaselineAlignment = BaselineAlignment.Baseline
            };

            return run;
        }

        /// <summary>
        /// Creates an inline run based on the specified text with special styling for types.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The created run.</returns>
        private Run CreateTypeRun(string text)
        {
            var run = CreateItalicRun(text);

            run.Foreground = BrushTypeRun;

            return run;
        }

        /// <summary>
        /// Creates an italic inline run based on the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Italic run.</returns>
        private Run CreateItalicRun(string text)
        {
            var run = CreateRun(text);

            run.FontStyle = FontStyles.Italic;

            return run;
        }

        /// <summary>
        /// Gets the opening string for the specified code item.
        /// </summary>
        /// <param name="codeItem">The code item.</param>
        /// <returns>The opening string, otherwise null.</returns>
        private static string GetOpeningString(ICodeItemParameters codeItem)
        {
            var property = codeItem as CodeItemProperty;
            if (property != null)
            {
                return property.IsIndexer ? "[" : null;
            }

            return "(";
        }

        /// <summary>
        /// Gets the closing string for the specified code item.
        /// </summary>
        /// <param name="codeItem">The code item.</param>
        /// <returns>The closing string, otherwise null.</returns>
        private static string GetClosingString(ICodeItemParameters codeItem)
        {
            var property = codeItem as CodeItemProperty;
            if (property != null)
            {
                return property.IsIndexer ? "]" : null;
            }

            return ")";
        }

        #endregion Methods
    }
}