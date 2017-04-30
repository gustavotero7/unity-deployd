# Deployd + Unity 3D
Simple unity integration with deployd server.

# Basic Usage
## Methods
### SignIn(): Use this to authenticate a user with email and password 
* `SignIn(string username, string password, AuthCallBack onSuccess, AuthCallBack onError)` 
  * username: user name, email of the current user
  * password: password of the current user
  * onSuccess: callback when the evalidation is successful
  * onError: callback when the validation returns null or custom error
  
### SignUp(): Create a new user account 
* `SignUp(string username, string password, string name, string lastname, AuthCallBack onSuccess, AuthCallBack onError)` 
  * username: user name, nickname of the current user
  * password: password, min 4 characters
  * name: User first name
  * lastname: User last name
  * onSuccess: callback when the evalidation is successful
  * onError: callback when the validation returns null or custom error
  
### DataUpload(): Upload a json object to the server 
* `DataUpload(string _path, Dictionary<string, string> _data, AuthCallBack onSuccess, AuthCallBack onError)` 
  * _path: Path to be uploaded (table name)
  * _data: JSON object with the upload request data
  * onSuccess: callback when the evalidation is successful
  * onError: callback when the validation returns null or custom error
  
### DataDownload(): Download data from the server (JSON object) 
* `DataDownload(string _path, AuthCallBack onSuccess, AuthCallBack onError)` 
  * _path: Path of the data you want to download (table), optional you can include id or GET query
  * onSuccess: callback when the evalidation is successful
  * onError: callback when the validation returns null or custom error
  
### DataDrop(): Request to delete data from the server 
### BE CAREFUL, YOU CANT UNDO OR RECOVER THE DELETED DATA
* `DataDrop(string _path, AuthCallBack onSuccess, AuthCallBack onError)` 
  * _path: path to the object/record you want to delete
  * onSuccess: callback when the evalidation is successful
  * onError: callback when the validation returns null or custom error

## All the data is returned in a string JSON, you can use SimpleJSON to cast it easly
    private void Success(string result) // AuthCallBack onSuccess
    {
        JSONNode castResult = JSON.Parse(result);
        Debug.LogError(castResult["key"]);
        
        //also you can loop over the result
        foreach (var item in castResult.Children)
        {
            Debug.LogError(item.Value);
        }
    }

# Support
For any help post an issue or write to gustavo@chimera.digital or support@chimera.digital

# Credits
* MiniJSON Copyright (c) 2013 Calvin Rien
* SimpleJSON Written by Bunny83 Modified by oPless
* deployd www.deployd.com - Licensed under the Apache License, Version 2.0 (the "License")
* Deployd.cs Gustavo Otero - gustavo@chimera.digital
