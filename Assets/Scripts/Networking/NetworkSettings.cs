using System;
using System.Collections;
using System.Collections.Generic;


public class InitialAudioData {
    public string player;
    public string word;
    public string gameversion;
    public string authenticationtoken;
    public string dataencoding;
    public string datatype;
    public int packetnr;
    public string data;
    public string build;
    public DateTime builddate;
    public string device;
    public string microphone;
    public string challengetype;
}

public class EstablishedAudioData {
    public string player;
    public string authenticationtoken;
    public int packetnr;
    public string data;
}

public class LevelEvent {
    public string player;
    public string level;
    public bool passed;
    public bool medal;
}

public class PlayerEvent {
    public string player;
}

public class SampleEvent {
    public string player;
    public string level;
    public string word;
    public bool fromPop;
}

public class AnalyticsEvent {
    public string eventname;
    public string player;
    public string eventtarget;
    public string view;
    public string level;
    public string levelType;
    public float leveltime;
    public float sessiontime;
    public string action;
    public float startxcoord;
    public float startycoord;
    public float endxcoord;
    public float endycoord;
    public string screensize;
    public string build;
    public DateTime builddate;
    public string platform;
    public string device;
    public string audioinputdevice;
    public string audiooutputdevice;
    public string serverurl;
    public string sessionid;

    public bool medal;
    public int avgStars;
    public int totalStars;
    public int stones;

}

public class ScoreData
{
    public int score;
}