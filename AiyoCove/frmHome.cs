using AiyoCove.speech;
using NAudio.Wave;
using System.ComponentModel;
using System.Diagnostics;
using vision;

namespace AiyoCove
{
    public partial class frmHome : Form
    {

        public frmHome()
        {
            InitializeComponent();
            poseDetection.PoseDetectMessage += PoseDetectMessageHandler;
        }

        #region "Whisper"

        WhisperWrapper? _wrapper;

        private async void btnLoad_Click(object sender, EventArgs e)
        {
            //whisper = new("encoder_model.onnx", "decoder_model_merged.onnx");
            _wrapper = await WhisperWrapper.CreateAsync(@"E:\Intel\Speech\Olive\examples\whisper\models\whisper_cpu_int8\output_model\whisper_medium_int8_cpu_ort_1.18.0.onnx");
        }

        private async void btnBrowse_Click(object sender, EventArgs e)
        {
            if (_wrapper == null)
            {
                MessageBox.Show("Load first");
                return;
            }

            Debug.WriteLine($"[{DateTime.Now.ToString("mm:ss")}]Process started.");

            OpenFileDialog dialog = new OpenFileDialog();
            DialogResult result = dialog.ShowDialog();
            if (result != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName)) return;

            List<byte[]> preparedData = prepareSpeechData(wavFileDialog.FileName);
            Debug.WriteLine($"[{DateTime.Now.ToString("mm:ss")}]Audio read/convert completed.");

            string transcribedText = string.Empty;
            foreach (byte[] data in preparedData)
            {
                transcribedText += await _wrapper.TranscribeAsync(data, "Mandarin", WhisperWrapper.TaskType.Transcribe, false);
            }
            txtResult.Text = transcribedText;
            Debug.WriteLine($"[{DateTime.Now.ToString("mm:ss")}]Process completed.");

        }

        private List<byte[]> prepareSpeechData(string audioPath)
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

            List<byte[]> splitedData = new();
            if (audioStream.Length > bytesLimited)
            {
                byte[] buf = new byte[bytesLimited];
                int readCnt = 0;
                audioStream.Position = 0;
                while ((readCnt = audioStream.Read(buf, 0, buf.Length)) > 0)
                {
                    splitedData.Add(buf[0..readCnt]);
                }
            }
            else
            {
                splitedData.Add(audioStream.ToArray());
            }

            return splitedData;
        }

        #endregion


        #region "PoseDetection"

        PoseDetection poseDetection = new();

        private void PoseDetectMessageHandler(object? sender, string message)
        {
            this.Invoke(() => txtPoseDetectMessages.Text += message + "\r\n");
        }

        private void btnPoseImagesPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult folderRet = folderBrowserDialog.ShowDialog();
            if (folderRet != DialogResult.OK) return;
            txtPoseImagesPath.Text = folderBrowserDialog.SelectedPath;
        }

        private async void btnPoseDDetectStart_Click(object sender, EventArgs e)
        {
            if (cbPoseModel.SelectedIndex < 0 || string.IsNullOrWhiteSpace(txtPoseDetectModelPath.Text))
            {
                MessageBox.Show("Setting your model.");
                return;
            }
            txtPoseDetectMessages.Text = string.Empty;
            await poseDetection.StartDetectAsync(txtPoseImagesPath.Text, cbPoseModel.SelectedIndex, txtPoseDetectModelPath.Text);
        }

        private void btPoseDetectModelPathBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            DialogResult result = dialog.ShowDialog();
            if (result != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName)) return;

            if (dialog.FileName != txtPoseDetectModelPath.Text)
            {
                poseDetection.FreeSession(cbPoseModel.SelectedIndex);
            }

            txtPoseDetectModelPath.Text = dialog.FileName;
        }

        #endregion


        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_wrapper != null) _wrapper.Dispose();
            poseDetection.Dispose();
            
        }
    }
}
