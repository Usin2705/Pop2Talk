using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegistrationForm{

	string email;

	string password;

	bool wantsNewsletter;

	public RegistrationForm(string newEmail, string newPassword, bool doesWantNewsletter)
	{
		Email = newEmail;
		Password = newPassword;
		WantsNewsletter = doesWantNewsletter;
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

	public bool WantsNewsletter
	{
		get
		{
			return wantsNewsletter;
		}

		set
		{
			wantsNewsletter = value;
		}
	}

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
}
