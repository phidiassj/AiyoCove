using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NAudio.Wave;
using Spectrogram;
using FftSharp;
using SkiaSharp;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AiyoCove.speech;

public class WhisperTranscriber
{
    private InferenceSession encoderSession;
    private InferenceSession decoderSession;

    // 從模型設定中取得的重要參數
    private const int DecoderStartTokenId = 50258;
    private const int EosTokenId = 50257;
    private const int MaxLength = 448;
    private const int NumMelBins = 128;

    Dictionary<string, int> VocabDic = new Dictionary<string, int>();

    public WhisperTranscriber(string encoderModelPath, string decoderModelPath)
    {
        // 建立 ONNX Runtime Session（指定 CPU 執行）
        string enPath = Path.Combine(Application.StartupPath, encoderModelPath);
        string dePath = Path.Combine(Application.StartupPath, decoderModelPath);
        SessionOptions sessionOptions = new();
        encoderSession = new InferenceSession(enPath, sessionOptions);
        decoderSession = new InferenceSession(dePath, sessionOptions);
        loadVocabJson();
    }

    /// <summary>
    /// 對 MemoryStream 中的 WAV 音訊進行語音轉文字
    /// </summary>
    /// <param name="wavStream">包含 WAV 音訊資料的 MemoryStream</param>
    /// <returns>轉錄後的文字</returns>
    public string TranscribeAudio(MemoryStream wavStream)
    {
        Debug.WriteLine($"wavStream length: {wavStream.Length}");

        // 1. 載入 WAV 音訊，取得正規化後的浮點數陣列 (單聲道, 16kHz)
        float[] audioData = LoadWav(wavStream);
        Debug.WriteLine($"audioData length: {audioData.Length}");

        // 2. 計算 Mel Spectrogram
        float[,] melSpectrogram = ComputeMelSpectrogram(audioData, NumMelBins);
        Debug.WriteLine($"melSpectrogram length: {melSpectrogram.Length}");

        // 3. 執行 Encoder 推論
        // 假設 melSpectrogram 的 shape 為 [num_mel_bins, time_steps]
        int timeSteps = melSpectrogram.GetLength(1);
        Debug.WriteLine($"timeSteps: {timeSteps}");

        // 建立 Tensor，shape 為 [1, num_mel_bins, time_steps]
        var encoderInput = new DenseTensor<float>(new[] { 1, NumMelBins, timeSteps });
        for (int i = 0; i < NumMelBins; i++)
        {
            for (int j = 0; j < timeSteps; j++)
            {
                encoderInput[0, i, j] = melSpectrogram[i, j];
            }
        }

        // 建立 encoder 輸入
        var encoderInputs = new List<NamedOnnxValue>
            {
                // 輸入名稱需根據模型調整
                //NamedOnnxValue.CreateFromTensor("input", encoderInput)
                NamedOnnxValue.CreateFromTensor("input_features", encoderInput)
            };

        // 執行 encoder 推論
        using var encoderResults = encoderSession.Run(encoderInputs);
        // 取得 encoder 的輸出 (名稱 "encoder_out" 為示意，請根據實際情況調整)
        //var encoderOutputTensor = encoderResults.First(x => x.Name == "encoder_out").AsTensor<float>();
        var encoderOutputTensor = encoderResults.First(x => x.Name == "last_hidden_state").AsTensor<float>();
        Debug.WriteLine($"last_hidden_state length: {encoderOutputTensor.Count()}");


        // 4. 使用 Decoder 進行自動回歸解碼
        // 初始化 decoder 輸入 token 為 decoder_start_token_id
        List<int> tokenIds = new List<int> { DecoderStartTokenId };

        // 迭代進行解碼
        for (int step = 0; step < MaxLength; step++)
        {
            // 建立 decoder 輸入 token 的 Tensor，形狀為 [1, 當前 token 序列長度]
            var tokenTensor = new DenseTensor<long>(new[] { 1, tokenIds.Count });
            for (int i = 0; i < tokenIds.Count; i++)
            {
                tokenTensor[0, i] = tokenIds[i];
            }

            var useCacheBranchTensor = new DenseTensor<bool>(new[] { 1 }, true); // 或 true，根據需要設定

            // 準備 decoder 的輸入：包括當前 token 序列與 encoder 隱藏狀態
            var decoderInputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", tokenTensor),
                    NamedOnnxValue.CreateFromTensor("encoder_hidden_states", encoderOutputTensor),
                    NamedOnnxValue.CreateFromTensor("use_cache_branch", useCacheBranchTensor),
                };

            // 執行 decoder 推論
            using var decoderResults = decoderSession.Run(decoderInputs);
            // 取得 decoder 的 logits 輸出 (名稱 "logits" 為示意)
            var logits = decoderResults.First(x => x.Name == "logits").AsTensor<float>();

            // logits 的 shape 假設為 [1, sequence_length, vocab_size]
            int vocabSize = logits.Dimensions[2];
            int currentLength = logits.Dimensions[1];

            // 取得最後一個 token 的 logits 分數
            var lastLogits = new float[vocabSize];
            for (int i = 0; i < vocabSize; i++)
            {
                lastLogits[i] = logits[0, currentLength - 1, i];
            }

            // 使用簡單的 argmax 選取機率最高的 token
            int nextTokenId = ArgMax(lastLogits);

            tokenIds.Add(nextTokenId);

            // 若遇到結束 token 則停止解碼
            if (nextTokenId == EosTokenId)
                break;
        }

