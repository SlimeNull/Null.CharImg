using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharImg
{
    class CharImg
    {
        static char GetDrawChar(ref int index, ref char[] str)
        {
            if (index >= str.Length)
                index = 0;

            return str[index++];
        }
        static int IntegerClamp(int src, int min, int max)
        {
            if (src < min)
                return min;
            else if (src > max)
                return max;
            else
                return src;
        }                // 将数字限制在某个范围内
        static Color ColorMix(Color foreColor, Color backColor, int mixRate = 100)
        {
            float fMixRate = mixRate / 100f, fRtnRate = 1 - fMixRate;

            int tarR = (int)((foreColor.R * 2 - backColor.R) * fMixRate + foreColor.R * fRtnRate),
                tarG = (int)((foreColor.G * 2 - backColor.G) * fMixRate + foreColor.G * fRtnRate),
                tarB = (int)((foreColor.B * 2 - backColor.B) * fMixRate + foreColor.B * fRtnRate);

            tarR = IntegerClamp(tarR, 0, 255);
            tarG = IntegerClamp(tarG, 0, 255);
            tarB = IntegerClamp(tarB, 0, 255);

            return Color.FromArgb(tarR, tarG, tarB);
        }           // Mix, 对颜色进行运算
        static Color ColorRtn(Color foreColor, Color _backColor, int _rate)
        {
            return foreColor;
        }           // Rtn return, 直接返回原来的颜色
        static Color GetAverageColor(LockBitmap lockBmp)
        {
            long rTotal = 0, gTotal = 0, bTotal = 0;
            int pixelCount = lockBmp.Width * lockBmp.Height;

            Color pixel;
            for (int i = 0; i < lockBmp.Height; i++)
            {
                for (int j = 0; j < lockBmp.Width; j++)
                {
                    pixel = lockBmp.GetPixel(j, i);

                    rTotal += pixel.R;
                    gTotal += pixel.G;
                    bTotal += pixel.B;
                }
            }
            return Color.FromArgb((int)(rTotal / pixelCount), (int)(gTotal / pixelCount), (int)(bTotal / pixelCount));
        }
        public class DrawOption
        {
            public Size CharSize;
            public Size MaxSize;
            public Font DrawFont;
            public Color BackgroundColor;
            public int CorrectRate;
            public bool Compress;
            public bool Opposite;
            public bool AutoCorrect;
            public bool AutoBackground;
            public bool HighDefinition;
            public char[] Chars;

            public DrawOption()
            {
                CharSize = new Size(8, 16);
                MaxSize = Size.Empty;
                DrawFont = DefaultFont;
                BackgroundColor = Color.Black;
                CorrectRate = 100;
                Compress = false;
                Opposite = false;
                AutoCorrect = false;
                AutoBackground = false;
                HighDefinition = false;
                Chars = DefaultChars;
            }

            public static DrawOption Default
            {
                get
                {
                    return new DrawOption();
                }
            }

            static Font DefaultFont = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Regular, GraphicsUnit.Pixel);
            static char[] DefaultChars = new char[] { 'W' };
        }

        /// <summary>
        /// 生成字符图像混合位图
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="output"></param>
        /// <param name="drawOption"></param>
        public static void GenCharImg(Image srcImg, out Bitmap output, DrawOption drawOption)
        {
            Bitmap srcBmp = (Bitmap)(Image)srcImg.Clone();
            Bitmap rstBmp;

            Color bgColor = drawOption.BackgroundColor;
            Brush bgBrush = new SolidBrush(bgColor);

            Func<Color, Color, int, Color> colorDealer;

            if (drawOption.AutoCorrect)
                colorDealer = ColorMix;
            else
                colorDealer = ColorRtn;

            if (drawOption.Compress)
            {
                Image tmpImg = srcBmp;

                if (drawOption.HighDefinition)
                {
                    int hDWidth = drawOption.MaxSize.Width * drawOption.CharSize.Width,
                        hDHeight = drawOption.MaxSize.Height * drawOption.CharSize.Height;

                    if (hDWidth != 0 && srcBmp.Width > hDWidth)
                        tmpImg = tmpImg.GetThumbnailImage(hDWidth, (int)((float)hDWidth / tmpImg.Width * tmpImg.Height), () => false, IntPtr.Zero);
                    if (hDHeight != 0 && srcBmp.Height > hDHeight)
                        tmpImg = tmpImg.GetThumbnailImage((int)((float)hDHeight / tmpImg.Height * tmpImg.Width), hDHeight, () => false, IntPtr.Zero);
                }
                else
                {
                    if (drawOption.MaxSize.Width != 0 && srcBmp.Width > drawOption.MaxSize.Width)
                        tmpImg = tmpImg.GetThumbnailImage(drawOption.MaxSize.Width, (int)((float)drawOption.MaxSize.Width / tmpImg.Width * tmpImg.Height), () => false, IntPtr.Zero);
                    if (drawOption.MaxSize.Height != 0 && srcBmp.Height > drawOption.MaxSize.Height)
                        tmpImg = tmpImg.GetThumbnailImage((int)((float)drawOption.MaxSize.Height / tmpImg.Height * tmpImg.Width), drawOption.MaxSize.Height, () => false, IntPtr.Zero);
                }

                if (srcBmp != tmpImg)
                    srcBmp.Dispose();

                srcBmp = (Bitmap)tmpImg;
            }

            LockBitmap lockSrc = new LockBitmap(srcBmp);            // 实例化后就自动锁住了

            if (drawOption.AutoBackground)
            {
                bgColor = GetAverageColor(lockSrc);
                bgBrush = new SolidBrush(bgColor);
            }

            if (drawOption.HighDefinition)
            {
                Bitmap maskBmp = new Bitmap(srcBmp.Width, srcBmp.Height, PixelFormat.Format24bppRgb);
                maskBmp.SetResolution(srcBmp.HorizontalResolution, srcBmp.VerticalResolution);

                rstBmp = new Bitmap(srcBmp.Width, srcBmp.Height, srcBmp.PixelFormat);
                rstBmp.SetResolution(srcBmp.HorizontalResolution, srcBmp.VerticalResolution);

                Graphics maskG = Graphics.FromImage(maskBmp);
                maskG.FillRectangle(new SolidBrush(Color.Black), new Rectangle(Point.Empty, maskBmp.Size));

                int charIndex = 0;
                SolidBrush whiteBrush = new SolidBrush(Color.White);          // 用于蒙版中字的白色
                for (int i = 0; i < srcBmp.Height; i += drawOption.CharSize.Height)
                {
                    for (int j = 0; j < srcBmp.Width; j += drawOption.CharSize.Width)
                    {
                        maskG.DrawString(
                            GetDrawChar(ref charIndex, ref drawOption.Chars).ToString(),
                            drawOption.DrawFont,
                            whiteBrush,
                            j,
                            i);
                    }
                }
                maskG.Dispose();
                
                LockBitmap lockMask = new LockBitmap(maskBmp);
                LockBitmap lockRst = new LockBitmap(rstBmp);

                if (drawOption.Opposite)
                {
                    for (int i = 0; i < srcBmp.Height; i++)
                    {
                        for (int j = 0; j < srcBmp.Width; j++)
                        {
                            Color srcPixel = lockSrc.GetPixel(j, i);
                            Color maskPixel = lockMask.GetPixel(j, i);

                            lockRst.SetPixel(j, i, Color.FromArgb(
                                (int)(srcPixel.R * (1 - maskPixel.R / 256f) + bgColor.R * (maskPixel.R / 256f)),
                                (int)(srcPixel.G * (1 - maskPixel.G / 256f) + bgColor.G * (maskPixel.G / 256f)),
                                (int)(srcPixel.B * (1 - maskPixel.B / 256f) + bgColor.B * (maskPixel.B / 256f))));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < srcBmp.Height; i++)
                    {
                        for (int j = 0; j < srcBmp.Width; j++)
                        {
                            Color srcPixel = lockSrc.GetPixel(j, i);
                            Color maskPixel = lockMask.GetPixel(j, i);

                            lockRst.SetPixel(j, i, Color.FromArgb(
                                (int)(srcPixel.R * (maskPixel.R / 256f) + bgColor.R * (1 - (maskPixel.R / 256f))),
                                (int)(srcPixel.G * (maskPixel.G / 256f) + bgColor.G * (1 - (maskPixel.G / 256f))),
                                (int)(srcPixel.B * (maskPixel.B / 256f) + bgColor.B * (1 - (maskPixel.B / 256f)))));
                        }
                    }
                }

                lockMask.UnlockBits();
                lockMask = null;
                maskBmp.Dispose();
                lockRst.UnlockBits();
                lockRst = null;

                output = rstBmp;
            }
            else
            {
                rstBmp = new Bitmap(drawOption.CharSize.Width * srcBmp.Width, drawOption.CharSize.Height * srcBmp.Height, srcBmp.PixelFormat);
                rstBmp.SetResolution(srcBmp.HorizontalResolution, srcBmp.VerticalResolution);

                Graphics rstG = Graphics.FromImage(rstBmp);

                if (drawOption.Opposite)
                {
                    for (int i = 0; i < srcBmp.Height; i++)
                    {
                        for (int j = 0; j < srcBmp.Width; j++)
                        {
                            Color currentPixel = lockSrc.GetPixel(j, i);
                            Color currentColor = colorDealer.Invoke(currentPixel, drawOption.BackgroundColor, drawOption.CorrectRate);
                            Brush currentBrush = new SolidBrush(currentColor);

                            rstG.FillRectangle(
                                currentBrush,
                                drawOption.CharSize.Width * j,
                                drawOption.CharSize.Height * i,
                                drawOption.CharSize.Width,
                                drawOption.CharSize.Height);
                        }
                    }

                    int charIndex = 0;
                    for (int i = 0; i < srcBmp.Height; i++)
                    {
                        for (int j = 0; j < srcBmp.Width; j++)
                        {
                            rstG.DrawString(
                                GetDrawChar(ref charIndex, ref drawOption.Chars).ToString(),
                                drawOption.DrawFont,
                                bgBrush,
                                j * drawOption.CharSize.Width,
                                i * drawOption.CharSize.Height);
                        }
                    }
                }
                else
                {
                    rstG.FillRectangle(bgBrush, 0, 0, rstBmp.Width, rstBmp.Height);        // 先填充一波颜色

                    int charIndex = 0;
                    for (int i = 0; i < srcBmp.Height; i++)
                    {
                        for (int j = 0; j < srcBmp.Width; j++)
                        {
                            Color currentPixel = lockSrc.GetPixel(j, i);
                            Color currentColor = colorDealer.Invoke(currentPixel, drawOption.BackgroundColor, drawOption.CorrectRate);
                            Brush currentBrush = new SolidBrush(currentColor);

                            rstG.DrawString(
                                GetDrawChar(ref charIndex, ref drawOption.Chars).ToString(),
                                drawOption.DrawFont,
                                currentBrush,
                                j * drawOption.CharSize.Width,
                                i * drawOption.CharSize.Height);
                        }
                    }
                }

                rstG.Dispose();
                output = rstBmp;
            }

            lockSrc.UnlockBits();
            lockSrc = null;
            srcBmp.Dispose();
        }
        public static void GetCharImg(Image srcImg, out Bitmap output)
        {
            GenCharImg(srcImg, out output, DrawOption.Default);
        }
    }
}
