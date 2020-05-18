using UnityEngine;
#if PLATFORM_IOS
//using UnityEngine.iOS;
#endif
#if PLATFORM_ANDROID
//using UnityEngine.Android;
#endif
using System.Collections;

// Show WebCams and Microphones on an iPhone/iPad.
// Make sure NSCameraUsageDescription and NSMicrophoneUsageDescription
// are in the Info.plist.

public class GameHandler : MonoBehaviour
{

    public string emailHolder;
    void Start()
    {
        /*findWebCams();
        #if PLATFORM_IOS
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.Log("webcam found");
            }
            else
            {
                Debug.Log("webcam not found");
            }
        #endif
        #if PLATFORM_ANDROID
            yield return Permission.HasUserAuthorizedPermission(Permission.WebCam);
            if (Permission.HasUserAuthorizedPermission(Permission.WebCam))
            {
                Debug.Log("webcam found");
            }
            else
            {
                Debug.Log("webcam not found");
            }
        #endif*/
        findMicrophones();
#if PLATFORM_IOS
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Debug.Log("Microphone found");
            }
            else
            {
                Debug.Log("Microphone not found");
            }
#endif
        #if PLATFORM_ANDROID
       /* yield return Permission.HasUserAuthorizedPermission(Permission.Microphone);
            if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Debug.Log("Microphone found");
            }
            else
            {
                Debug.Log("Microphone not found");
            } */
        #endif
    }

    /*void findWebCams()
    {
        foreach (var device in WebCamTexture.devices)
        {
            Debug.Log("Name: " + device.name);
        }
    }*/

    void findMicrophones()
    {
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }
    }
}