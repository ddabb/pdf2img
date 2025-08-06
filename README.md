# PDF2IMG 工具

PDF2IMG 是一个命令行工具，用于将PDF文件转换为图片格式（PNG、JPEG、BMP或TIFF）。

## 功能特点

- 将PDF文件转换为多种图片格式（PNG、JPEG、BMP、TIFF）
- 默认使用PDF页面的原始尺寸（1点=1像素）
- 可选择指定DPI值进行缩放
- 支持选择特定页面范围进行转换
- 自动创建输出目录
- 支持批量处理当前目录下的所有PDF文件

## 安装

```bash
dotnet tool install --global Pdf2Img
```

## 使用方法

```
用法: pdf2img [选项] <输入PDF文件>

选项:
  -i, --input <path>       输入PDF文件路径
  -o, --output <path>      输出图片目录路径 (默认: 当前目录)
  -f, --format <format>    输出图片格式: png(默认), jpg, bmp, tiff
  -d, --dpi <number>       输出图片DPI
                          不指定DPI时，默认使用PDF页面的原始尺寸
                          较低的DPI值(如72-150)生成较小的文件，适合屏幕显示
                          较高的DPI值(如300-600)生成较大的文件，适合打印
  -p, --pages <range>      要转换的页面范围 (例如: 1-5,7,9-10)
                          不指定则转换所有页面
  -h, --help               显示帮助信息
```

## 示例

```bash
# 处理当前目录下的所有PDF文件
pdf2img

# 使用原始尺寸转换指定PDF文件
pdf2img input.pdf

# 指定输出目录
pdf2img -i input.pdf -o C:\Images\

# 使用指定DPI值转换为JPG格式
pdf2img -i input.pdf -f jpg -d 150

# 只转换特定页面
pdf2img -i input.pdf -p 1-3,5,7-9
```

## 输出

转换后的图片将保存在以PDF文件名命名的子目录中，每个页面生成一个单独的图片文件，文件名格式为：`{PDF文件名}_page{页码}.{扩展名}`。

## 系统要求

- .NET 6.0 或更高版本