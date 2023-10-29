using System;
using System.IO;
using System.Linq;
using System.Threading;
using NAudio.Wave;
using System.Reflection;
using Dalamud.Logging;

namespace FishersIntuition.Utils;

// TODO: http://mark-dot-net.blogspot.com/2014/02/fire-and-forget-audio-playback-with.html
internal class SoundEngine
{
    private readonly string[] _resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();

    public void Play(string path, float volume = 1f)
    {
        new Thread(() =>
        {
            var reader = GetWaveReader(path);
            if (reader == null)
                return;

            volume = Math.Max(0, Math.Min(volume, 1));

            using WaveChannel32 channel = new(reader);
            channel.Volume = volume;
            channel.PadWithZeroes = false;

            using (reader)
            {
                using var output = new DirectSoundOut(DirectSoundOut.DSDEVID_DefaultPlayback);
                output.Init(channel);
                output.Play();

                while (output.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(500);
                }
            }
        }).Start();
    }

    private WaveStream GetWaveReader(string path)
    {
        if (LoadFromPath(path, out var stream))
        {
            return stream;
        }

        return LoadFromResource(path, out stream) ? stream : null;
    }

    private bool LoadFromPath(string path, out WaveStream stream)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            stream = null;
            return false;
        }

        if (!File.Exists(path))
        {
            stream = null;
            return false;
        }

        try
        {
            stream = new MediaFoundationReader(path);
            PluginLog.Debug($"Loaded sound file from path \"{path}\"");
            return true;
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could load sound file from path {path}. {e.Message}");
        }

        stream = null;
        return false;
    }

    private bool LoadFromResource(string resource, out WaveStream stream)
    {
        if (!_resources.Contains(resource))
        {
            stream = null;
            return false;
        }

        var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
        stream = new WaveFileReader(resourceStream);
        return true;
    }
}