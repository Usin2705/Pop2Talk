using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserData {
    public string id;
    public string username;
    public bool consent;
    public int level;
	public string session_id;
    public int project_id;

    public UserData(string _id, string _username, bool _consent, int _level, string _session_id, int _project_id)
    {
        this.id = _id;
        this.username = _username;
        this.consent = _consent;
        this.level = _level;
        this.session_id = _session_id;
        this.project_id = _project_id;
    }
}