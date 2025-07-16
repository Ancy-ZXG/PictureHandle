using ImageMagick;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicHandle
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path1 = @textBox1.Text;
            string path2 = @textBox2.Text;
            if (string.IsNullOrWhiteSpace(path1) || string.IsNullOrWhiteSpace(path2))
            {
                MessageBox.Show("请输入路径后再进行镜像复制");
                return;
            }
            if (!Directory.Exists(path1))
            {
                MessageBox.Show("请输入原始图片路径后再进行镜像复制");
                return;
            }
            if (!Directory.Exists(path2))
            {
                Directory.CreateDirectory(path2);
            }
            this.Enabled = false;
            string[] extensions = { "*.bmp", "*.tiff"};
            foreach (string ext in extensions)
            {
                string[] files = Directory.GetFiles(path1, ext);
                foreach (string file in files)
                {
                    // 加载原始图像
                    string fileName = Path.GetFileName(file);
                    string inputImagePath = file;
                    string outputImagePath = Path.Combine(path2, fileName);
                    // 使用Bitmap类加载图像
                    using (Bitmap originalImage = new Bitmap(inputImagePath))
                    {
                        if (ext == "*.bmp")
                        {
                            Bitmap bmp = MirrorImage(originalImage);
                            bmp.Save(outputImagePath, ImageFormat.Bmp);
                            //SaveAs8BitBMP(originalImage, outputImagePath);
                        }
                        else if (ext == "*.tiff")
                        {
                            FlipTiff_Magick_FrameworkCompatible(inputImagePath, outputImagePath);
                            //Bitmap bmp = MirrorImage16(originalImage);
                            //bmp.Save(outputImagePath, ImageFormat.Tiff);
                            //SaveAs8BitTiff(originalImage, outputImagePath);
                        }
                    }
                    //// 使用Bitmap类加载图像
                    //using (Bitmap originalImage = new Bitmap(inputImagePath))
                    //{
                    //    // 创建新的图像以进行水平镜像处理
                    //    using (Bitmap mirroredImage = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format8bppIndexed))
                    //    {
                    //        // 创建一个Graphics对象，用于在新的图像上绘制
                    //        using (Graphics g = Graphics.FromImage(mirroredImage))
                    //        {
                    //            // 设置图像翻转
                    //            g.DrawImage(originalImage, new Rectangle(0, 0, originalImage.Width, originalImage.Height),
                    //                new Rectangle(originalImage.Width - 1, 0, -originalImage.Width, originalImage.Height),
                    //                GraphicsUnit.Pixel);
                    //        }

                    //        // 保存处理后的图像
                    //        mirroredImage.Save(outputImagePath);
                    //    }
                    //}
            }

            }
            MessageBox.Show("复制成功");
            this.Enabled = true;
        }
        private void SaveAs8BitBMP(Bitmap originalImage, string outputPath)
        {
            // 创建一个新的与原图相同大小的位图
            Bitmap mirroredImage = new Bitmap(originalImage.Width, originalImage.Height,PixelFormat.Format8bppIndexed);

            // 获取原图的调色板
            ColorPalette palette = originalImage.Palette;
            mirroredImage.Palette = palette;

            // 镜像操作：从右到左、从上到下进行像素拷贝
            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    // 获取原图像素
                    Color pixelColor = originalImage.GetPixel(x, y);

                    // 设置镜像位置的像素
                    mirroredImage.SetPixel(originalImage.Width - 1 - x, y, pixelColor);
                }
            }
            mirroredImage.Save(outputPath, ImageFormat.Bmp);
        }
        private void SaveAs8BitTiff(Bitmap originalImage, string outputPath)
        {
            // 创建一个新的与原图相同大小的位图
            Bitmap mirroredImage = new Bitmap(originalImage.Width, originalImage.Height);

            // 获取原图的调色板
            ColorPalette palette = originalImage.Palette;
            mirroredImage.Palette = palette;

            // 镜像操作：从右到左、从上到下进行像素拷贝
            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    // 获取原图像素
                    Color pixelColor = originalImage.GetPixel(x, y);

                    // 设置镜像位置的像素
                    mirroredImage.SetPixel(originalImage.Width - 1 - x, y, pixelColor);
                }
            }

            mirroredImage.Save(outputPath, ImageFormat.Tiff);
        }
        // 水平镜像拷贝操作
        private Bitmap MirrorImage(Bitmap originalImage)
        {
            // 创建一个与原图像大小相同的8位BMP图像
            Bitmap mirroredImage = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format8bppIndexed);

            // 获取原图像的调色板
            ColorPalette palette = originalImage.Palette;
            mirroredImage.Palette = palette;

            // 获取原图像的像素数据
            BitmapData originalData = originalImage.LockBits(new Rectangle(0, 0, originalImage.Width, originalImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData mirroredData = mirroredImage.LockBits(new Rectangle(0, 0, mirroredImage.Width, mirroredImage.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            // 获取像素数据的字节数组
            int width = originalImage.Width;
            int height = originalImage.Height;
            int bytesPerPixel = 1; // 8位图像每个像素占1字节
            int stride = originalData.Stride;  // 每行的字节数

            // 创建字节数组来存储原图和镜像后的像素数据
            byte[] originalPixels = new byte[originalData.Height * stride];
            byte[] mirroredPixels = new byte[mirroredData.Height * stride];

            // 从原图中读取像素数据
            System.Runtime.InteropServices.Marshal.Copy(originalData.Scan0, originalPixels, 0, originalPixels.Length);

            // 水平镜像拷贝（交换每行像素的左右位置）
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 获取原图中像素的索引
                    byte pixelValue = originalPixels[y * stride + x];

                    // 设置镜像后的像素（左右镜像）
                    mirroredPixels[y * stride + (width - 1 - x)] = pixelValue;
                }
            }

            // 将镜像后的像素数据写回到新图像中
            System.Runtime.InteropServices.Marshal.Copy(mirroredPixels, 0, mirroredData.Scan0, mirroredPixels.Length);

            // 解锁图像数据
            originalImage.UnlockBits(originalData);
            mirroredImage.UnlockBits(mirroredData);

            return mirroredImage;
        }
        // 水平镜像拷贝操作
        private Bitmap MirrorImage16(Bitmap originalImage)
        {
            // 创建一个与原图像大小相同的16位TIFF图像
            Bitmap mirroredImage = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format16bppGrayScale);

            // 获取原图像的像素数据
            BitmapData originalData = originalImage.LockBits(new Rectangle(0, 0, originalImage.Width, originalImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppGrayScale);
            BitmapData mirroredData = mirroredImage.LockBits(new Rectangle(0, 0, mirroredImage.Width, mirroredImage.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppGrayScale);

            // 获取像素数据的字节数组
            int width = originalImage.Width;
            int height = originalImage.Height;
            int bytesPerPixel = 2; // 16位图像每个像素占2字节
            int stride = originalData.Stride;  // 每行的字节数

            // 创建字节数组来存储原图和镜像后的像素数据
            byte[] originalPixels = new byte[stride * height];
            byte[] mirroredPixels = new byte[stride * height];

            // 从原图中读取像素数据
            System.Runtime.InteropServices.Marshal.Copy(originalData.Scan0, originalPixels, 0, originalPixels.Length);

            // 水平镜像拷贝（交换每行像素的左右位置）
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 获取原图中像素的16位值（每个像素占2字节）
                    byte byte1 = originalPixels[y * stride + x * bytesPerPixel];
                    byte byte2 = originalPixels[y * stride + x * bytesPerPixel + 1];

                    // 设置镜像后的像素（左右镜像）
                    mirroredPixels[y * stride + (width - 1 - x) * bytesPerPixel] = byte1;
                    mirroredPixels[y * stride + (width - 1 - x) * bytesPerPixel + 1] = byte2;
                }
            }

            // 将镜像后的像素数据写回到新图像中
            System.Runtime.InteropServices.Marshal.Copy(mirroredPixels, 0, mirroredData.Scan0, mirroredPixels.Length);

            // 解锁图像数据
            originalImage.UnlockBits(originalData);
            mirroredImage.UnlockBits(mirroredData);

            return mirroredImage;
        }

        void FlipTiff_Magick_FrameworkCompatible(string srcPath, string dstPath)
        {
            // ① 显式读取为 16-bit TIFF
            var readSettings = new MagickReadSettings
            {
                Format = MagickFormat.Tiff,
                Depth = 16
            };

            using (var image = new MagickImage(srcPath, readSettings))
            {
                // ② 水平翻转
                image.Flop();

                // ③ 设置写出格式和压缩方式
                image.Format = MagickFormat.Tiff;
                image.Depth = 16;

                // 设置压缩方式：可选值 "None", "LZW", "Zip", "Group4", ...
                image.Settings.SetDefine(MagickFormat.Tiff, "compression", "None");

                // ④ 写出文件
                image.Write(dstPath);
            }
        }
    }
}
