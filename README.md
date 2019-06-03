Rocky mascot augmented reality app.

Unity Setup Notes:
ARFoundations requires Unity 2019.1 +, not sure if this is necessary or just recommended.
  * Make sure to install the Unity recommended Android SDK and NDK from Unity Hub before starting Android development.
  * This project also requires the [Google Firebase Unity SDK 6.0.0](https://firebase.google.com/download/unity). Once the SDK has been downloaded, import FirebaseMessaging.unitypackage into your project. 
  * You will also need the Firebase configuration files created through the Firebase console. See the [Unity Firebase Cloud Messaging app tutorial](https://firebase.google.com/docs/cloud-messaging/unity/client) for more information.
 
Settings to Check Before Building to a Device:
  * In *Build Settings/Player Settings/XR Settings* make sure that 'AR Core supported' is not checked. If it is, there will be a    clash with the ARFoundations plugin at build.
  * In *Edit/Project Settings/iOS/Other Settings* check 'requires ARKit support' and make sure 'Architecture' is set to arm64.
  * In *Edit/Project Settings/Android/Other Settings* make sure the 'target API' is at least Android 7.0.
