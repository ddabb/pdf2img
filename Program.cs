﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Processing;

namespace Pdf2Img
{
    class Program
    {
        // 定义输出图片格式枚举
        public enum OutputImageFormat
        {
            PNG,
            JPEG,
            BMP,
            TIFF
        }

        static int Main(string[] args)
        {
            try
            {
                // 设置编码以支持中文
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                // 创建命令行参数解析器
                var parser = new CommandLineParser(args);

                // 解析命令行参数
                if (parser.HasFlag("--help") || parser.HasFlag("-h"))
                {
                    ShowHelp();
                    return 0;
                }

                // 如果没有提供任何参数，自动处理当前目录下的所有PDF文件
                if (args.Length == 0)
                {
                    Console.WriteLine("未提供参数，将自动处理当前目录下的所有PDF文件...");
                    string currentDirectory = Directory.GetCurrentDirectory();
                    string[] pdfFiles = Directory.GetFiles(currentDirectory, "*.pdf");
                    
                    if (pdfFiles.Length == 0)
                    {
                        Console.WriteLine("当前目录下没有找到PDF文件。");
                        ShowHelp();
                        return 1;
                    }
                    
                    Console.WriteLine($"找到 {pdfFiles.Length} 个PDF文件，开始处理...");
                    int successCount = 0;
                    
                    foreach (string pdfFile in pdfFiles)
                    {
                        try
                        {
                            Console.WriteLine($"\n处理文件: {Path.GetFileName(pdfFile)}");
                            ConvertPdfToImages(pdfFile, currentDirectory, OutputImageFormat.PNG, 300, null);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"处理文件 {Path.GetFileName(pdfFile)} 时出错: {ex.Message}");
                        }
                    }
                    
                    Console.WriteLine($"\n处理完成，成功转换 {successCount}/{pdfFiles.Length} 个PDF文件。");
                    return successCount > 0 ? 0 : 1;
                }

                // 获取输入路径
                string inputPath = parser.GetOptionValue("-i") ?? parser.GetOptionValue("--input");
                if (string.IsNullOrEmpty(inputPath) && args.Length > 0 && !args[0].StartsWith("-"))
                {
                    // 如果没有使用-i参数，但提供了位置参数，则使用第一个位置参数作为输入文件
                    inputPath = args[0];
                }

                if (string.IsNullOrEmpty(inputPath))
                {
                    Console.WriteLine("错误: 未指定输入PDF文件。");
                    ShowHelp();
                    return 1;
                }

                // 获取输出路径
                string outputPath = parser.GetOptionValue("-o") ?? parser.GetOptionValue("--output");
                if (string.IsNullOrEmpty(outputPath))
                {
                    // 如果没有指定输出路径，则使用当前目录
                    outputPath = Directory.GetCurrentDirectory();
                }

                // 获取输出格式
                string formatStr = parser.GetOptionValue("-f") ?? parser.GetOptionValue("--format") ?? "png";
                OutputImageFormat format;
                switch (formatStr.ToLowerInvariant())
                {
                    case "jpg":
                    case "jpeg":
                        format = OutputImageFormat.JPEG;
                        break;
                    case "bmp":
                        format = OutputImageFormat.BMP;
                        break;
                    case "tiff":
                        format = OutputImageFormat.TIFF;
                        break;
                    case "png":
                    default:
                        format = OutputImageFormat.PNG;
                        break;
                }

                // 获取DPI设置
                int dpi = 300; // 默认DPI
                string dpiStr = parser.GetOptionValue("-d") ?? parser.GetOptionValue("--dpi");
                if (!string.IsNullOrEmpty(dpiStr) && int.TryParse(dpiStr, out int parsedDpi))
                {
                    dpi = parsedDpi;
                }

                // 获取页面范围
                string pageRangeStr = parser.GetOptionValue("-p") ?? parser.GetOptionValue("--pages");
                List<int> pageRange = null;
                if (!string.IsNullOrEmpty(pageRangeStr))
                {
                    pageRange = ParsePageRange(pageRangeStr);
                }

                // 执行转换
                ConvertPdfToImages(inputPath, outputPath, format, dpi, pageRange);
                return 0;
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.WriteLine($"错误: 找不到文件 - {ex.Message}");
                return 1;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.Error.WriteLine($"错误: 没有足够的权限访问文件 - {ex.Message}");
                return 1;
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"错误: 文件读写错误 - {ex.Message}");
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"错误: {ex.Message}");
                Console.Error.WriteLine($"详细信息: {ex.StackTrace}");
                return 1;
            }
        }

        /// <summary>
        /// 命令行参数解析器
        /// </summary>
        private class CommandLineParser
        {
            private readonly Dictionary<string, List<string>> _options = new Dictionary<string, List<string>>();
            private readonly HashSet<string> _flags = new HashSet<string>();

            public CommandLineParser(string[] args)
            {
                string currentOption = null;
                List<string> currentValues = null;

                foreach (var arg in args)
                {
                    if (arg.StartsWith("-"))
                    {
                        // 如果当前有选项，保存它
                        if (currentOption != null)
                        {
                            if (currentValues.Count == 0)
                            {
                                // 如果没有值，则视为标志
                                _flags.Add(currentOption);
                            }
                            else
                            {
                                _options[currentOption] = currentValues;
                            }
                        }

                        // 开始新的选项
                        currentOption = arg;
                        currentValues = new List<string>();
                    }
                    else if (currentOption != null)
                    {
                        // 添加值到当前选项
                        currentValues.Add(arg);
                    }
                }

                // 处理最后一个选项
                if (currentOption != null)
                {
                    if (currentValues.Count == 0)
                    {
                        _flags.Add(currentOption);
                    }
                    else
                    {
                        _options[currentOption] = currentValues;
                    }
                }
            }

            public bool HasFlag(string flag)
            {
                return _flags.Contains(flag);
            }

            public bool HasOption(string option)
            {
                return _options.ContainsKey(option);
            }

            public string GetOptionValue(string option)
            {
                if (_options.TryGetValue(option, out var values) && values.Count > 0)
                {
                    return values[0];
                }
                return null;
            }

            public List<string> GetOptionValues(string option)
            {
                if (_options.TryGetValue(option, out var values))
                {
                    return values;
                }
                return null;
            }
        }

        /// <summary>
        /// 显示帮助信息
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("用法: pdf2img [选项] <输入PDF文件>");
            Console.WriteLine();
            Console.WriteLine("选项:");
            Console.WriteLine("  -i, --input <path>       输入PDF文件路径");
            Console.WriteLine("  -o, --output <path>      输出图片目录路径 (默认: 当前目录)");
            Console.WriteLine("  -f, --format <format>    输出图片格式: png(默认), jpg, bmp, tiff");
            Console.WriteLine("  -d, --dpi <number>       输出图片DPI (默认: 300)");
            Console.WriteLine("  -p, --pages <range>      要转换的页面范围 (例如: 1-5,7,9-10)");
            Console.WriteLine("                          不指定则转换所有页面");
            Console.WriteLine("  -h, --help               显示帮助信息");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  pdf2img                           # 处理当前目录下的所有PDF文件");
            Console.WriteLine("  pdf2img input.pdf");
            Console.WriteLine("  pdf2img -i input.pdf -o C:\\Images\\");
            Console.WriteLine("  pdf2img -i input.pdf -f jpg -d 150");
            Console.WriteLine("  pdf2img -i input.pdf -p 1-3,5,7-9");
        }

        /// <summary>
        /// 解析页面范围字符串
        /// </summary>
        /// <param name="rangeStr">格式如 "1-5,7,9-10"</param>
        /// <returns>页面索引列表</returns>
        private static List<int> ParsePageRange(string rangeStr)
        {
            var result = new List<int>();
            var parts = rangeStr.Split(',');

            foreach (var part in parts)
            {
                if (part.Contains('-'))
                {
                    var range = part.Split('-');
                    if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                    {
                        // 页面索引从0开始，但用户输入从1开始
                        for (int i = start; i <= end; i++)
                        {
                            result.Add(i - 1);
                        }
                    }
                }
                else if (int.TryParse(part, out int page))
                {
                    // 页面索引从0开始，但用户输入从1开始
                    result.Add(page - 1);
                }
            }

            return result;
        }

        /// <summary>
        /// 将PDF转换为图片
        /// </summary>
        /// <param name="inputPath">输入PDF路径</param>
        /// <param name="outputPath">输出图片目录路径</param>
        /// <param name="format">输出图片格式</param>
        /// <param name="dpi">输出图片DPI</param>
        /// <param name="pageRange">要转换的页面索引列表，为null则转换所有页面</param>
        private static void ConvertPdfToImages(string inputPath, string outputPath, OutputImageFormat format, int dpi, List<int> pageRange)
        {
            // 验证输入文件存在
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"找不到输入PDF文件: {inputPath}");
            }

            // 获取输入文件名（不含扩展名）
            string inputFileName = Path.GetFileNameWithoutExtension(inputPath);
            
            // 创建专门的输出目录（如果有多页）
            string finalOutputPath;
            try
            {
                // 获取输入PDF文件所在的目录
                string inputDirectory = Path.GetDirectoryName(inputPath) ?? "";
                
                // 如果用户没有指定输出路径，则在输入文件的同级目录创建输出文件夹
                if (string.IsNullOrEmpty(outputPath) || outputPath == Directory.GetCurrentDirectory())
                {
                    // 创建以PDF文件名命名的子文件夹，与输入文件在同一目录下
                    finalOutputPath = Path.Combine(inputDirectory, inputFileName);
                }
                else
                {
                    // 使用用户指定的输出路径
                    finalOutputPath = Path.Combine(outputPath, inputFileName);
                }
                
                // 确保输出目录存在
                if (!Directory.Exists(finalOutputPath))
                {
                    Directory.CreateDirectory(finalOutputPath);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new IOException($"创建输出目录时出错: {ex.Message}", ex);
            }

            // 检查是否为Windows平台
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            // 创建PDF文档实例
            using (PdfDocument pdf = new PdfDocument())
            {
                try
                {
                    // 加载PDF文件
                    Console.WriteLine($"正在加载PDF文件: {inputPath}");
                    pdf.LoadFromFile(inputPath);

                    // 获取页面数量
                    int pageCount = pdf.Pages.Count;
                    Console.WriteLine($"PDF文件共有 {pageCount} 页");

                    // 确定要处理的页面
                    List<int> pagesToProcess;
                    if (pageRange != null && pageRange.Count > 0)
                    {
                        // 过滤掉超出范围的页面索引
                        pagesToProcess = pageRange.Where(p => p >= 0 && p < pageCount).ToList();
                        Console.WriteLine($"将处理指定的 {pagesToProcess.Count} 页");
                    }
                    else
                    {
                        // 处理所有页面
                        pagesToProcess = Enumerable.Range(0, pageCount).ToList();
                        Console.WriteLine($"将处理所有 {pageCount} 页");
                    }

                    // 转换每一页
                    int processedCount = 0;
                    int errorCount = 0;
                    foreach (int pageIndex in pagesToProcess)
                    {
                        processedCount++;
                        int pageNumber = pageIndex + 1; // 用于显示的页码（从1开始）
                        Console.Write($"\r正在处理第 {pageNumber}/{pageCount} 页 ({processedCount}/{pagesToProcess.Count})");

                        try
                        {
                            // 获取页面尺寸
                            var pdfPage = pdf.Pages[pageIndex];
                            float pageWidth = pdfPage.Size.Width;
                            float pageHeight = pdfPage.Size.Height;
                            
                            // 生成输出文件名
                            string extension;
                            IImageEncoder encoder;

                            switch (format)
                            {
                                case OutputImageFormat.JPEG:
                                    extension = ".jpg";
                                    encoder = new JpegEncoder { Quality = 90 };
                                    break;
                                case OutputImageFormat.BMP:
                                    extension = ".bmp";
                                    encoder = new BmpEncoder();
                                    break;
                                case OutputImageFormat.TIFF:
                                    extension = ".tiff";
                                    encoder = new TiffEncoder();
                                    break;
                                case OutputImageFormat.PNG:
                                default:
                                    extension = ".png";
                                    encoder = new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression };
                                    break;
                            }

                            string outputFileName = $"{inputFileName}_page{pageNumber:D3}{extension}";
                            string outputFilePath = Path.Combine(finalOutputPath, outputFileName);

                            if (isWindows)
                            {
                                // 在Windows平台上使用Spire.PDF的SaveAsImage方法
                                using (System.Drawing.Image image = pdf.SaveAsImage(pageIndex, PdfImageType.Bitmap, dpi, dpi))
                                {
                                    // 保存图片
                                    image.Save(outputFilePath, GetSystemDrawingImageFormat(format));
                                }
                            }
                            else
                            {
                                // 在非Windows平台上使用替代方法
                                // 注意：这里需要先将Spire.PDF生成的图像转换为内存流，然后用ImageSharp处理
                                using (var memoryStream = new MemoryStream())
                                {
                                    // 先保存到内存流
                                    using (System.Drawing.Image image = pdf.SaveAsImage(pageIndex, PdfImageType.Bitmap, dpi, dpi))
                                    {
                                        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                                    }

                                    // 重置内存流位置
                                    memoryStream.Position = 0;

                                    // 使用ImageSharp加载和保存图像
                                    using (var imageSharp = Image.Load(memoryStream))
                                    {
                                        // 保存图片
                                        imageSharp.Save(outputFilePath, encoder);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            Console.WriteLine($"\n处理第 {pageNumber} 页时出错: {ex.Message}");
                            if (errorCount >= 3)
                            {
                                Console.WriteLine("连续出现多个错误，请检查PDF文件是否损坏或权限是否正确。");
                                if (processedCount > 3)
                                {
                                    Console.WriteLine("已处理部分页面，将继续处理剩余页面...");
                                    continue;
                                }
                                else
                                {
                                    throw new Exception("PDF处理失败，请检查文件格式或权限。", ex);
                                }
                            }
                        }
                    }

                    Console.WriteLine("\n转换完成！");
                    Console.WriteLine($"图片已保存到: {finalOutputPath}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"处理PDF文件时出错: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 获取System.Drawing.Imaging.ImageFormat对象（仅用于Windows平台）
        /// </summary>
        private static System.Drawing.Imaging.ImageFormat GetSystemDrawingImageFormat(OutputImageFormat format)
        {
            switch (format)
            {
                case OutputImageFormat.JPEG:
                    return System.Drawing.Imaging.ImageFormat.Jpeg;
                case OutputImageFormat.BMP:
                    return System.Drawing.Imaging.ImageFormat.Bmp;
                case OutputImageFormat.TIFF:
                    return System.Drawing.Imaging.ImageFormat.Tiff;
                case OutputImageFormat.PNG:
                default:
                    return System.Drawing.Imaging.ImageFormat.Png;
            }
        }
    }
}