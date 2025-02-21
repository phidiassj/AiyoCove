using Microsoft.ML.OnnxRuntime.Tensors;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace helper;

public class ImageHelper
{
    private static readonly float[] Mean = [0.485f, 0.456f, 0.406f];
    private static readonly float[] StdDev = [0.229f, 0.224f, 0.225f];

    public static Tensor<float> PreprocessBitmapWithStdDev(Bitmap bitmap, Tensor<float> input)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;

        BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

        int stride = bmpData.Stride;
        IntPtr ptr = bmpData.Scan0;
        int bytes = Math.Abs(stride) * height;
        byte[] rgbValues = new byte[bytes];

        Marshal.Copy(ptr, rgbValues, 0, bytes);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * stride + x * 3;
                byte blue = rgbValues[index];
                byte green = rgbValues[index + 1];
                byte red = rgbValues[index + 2];

                input[0, 0, y, x] = ((red / 255f) - Mean[0]) / StdDev[0];
                input[0, 1, y, x] = ((green / 255f) - Mean[1]) / StdDev[1];
                input[0, 2, y, x] = ((blue / 255f) - Mean[2]) / StdDev[2];
            }
        }

        bitmap.UnlockBits(bmpData);

        return input;
    }

    public static Tensor<int> PreprocessBitmapWithStdDevForMovenet(Bitmap bitmap, Tensor<int> input)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;

        BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

        int stride = bmpData.Stride;
        IntPtr ptr = bmpData.Scan0;
        int bytes = Math.Abs(stride) * height;
        byte[] rgbValues = new byte[bytes];

        Marshal.Copy(ptr, rgbValues, 0, bytes);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * stride + x * 3;
                int blue = rgbValues[index];
                int green = rgbValues[index + 1];
                int red = rgbValues[index + 2];

                input[0, y, x, 0] = red;
                input[0, y, x, 1] = green;
                input[0, y, x, 2] = blue;
            }
        }

        bitmap.UnlockBits(bmpData);

        return input;
    }

    public static List<(float X, float Y)> PostProcessResults(Tensor<float> heatmaps, float originalWidth, float originalHeight, float outputWidth, float outputHeight)
    {
        List<(float X, float Y)> keypointCoordinates = [];

        // Scaling factors from heatmap (64x48) directly to original image size
        float scale_x = originalWidth / outputWidth;
        float scale_y = originalHeight / outputHeight;

        int numKeypoints = heatmaps.Dimensions[1];
        int heatmapWidth = heatmaps.Dimensions[2];
        int heatmapHeight = heatmaps.Dimensions[3];

        for (int i = 0; i < numKeypoints; i++)
        {
            float maxVal = float.MinValue;
            int maxX = 0, maxY = 0;

            for (int x = 0; x < heatmapWidth; x++)
            {
                for (int y = 0; y < heatmapHeight; y++)
                {
                    float value = heatmaps[0, i, y, x];
                    if (value > maxVal)
                    {
                        maxVal = value;
                        maxX = x;
                        maxY = y;
                    }
                }
            }

            float scaledX = maxX * scale_x;
            float scaledY = maxY * scale_y;

            keypointCoordinates.Add((scaledX, scaledY));
        }

        return keypointCoordinates;
    }

}
