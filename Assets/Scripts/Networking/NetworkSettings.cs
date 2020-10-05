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
	public int retryCount;
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
	public float leveltime = -1;
	public float sessiontime = -1;
	public string action;
	public float startxcoord = -1;
	public float startycoord = -1;
	public float endxcoord = -1;
	public float endycoord = -1;
	public string screensize;
	public string build;
	public DateTime builddate;
	public string platform;
	public string device;
	public string audioinputdevice;
	public string audiooutputdevice;
	public string serverurl;
	public string sessionid;
	public string word;

	public bool medal;
	public bool fromPop;

	public float avgStars = -1;
	public int totalStars = -1;
	public int completedCards = -1;
	public int stones = -1;

}