﻿using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AirVRServerAudioOutputRouter : MonoBehaviour {
    private const int InvalidAudioRendererID = int.MaxValue;
    
    public enum Input {
        AudioListener,
        AudioPlugin
    }

    public enum Output {
        All,
        One
    }

    private AudioMixer _prevTargetAudioMixer;
    private int _prevAudioRendererID;

    public Input input;
    public Output output;
    public AudioMixer targetAudioMixer;
    public string exposedRendererIDParameterName;
    public AirVRCameraRig targetCameraRig;

    void OnEnable() {
        if (input == Input.AudioPlugin && targetAudioMixer == null) {
            Debug.LogWarning("[WARNING] Please specify an audio mixer which includes an AirVRServerAudioOutputPlugin when input is AudioPlugin.");
        }

        if (output == Output.One && targetCameraRig == null) {
            Debug.LogWarning("[WARNING] Please specify an AirVRCameraRig when output is One.");
        }

        float rendererID = 0.0f;
        if (targetAudioMixer != null && targetAudioMixer.GetFloat(exposedRendererIDParameterName, out rendererID) == false) {
            Debug.LogWarning("[WARNING] Please expose the RendererID parameter of AirVR Server Audio Output plugin.");
        }
    }

    void Update() {
        if (input == Input.AudioListener) {
            return;
        }

        int rendererID = InvalidAudioRendererID;
        AirVRServer.GetAudioRendererID(targetCameraRig, ref rendererID);
        if (_prevTargetAudioMixer != targetAudioMixer || _prevAudioRendererID != rendererID) {
            _prevTargetAudioMixer = targetAudioMixer;
            _prevAudioRendererID = rendererID;

            if (targetAudioMixer != null) {
                targetAudioMixer.SetFloat(exposedRendererIDParameterName, rendererID);
            }
        }
    }

    void OnDisable() {
        if (targetAudioMixer != null) {
            targetAudioMixer.SetFloat(exposedRendererIDParameterName, InvalidAudioRendererID);
        }
    }

    void OnAudioFilterRead(float[] data, int channels) {
        if (input == Input.AudioPlugin) {
            return;
        }

        if (output == Output.All) {
            AirVRServer.SendAudioFrameToAllCameraRigs(data, data.Length / channels, channels, AudioSettings.dspTime);
        }
        else if (targetCameraRig != null) {
            AirVRServer.SendAudioFrame(targetCameraRig, data, data.Length / channels, channels, AudioSettings.dspTime);
        }
    }

    internal void SetInputToAudioListener() {
        input = Input.AudioListener;
    }

    internal void SetInputToAudioPlugin(AudioMixer targetAudioMixer, string exposedRendererIDParameterName) {
        input = Input.AudioPlugin;
        this.targetAudioMixer = targetAudioMixer;
        this.exposedRendererIDParameterName = exposedRendererIDParameterName;
    }

    internal void SetOutputToAll() {
        output = Output.All;
        targetCameraRig = null;
    }

    internal void SetOutputToOne(AirVRCameraRig targetCameraRig) {
        output = Output.One;
        this.targetCameraRig = targetCameraRig;
    }
}