        // 5. 將 token 序列轉換為文字
        string transcript = ConvertTokensToText(tokenIds);
        return transcript;
    }

    /// <summary>
    /// 利用 NAudio 讀取 WAV 檔案，並轉換成浮點數陣列 (單聲道，16kHz，數值範圍 -1.0 ~ 1.0)
    /// </summary>
    private float[] LoadWav(MemoryStream stream)
    {
        WaveFileReader reader = new WaveFileReader(stream);
        // 若非單聲道 16000，轉成單聲道 16000
        if (reader.WaveFormat.Channels != 1 || reader.WaveFormat.SampleRate != 16000)
        {
            using var pcmStream = WaveFormatConversionStream.CreatePcmStream(reader);
            using var resampler = new MediaFoundationResampler(pcmStream, new WaveFormat(16000, 16, 1))
            {
                ResamplerQuality = 60
            };
            using MemoryStream waveStream = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(waveStream, resampler);
            waveStream.Position = 0;
            stream = new MemoryStream(waveStream.ToArray());
            stream.Position = 0;
            reader = new WaveFileReader(stream);
        }

        return ReadAllSamples(reader.ToSampleProvider());
    }

    private float[] ReadAllSamples(ISampleProvider sampleProvider)
    {
        List<float> samples = new List<float>();
        float[] buffer = new float[1024];
        int read;
        while ((read = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
        {
            samples.AddRange(buffer.Take(read));
        }
        // 若音訊取樣率非 16000Hz，可利用 NAudio 進行重採樣 (此處範例假設已為 16kHz)
        return samples.ToArray();
    }

    private float[,] ComputeMelSpectrogram(float[] audioData, int numMelBins)
    {
        // 1. 將 float[] 轉換為 double[]
        double[] audioDataDouble = audioData.Select(x => (double)x).ToArray();

        // 2. 初始化 SpectrogramGenerator
        int sampleRate = 16000; // 假設取樣率為 16kHz
        int n_fft = 512;
        int hop_length = 315; // 160;
        //int fftSize = 400; // 可調整
        //int stepSize = 160; // 可調整
        //int maxFreq = 8000; // 可調整，根據模型需求調整
        var sg = new SpectrogramGenerator(sampleRate, fftSize: n_fft, stepSize: hop_length);
        sg.Add(audioDataDouble);

        // 3. 產生 Mel Spectrogram Bitmap
        SkiaSharp.SKBitmap bmp = sg.GetBitmapMel(melBinCount: numMelBins); // melSizePoints 就是你的 numMelBins
        Console.WriteLine($"Bitmap Width: {bmp.Width}, Height: {bmp.Height}");

        // 4. 將 Bitmap 轉換為 float[,] 並進行對數轉換
        int timeSteps = bmp.Width;
        float[,] melSpectrogram = new float[numMelBins, timeSteps];
        int expectedTimeSteps = 3000;
        float[,] paddedMelSpectrogram = new float[numMelBins, expectedTimeSteps];

        for (int i = 0; i < numMelBins; i++)
        {
            //if (i>bmp.Height) break;
            for (int j = 0; j < timeSteps; j++)
            {
                //if (j > bmp.Width) continue;
                SKColor color = bmp.GetPixel(j, i);
                // 假設顏色亮度代表能量值，並且已經經過某種程度的縮放
                double amplitude = color.Red / 255.0; // 歸一化到 0-1 範圍

                // 對數轉換 (加上一個小的常數避免 log(0))
                paddedMelSpectrogram[i, j] = (float)Math.Log10(amplitude * 10000 + 1e-10); // 縮放和偏移可以根據需要調整
            }
        }

        return paddedMelSpectrogram;
    }


    // 輔助函數：讀取 WAV 檔案 (使用 NAudio)
    (double[] audio, int sampleRate) ReadMono(string filePath, double multiplier = 1.0)
    {
        using var afr = new AudioFileReader(filePath);
        int sampleRate = afr.WaveFormat.SampleRate;
        int bytesPerSample = afr.WaveFormat.BitsPerSample / 8;
        int sampleCount = (int)(afr.Length / bytesPerSample);
        int channelCount = afr.WaveFormat.Channels;
        var audio = new List<double>(sampleCount);
        var buffer = new float[sampleRate * channelCount];
        int samplesRead = 0;
        while ((samplesRead = afr.Read(buffer, 0, buffer.Length)) > 0)
            audio.AddRange(buffer.Take(samplesRead).Select(x => x * multiplier));
        return (audio.ToArray(), sampleRate);
    }

    ///// <summary>
    ///// 計算 Mel Spectrogram
    ///// ※此範例僅為示意，實際應依照 Whisper 模型前處理流程進行 STFT、對數壓縮與 Mel filter bank 計算
    ///// </summary>
    //private float[,] ComputeMelSpectrogram(float[] audioData, int numMelBins)
    //{
    //    // 假設 timeSteps 為 300 (實際值依前處理而定)
    //    int timeSteps = 3000;
    //    float[,] melSpectrogram = new float[numMelBins, timeSteps];

    //    // 填入 dummy 數值 (實際上需計算真實的 Mel Spectrogram)
    //    for (int i = 0; i < numMelBins; i++)
    //    {
    //        for (int j = 0; j < timeSteps; j++)
    //        {
    //            melSpectrogram[i, j] = 0.0f;
    //        }
    //    }
    //    return melSpectrogram;
    //}

    /// <summary>
    /// 簡單的 ArgMax 實作，回傳陣列中最大值的索引
    /// </summary>
    private int ArgMax(float[] array)
    {
        int maxIndex = 0;
        float maxVal = array[0];
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] > maxVal)
            {
                maxVal = array[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    /// <summary>
    /// 將 token id 序列轉換成文字
    /// ※此處僅示範簡單對照，實際上應根據完整詞彙表 (token id -> 字符) 進行轉換
    /// </summary>
    private string ConvertTokensToText(List<int> tokenIds)
    {
        // 簡單範例：僅定義特殊 token 的對應，其他 token 以 <id> 表示
        //Dictionary<int, string> vocab = new Dictionary<int, string>
        //    {
        //        {50257, ""}, // EOS token
        //        {50258, ""}  // 開始 token (不輸出)
        //        // 其他 token 的對應需從完整詞彙表中取得
        //    };

        //List<string> tokens = new List<string>();
        //foreach (var id in tokenIds)
        //{
        //    if (vocab.ContainsKey(id))
        //    {
        //        tokens.Add(vocab[id]);
        //    }
        //    else
        //    {
        //        tokens.Add($"<{id}>");
        //    }
        //}
        //return string.Join("", tokens);

        int matchCnt = 0;

        List<string> tokens = new List<string>();
        foreach (int id in tokenIds)
        {
            var rec = VocabDic.FirstOrDefault(x => x.Value == id);
            if (rec.Key != null)
            {
                tokens.Add(rec.Key);
                matchCnt++;
                continue;
            }
        }
        Debug.WriteLine($"Total: {tokens.Count}, Match: {matchCnt}");
        return string.Join("", tokens);

    }

    private void loadVocabJson()
    {
        string vocabPath = Path.Combine(Application.StartupPath, "json", "vocab.json");
        if (!File.Exists(vocabPath)) throw new Exception(@"Missing important file [.\json\vocab.json].");

        string vocabCont = File.ReadAllText(vocabPath, System.Text.Encoding.UTF8);
        if (string.IsNullOrWhiteSpace(vocabCont)) throw new Exception("vocab.json is empty.");

        var ret = JsonConvert.DeserializeObject<Dictionary<string, int>>(vocabCont);
        if (ret != null) VocabDic = ret;
    }

    

}
