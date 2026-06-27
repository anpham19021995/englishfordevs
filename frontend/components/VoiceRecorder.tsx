"use client";

import { Mic, Square } from "lucide-react";
import { useEffect, useRef, useState } from "react";

type VoiceRecorderProps = {
  disabled: boolean;
  isTranscribing: boolean;
  onAudioReady: (audio: Blob) => void;
  onError: (message: string) => void;
};

type AudioContextConstructor = typeof AudioContext;

const maxRecordingSeconds = 60;
const outputSampleRate = 16000;

export function VoiceRecorder({
  disabled,
  isTranscribing,
  onAudioReady,
  onError,
}: VoiceRecorderProps) {
  const [isRecording, setIsRecording] = useState(false);
  const [recordingSeconds, setRecordingSeconds] = useState(0);
  const audioContextRef = useRef<AudioContext | null>(null);
  const processorRef = useRef<ScriptProcessorNode | null>(null);
  const sourceRef = useRef<MediaStreamAudioSourceNode | null>(null);
  const streamRef = useRef<MediaStream | null>(null);
  const chunksRef = useRef<Float32Array[]>([]);
  const sampleRateRef = useRef(outputSampleRate);

  useEffect(() => {
    if (!isRecording) {
      return;
    }

    const timer = window.setInterval(() => {
      setRecordingSeconds((seconds) => {
        if (seconds + 1 >= maxRecordingSeconds) {
          void stopRecording();
          return maxRecordingSeconds;
        }

        return seconds + 1;
      });
    }, 1000);

    return () => window.clearInterval(timer);
  }, [isRecording]);

  useEffect(() => () => cleanup(), []);

  async function startRecording() {
    if (!navigator.mediaDevices?.getUserMedia) {
      onError("Voice recording is not supported in this browser.");
      return;
    }

    try {
      chunksRef.current = [];
      setRecordingSeconds(0);

      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      const AudioContextClass =
        window.AudioContext ??
        ((window as unknown as { webkitAudioContext?: AudioContextConstructor })
          .webkitAudioContext);

      if (!AudioContextClass) {
        throw new Error("Audio recording is not supported in this browser.");
      }

      const audioContext = new AudioContextClass();
      const source = audioContext.createMediaStreamSource(stream);
      const processor = audioContext.createScriptProcessor(4096, 1, 1);

      sampleRateRef.current = audioContext.sampleRate;
      processor.onaudioprocess = (event) => {
        chunksRef.current.push(new Float32Array(event.inputBuffer.getChannelData(0)));
      };

      source.connect(processor);
      processor.connect(audioContext.destination);

      streamRef.current = stream;
      audioContextRef.current = audioContext;
      sourceRef.current = source;
      processorRef.current = processor;
      setIsRecording(true);
    } catch (error) {
      cleanup();
      onError(error instanceof Error ? error.message : "Could not start recording.");
    }
  }

  async function stopRecording() {
    if (!isRecording) {
      return;
    }

    setIsRecording(false);

    const inputSampleRate = sampleRateRef.current;
    const chunks = chunksRef.current;
    cleanup();

    if (chunks.length === 0) {
      onError("No audio was recorded.");
      return;
    }

    const audio = mergeChunks(chunks);
    const resampled = downsample(audio, inputSampleRate, outputSampleRate);
    onAudioReady(encodeWav(resampled, outputSampleRate));
  }

  function cleanup() {
    processorRef.current?.disconnect();
    sourceRef.current?.disconnect();
    streamRef.current?.getTracks().forEach((track) => track.stop());
    void audioContextRef.current?.close();

    processorRef.current = null;
    sourceRef.current = null;
    streamRef.current = null;
    audioContextRef.current = null;
  }

  return (
    <button
      className={`secondary-button voice-button${isRecording ? " is-recording" : ""}`}
      type="button"
      disabled={disabled || isTranscribing}
      onClick={() => {
        if (isRecording) {
          void stopRecording();
        } else {
          void startRecording();
        }
      }}
    >
      {isRecording ? (
        <Square size={16} aria-hidden="true" />
      ) : (
        <Mic size={16} aria-hidden="true" />
      )}
      {isTranscribing
        ? "Transcribing"
        : isRecording
          ? `Stop ${formatSeconds(recordingSeconds)}`
          : "Record"}
    </button>
  );
}

function mergeChunks(chunks: Float32Array[]) {
  const length = chunks.reduce((total, chunk) => total + chunk.length, 0);
  const merged = new Float32Array(length);
  let offset = 0;

  for (const chunk of chunks) {
    merged.set(chunk, offset);
    offset += chunk.length;
  }

  return merged;
}

function downsample(input: Float32Array, inputSampleRate: number, targetSampleRate: number) {
  if (inputSampleRate === targetSampleRate) {
    return input;
  }

  const ratio = inputSampleRate / targetSampleRate;
  const outputLength = Math.round(input.length / ratio);
  const output = new Float32Array(outputLength);

  for (let i = 0; i < outputLength; i++) {
    const start = Math.floor(i * ratio);
    const end = Math.min(Math.floor((i + 1) * ratio), input.length);
    let sum = 0;

    for (let j = start; j < end; j++) {
      sum += input[j];
    }

    output[i] = sum / Math.max(1, end - start);
  }

  return output;
}

function encodeWav(samples: Float32Array, sampleRate: number) {
  const buffer = new ArrayBuffer(44 + samples.length * 2);
  const view = new DataView(buffer);

  writeString(view, 0, "RIFF");
  view.setUint32(4, 36 + samples.length * 2, true);
  writeString(view, 8, "WAVE");
  writeString(view, 12, "fmt ");
  view.setUint32(16, 16, true);
  view.setUint16(20, 1, true);
  view.setUint16(22, 1, true);
  view.setUint32(24, sampleRate, true);
  view.setUint32(28, sampleRate * 2, true);
  view.setUint16(32, 2, true);
  view.setUint16(34, 16, true);
  writeString(view, 36, "data");
  view.setUint32(40, samples.length * 2, true);

  let offset = 44;

  for (const sample of samples) {
    const clamped = Math.max(-1, Math.min(1, sample));
    view.setInt16(offset, clamped < 0 ? clamped * 0x8000 : clamped * 0x7fff, true);
    offset += 2;
  }

  return new Blob([view], { type: "audio/wav" });
}

function writeString(view: DataView, offset: number, value: string) {
  for (let i = 0; i < value.length; i++) {
    view.setUint8(offset + i, value.charCodeAt(i));
  }
}

function formatSeconds(seconds: number) {
  return `0:${seconds.toString().padStart(2, "0")}`;
}
