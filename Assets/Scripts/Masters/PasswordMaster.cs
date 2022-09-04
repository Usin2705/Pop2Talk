using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public enum PasswordScore {
	Blank = 0,
	VeryWeak = 1,
	Weak = 2,
	Medium = 3,
	Strong = 4,
	VeryStrong = 5
}

public static class PasswordMaster {

	public static PasswordScore CheckStrength(string password) {
		int score = 2;

		if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
			score = (int)PasswordScore.Blank;
		if (password.Length < 4)
			score = (int)PasswordScore.VeryWeak;
		if (password.Length >= 8)
			score++;
		if (password.Length >= 12)
			score++;
		if (Regex.IsMatch(password, @"[0-9]+(\.[0-9][0-9]?)?", RegexOptions.ECMAScript)) {
			Debug.Log("Password has numbers");
			score++;
		}
		if (Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z]).+$", RegexOptions.ECMAScript)) {
			Debug.Log("Password has upper- and lowercase");
			score++;
		}
		if (Regex.IsMatch(password, @"[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]", RegexOptions.ECMAScript)) {
			Debug.Log("Password has special symbol");
			score++;
		}

		Debug.Log(score);
		return (PasswordScore)score;
	}

	public static PasswordScore GetRequiredScore() {
		return PasswordScore.Weak;
	}

}
