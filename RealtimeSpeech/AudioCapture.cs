using Godot;
using System;
using System.Collections.Generic;

public partial class AudioCapture : Node
{
    public event Action<byte[]> OnChunkReady;

    private AudioEffectRecord recorder;
    private int lastSamplePos = 0;
    private int sampleRate;
    private List<byte> buffer = new List<byte>();

    public override void _Ready()
    {
        int busIndex = AudioServer.GetBusIndex("Record");
        recorder = AudioServer.GetBusEffect(busIndex, 0) as AudioEffectRecord;
        recorder.SetRecordingActive(true);

        sampleRate = (int)AudioServer.GetMixRate();
        GD.Print("AudioCapture mix rate: ", sampleRate);
    }

    public override void _Process(double delta)
    {
        if (recorder == null || !recorder.IsRecordingActive())
            return;

        var sample = recorder.GetRecording();
        if (sample == null || sample.Data.Length == 0)
            return;

        int totalSamples = sample.Data.Length / (sample.Stereo ? 4 : 2);
        int start = lastSamplePos * (sample.Stereo ? 4 : 2);
        if (start >= sample.Data.Length)
            return;

        int len = sample.Data.Length - start;
        byte[] newAudio = new byte[len];
        Array.Copy(sample.Data, start, newAudio, 0, len);

        byte[] mono = sample.Stereo ? MixStereoToMono(newAudio) : newAudio;
        byte[] resampled = ResampleTo16kHz(mono, sampleRate);

        buffer.AddRange(resampled);

        if (buffer.Count >= 3200)
        {
            byte[] chunk = buffer.ToArray();
            buffer.Clear();
            OnChunkReady?.Invoke(chunk);
        }

        lastSamplePos = totalSamples;
    }

    private byte[] MixStereoToMono(byte[] input)
    {
        byte[] output = new byte[input.Length / 2];
        int idx = 0;

        for (int i = 0; i + 3 < input.Length; i += 4)
        {
            short left = BitConverter.ToInt16(input, i);
            short right = BitConverter.ToInt16(input, i + 2);
            short mixed = (short)((left + right) / 2);

            byte[] b = BitConverter.GetBytes(mixed);
            output[idx++] = b[0];
            output[idx++] = b[1];
        }

        return output;
    }

    private byte[] ResampleTo16kHz(byte[] input, int rate)
    {
        if (rate == 16000)
            return input;

        int bytesPerSample = 2;
        int inputSamples = input.Length / bytesPerSample;

        double ratio = 16000.0 / rate;
        int outputSamples = (int)(inputSamples * ratio);

        byte[] output = new byte[outputSamples * bytesPerSample];

        for (int i = 0; i < outputSamples; i++)
        {
            double srcIndex = i / ratio;
            int i0 = (int)srcIndex;
            int i1 = Math.Min(i0 + 1, inputSamples - 1);
            double t = srcIndex - i0;

            short s0 = BitConverter.ToInt16(input, i0 * bytesPerSample);
            short s1 = BitConverter.ToInt16(input, i1 * bytesPerSample);

            short s = (short)(s0 + (s1 - s0) * t);

            byte[] b = BitConverter.GetBytes(s);
            int outOffset = i * bytesPerSample;
            output[outOffset] = b[0];
            output[outOffset + 1] = b[1];
        }

        return output;
    }
}
