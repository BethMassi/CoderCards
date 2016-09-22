#r "Newtonsoft.Json"
#r "System.Drawing"

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

static double RoundScore(double score) => Math.Round(score * 100);

static void NormalizeScores(Scores scores)
{
    scores.Anger = RoundScore(scores.Anger);
    scores.Happiness = RoundScore(scores.Happiness);
    scores.Neutral = RoundScore(scores.Neutral);
    scores.Sadness = RoundScore(scores.Sadness);
    scores.Surprise = RoundScore(scores.Surprise);
}

static Image MergeCardImage(Image card, byte[] imageBytes, Tuple<string, string> personInfo, double score)
{
    using (MemoryStream faceImageStream = new MemoryStream(imageBytes)) 
    {
        const int topLeftFaceX   = 87;
        const int topLeftFaceY   = 188;
        const int faceRect       = 647;
        const int nameTextX      = 56;
        const int nameTextY      = 65;
        const int titleTextY     = 108;
        const int nameWidth      = 430;
        const int scoreX         = 645;
        const int scoreY         = 62;
        const int scoreWidth     = 117;

        using (Image faceImage = Image.FromStream(faceImageStream, true)) 
        {
            using (Graphics g = Graphics.FromImage(card)) 
            {
                g.DrawImage(faceImage, topLeftFaceX, topLeftFaceY, faceRect, faceRect);
                RenderText(g, nameTextX, nameTextY, nameWidth, personInfo.Item1);
                RenderText(g, nameTextX, titleTextY, nameWidth, personInfo.Item2);

                RenderScore(g, scoreX, scoreY, scoreWidth, score.ToString());
            }

            return card;
        }
    }
}

// save with higher quality than the default, to avoid jpeg artifacts on the text and numbers
static void SaveAsJpeg(Image image, Stream outputStream)
{
    var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
    var qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
    var encoderParams = new EncoderParameters(1);
    encoderParams.Param[0] = new EncoderParameter(qualityEncoder, 90L);

    image.Save(outputStream, jpgEncoder, encoderParams);
}

static ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) {
          return codec;
        }
    }
    return null;
}

static void RenderText(Graphics graphics, int xPos, int yPos, int width, string text)
{
    var brush = new SolidBrush(Color.Black);
    var fontSize = 36;
    var font = new Font("Microsoft Sans Serif", fontSize, FontStyle.Bold);
    SizeF size;

    do {
        font = new Font("Microsoft Sans Serif", fontSize--, FontStyle.Bold);
        size = graphics.MeasureString(text, font);
    }
    while (size.Width > width);

    graphics.DrawString(text, font, brush, xPos, yPos);
}

static void RenderScore(Graphics graphics, int xPos, int yPos, int width, string score)
{
  var brush = new SolidBrush(Color.Black);
  var fontSize = 38;
  var font = new Font("Microsoft Sans Serif", fontSize, FontStyle.Bold);
  SizeF size = graphics.MeasureString(score, font);

  graphics.DrawString(score, font, brush, width - size.Width + xPos, yPos);
}

static string GetFullImagePath(string filename)
{
    var appPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "site", "wwwroot", "CardGenerator", "assets");
    return Path.Combine(appPath, filename);
}

static Tuple<string, string> GetNameAndTitle(string filename)
{
    string[] words = filename.Split('-');
    return Tuple.Create(words[0], words[1]);
}

public class Face
{
    public FaceRectangle FaceRectangle { get; set; }
    public Scores Scores { get; set; }
}

public class FaceRectangle
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class Scores
{
    public double Anger { get; set; }
    public double Contempt { get; set; }
    public double Disgust { get; set; }
    public double Fear { get; set; }
    public double Happiness { get; set; }
    public double Neutral { get; set; }
    public double Sadness { get; set; }
    public double Surprise { get; set; }
}

