using System;
using System.Collections.Generic;

using Raylib_cs;
using static Raylib_cs.Raylib;

public class AudioController {

    //======================================================

    public enum trackPart {
        intro,
        loop,
        end
    }

    Dictionary<string, Dictionary<trackPart, Music>> tracks = new Dictionary<string, Dictionary<trackPart, Music>>();
    List<AudioTrack> currentlyPlaying = new List<AudioTrack>();

    List<AudioTrack> toRemove = new List<AudioTrack>();

    //------------------------------------------------------

    public void update(float delta) {
        
        foreach (AudioTrack track in currentlyPlaying) {
            Music currentPart = track.trackParts[track.currentlyPlaying];
            
            if (track.currentlyDoing == AudioTrack.trackDoing.fadingIn) {

                track.trackVolume = Math.Min(1, track.trackVolume + 0.01f);
                SetMusicVolume(currentPart, track.trackVolume);

                if (track.trackVolume == 1)
                    track.currentlyDoing = AudioTrack.trackDoing.playing;
            }
            else if (track.currentlyDoing == AudioTrack.trackDoing.fadingOut) {
                track.trackVolume = Math.Max(0, track.trackVolume - 0.01f);
                SetMusicVolume(currentPart, track.trackVolume);

                if (track.trackVolume == 0) {
                    toRemove.Add(track);
                }
            }
            if (!IsMusicPlaying(currentPart)) {
                if (track.currentlyPlaying == trackPart.intro) {
                    if (track.trackParts.ContainsKey(trackPart.loop)) {
                        track.currentlyPlaying = trackPart.loop;
                        PlayMusicStream(track.getCurrentTrack());
                        SetMusicVolume(track.getCurrentTrack(), track.trackVolume);
                    }
                }
            }
            UpdateMusicStream(currentPart);
        }

        foreach (AudioTrack track in toRemove) {
            
            StopMusicStream(track.getCurrentTrack());
            currentlyPlaying.Remove(track);
        }
        toRemove.Clear();
    }

    //------------------------------------------------------

    public void addTrack(string name, Music newTrack) {

        Dictionary<trackPart, Music> musicDict = new Dictionary<trackPart, Music>();
        musicDict[trackPart.intro] = newTrack;

        tracks.Add(name, musicDict);
    }

    public void addTrack(string name, Dictionary<trackPart, Music> musicDict) {

        if (musicDict.ContainsKey(trackPart.intro)) {
            Music track = musicDict[trackPart.intro];
            track.looping = 0;
            musicDict[trackPart.intro] = track;
        }
        if (musicDict.ContainsKey(trackPart.loop)) {
            Music track = musicDict[trackPart.loop];
            track.looping = 1;
            musicDict[trackPart.loop] = track;
        }
        if (musicDict.ContainsKey(trackPart.end)) {
            Music track = musicDict[trackPart.end];
            track.looping = 0;
            musicDict[trackPart.end] = track;
        }
        tracks.Add(name, musicDict);
    }

    //------------------------------------------------------

    public void playTrack(string trackName, bool fadein) {
        if (tracks.ContainsKey(trackName)) {

            AudioTrack newTrack = new AudioTrack(tracks[trackName]);
            newTrack.trackName = trackName;
            newTrack.currentlyPlaying = trackPart.intro;

            if (fadein) {
                newTrack.trackVolume = 0;
                newTrack.currentlyDoing = AudioTrack.trackDoing.fadingIn;
            }

            if (IsMusicPlaying(newTrack.getCurrentTrack())) {
                AudioTrack toRemove = null;
                foreach (AudioTrack track in currentlyPlaying) {
                    if (track.getCurrentTrack().Equals(newTrack.getCurrentTrack())) {
                        toRemove = track;
                    }
                }
                if (toRemove != null) { 
                    currentlyPlaying.Remove(toRemove);
                }
                StopMusicStream(newTrack.getCurrentTrack());
            }   
            
            currentlyPlaying.Add(newTrack);
            PlayMusicStream(newTrack.getCurrentTrack());
            SetMusicVolume(newTrack.getCurrentTrack(), newTrack.trackVolume);
            

        }
        else {
            Console.WriteLine("YOu typed the name wrong dummy");
        }
    }

    public void stopTrack(string trackName, bool fadeout) {
        AudioTrack toRemove = null;
        foreach (AudioTrack track in currentlyPlaying) {
            if (track.trackName == trackName) {
                toRemove = track;
                break;
            }
        }
        if (toRemove != null) {
            if (fadeout) toRemove.currentlyDoing = AudioTrack.trackDoing.fadingOut;
            else {
                StopMusicStream(toRemove.getCurrentTrack());
                currentlyPlaying.Remove(toRemove);
            }
        }
    }

    public void stopAll(bool fadeout) {
        foreach (AudioTrack track in currentlyPlaying) {
            if (fadeout) {
                track.currentlyDoing = AudioTrack.trackDoing.fadingOut;
            }
            else {
                StopMusicStream(track.trackParts[track.currentlyPlaying]);
            }
        }
        if (!fadeout)   currentlyPlaying.Clear();
    }

    //------------------------------------------------------

    

    //======================================================

    public class AudioTrack {

        public enum trackDoing {
            playing,
            fadingOut,
            fadingIn,
            stopped
        }

        public string trackName;
        public Dictionary<trackPart, Music> trackParts = new Dictionary<trackPart, Music>();

        public trackPart currentlyPlaying = trackPart.intro;

        public trackDoing currentlyDoing = trackDoing.playing;
        
        public float trackVolume = 1;

        public AudioTrack(Dictionary<trackPart, Music> musicDict) {
            trackParts = musicDict;
        }

        public Music getCurrentTrack() {
            return trackParts[currentlyPlaying];
        }
    }

    //======================================================
}

