/* 
 * By Gustavo Otero
 * gustavotero7@gmail.com - gustavo@chimera.digital
 * 
*/
//#define USE_FB
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;
using System.Security.Cryptography;
#if USE_FB
using Facebook.Unity;
#endif

public class Deployd : MonoBehaviour
{

    public const string endpoint = "https://api.exaple.com";// your api endpoint

    public const string secretkey = "###"; //the secret key to validate server requests
    public const string FBpwd = "###"; //default facebbok password for new users

    private static Deployd instance;

    private static Deployd Instance
    {
        get
        {
            return instance;
        }
    }

    public delegate void DeploydCallBack(string result);

    void Awake()
    {
        //Keep only one instance of deployd class
        if (Instance == null)
        {
            instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    ///  Validate username and password withh the server
    /// </summary>
    /// <param name="username">user name, email of the current user</param>
    /// <param name="password">password of the current user</param>
    /// <param name="onSuccess">callback when the evalidation is successful</param>
    /// <param name="onError">callback when the validation returns null or custom error</param>
    public static void SignIn(string username, string password, DeploydCallBack onSuccess, DeploydCallBack onError)
    {
        Instance.StartCoroutine(Instance.StartSignIn(username, password, onSuccess, onError));
    }

    /// <summary>
    /// Start a coroutone to validate the username and password called from SignIn()
    /// </summary>
    /// <param name="username">user name, email of the current user</param>
    /// <param name="password">password of the current user</param>
    /// <param name="onSuccess">callback when the evalidation is successful</param>
    /// <param name="onError">callback when the validation returns null or custom error</param>
    /// <returns>User object trough callback string (JSON)</returns>
    private IEnumerator StartSignIn(string username, string password, DeploydCallBack onSuccess, DeploydCallBack onError)
    {
        WWWForm form = new WWWForm();
        Dictionary<string, string> headers = form.headers;
        headers["Content-Type"] = "application/json";
        headers["Accept"] = "application/json";
        Hashtable data = new Hashtable();
        data["username"] = "u" + LongHash(username.ToLower());
        data["password"] = password;
        data["secret"] = secretkey;
        string json = MiniJSON.Json.Serialize(data);

        byte[] bytes = Encoding.UTF8.GetBytes(ValidateJson(json));

        string url = endpoint + "/users/login";
        url += (url.Contains("?") ? "&" : "?") + "secret=" + secretkey + "&platform=" + Application.platform.ToString();


        WWW www = new WWW(url, bytes, headers);

        yield return www;

        if (System.String.IsNullOrEmpty(www.error))
        {
            JSONNode node = JSON.Parse(www.text);
            if (node["uid"].Value != "") //succesful login
            {
                onSuccess(node.ToString());

            }
            else //Other errors, check credentials
            {
                onSuccess(node.ToString());
            }
        }
        else
        {
            try
            {
                if (JSON.Parse(www.text)["message"].Value != "") //deployd request error
                {
                    onError(JSON.Parse(www.text)["message"].Value);
                }
                else
                {
                    onError(www.error); // www request error
                }
            }
            catch (Exception)
            {
                onError(www.error); // www request error
            }
        }
    }

    /// <summary>
    /// Create a new user account
    /// </summary>
    /// <param name="username">username, usually email address</param>
    /// <param name="password">password, min 4 characters</param>
    /// <param name="name">User first name</param>
    /// <param name="lastname">User last name</param>
    /// <param name="onSuccess">callback when the user  creation request is successful</param>
    /// <param name="onError">callback when the user  creation request returns null or custom error</param>
    public static void SignUp(string username, string password, string name, string lastname, DeploydCallBack onSuccess, DeploydCallBack onError)
    {
        Instance.StartCoroutine(Instance.StartSignUp(username, password, name, lastname, onSuccess, onError));
    }
    /// <summary>
    /// Create a new user account
    /// </summary>
    /// <param name="username">username, usually email address</param>
    /// <param name="password">password, min 4 characters</param>
    /// <param name="name">User first name</param>
    /// <param name="lastname">User last name</param>
    /// <param name="onSuccess">callback when the user  creation request is successful</param>
    /// <param name="onError">callback when the user  creation request returns null or custom error</param>
    /// <returns>User object trough callback string (JSON)</returns>
    IEnumerator StartSignUp(string username, string password, string name, string lastname, DeploydCallBack onSuccess, DeploydCallBack onError)
    {
        WWWForm form = new WWWForm();
        Dictionary<string, string> headers = form.headers;
        headers["Content-Type"] = "application/json";
        headers["Accept"] = "application/json";
        Hashtable data = new Hashtable();
        data["username"] = "u" + LongHash(username.ToLower());
        data["email"] = username.ToLower();
        data["password"] = password;
        data["name"] = name;
        data["lastname"] = lastname;
        data["secret"] = secretkey;
        string json = MiniJSON.Json.Serialize(data);
        byte[] bytes = Encoding.UTF8.GetBytes(ValidateJson(json));


        string url = endpoint + "/users";
        url += (url.Contains("?") ? "&" : "?") + "secret=" + secretkey + "&platform=" + Application.platform.ToString();
        WWW www = new WWW(url, bytes, headers);

        yield return www;

        if (System.String.IsNullOrEmpty(www.error))
        {
            JSONNode node = JSON.Parse(www.text);
            if (node["id"].Value != "") //succesful login
            {
                onSuccess(node.ToString());
            }
            else //Other errors, check credentials
            {
                onError("Error conectando con el servicio, por favor intente mas tarde.");
            }
        }
        else
        {
            try
            {
                if (JSON.Parse(www.text)["errors"]["username"].Value != "") //deployd request error
                {
                    onError(JSON.Parse(www.text)["errors"]["username"].Value);
                }
                else
                {
                    onError(www.error); // www request error
                }
            }
            catch (Exception)
            {
                onError(www.error); // www request error
            }
        }
    }

    /// <summary>
    /// Upload a json object to the server
    /// </summary>
    /// <param name="_path">Path to be uploaded (table)</param>
    /// <param name="_data">JSON object with the upload request data</param>
    /// <param name="onSuccess">callback when the upload request is successful</param>
    /// <param name="onError">callback when the upload request returns null or custom error</param>
    public static void DataUpload(string _path, Dictionary<string, string> _data, DeploydCallBack onSuccess, DeploydCallBack onError)
    {
        Instance.StartCoroutine(Instance.StartDataUpload(_path, _data, onSuccess, onError));
    }

    /// <summary>
    /// Upload a json object to the server
    /// </summary>
    /// <param name="_path">Path to be uploaded (table)</param>
    /// <param name="_data">JSON object with the upload request data</param>
    /// <param name="onSuccess">callback when the upload request is successful</param>
    /// <param name="onError">callback when the upload request returns null or custom error</param>
    /// <returns>Created object (JSON)</returns>
    IEnumerator StartDataUpload(string _path, Dictionary<string, string> _data, DeploydCallBack onSuccess, DeploydCallBack onError)
    {
        WWWForm form = new WWWForm();
        Dictionary<string, string> headers = form.headers;
        headers["Content-Type"] = "application/json";
        headers["Accept"] = "application/json";
        Hashtable data = new Hashtable();
        foreach (var item in _data)
        {
            data[item.Key] = item.Value;
        }
        data["secret"] = secretkey;

        string json = MiniJSON.Json.Serialize(data);

        byte[] bytes = Encoding.UTF8.GetBytes(ValidateJson(json));

        string url = endpoint + _path;
        url += (url.Contains("?") ? "&" : "?") + "secret=" + secretkey + "&platform=" + Application.platform.ToString();
        WWW www = new WWW(url, bytes, headers);
        yield return www;
        if (System.String.IsNullOrEmpty(www.error))
        {
            JSONNode node = JSON.Parse(www.text);
            if (node["id"].Value != "") //succesful upload
            {
                onSuccess(node.ToString());
            }
            else //Other errors
            {
                onSuccess(node.ToString());
            }
        }
        else
        {
            try
            {
                if (JSON.Parse(www.text)["message"].Value != "") //deployd request error
                {
                    onError(JSON.Parse(www.text)["message"].Value);
                }
                else
                {
                    onError(www.error); // www request error
                }
            }
            catch (Exception)
            {
                onError(www.error); // www request error

            }
        }
    }

#if USE_FB

    /// <summary>
    /// Get the Facebook user data, and check if the email is registered in the server and use the existing account or create new with the facebook email
    /// </summary>
    /// <param name="onSuccess">callback when the facebook login is successful</param>
    /// <param name="onError">callback when the facebook login returns null or custom error</param>
    public static void FacebookSignIn( AuthCallBack onSuccess, AuthCallBack onError)
    {
        instance.onFBSuccess = onSuccess;
        instance.onFBError = onError;

        Facebook.Unity.FB.LogInWithReadPermissions(
        new List<string>() { "public_profile", "email" },
            instance.FBAuthCallback
        );
    }

    public AuthCallBack onFBSuccess, onFBError;
    void FBAuthCallback(Facebook.Unity.IResult result)
    {

        // Some platforms return the empty string instead of null.
        if (!string.IsNullOrEmpty(result.Error))
        {

            //"Error - Check log for details";
            //result.Error;
            onFBError(result.Error);
        }
        else if (result.Cancelled)
        {
            //"Cancelled - Check log for details";
            //"Cancelled Response:\n" + result.RawResult;
            onFBError("Cancelled");
        }
        else if (!string.IsNullOrEmpty(result.RawResult))
        {
            FB.API("/me?fields=id,first_name,last_name,email", Facebook.Unity.HttpMethod.GET, instance.FetchProfileCallback, new Dictionary<string, string>() { });
        }
        else
        {
            //"Empty Response\n";
            onFBError("Empty Response");
        }
    }

    void FetchProfileCallback(IGraphResult result)
    {
        Instance.StartCoroutine(Instance.StartFacebookSignIn(result.ResultDictionary["first_name"].ToString(),
            result.ResultDictionary["last_name"].ToString(), result.ResultDictionary["email"].ToString(),
            result.ResultDictionary["id"].ToString(), onFBSuccess, onFBError));
    }

    /// <summary>
    /// Get the Facebook user data, and check if the email is registered in the server and use the existing account or create new with the facebook email
    /// </summary>
    /// <param name="name">User first name</param>
    /// <param name="lastname">User last name</param>
    /// <param name="email">user email addres</param>
    /// <param name="onSuccess">callback when the facebook login is successful</param>
    /// <param name="onError">callback when the facebook login returns null or custom error</param>
    /// <returns>Created or existing user object (JSON)</returns>
    IEnumerator StartFacebookSignIn(string name, string lastname, string email, string fbid, AuthCallBack onSuccess, AuthCallBack onError)
    {
        WWWForm form = new WWWForm();
        Dictionary<string, string> headers = form.headers;
        headers["Content-Type"] = "application/json";
        headers["Accept"] = "application/json";
        Hashtable data = new Hashtable(); //Object to create new account with facebook data
        data["username"] = "u"+LongHash(email.ToLower()).ToString();
        data["fbid"] = fbid;
        data["email"] = email.ToLower();
        data["verified"] = true;
        data["password"] = FBpwd; //default account when the user uses facebok for first time, it can be changed later by the user (look for dpd-mail or dpd-email)
        data["name"] = name;
        data["lastname"] = lastname;
        data["secret"] = secretkey;
        string json = MiniJSON.Json.Serialize(data);
        byte[] bytes = Encoding.UTF8.GetBytes(ValidateJson(json));

        string url = endpoint + "/users";
        url += (url.Contains("?") ? "&" : "?") + "secret=" + secretkey + "&platform=" + Application.platform.ToString();
        WWW www = new WWW(url, bytes, headers);
        yield return www;
        if (System.String.IsNullOrEmpty(www.error))
        {
            JSONNode node = JSON.Parse(www.text);
            if (node["id"].Value != "") //succesful login
            {
                onSuccess(node.ToString());
            }
            else //Other errors, check credentials
            {
                onError("Error conectando con el servicio, por favor intente mas tarde.");
            }
        }
        else
        {
            string _getUserIdURL = endpoint + "/users?username=" + data["username"];

            _getUserIdURL += (_getUserIdURL.Contains("?") ? "&" : "?") + "secret=" + secretkey;

            www = new WWW(_getUserIdURL);
            yield return www;
            JSONNode node = JSON.Parse(www.text);
            if (node[0]["id"].Value != "") //succesful login
            {
                onSuccess(node[0].ToString());
            }
            else //Other errors, check credentials
            {
                onError("Error conectando con el servicio, por favor intente mas tarde.");
            }
        }
    }
#endif
    /// <summary>
    /// Download data from the server (JSON object)
    /// </summary>
    /// <param name="_path">Path of the data you want to download (table), optional you can include id or GET query</param>
    /// <param name="onSuccess">callback when the dowload was successful</param>
    /// <param name="onError">callback when the download returns null or custom error</param>
    public static void DataDownload(string _path, DeploydCallBack onSuccess, DeploydCallBack onError)
    {
        Instance.StartCoroutine(Instance.StartDataDownload(_path, onSuccess, onError));
    }

    /// <summary>
    /// Download data from the server (JSON object)
    /// </summary>
    /// <param name="_path">Path of the data you want to download (table), optional you can include id or GET query</param>
    /// <param name="onSuccess">callback when the dowload was successful</param>
    /// <param name="onError">callback when the download returns null or custom error</param>
    /// <returns>The requested data as JSON object</returns>
    IEnumerator StartDataDownload(string _path, DeploydCallBack onSuccess, DeploydCallBack onError)
    {
        string url = endpoint + _path;
        url += (url.Contains("?") ? "&" : "?") + "secret=" + secretkey + "&platform=" + Application.platform.ToString();
        WWW www = new WWW(url);

        yield return www;
        Debug.Log(www.text);
        Debug.Log(www.error);

        if (System.String.IsNullOrEmpty(www.error))
        {
            JSONNode node = JSONNode.Parse(www.text);
            if (node["id"].Value != "") //succesful upload
            {
                onSuccess(node.ToString());
            }
            else //Other errors
            {
                onSuccess(node.ToString());
            }
        }
        else
        {
            try
            {
                if (JSON.Parse(www.text)["message"].Value != "") //deployd request error
                {
                    onError(JSON.Parse(www.text)["message"].Value);
                }
                else
                {
                    onError(www.error); // www request error
                }
            }
            catch (Exception)
            {
                onError(www.error); // www request error

            }
        }
    }

    /// <summary>
    /// Request to delete data from the server 
    /// BE CAREFUL, YOU CANT UNDO OR RECOVER THE DELETED DATA
    /// </summary>
    /// <param name="_path">path to the object/record you want to delete</param>
    /// <param name="onSuccess">callback when the drop request was successful</param>
    /// <param name="onError">callback when the drop request returns null or custom error</param>
    public static void DataDrop(string _path, DeploydCallBack onSuccess, DeploydCallBack onError)
    {
        Instance.StartCoroutine(Instance.StartDataDrop(_path, onSuccess, onError));
    }
    /// <summary>
    /// Request to delete data from the server 
    /// BE CAREFUL, YOU CANT UNDO OR RECOVER THE DELETED DATA
    /// </summary>
    /// <param name="_path">path to the object/record you want to delete</param>
    /// <param name="onSuccess">callback when the drop request was successful</param>
    /// <param name="onError">callback when the drop request returns null or custom error</param>
    /// <returns>request status/results</returns>
    IEnumerator StartDataDrop(string _path, DeploydCallBack onSuccess, DeploydCallBack onError)
    {

        string url = endpoint + _path;
        url += (url.Contains("?") ? "&" : "?") + "secret=" + secretkey + "&platform=" + Application.platform.ToString();

        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Delete(url);
        request.Send();
        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }

        if (System.String.IsNullOrEmpty(request.error))
        {
            onSuccess(request.responseCode.ToString());
        }
        else
        {
            onError(request.error); // www request error
        }
    }

    struct UserLocation
    {
        public string country;
        public string city;
        public string _lat;
        public string _lon;
    }

    /// <summary>
    /// Upload a json object to the server
    /// </summary>
    /// <param name="_path">Path to be uploaded (table)</param>
    /// <param name="_data">JSON object with the upload request data</param>
    /// <param name="onSuccess">callback when the upload request is successful</param>
    /// <param name="onError">callback when the upload request returns null or custom error</param>
    public static void PostAnalytics(string userID, string eventName, string platform, string location, string customData, DeploydCallBack onSuccess, DeploydCallBack onError)
    {
        Instance.StartCoroutine(Instance.StartPostAnalytics(userID, eventName, platform, location, customData, onSuccess, onError));
    }

    /// <summary>
    /// Upload a json object to the server
    /// </summary>
    /// <param name="_path">Path to be uploaded (table)</param>
    /// <param name="_data">JSON object with the upload request data</param>
    /// <param name="onSuccess">callback when the upload request is successful</param>
    /// <param name="onError">callback when the upload request returns null or custom error</param>
    /// <returns>Created object (JSON)</returns>
    IEnumerator StartPostAnalytics(string userID, string eventName, string platform, string location, string customData, DeploydCallBack onSuccess, DeploydCallBack onError)
    {
        WWWForm form = new WWWForm();
        Dictionary<string, string> headers = form.headers;
        headers["Content-Type"] = "application/json";
        headers["Accept"] = "application/json";
        Hashtable data = new Hashtable();

        data["eventname"] = eventName;
        data["user"] = userID;
        data["platform"] = platform;
        data["location"] = location;
        data["customdata"] = customData;

        string json = MiniJSON.Json.Serialize(data);

        byte[] bytes = Encoding.UTF8.GetBytes(ValidateJson(json));

        string url = endpoint + "";
        url += (url.Contains("?") ? "&" : "?") + "secret=" + secretkey;
        WWW www = new WWW(url, bytes, headers);
        yield return www;
        if (System.String.IsNullOrEmpty(www.error))
        {
            JSONNode node = JSON.Parse(www.text);
            if (node["id"].Value != "") //succesful upload
            {
                onSuccess(node.ToString());
            }
            else //Other errors
            {
                onSuccess(node.ToString());
            }
        }
        else
        {
            try
            {
                if (JSON.Parse(www.text)["message"].Value != "") //deployd request error
                {
                    onError(JSON.Parse(www.text)["message"].Value);
                }
                else
                {
                    onError(www.error); // www request error
                }
            }
            catch (Exception)
            {
                onError(www.error); // www request error

            }
        }
    }


    string ValidateJson(string _toValidate)
    {
        string _validated = (_toValidate.Replace(@"\", ""));
        _validated = (_validated.Replace("\"{", "{"));
        _validated = (_validated.Replace("}\"", "}"));
        _validated = (_validated.Replace("\"[", "["));
        _validated = (_validated.Replace("]\"", "]"));
        return _validated;
    }

    public static long LongHash(String input)
    {
        long h = 98764321269L;
        int l = input.Length;
        char[] chars = input.ToCharArray();

        for (int i = 0; i < l; i++)
        {
            h = 31 * h + chars[i];
        }
        return h;
    }
}
