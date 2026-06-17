using NAudio.Wave;

namespace Ezvcc.App.Audio;

public sealed class AudioEngine : IDisposable
{
    private const int LevelReportIntervalMs = 33;

    private WasapiCapture? _capture;
    private WasapiOut? _output;
    private BufferedWaveProvider? _buffer;

    private float _peakSinceLastReport;
    private int _lastReportTick;

    public event EventHandler<AudioLevelEventArgs>? LevelUpdated;

    public bool IsRunning { get; private set; }

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        _capture = new WasapiCapture();
        _buffer = new BufferedWaveProvider(_capture.WaveFormat)
        {
            DiscardOnBufferOverflow = true,
            BufferDuration = TimeSpan.FromMilliseconds(200),
        };
        _capture.DataAvailable += OnDataAvailable;

        _output = new WasapiOut();
        _output.Init(_buffer);
        _output.Play();
        _capture.StartRecording();

        _peakSinceLastReport = 0f;
        _lastReportTick = Environment.TickCount;
        IsRunning = true;
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }

        if (_capture != null)
        {
            _capture.DataAvailable -= OnDataAvailable;
            _capture.StopRecording();
            _capture.Dispose();
            _capture = null;
        }

        if (_output != null)
        {
            _output.Stop();
            _output.Dispose();
            _output = null;
        }

        _buffer = null;
        IsRunning = false;
        RaiseLevel(0f);
    }

    public void Dispose()
    {
        Stop();
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        _buffer?.AddSamples(e.Buffer, 0, e.BytesRecorded);

        if (_capture == null || e.BytesRecorded == 0)
        {
            return;
        }

        float peak = ComputePeak(e.Buffer, e.BytesRecorded, _capture.WaveFormat);
        if (peak > _peakSinceLastReport)
        {
            _peakSinceLastReport = peak;
        }

        int now = Environment.TickCount;
        if (now - _lastReportTick >= LevelReportIntervalMs)
        {
            RaiseLevel(_peakSinceLastReport);
            _peakSinceLastReport = 0f;
            _lastReportTick = now;
        }
    }

    private void RaiseLevel(float peak)
    {
        LevelUpdated?.Invoke(this, new AudioLevelEventArgs(peak));
    }

    private static float ComputePeak(byte[] buffer, int bytesRecorded, WaveFormat format)
    {
        float peak = 0f;

        if (format.Encoding == WaveFormatEncoding.IeeeFloat && format.BitsPerSample == 32)
        {
            for (int i = 0; i + 4 <= bytesRecorded; i += 4)
            {
                float sample = BitConverter.ToSingle(buffer, i);
                float abs = Math.Abs(sample);
                if (abs > peak)
                {
                    peak = abs;
                }
            }
        }
        else if (format.Encoding == WaveFormatEncoding.Pcm && format.BitsPerSample == 16)
        {
            for (int i = 0; i + 2 <= bytesRecorded; i += 2)
            {
                short sample = BitConverter.ToInt16(buffer, i);
                float abs = Math.Abs(sample / 32768f);
                if (abs > peak)
                {
                    peak = abs;
                }
            }
        }

        if (peak > 1f)
        {
            peak = 1f;
        }

        return peak;
    }
}
