﻿using System;
using System.Collections.Generic;

// Track is the container of all patterns in a musical track. In anticipation of
// format updates, each format version is a derived class of TrackBase.
//
// Because class names are not serialized, we can change class names
// however we want without breaking old files, so the current version
// class will always be called "Track", and deprecated versions will be
// called "TrackVersion1" or such.

[Serializable]
public class TrackBase
{
    public string version;

    private string Serialize()
    {
        return UnityEngine.JsonUtility.ToJson(this, prettyPrint: true);
    }
    private static TrackBase Deserialize(string json)
    {
        string version = UnityEngine.JsonUtility.FromJson<TrackBase>(json).version;
        switch (version)
        {
            case Track.kVersion:
                return UnityEngine.JsonUtility.FromJson<Track>(json);
                // For non-current versions, maybe attempt conversion?
            default:
                throw new Exception($"Unknown version: {version}");
        }
    }

    // The clone will retain the same Guid.
    public TrackBase Clone()
    {
        return Deserialize(Serialize());
    }

    public void SaveToFile(string path)
    {
        System.IO.File.WriteAllText(path, Serialize());
    }

    public static TrackBase LoadFromFile(string path)
    {
        string fileContent = System.IO.File.ReadAllText(path);
        return Deserialize(fileContent);
    }
}

// Heavily inspired by bmson:
// https://bmson-spec.readthedocs.io/en/master/doc/index.html#format-overview
[Serializable]
public class Track : TrackBase
{
    public const string kVersion = "1";
    public Track() { version = kVersion; }
    public Track(string title, string artist)
    {
        version = kVersion;
        trackMetadata = new TrackMetadata();
        trackMetadata.guid = Guid.NewGuid().ToString();
        trackMetadata.title = title;
        trackMetadata.artist = artist;
        patterns = new List<Pattern>();
    }

    public TrackMetadata trackMetadata;
    public List<Pattern> patterns;

    public void SortPatterns()
    {
        patterns.Sort((Pattern p1, Pattern p2) =>
        {
            if (p1.patternMetadata.controlScheme != p2.patternMetadata.controlScheme)
            {
                return (int)p1.patternMetadata.controlScheme -
                    (int)p2.patternMetadata.controlScheme;
            }
            else
            {
                return p1.patternMetadata.level - p2.patternMetadata.level;
            }
        });
    }
}

[Serializable]
public class TrackMetadata
{
    public string guid;

    // Text stuff.

    public string title;
    public string artist;
    public string genre;

    // In track select screen.

    // Filename of eyecatch image.
    public string eyecatchImage;
    // Filename of preview music.
    public string previewTrack;
    // In seconds.
    public double previewStartTime;
    public double previewEndTime;

    // In gameplay.

    // Filename of background image, used in loading screen
    public string backImage;
    // Filename of background animation (BGA)
    // If empty, will show background image
    public string bga;
    // Play BGA from this time.
    public double bgaOffset;
}

[Serializable]
public class Pattern
{
    public PatternMetadata patternMetadata;
    public List<BpmEvent> bpmEvents;
    public List<SoundChannel> soundChannels;

    public const int pulsesPerBeat = 240;
    public const int minLevel = 1;
    public const int maxLevel = 12;
    public const double minBpm = 1.0;
    public const double maxBpm = 1000.0;
    public const int minBps = 1;
    public const int maxBps = 128;

    public Pattern()
    {
        patternMetadata = new PatternMetadata();
        patternMetadata.guid = Guid.NewGuid().ToString();
        patternMetadata.patternName = "New pattern";
        bpmEvents = new List<BpmEvent>();
        soundChannels = new List<SoundChannel>();
    }

    public Pattern CloneWithDifferentGuid()
    {
        string json = UnityEngine.JsonUtility.ToJson(this, prettyPrint: false);
        Pattern clone = UnityEngine.JsonUtility.FromJson<Pattern>(json);
        clone.patternMetadata.guid = Guid.NewGuid().ToString();
        return clone;
    }

    public void CreateListsIfNull()
    {
        if (bpmEvents == null)
        {
            bpmEvents = new List<BpmEvent>();
        }
        if (soundChannels == null)
        {
            soundChannels = new List<SoundChannel>();
        }
    }

    // Assumes no note exists at the same location.
    public void AddNote(Note n, string sound)
    {
        if (soundChannels == null)
        {
            soundChannels = new List<SoundChannel>();
        }
        SoundChannel channel = soundChannels.Find(
            (SoundChannel c) => { return c.name == sound; });
        if (channel == null)
        {
            channel = new SoundChannel();
            channel.name = sound;
            channel.notes = new List<Note>();
            soundChannels.Add(channel);
        }
        channel.notes.Add(n);
    }

    public void ModifyNoteKeysound(Note n, string oldSound, string newSound)
    {
        SoundChannel oldChannel = soundChannels.Find(
            (SoundChannel c) => { return c.name == oldSound; });
        if (oldChannel == null)
        {
            throw new Exception(
                $"Sound channel {oldSound} not found in pattern when modifying keysound.");
        }
        SoundChannel newChannel = soundChannels.Find(
            (SoundChannel c) => { return c.name == newSound; });
        if (newChannel == null)
        {
            newChannel = new SoundChannel();
            newChannel.name = newSound;
            newChannel.notes = new List<Note>();
            soundChannels.Add(newChannel);
        }

        oldChannel.notes.Remove(n);
        newChannel.notes.Add(n);
    }

