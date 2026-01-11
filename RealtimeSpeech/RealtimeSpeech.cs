using Godot;
using System;
using Vosk;
using Newtonsoft.Json.Linq;

public partial class RealtimeSpeech : Node
{
    [Signal] public delegate void SpeechRecognizedTextEventHandler(string text);
    [Signal] public delegate void SpeechPartialTextEventHandler(string text);

    [Export] public string ModelPath = "res://RealtimeSpeech/models/en_us_small";

    private Model model;
    private VoskRecognizer recognizer;
    private AudioCapture audio;

    public override void _Ready()
    {
        VoskNativeLoader.Load();

        string modelDir = VoskModelLoader.ExtractModel(ModelPath);
        GD.Print("Vosk model directory: ", modelDir);

        model = new Model(modelDir);
        recognizer = new VoskRecognizer(model, 16000);

        audio = GetNode<AudioCapture>("AudioCapture");
        audio.OnChunkReady += ProcessChunk;
    }

    private void ProcessChunk(byte[] chunk)
    {
        if (recognizer.AcceptWaveform(chunk, chunk.Length))
        {
            string final = recognizer.FinalResult();
            JObject obj = JObject.Parse(final);
            string text = (string)obj["text"];

            if (!string.IsNullOrWhiteSpace(text))
                EmitSignal(SignalName.SpeechRecognizedText, text);
        }
        else
        {
            string partial = recognizer.PartialResult();
            JObject obj = JObject.Parse(partial);
            string text = (string)obj["partial"];

            if (!string.IsNullOrWhiteSpace(text))
                EmitSignal(SignalName.SpeechPartialText, text);
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            recognizer?.Dispose();
            model?.Dispose();
            GetTree().Quit();
        }
    }
}
