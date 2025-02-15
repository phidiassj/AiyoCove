using AiyoCove.speech;
using NAudio.Wave;
using System.ComponentModel;
using System.Diagnostics;

namespace AiyoCove
{
    public partial class frmHome : Form
    {
        WhisperWrapper? _wrapper;

        public frmHome()
        {
            InitializeComponent();
            // 模型預設存放到程式啟動路徑，請視需要調整
            string modelPath = Path.Combine(Application.StartupPath, "whisper_medium_int8_cpu_ort_1.18.0.onnx");
            Task.Run(async() =>
            {
                _wrapper = await WhisperWrapper.CreateAsync(modelPath);
            });
        }

        private async void btnBrowse_Click(object sender, EventArgs e)
        {
            if (_wrapper == null)
            {
                MessageBox.Show("模型尚未載入完成...請稍候...");
                return;
            }

            Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")}...Process started.");

            DialogResult result = wavFileDialog.ShowDialog();
            if (result != DialogResult.OK || string.IsNullOrWhiteSpace(wavFileDialog.FileName)) return;

            txtWavFile.Text = wavFileDialog.FileName;
            Application.DoEvents();

            byte[] wavBytes = prepareSpeechData(wavFileDialog.FileName);
            Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")}...Audio read/convert completed.");

            txtResult.Text = await _wrapper.TranscribeAsync(wavBytes, "Mandarin", WhisperWrapper.TaskType.Transcribe, false);
            Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")}...Process completed.");
        }

        private byte[] prepareSpeechData(string audioPath)
        {
            // NAudio 的 MediaFoundationReader 可以接受 MP3 等多種音源格式
            using MediaFoundationReader reader = new MediaFoundationReader(audioPath);

            // Whisper 只接受最長 30 秒，16bits、16000KHz、單聲道的 PCM 音源轉譯
            WaveFormat targetFormat = new(16000, 16, 1);
            int bytesLimited = targetFormat.AverageBytesPerSecond * 30;

            // 儲存處理後音源的串流
            MemoryStream audioStream = new();

            // 檢查音源格式
            if (reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm ||
                reader.WaveFormat.Channels != 1 ||
                reader.WaveFormat.SampleRate != 16000 ||
                reader.WaveFormat.BitsPerSample != 16)
            {
                // 如果格式不符先重新採樣
                using var pcmStream = WaveFormatConversionStream.CreatePcmStream(reader);
                using var resampler = new MediaFoundationResampler(pcmStream, targetFormat)
                {
                    ResamplerQuality = 60
                };
                WaveFileWriter.WriteWavFileToStream(audioStream, resampler);
            }
            else
            {
                WaveFileWriter.WriteWavFileToStream(audioStream, reader);
            }

            if (audioStream.Length > bytesLimited)
            {
                return audioStream.ToArray()[0..bytesLimited];
            }
            else
            {
                return audioStream.ToArray();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_wrapper != null) _wrapper.Dispose();
        }
    }
}
