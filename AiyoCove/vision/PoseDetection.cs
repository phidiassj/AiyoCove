using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using helper;
using System.Diagnostics;

namespace vision;

public class PoseDetection : IDisposable
{
    public EventHandler<string>? PoseDetectMessage;

    public InferenceSession? hrnetSession;
    public InferenceSession? yolo11Session;
    public InferenceSession? movenetSession;
    private bool disposedValue;

    public async Task StartDetectAsync(string ImagePath, int ModelIndex, string ModelPath)
    {
        if (string.IsNullOrWhiteSpace(ImagePath) || !Directory.Exists(ImagePath))
        {
            if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, "ImagePath not exists.");
            return;
        }
        if (ModelIndex < 0 || ModelIndex > 2)
        {
            if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, "ModelIndex must between 0 and 2.");
            return;
        }

        DirectoryInfo imageDirectory = new(ImagePath);
        List<FileInfo> processingImages = imageDirectory.GetFiles().ToList().Where(x => 
                                                        x.Extension == ".jpg" ||
                                                        x.Extension == ".jpeg" ||
                                                        x.Extension == ".png" ||
                                                        x.Extension == ".bmp"
                                                        ).ToList();
        if (processingImages.Count <= 0)
        {
            if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, "Can not find any image to process.");
            return;
        }

        if (ModelIndex == 0)
        {
            if (hrnetSession == null)
            {
                if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, "Start loading model...");
                Stopwatch stopwatch = Stopwatch.StartNew();
                SessionOptions options = new();
                options.RegisterOrtExtensions();
                options.AppendExecutionProvider_CPU();
                options.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_INFO;
                hrnetSession = new InferenceSession(ModelPath, options);
                if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, $"Model loaded... {stopwatch.ElapsedMilliseconds} ms");
            }
            await detectByHRNetAsync(processingImages);
        }
        else if (ModelIndex == 1)
        {
            if (movenetSession == null)
            {
                if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, "Start loading model...");
                Stopwatch stopwatch = Stopwatch.StartNew();
                SessionOptions options = new();
                options.RegisterOrtExtensions();
                options.AppendExecutionProvider_CPU();
                options.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_INFO;
                movenetSession = new InferenceSession(ModelPath, options);
                if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, $"Model loaded... {stopwatch.ElapsedMilliseconds} ms");
            }
            await detectByMovenetAsync(processingImages);
        }
        else if (ModelIndex == 2)
        {
            if (yolo11Session == null)
            {
                if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, "Start loading model...");
                Stopwatch stopwatch = Stopwatch.StartNew();
                SessionOptions options = new();
                options.RegisterOrtExtensions();
                options.AppendExecutionProvider_CPU();
                options.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_INFO;
                yolo11Session = new InferenceSession(ModelPath, options);
                if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, $"Model loaded... {stopwatch.ElapsedMilliseconds} ms");
            }
            await detectByYolo11Async(processingImages);
        }
        else
        {
            if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, "Unsupported model.");
        }



    }

    private async Task detectByYolo11Async(List<FileInfo> processingImages)
    {
        if (yolo11Session == null)
        {
            if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, $"Load model first!");
            return;
        }

        int modelInputWidth = 640;
        int modelInputHeight = 640;

        Stopwatch stopwatch = Stopwatch.StartNew();
        var predictions = await Task.Run(() =>
        {
            List<(int idx, Yolo11Detection keypoints)> outputData = new();

            foreach (FileInfo imageFile in processingImages)
            {
                using Bitmap curImage = new Bitmap(imageFile.FullName);

                Tensor<float> input = new DenseTensor<float>([1, 3, modelInputWidth, modelInputHeight]);
                input = ImageHelper.PreprocessBitmapWithStdDev(curImage, input);

                var inputMetadataName = yolo11Session.InputNames[0];

                var onnxInputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor(inputMetadataName, input)
                };

                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = yolo11Session.Run(onnxInputs);
                var heatmaps = results[0].AsTensor<float>();
                float[] heatmapsAry = heatmaps.ToArray();

                float maxConf = -1F;
                Yolo11Detection? maxDetection = null;
                for (int detectCnt = 0; detectCnt < 8400; detectCnt++)
                {
                    int startIdx = detectCnt * 56;
                    if (heatmaps[0, 4, detectCnt] > maxConf)
                    {
                        maxConf = heatmaps[0, 4, detectCnt];
                        maxDetection = new()
                        {
                            X1 = heatmaps[0, 0, detectCnt],
                            Y1 = heatmaps[0, 1, detectCnt],
                            X2 = heatmaps[0, 2, detectCnt],
                            Y2 = heatmaps[0, 3, detectCnt],
                            Confidence = heatmaps[0, 4, detectCnt],
                            Keypoints = new float[51]
                        };
                        for(int cnt = 5; cnt < 56; cnt++)
                        {
                            maxDetection.Value.Keypoints[cnt - 5] = heatmaps[0, cnt, detectCnt];
                        }
                    }
                }

                if (maxDetection != null)
                {
                    outputData.Add(new(outputData.Count, maxDetection.Value));
                }
                else
                {
                    outputData.Add(new(outputData.Count, new Yolo11Detection() { Confidence = -1 }));
                }
            }
            return outputData;
        });

        stopwatch.Stop();
        var timeUse = stopwatch.ElapsedMilliseconds;

        if (PoseDetectMessage != null && predictions != null)
        {
            PoseDetectMessage.Invoke(this, $"Using {timeUse} ms for {predictions.Count} images.");
            foreach (var prediction in predictions)
            {
                if (prediction.keypoints.Confidence < 0)
                {
                    PoseDetectMessage.Invoke(this, $"Image {prediction.idx} out of confidence");
                    continue;
                }

                PoseDetectMessage.Invoke(this, $"Image {prediction.idx}, ({prediction.keypoints.X1},{prediction.keypoints.Y1}) to ({prediction.keypoints.X2},{prediction.keypoints.Y2}), avg confidence: {prediction.keypoints.Confidence}");
                for(var pointCnt = 0; pointCnt < 17; pointCnt++)
                {
                    PoseDetectMessage.Invoke(this, $"X={prediction.keypoints.Keypoints[pointCnt * 3]}, Y={prediction.keypoints.Keypoints[(pointCnt * 3) + 1]}, confidence={prediction.keypoints.Keypoints[(pointCnt * 3) + 2]}");
                    pointCnt++;
                }
            }
        }

    }

    private async Task detectByMovenetAsync(List<FileInfo> processingImages)
    {
        if (movenetSession == null)
        {
            if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, $"Load model first!");
            return;
        }

        int modelInputWidth = 256;
        int modelInputHeight = 256;

        Stopwatch stopwatch = Stopwatch.StartNew();
        var predictions = await Task.Run(() =>
        {
            List<(string? name, int idx, List<(float X, float Y, float C)> keypoints)> outputData = new();

            foreach (FileInfo imageFile in processingImages)
            {
                using Bitmap curImage = new Bitmap(imageFile.FullName);

                Tensor<int> input = new DenseTensor<int>([1, modelInputWidth, modelInputHeight, 3]);
                input = ImageHelper.PreprocessBitmapWithStdDevForMovenet(curImage, input);

                var inputMetadataName = movenetSession.InputNames[0];

                var onnxInputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor(inputMetadataName, input)
                };

                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = movenetSession.Run(onnxInputs);
                var heatmaps = results[0].AsTensor<float>();

                var outputName = movenetSession.OutputNames[0];
                var outputDimensions = movenetSession.OutputMetadata[outputName].Dimensions;

                List<(float X, float Y, float C)> keyPoints = new();
                for(int keyPointCnt = 0; keyPointCnt < 17; keyPointCnt++)
                {
                    keyPoints.Add(new(
                        heatmaps[0, 0, keyPointCnt, 0], 
                        heatmaps[0, 0, keyPointCnt, 1], 
                        heatmaps[0, 0, keyPointCnt, 2]));
                }

                outputData.Add(new(outputName, outputData.Count, keyPoints));
            }

            return outputData;
        });

        stopwatch.Stop();
        var timeUse = stopwatch.ElapsedMilliseconds;

        if (PoseDetectMessage != null && predictions != null)
        {
            PoseDetectMessage.Invoke(this, $"Using {timeUse} ms for {predictions.Count} images.");
            foreach (var prediction in predictions)
            {
                PoseDetectMessage.Invoke(this, $"Image {prediction.idx}, Name [{prediction.name}]");
                for (var pointCnt = 0; pointCnt < 17; pointCnt++)
                {
                    PoseDetectMessage.Invoke(this, $"Point={pointCnt}, X={(int)(prediction.keypoints[pointCnt].X * 256)}, Y={(int)(prediction.keypoints[pointCnt].Y * 256)}, confidence={prediction.keypoints[pointCnt].C}");
                }
            }
        }


    }

    private async Task detectByHRNetAsync(List<FileInfo> processingImages)
    {
        if (hrnetSession == null)
        {
            if (PoseDetectMessage != null) PoseDetectMessage.Invoke(this, $"Load model first!");
            return;
        }

        int modelInputWidth = 256;
        int modelInputHeight = 192;

        Stopwatch stopwatch = Stopwatch.StartNew();
        var predictions = await Task.Run(() =>
        {
            List<(string? name, int idx, List<(float X, float Y)> keypoints)> outputData = new();

            foreach (FileInfo imageFile in processingImages)
            {
                using Bitmap curImage = new Bitmap(imageFile.FullName);

                Tensor<float> input = new DenseTensor<float>([1, 3, modelInputWidth, modelInputHeight]);
                input = ImageHelper.PreprocessBitmapWithStdDev(curImage, input);

                var inputMetadataName = hrnetSession.InputNames[0];

                var onnxInputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor(inputMetadataName, input)
                };

                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = hrnetSession.Run(onnxInputs);
                var heatmaps = results[0].AsTensor<float>();

                var outputName = hrnetSession.OutputNames[0];
                var outputDimensions = hrnetSession.OutputMetadata[outputName].Dimensions;

                float outputWidth = outputDimensions[2];
                float outputHeight = outputDimensions[3];

                List<(float X, float Y)> keypointCoordinates = ImageHelper.PostProcessResults(heatmaps, modelInputWidth, modelInputHeight, outputWidth, outputHeight);

                outputData.Add(new(outputName, outputData.Count, keypointCoordinates));
            }

            return outputData;
        });

        stopwatch.Stop();
        var timeUse = stopwatch.ElapsedMilliseconds;

        if (PoseDetectMessage != null && predictions != null)
        {
            PoseDetectMessage.Invoke(this, $"Using {timeUse} ms for {predictions.Count} images.");
            foreach (var prediction in predictions)
            {
                PoseDetectMessage.Invoke(this, $"Image {prediction.idx}, Name [{prediction.name}]");
                int pointCnt = 0;
                foreach(var keypoint in prediction.keypoints)
                {
                    PoseDetectMessage.Invoke(this, $"[{pointCnt}] {keypoint.X} {keypoint.Y}");
                    pointCnt++;
                }
            }
        }


    }

    public void FreeSession(int ModelIndex)
    {
        if (ModelIndex == 0 && hrnetSession != null)
        {
            hrnetSession = null;
        }
        else if (ModelIndex == 1 && movenetSession != null)
        {
            movenetSession = null;
        }
        else if (ModelIndex == 2 && yolo11Session != null)
        {
            yolo11Session = null;
        }
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: 處置受控狀態 (受控物件)
                try
                {
                    if (hrnetSession != null) hrnetSession.Dispose();
                    if (yolo11Session != null) yolo11Session.Dispose();
                    if (movenetSession != null) movenetSession.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
            // TODO: 將大型欄位設為 Null
            disposedValue = true;
        }
    }

    // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
    // ~PoseDetection()
    // {
    //     // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public struct Yolo11Detection
{
    public float X1 { get; set; }
    public float Y1 { get; set; }
    public float X2 { get; set; }
    public float Y2 { get; set; }
    public float Confidence { get; set; }
    public float[] Keypoints { get; set; } // 17組關鍵點，每組包含x,y,confidence
}