    public void DeleteNote(Note n, string sound)
    {
        SoundChannel channel = soundChannels.Find(
            (SoundChannel c) => { return c.name == sound; });
        if (channel == null)
        {
            throw new Exception(
                $"Sound channel {sound} not found in pattern when deleting.");
        }
        channel.notes.Remove(n);
    }

    // Sort BPM events by pulse, then fill their time fields.
    // Enables CalculateTimeOfAllNotes, TimeToPulse and PulseToTime.
    public void PrepareForTimeCalculation()
    {
        bpmEvents.Sort((BpmEvent e1, BpmEvent e2) =>
        {
            return e1.pulse - e2.pulse;
        });

        float currentBpm = (float)patternMetadata.initBpm;
        float currentTime = (float)patternMetadata.firstBeatOffset;
        int currentPulse = 0;
        // beat / minute = currentBpm
        // pulse / beat = pulsesPerBeat
        // ==>
        // pulse / minute = pulsesPerBeat * currentBpm
        // ==>
        // minute / pulse = 1f / (pulsesPerBeat * currentBpm)
        // ==>
        // second / pulse = 60f / (pulsesPerBeat * currentBpm)
        float secondsPerPulse = 60f / (pulsesPerBeat * currentBpm);

        foreach (BpmEvent e in bpmEvents)
        {
            e.time = currentTime + secondsPerPulse * (e.pulse - currentPulse);

            currentBpm = (float)e.bpm;
            currentTime = e.time;
            currentPulse = e.pulse;
            secondsPerPulse = 60f / (pulsesPerBeat * currentBpm);
        }
    }

    public void CalculateTimeOfAllNotes()
    {
        foreach (SoundChannel c in soundChannels)
        {
            foreach (Note n in c.notes)
            {
                n.time = PulseToTime(n.pulse);
            }
        }
    }

    // Works for negative times too.
    public float TimeToPulse(float time)
    {
        float referenceBpm = (float)patternMetadata.initBpm;
        float referenceTime = (float)patternMetadata.firstBeatOffset;
        int referencePulse = 0;

        // Find the immediate BpmEvent before specified pulse.
        for (int i = bpmEvents.Count - 1; i >= 0; i--)
        {
            BpmEvent e = bpmEvents[i];
            if (e.time <= time)
            {
                referenceBpm = (float)e.bpm;
                referenceTime = e.time;
                referencePulse = e.pulse;
                break;
            }
        }

        float secondsPerPulse = 60f / (pulsesPerBeat * referenceBpm);

        return referencePulse + (time - referenceTime) / secondsPerPulse;
    }

    // Works for negative pulses too.
    public float PulseToTime(int pulse)
    {
        float referenceBpm = (float)patternMetadata.initBpm;
        float referenceTime = (float)patternMetadata.firstBeatOffset;
        int referencePulse = 0;

        // Find the immediate BpmEvent before specified pulse.
        for (int i = bpmEvents.Count - 1; i >= 0; i--)
        {
            BpmEvent e = bpmEvents[i];
            if (e.pulse <= pulse)
            {
                referenceBpm = (float)e.bpm;
                referenceTime = e.time;
                referencePulse = e.pulse;
                break;
            }
        }

        float secondsPerPulse = 60f / (pulsesPerBeat * referenceBpm);

        return referenceTime + secondsPerPulse * (pulse - referencePulse);
    }
}

[Serializable]
public enum ControlScheme
{
    Touch = 0,
    Keys = 1,
    KM = 2
}

[Serializable]
public class PatternMetadata
{
    public string guid;

    public string patternName;
    public int level;
    public ControlScheme controlScheme;
    public string author;

    // The backing track played in game.
    // This always plays from the beginning.
    // If no keysounds, this should be the entire track.
    public string backingTrack;
    // Beat 0 starts at this time.
    public double firstBeatOffset;

    // These can be changed by events.
    public double initBpm;
    // BPS: beats per scan.
    public int bps;
}

[Serializable]
public class BpmEvent
{
    public int pulse;
    public double bpm;
    [NonSerialized]
    public float time;
}

[Serializable]
public class SoundChannel
{
    // Sound file name.
    public string name;
    // Notes using this sound.
    public List<Note> notes;
}

[Serializable]
public enum NoteType
{
    Basic,
    ChainHead,
    ChainNode,
    HoldStart,
    HoldEnd,
    DragHead,
    DragNode,
    RepeatHead,
    RepeatHeadHold,
    Repeat,
    RepeatHoldStart,
    RepeatHoldEnd,
}

[Serializable]
public class Note
{
    public int lane;
    public int pulse;
    public NoteType type;
    [NonSerialized]
    public float time;

    public Note Clone()
    {
        return new Note()
        {
            lane = this.lane,
            pulse = this.pulse,
            type = this.type
        };
    }
}