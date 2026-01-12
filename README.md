# Realtime Speech Recognition for Godot (C#)

Offline, real-time speech recognition for Godot (C# / .NET) using the Vosk
speech recognition engine.

Cloning this repository should be enough to get you started.

---

## Features

- Real-time microphone speech recognition
- Offline recognition (no internet required)
- Partial (live) and final recognition results
- Godot 4.x C# support
- Easy integration into existing projects

---

## Getting Started

### Clone Project

This project is ready to go to get started. Simply clone the project:

```bash
git clone https://github.com/paragon-xr/Realtime-Speech-Godot-C-Sharp
```

1. Open the project in Godot 4.x (Mono / .NET)

2. Ensure your microphone is available

3. Run the project

The repository already includes:

- A working RealtimeSpeech node

- Audio capture setup

- A bundled Vosk English speech model

---

### Included Speech Model

This project includes the Vosk English (US) small model:

Model: ```vosk-model-small-en-us-0.15```

Official models page:
https://alphacephei.com/vosk/models

You may replace this model with another Vosk model if desired.
Larger models provide better accuracy but require more CPU and memory.

---

### Adding This to an Existing Godot C# Project
1. Copy the following folder into your project:

```res://RealtimeSpeech/```

2. Add Required NuGet Packages

```xml
<ItemGroup>
  <PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
  <PackageReference Include="Vosk" Version="0.3.38" />
</ItemGroup>
```

3. Export Settings

Vosk relies on native libraries and model files that must be included in exported
builds.

 - In Godot → Export → Resources → Filters to export non-resource files/folders, add the following paths:

```res://RealtimeSpeech/vosklibs/**, res://RealtimeSpeech/models/**```

 - For Android, check the box "Record Audio" in the Options tab


Without this step, speech recognition will not work in exported builds.

4. Project Settings → Audio → Driver → Enable Input is check on

5. (Optional) The RealtimeSpeech folder contains an audio bus layout for the Godot's microphone to work. If you are using beyond a default audio mix, it may be worth checking out that layout.tres file as well as the AudioStreamPlayer (MicrophoneStream) child of the RealtimeSpeech scene.

---

## Usage Example

Inside RealtimeSpeech folder is a RealtimeSpeech scene. It contains everything needed to drop in including a MicrophoneStream node. Simple drop this scene as a child of an existing scene.

The RealtimeSpeech node emits two signals that can be accessed via code or the editor:

- SpeechPartialText(string text) — partial (live) recognition

- SpeechRecognizedText(string text) — final recognition result

Example:

```
public override void _Ready()
{
    var speech = GetNode<RealtimeSpeech>("RealtimeSpeech");

    speech.SpeechPartialText += OnPartialSpeech;
    speech.SpeechRecognizedText += OnFinalSpeech;
}

private void OnPartialSpeech(string text)
{
    GD.Print("Partial: ", text);
}

private void OnFinalSpeech(string text)
{
    GD.Print("Final: ", text);
}
```

## Supported Platforms
Currently Supported

- Windows

- Linux

- macOS*

- Android arm64, including VR

_*There is a known Godot 4.5 bug with mic problems for macOS. It has been fixed with v4.6 but should work on versions before 4.5 too._

Planned Support

- iOS

- Multithreaded recognition

- Performance improvements

### Notes

- Recognition currently runs on the main thread

- Partial results may be noisy by nature

- All recognition is performed locally (offline)

## License

This project is released under a permissive license.

You are free to use, modify, and include this project in personal or commercial
projects at your own risk.

Attribution is not required, but credit is appreciated.