using Null.Library;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharImg
{
    class Program
    {
        public static string HelpText =
            "Null.CharImg: 字符图像混合工具" +
            "\n" +
            "\nNull.CharImg [ -{CharWidth | CWidth} 字符占宽 ]" +
            "\n             [ -{CharHeight | CHeight} 字符占高 ]" +
            "\n             [ -{MaxWidth | MWidth} 最大宽度 ]" +
            "\n             [ -{MaxHeight | MHeight} 最大高度 ]" +
            "\n             [ -{FontSize | FSize} 字体大小 ]" +
            "\n             [ -{String | Str} 字符串内容 ]" +
            "\n             [ -{CorrectRate | CRate} 颜色矫正程度]" +
            "\n             [ -{BackgroundColor | BgColor} 背景颜色 ]" +
            "\n             [ -{FontFamily | Font} 字体 ]" +
            "\n             [ /{FontBold | Bold} ]" +
            "\n             [ /{Opposite | Oppo} ]" +
            "\n             [ /{AutoCorrect | Correct}]" +
            "\n             [ /{AutoBackground | AutoBg}]" +
            "\n             [ /{HighDefinition | HD}]" +
            "\n             -{Output | Out} 输出路径" +
            "\n             *源文件" +
            "\n" +
            "\nNull.CharImg {}" +
            "\n" +
            "\n      其中参数的概述如下:" +
            "\n             字符占宽, 指图像中, 一个字符独占的宽度, 默认8" +
            "\n             字符占高, 字符独占的高度, 与字符占宽决定了字符的间距, 默认16" +
            "\n             最大宽度, 指输出图像中, 横向最多含多少个字符" +
            "\n             最大高度, 指输出图像中, 纵向最多含多少个字符" +
            "\n             字体大小, 指字体大小, 单位是像素, 默认16" +
            "\n             字符串内容, 要显示在图像中的字符, 将逐个显示" +
            "\n             颜色矫正程度, 指定了自动颜色矫正的程度, 值域是0~100, 默认100" +
            "\n             背景颜色, 表示图像背景颜色, 使用十六进制代码表示" +
            "\n             输出路径, 表示输出图像的保存路径" +
            "\n             字体, 表示图像中字符所使用的字体, 默认是Sans Serif" +
            "\n             FontBold, 粗体, 表示是否使用粗体" +
            "\n             Opposite, 使背景与前景颜色反转" +
            "\n             AutoCorrect, 自动矫正颜色" +
            "\n             AutoBackground, 自动判断背景色" +
            "\n             HighDefinition, 高分辨率模式" +
            "\n" +
            "\n      关于自动矫正颜色:" +
            "\n             自动矫正颜色旨在尝试调节前景色的色彩, 以做到前景与背景结合时, 颜色" +
            "\n             趋近于原图, 如果背景颜色指定了亮度较高的颜色, 例如白色, 这个功能会" +
            "\n             启到很好的作用, 如果你认为颜色矫正有些严重, 可以使用 CorrectRate" +
            "\n             参数来指定自动调节的力度, 尝试调节为80, 甚至更低的值来适应." +
            "\n" +
            "\n      关于自动判断背景色" +
            "\n             当启动该功能时, 用户设置的颜色作废, 程序会自动计算整个原图像的平均色" +
            "\n             作为输出图像的背景色" +
            "\n" +
            "\n      关于高分辨率模式:" +
            "\n             当启用高分辨率时, 图像中的一个字符不再单单是一种颜色, 而是使用彩色," +
            "\n             以使输出图像更能趋近于原图的形状与颜色, 同时Opposite也是受支持的" +
            "\n" +
            "\n      关于程序:" +
            "\n             作者: 诺尔, Null; 擅长:摸鱼" +
            "\n             Github仓库: https://github.com/SlimeNull/Null.CharImg";

        class StartupArgs
        {
            public string CHARWIDTH = "0";                 // 字宽
            public string CHARHEIGHT = "0";                // 字高
            public string MAXWIDTH = "0";                  // 图像最大宽 (压制选项
            public string MAXHEIGHT = "0";                 // 图像最大高
            public string FONTSIZE = "0";                  // 字体大小
            public string CORRECTRATE = "0";               // 颜色矫正程度

            public string CWIDTH = "0";
            public string CHEIGHT = "0";
            public string MWIDTH = "0";
            public string MHEIGHT = "0";
            public string FSIZE = "0";
            public string CRATE = "0";

            public string STRING = string.Empty;             // 字符串
            public string STR = string.Empty;
            public string BACKGROUNDCOLOR = string.Empty;    // 背景颜色
            public string BGCOLOR = string.Empty;

            public string OUTPUT = string.Empty;        // 输出
            public string OUT = string.Empty;

            public string FONTFAMILY = string.Empty;
            public string FONT = string.Empty;          // 字体

            public bool FONTBOLD = false;            // 粗体
            public bool BOLD = false;
            public bool OPPOSITE = false;            // 相反
            public bool OPPO = false;
            public bool AUTOCORRECT = false;         // 自动矫正颜色
            public bool CORRECT = false;
            public bool AUTOBACKGROUND = false;      // 自动判断背景颜色
            public bool AUTOBG = false;
            public bool HIGHDEFINITION = false;      // 高清晰度
            public bool HD = false;

            public bool HELP = false;
            public bool H = false;

            public List<string> Content;
            public List<string> Booleans;

            public ExecuteArgs ExecuteArgs;
            public void DeepParse(bool acceptHelp = true)
            {
                if (acceptHelp)
                    CheckHelp();

                ExecuteArgs = new ExecuteArgs();

                bool
                    argsIntCorrect = // 表示参数是否指定了正确的值
                    int.TryParse(CHARWIDTH, out int CharWidth) &
                    int.TryParse(CHARHEIGHT, out int CharHeight) &
                    int.TryParse(MAXWIDTH, out int MaxWidth) &
                    int.TryParse(MAXHEIGHT, out int MaxHeight) &
                    int.TryParse(FONTSIZE, out int FontSize) &
                    int.TryParse(CORRECTRATE, out int CorrectRate) &
                    int.TryParse(CWIDTH, out int CWidth) &
                    int.TryParse(CHEIGHT, out int CHeight) &
                    int.TryParse(MWIDTH, out int MWidth) &
                    int.TryParse(MHEIGHT, out int MHeight) &
                    int.TryParse(FSIZE, out int FSize) &
                    int.TryParse(CRATE, out int CRate),
                    FontBold = FONTBOLD || BOLD,
                    Opposite = OPPOSITE || OPPO,
                    AutoCorrect = AUTOCORRECT || CORRECT,
                    AutoBackground = AUTOBACKGROUND || AUTOBG,
                    HighDefinition = HIGHDEFINITION || HD,
                    CharWidthUndefined = CharWidth == 0 && CWidth == 0,
                    CharHeightUndefined = CharHeight == 0 && CHeight == 0,
                    NoCompress = MaxWidth == 0 && MWidth == 0 && MaxHeight == 0 && MHeight == 0,
                    FontSizeUndefined = FontSize == 0 && FSize == 0,
                    StringUndefined = STRING == string.Empty && STR == string.Empty,
                    OutputUndefined = OUTPUT == string.Empty && OUT == string.Empty,
                    FontFamilyUndefined = FONTFAMILY == string.Empty && FONT == string.Empty,
                    BackgroundColorUndefined = BACKGROUNDCOLOR == string.Empty && BGCOLOR == string.Empty;

                if (!argsIntCorrect)
                    ErrorExit(
                        "参数错误",
                        "参数指定的值不是合法的, 程序无法继续运行",
                        "正在分析参数",
                        "需要指定为数字的参数没有指定为正确的值",
                        "已经成功获取了参数",
                        "检查命令行参数",
                        -1);

                if (CharWidthUndefined)
                    CharWidth = 8;
                if (CharHeightUndefined)
                    CharHeight = 16;
                if (FontSizeUndefined)
                    FontSize = 16;
                if (StringUndefined)
                    STRING = "F";
                if (FontFamilyUndefined)
                    FONTFAMILY = FontFamily.GenericSansSerif.Name;
                if (BackgroundColorUndefined)
                    BACKGROUNDCOLOR = "#000000";

                if (FontBold)
                    ExecuteArgs.FontStyle = FontStyle.Bold;
                else
                    ExecuteArgs.FontStyle = FontStyle.Regular;

                if (OutputUndefined)
                    ErrorExit(
                        "参数错误",
                        "没有指定输出路径",
                        "分析参数",
                        "程序需要指定输出路径以保存图像",
                        "已经获取了参数",
                        "指定一个合适的输出路径",
                        -1);

                ExecuteArgs.CharWidth = CharWidth + CWidth;
                ExecuteArgs.CharHeight = CharHeight + CHeight;
                ExecuteArgs.MaxWidth = MaxWidth + MWidth;
                ExecuteArgs.MaxHeight = MaxHeight + MHeight;
                ExecuteArgs.FontSize = FontSize + FSize;
                ExecuteArgs.CorrectRate = CorrectRate + CRate;

                ExecuteArgs.FontFamily = new FontFamily(FONTFAMILY + FONT);

                ExecuteArgs.NoCompress = NoCompress;
                ExecuteArgs.Opposite = Opposite;
                ExecuteArgs.AutoCorrect = AutoCorrect;
                ExecuteArgs.AutoBackground = AutoBackground;
                ExecuteArgs.HighDefinition = HighDefinition;

                ExecuteArgs.String = STRING + STR;
                ExecuteArgs.Output = OUTPUT + OUT;
                ExecuteArgs.BackgroundColor = BACKGROUNDCOLOR + BGCOLOR;

                ExecuteArgs.Content = Content.ToArray();
                ExecuteArgs.Booleans = Booleans.ToArray();
            }

            private void CheckHelp()
            {
                if (HELP || H || Booleans.Contains("?"))
                {
                    Console.WriteLine(HelpText);
                    Environment.Exit(0);
                }    
            }
        }
        class ExecuteArgs
        {
            public int CharWidth;
            public int CharHeight;
            public int MaxWidth;
            public int MaxHeight;
            public int FontSize;             // 单位是像素
            public int CorrectRate;
            public FontStyle FontStyle;
            public FontFamily FontFamily;
            public string String;
            public string Output;
            public string BackgroundColor;

            public bool NoCompress;
            public bool Opposite;
            public bool AutoCorrect;
            public bool AutoBackground;
            public bool HighDefinition;

            public string[] Content;
            public string[] Booleans;
        }
        static ExecuteArgs Initialize(string[] args)
        {
            StartupArgs startupArgs = new ConsArgs(args).ToObject<StartupArgs>();
            startupArgs.DeepParse();
            return startupArgs.ExecuteArgs;
        }
        static int index = 0;
        static void ErrorExit(string errTitle, string what, string doing, string why, string state, string solution, int exitCode)
        {
            Console.WriteLine(
                $"Null.CharImg 异常: {errTitle}" +
                $"\n   * 异常详细描述:" +
                $"\n        {what}" +
                $"\n   * 正在执行的操作:" +
                $"\n        {doing}" +
                $"\n   * 导致原因:" +
                $"\n        {why}" +
                $"\n   * 当前状态:" +
                $"\n        {state}" +
                $"\n   * 解决方案:" +
                $"\n        {solution}" +
                $"\n" +
                $"执行 'Null.CharImg /Help' 可获取帮助信息");

            Environment.Exit(exitCode);
        }
        static void Main(string[] args)
        {
            ExecuteArgs exeArgs = Initialize(args);

            try
            {
                Image srcImg = null;
                Bitmap rstBmp = null;

                Size charSize = new Size(exeArgs.CharWidth, exeArgs.CharHeight), maxSize = new Size(exeArgs.MaxWidth, exeArgs.MaxHeight);
                Font drawFont = new Font(exeArgs.FontFamily, exeArgs.FontSize, exeArgs.FontStyle, GraphicsUnit.Pixel);
                Color bgColor = ColorTranslator.FromHtml(exeArgs.BackgroundColor);
                char[] chars = exeArgs.String.ToCharArray();

                if (exeArgs.Content.Length == 1)
                {
                    try
                    {
                        srcImg = Image.FromFile(exeArgs.Content[0]);
                    }
                    catch (OutOfMemoryException)
                    {
                        ErrorExit(
                            "内存不足",
                            $"当前系统无法提供足够的内存以打开图像, 图像路径是:\"{exeArgs.Content[0]}\"",
                            "打开源图像",
                            "可能是:图像太大, 或格式无法识别",
                            "参数已经分析完毕",
                            "指定一个合适大小的, 格式常见的图像文件",
                            -2);
                    }
                    catch (FileNotFoundException)
                    {
                        ErrorExit(
                            "文件找不到",
                            "无法找到指定的源图像, 图像路径是:\"{exeArgs.Content[0]}\"",
                            "打开源图像",
                            "指定了错误的路径",
                            "参数已经分析完毕",
                            "检查指定的路径是否正确",
                            -2);
                    }
                    catch (ArgumentException ae)
                    {
                        ErrorExit(
                            "内部错误 (参数异常)",
                            $"参数信息: {ae.Message}",
                            "打开源图像",
                            "路径错误",
                            "参数已经分析完毕",
                            "检查指定的路径, 联系作者",
                            -2);
                    }

                    CharImg.DrawOption option = new CharImg.DrawOption()
                    {
                        CharSize = charSize,
                        MaxSize = maxSize,
                        DrawFont = drawFont,
                        BackgroundColor = bgColor,
                        Chars = chars,
                        Compress = !exeArgs.NoCompress,
                        Opposite = exeArgs.Opposite,
                        AutoCorrect = exeArgs.AutoCorrect,
                        AutoBackground = exeArgs.AutoBackground,
                        HighDefinition = exeArgs.HighDefinition,
                        CorrectRate = exeArgs.CorrectRate
                    };

                    try
                    {
                        CharImg.GenCharImg(srcImg, out rstBmp, option);
                    }
                    catch(Exception e)
                    {
                        ErrorExit(
                            "无法生成图像",
                            $"此异常没有被详细捕获, 类型:{e.GetType()}, 信息:{e.Message}",
                            "生成混合图像",
                            "可能是:可能是由于图像太大或图像格式不受支持",
                            "已经打开了源图像",
                            "检查源图像是否正常",
                            -3);
                    }

                    rstBmp.Save(exeArgs.Output);

                    srcImg.Dispose();
                    rstBmp.Dispose();
                    GC.Collect();
                }
                else if (exeArgs.Content.Length > 1)
                {
                    if (!Directory.Exists(exeArgs.Output))
                        Directory.CreateDirectory(exeArgs.Output);

                    foreach (string i in exeArgs.Content)
                    {
                        srcImg = Image.FromFile(i);

                        string name = Path.GetFileNameWithoutExtension(i);
                        string ext = Path.GetExtension(i);

                        string thisOutputPath = Function.CombinePath(exeArgs.Output, $"{name}-NCHRIMG-{new Random().Next()}{ext}");

                        CharImg.DrawOption option = new CharImg.DrawOption()
                        {
                            CharSize = charSize,
                            MaxSize = maxSize,
                            DrawFont = drawFont,
                            BackgroundColor = bgColor,
                            Chars = chars,
                            Compress = !exeArgs.NoCompress,
                            Opposite = exeArgs.Opposite,
                            AutoCorrect = exeArgs.AutoCorrect,
                            AutoBackground = exeArgs.AutoBackground,
                            HighDefinition = exeArgs.HighDefinition,
                            CorrectRate = exeArgs.CorrectRate
                        };

                        CharImg.GenCharImg(srcImg, out rstBmp, option);

                        rstBmp.Save(thisOutputPath);

                        srcImg.Dispose();
                        rstBmp.Dispose();
                        GC.Collect();
                    }
                }
                else
                {
                    ErrorExit(
                        "参数错误",
                        "源图像参数指定未指定",
                        "准备执行混合",
                        "你需要指定至少一个源图像以进行混合",
                        "已经完成了参数分析",
                        "",
                        -1);
                }
            }
            catch(Exception e)
            {
                ErrorExit("不可预知的异常",
                    $"程序在不可预知的情况下出现异常, 类型:{e.GetType()}, 消息:{e.Message}",
                    "未知",
                    "未知",
                    "未知",
                    "向作者报告此问题",
                    int.MinValue);
            }
        }
    }
}
