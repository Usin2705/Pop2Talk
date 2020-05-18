using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumer : MonoBehaviour {

	private string email;

	private string password;

	public string Email
	{
		get
		{
			return email;
		}

		set
		{
			email = value;
		}
	}

	public string Password
	{
		get
		{
			return password;
		}

		set
		{
			password = value;
		}
	}

	public Consumer() {
		email = "";
		password = "";
	}

	public Consumer(string newEmail, string newPassword)
	{
		email = newEmail;
		password = newPassword;
	}
}
