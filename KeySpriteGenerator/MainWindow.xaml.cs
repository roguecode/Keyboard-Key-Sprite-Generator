using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ColorFont;

namespace KeySpriteGenerator
{
  public partial class MainWindow : Window
  {
    FontInfo _selectedFont;
    BitmapSource _smallTemplateImage;
    BitmapSource _mediumTemplateImage;
    int _templatePadding = 70;

    public MainWindow()
    {
      InitializeComponent();
    }

    void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
      _selectedFont = FontInfo.GetControlFont(this.DummyTextBox);

      _smallTemplateImage = new BitmapImage(new Uri("Data/Small.png", UriKind.Relative));
      _mediumTemplateImage = new BitmapImage(new Uri("Data/Medium.png", UriKind.Relative));

      Lines.Text = File.ReadAllText("Data/Default.txt");

      Directory.CreateDirectory("./Output");

      RedrawPreview();
    }

    ColorFontDialog GetColorFontDialog()
    {
      return new ColorFontDialog
      {
        Owner = this,
        Font = _selectedFont,
      };
    }

    void ChooseFont_Click(object sender, RoutedEventArgs e)
    {
      var dialog = GetColorFontDialog();
      if (dialog.ShowDialog() == true)
      {
        _selectedFont = dialog.Font;
        RedrawPreview();
      }
    }

    void RedrawPreview()
    {
      if (_selectedFont == null)
        return;

      Img.Source = GenerateImage("A");
    }

    void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      RedrawPreview();
    }

    DrawingImage GenerateImage(string content, string fileName = null)
    {
      var visual = new DrawingVisual();
      using (DrawingContext drawingContext = visual.RenderOpen())
      {
        var templateAndText = GetTemplateAndText(content);
        var template = templateAndText.Item1;
        var text = templateAndText.Item2;
        drawingContext.DrawImage(template, new Rect(0, 0, template.PixelWidth, template.PixelHeight));

        var xOffset = (XPositionSlider.Value / XPositionSlider.Maximum) * template.PixelWidth;
        var yOffset = (YPositionSlider.Value / YPositionSlider.Maximum) * template.PixelHeight;
        drawingContext.DrawText(text, new Point(xOffset - (text.Width / 2), yOffset - (text.Height / 2)));
      }

      if (!string.IsNullOrEmpty(fileName))
      {
        var bitmap = new RenderTargetBitmap((int)visual.Drawing.Bounds.Width, (int)visual.Drawing.Bounds.Height, 96,
          96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using (var stream = new FileStream($"./Output/{fileName}.png", FileMode.Create))
          encoder.Save(stream);
      }

      return new DrawingImage(visual.Drawing);
    }

    Tuple<BitmapSource, FormattedText> GetTemplateAndText(string content)
    {
      var typeFace = new Typeface(_selectedFont.Family, _selectedFont.Style, _selectedFont.Weight, _selectedFont.Stretch);
      var text = new FormattedText(content, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeFace, _selectedFont.Size, _selectedFont.BrushColor);

      BitmapSource template;


      if (text.Width < (_smallTemplateImage.Width - _templatePadding) || content.Length <= 3)
        template = _smallTemplateImage;
      else
        template = _mediumTemplateImage;

      text = GetFormattedTextToFitInTemplate(template, text);

      return new Tuple<BitmapSource, FormattedText>(template, text);
    }

    FormattedText GetFormattedTextToFitInTemplate(BitmapSource template, FormattedText text)
    {
      var startFontSize = _selectedFont.Size;
      while (true)
      {
        if (text.Width > (template.PixelWidth - _templatePadding))
        {
          startFontSize--;
          text.SetFontSize(startFontSize);
        }
        else
          break;
      }

      return text;
    }

    void Generate_Click(object sender, RoutedEventArgs e)
    {
      var sw = new Stopwatch();
      sw.Start();
      var lines = Lines.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

      foreach (var line in lines)
      {
        var thisLine = line.Trim();
        if (string.IsNullOrEmpty(thisLine) || thisLine.StartsWith("//"))
          continue;

        var fileName = "";
        var displayName = "";

        var commaIndex = thisLine.IndexOf(",", StringComparison.Ordinal);

        if (commaIndex > -1)
        {
          fileName = thisLine.Substring(0, commaIndex);

          displayName = thisLine.Substring(commaIndex + 1);

          if (string.IsNullOrEmpty(displayName))
            displayName = fileName;
        }
        else
        {
          fileName = thisLine;
          displayName = fileName;
        }

        fileName = fileName.Trim();
        displayName = displayName.Trim();

        Debug.WriteLine($"Generating {fileName}.png with icon of: '{displayName}'");

        GenerateImage(displayName, fileName);
      }

      sw.Stop();
      Debug.WriteLine($"Images generated in {sw.ElapsedMilliseconds} MS.");
    }
  }
}
