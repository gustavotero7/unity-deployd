using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //Login
        Deployd.SignIn("user@user", "!@#$%^&*", SigninSuccess, SigninError);
	}

    private void SigninError(string error)
    {
        Debug.LogError(error);
    }

    private void SigninSuccess(string result)
    {
        Debug.Log(result);
    }
}
