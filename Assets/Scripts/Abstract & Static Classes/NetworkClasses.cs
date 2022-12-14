using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserData {
    public string id;
    public string username;
    public bool consent;
    public string role;
	public string access_token;

    public UserData(string _id, string _username, bool _consent, string _role, string _access_token)
    {
        this.id = _id;
        this.username = _username;
        this.consent = _consent;
        this.role = _role;
        this.access_token = _access_token;
    }
}