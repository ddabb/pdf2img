# PDF2IMG

一个简单易用的命令行工具，用于将PDF文件转换为图片。

## 功能特点

- 将PDF文件转换为多种格式的图片（PNG、JPEG、BMP、TIFF）
- 保持图片尺寸与PDF页面尺寸一致
- 支持指定输出图片的DPI
- 支持选择性转换特定页面
- 自动在PDF文件所在目录创建输出文件夹

## 安装

```bash
dotnet tool install --global Pdf2Img
```

## 使用方法

```bash
pdf2img [选项] <输入PDF文件>
```

### 选项

- `-i, --input <path>` - 输入PDF文件路径
- `-o, --output <path>` - 输出图片目录路径（默认：与输入文件同目录）
- `-f, --format <format>` - 输出图片格式：png（默认）、jpg、bmp、tiff
- `-d, --dpi <number>` - 输出图片DPI（默认：300）
- `-p, --pages <range>` - 要转换的页面范围（例如：1-5,7,9-10）
- `-h, --help` - 显示帮助信息

### 示例

```bash
# 处理当前目录下的所有PDF文件
pdf2img

# 转换整个PDF文件为PNG图片（默认格式）
pdf2img input.pdf

# 指定输入和输出路径
pdf2img -i C:\Documents\input.pdf -o D:\Images\

# 转换为JPEG格式，并设置DPI为150
pdf2img -i input.pdf -f jpg -d 150

# 只转换特定页面
pdf2img -i document.pdf -p 1-3,5,7-9
```

## 输出

程序会在指定的输出目录（或默认与输入文件同目录）下创建一个与PDF文件同名的文件夹，并将转换后的图片保存到该文件夹中。

例如，如果输入文件是`C:\Documents\report.pdf`，则输出图片将保存在`C:\Documents\report\`目录下。

## 系统要求

- .NET 6.0 或更高版本
- Windows 操作系统（由于使用了System.Drawing，主要支持Windows平台）

## 许可证

本项目使用MIT许可证。

### 第三方库许可证

本项目使用了以下第三方库：

- **SixLabors.ImageSharp** - 使用Apache 2.0许可证
- **PdfiumViewer** - 使用Apache 2.0许可证，基于Google的PDFium引擎，完全开源且无水印

请在使用本工具进行商业项目之前，确保您了解并遵守Spire.PDF的许可条款。
