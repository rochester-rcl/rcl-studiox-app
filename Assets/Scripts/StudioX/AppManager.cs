namespace StudioX
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.XR;
    using System.Text.RegularExpressions;
    using System.IO;

    ///<summary>Singleton class to be used to manage scene changes, Firebase support, and any other housekeeping.</summary>
    public class AppManager : MonoBehaviour
    {
        /// <summary> 
        /// Singleton <see cref="AppManager" /> instance
        /// </summary> 
        private static AppManager _instance;
        /// <summary> Lock used when setting and getting <see cref="AppManager"/> singleton.</summary>
        private static readonly object appManagerLock = new object();
        /// <summary> Indicates whether or not Firebase Cloud Messaging service is ready (set in <see cref="InitFirebase"/>).</summary>
        private bool firebaseReady;
        /// <summary> The Firebase app instance set in <see cref="InitFirebase"/>.</summary>
        private Firebase.FirebaseApp firebaseApp;
        /// <summary> The Firebase SDK version the project is using.</summary>
        private Version MinFirebaseSdkVersion = new Version("6.3.0");
        /// <summary> Placeholder for when Firebase SDK isn't found in <see cref="CheckFirebaseSDKVersion"/>.</summary>
        private const string _sdkNotFoundVersion = "0.0.0";
        /// <summary> The <see cref="FullscreenFade"/> controller used for fading between scenes.</summary>
        private FullscreenFade FadeController { get; set; }
        /// <summary> enum for keeping track of internal app state. </summary>
        private enum AppState { Loading, Landing, Menu, VR, Default };
        /// <summary> The current internal app state (<see cref="AppState" />). </summary>
        private AppState CurrentAppState { get; set; }
        /// <summary> The current VR device (i.e. cardboard or daydream) </summary>
        private const string VRDevice = "cardboard";
        // TODO THESE WILL BE REPLACED WITH REMOTE ASSET BUNDLES
        /// <summary> The names of the AssetBundles set to the app - deprecated.</summary>
        private string[] arBundleNames = { "foxyboiassets.unity3d", "groundboiassets.unity3d", "melioraassets.unity3d" };
        /// <summary> 
        /// All canvases found at the root level of the <see cref="loadingScreen"/> prefab. Used in <see cref="UpdateSortingOrder"/> 
        /// to allow for proper fading.
        /// </summary>
        private List<Canvas> loadingScreenCanvases;
        /// <summary>
        /// The path to the Firebase SDK.
        /// </summary>
        public static string FirebaseSdkDir { get; set; }
        /// <summary>
        /// The current Firebase SDK version used in the project.
        /// </summary>
        public Version FirebaseSdkVersion { get; set; }
        /// <summary>
        /// Optional landing scene to display when the app initially loads. If null, <see cref="loadingScreen"/> is used instead.
        /// </summary>
        public string landingScene;
        /// <summary>
        /// Prefab to display while scenes are being loaded.
        /// </summary>
        public GameObject loadingScreen;
        /// <summary> The main menu scene for the app </summary>
        public string menuScene;
        /// <summary> The List of available <see cref="UnityEngine.AssetBundle"/> instances. </summary>
        /// <example> These can be retrieved from any script as long as an AppManager instance is present (see below).</example>
        /// <code> List<AssetBundle> bundles = AppManager.GetManager().ARBundles;</code>
        public List<AssetBundle> ARBundles { get; set; }

        /// <summary>The optional name of the Firebase Messaging Topic the app will subscribe to.false Defaults to an empty string.
        /// <para><see cref="Firebase.Messaging.FirebaseMessaging.SubscribeAsync(string)"/> for more details</para> 	
        /// </summary>
        public string firebaseMessagingTopic;

        /// <summary>Static thread-safe singleton instance of AppManager</summary>
        public static AppManager Instance
        {
            get
            {
                lock (appManagerLock)
                {
                    return _instance;
                }
            }
        }
        /// <summary> Retrieves the active AppManager instance in the scene.</summary>
        /// <para> Note: because <see cref="AppManager"/> is not destroyed on scene load,
        /// the instance will be available in any scene once it has been instantiated.
        /// </para>
        public static AppManager GetManager()
        {
            return GameObject.FindObjectOfType<AppManager>();
        }
        /// <summary> 
        /// Performs initialization of <see cref="loadingScreen"/> and Firebase SDK. Retrieves remote asset bundles
        /// </summary>
        public void Start()
        {
            InitLoadingScreen();
            InitFirebase();
            DisplayLanding();
            StartCoroutine(LoadLocalAssetBundles());
        }
        /// <summary> Initializes loading screen prefab (if set) </summary>
        public void InitLoadingScreen()
        {
            if (loadingScreen)
            {
                loadingScreenCanvases = new List<Canvas>();
                loadingScreen = Instantiate(loadingScreen, new Vector3(0, 0, 0), Quaternion.identity);
                foreach (Transform child in loadingScreen.transform)
                {
                    Canvas canvas = child.gameObject.GetComponent<Canvas>();
                    if (canvas)
                    {
                        loadingScreenCanvases.Add(canvas);
                    }
                }
                DontDestroyOnLoad(loadingScreen);
                // initialize fading for loading screen
                FadeController = gameObject.AddComponent<FullscreenFade>();
                loadingScreen.SetActive(false);
                if (string.IsNullOrWhiteSpace(landingScene))
                {
                    ToggleLoadingScreen(true);
                }
            }
        }
        /// <summary> 
        /// Asynchronously toggles <see cref="loadingScreen"/> with either a fade in or fade out animation.
        /// </summary>
        /// <param name="active"> Toggles the active state of <see cref="loadingScreen"/>.</param>
        /// <returns> A <see cref="UnityEngine.Coroutine"/> that can optionally be waited upon in another Coroutine (see example).</returns>
        /// <example> See the example below for waiting on ToggleLoadingScreen in another Coroutine. </example>
        /// <code>
        /// // wait until loading screen has faded in 
        /// Coroutine cr = ToggleLoadingScreen(true);
        /// yield return cr;
        /// </code>
        public Coroutine ToggleLoadingScreen(bool active)
        {
            if (active)
            {
                return StartCoroutine(ShowLoadingScreen());
            }
            else
            {
                return StartCoroutine(HideLoadingScreen());
            }
        }
        /// <summary> Asynchronously hides <see cref="loadingScreen"/> with a fade out animation </summary>
        /// <returns> an IEnumerator that can be used with <see cref="UnityEngine.MonoBehaviour.StartCoroutine"/>.</returns>
        public IEnumerator HideLoadingScreen()
        {
            Coroutine fade = StartCoroutine(FadeAsync(false));
            yield return fade;
            loadingScreen.SetActive(false);
        }
        /// <summary> Asynchronously shows <see cref="loadingScreen"/> with a fade in animation </summary>
        /// <returns><see cref="IEnumerator"/> that can be used with <see cref="UnityEngine.MonoBehaviour.StartCoroutine"/>.</returns>
        public IEnumerator ShowLoadingScreen()
        {
            Coroutine fade = StartCoroutine(FadeAsync(false, true));
            yield return fade;
            loadingScreen.SetActive(true);
            fade = StartCoroutine(FadeAsync(true));
            yield return fade;
        }
        /// <summary> 
        /// Displays <see cref="landingScene"/> if not null. 
        /// <para>It will continue to display until a key is pressed or a screen is touched.</para>
        /// <para> If no <see cref="landingScene"/> is set, <see cref="loadingScene"/> will be used instead.</para>
        /// </summary>
        public void DisplayLanding()
        {
            if (!string.IsNullOrWhiteSpace(landingScene))
            {
                StartCoroutine(LoadAsyncScene(landingScene, AppState.Landing, false));
            }
            else
            {
                CurrentAppState = AppState.Landing;
                ToggleLoadingScreen(true);
                Debug.Log("No Landing Scene Set for App Manager. Showing Loading Screen Instead.");
            }
        }
        /// <summary> Asynchronously loads <see cref="menuScene"/>. </summary>
        /// <exception cref="UnityEngine.MissingComponentException">No <see cref="menuScene"/> attached to script.</exception>
        public void ShowMenuScene()
        {
            if (!string.IsNullOrWhiteSpace(menuScene))
            {
                StartCoroutine(LoadAsyncScene(menuScene, AppState.Menu));
            }
            else
            {
                throw new MissingComponentException("AppManager requires a Menu Scene.");
            }
        }
        /// <summary>Internal method for loading a scene asynchronously.</summary>
        /// <para>Fades out the current scene, (optionally) shows <see cref="loadingScreen"/> and fades in the new scene.</para>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="showLoadingScreen"> Optional parameter to show <see cref="loadingScreen"/> while the new scene loads (default is true).</param>
        /// <returns><see cref="IEnumerator"/> that can be used with <see cref="UnityEngine.MonoBehaviour.StartCoroutine"/>.</returns>
        private IEnumerator LoadAsyncSceneInternal(string sceneName, bool showLoadingScreen = true)
        {
            Coroutine fade;
            if (showLoadingScreen)
            {
                fade = ToggleLoadingScreen(true);
                yield return fade;
            }

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;
            while (!op.isDone)
            {
                if (op.progress >= 0.9f)
                {
                    fade = ToggleLoadingScreen(false);
                    yield return fade;
                    op.allowSceneActivation = true;
                }
                yield return null;
            }
            Coroutine fadeIn = StartCoroutine(FadeAsync(true, true));
            yield return fadeIn;
        }
        /// <summary>Loads local asset bundles (will be deprecated in favor of remote asset bundles).</summary>
        private IEnumerator LoadLocalAssetBundles()
        {
            ARBundles = new List<AssetBundle>();
            // I wonder if this has to be done on the main thread or if they can be done in parallel
            foreach (string bundlePath in arBundleNames)
            {
                string absPath = Path.Combine(Application.streamingAssetsPath, bundlePath);
                var bundleRequest = AssetBundle.LoadFromFileAsync(absPath);
                yield return bundleRequest;
                AssetBundle b = bundleRequest.assetBundle;
                if (b) ARBundles.Add(b);
            }
        }
        /// <summary>Loads a scene asynchronously.</summary>
        /// <para>Fades out the current scene, (optionally) shows <see cref="loadingScreen"/> and fades in the new scene.</para>
        /// <para>Note: calls <see cref="LoadAsyncSceneInternal" /> behind the scenes but also does some work to transition 
        /// <see cref="currentAppState"/> based on 2D or VR state.</para>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="showLoadingScreen"> Optional parameter to show <see cref="loadingScreen"/> while the new scene loads (default is true).</param>
        /// <returns><see cref="IEnumerator"/> that can be used with <see cref="UnityEngine.MonoBehaviour.StartCoroutine"/>.</returns>
        private IEnumerator LoadAsyncScene(string sceneName, bool showLoadingScreen = true)
        {
            if (CurrentAppState == AppState.VR)
            {
                Coroutine enable2D = StartCoroutine(Enable2D());
                yield return enable2D;
            }
            CurrentAppState = AppState.Loading;
            Coroutine loader = StartCoroutine(LoadAsyncSceneInternal(sceneName, showLoadingScreen));
            yield return loader;
            CurrentAppState = AppState.Default;
        }
        /// <summary>Loads a scene asynchronously.</summary>
        /// <para>Fades out the current scene, (optionally) shows <see cref="loadingScreen"/> and fades in the new scene.</para>
        /// <para>Note: calls <see cref="LoadAsyncSceneInternal" /> behind the scenes but also does some work to transition 
        /// <see cref="CurrentAppState"/> based on 2D or VR state.</para>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="showLoadingScreen"> Optional parameter to show <see cref="loadingScreen"/> while the new scene loads (default is true).</param>
        /// <param name="end">The <see cref="AppState"/> that will be set once the scene has been loaded.</param>
        /// <returns><see cref="IEnumerator"/> that can be used with <see cref="UnityEngine.MonoBehaviour.StartCoroutine"/>.</returns>
        private IEnumerator LoadAsyncScene(string sceneName, AppState end, bool showLoadingScreen = true)
        {
            if (CurrentAppState == AppState.VR)
            {
                Coroutine enable2D = StartCoroutine(Enable2D());
                yield return enable2D;
            }
            CurrentAppState = AppState.Loading;
            Coroutine loader = StartCoroutine(LoadAsyncSceneInternal(sceneName, showLoadingScreen));
            yield return loader;
            CurrentAppState = end;
        }
        /// <summary>Loads a scene asynchronously.</summary>
        /// <para>Fades out the current scene, (optionally) shows <see cref="loadingScreen"/> and fades in the new scene.</para>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="showLoadingScreen"> Optional parameter to show <see cref="loadingScreen"/> while the new scene loads (default is true).</param>
        public void LoadScene(string sceneName, bool showLoadingScreen = true)
        {
            StartCoroutine(LoadAsyncScene(sceneName, showLoadingScreen));
        }
        /// <summary>Loads a scene asynchronously.</summary>
        /// <para>Fades out the current scene, (optionally) shows <see cref="loadingScreen"/> and fades in the new scene.</para>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="end">The <see cref="AppState"/> that will be set once the scene has been loaded.</param>
        /// <param name="showLoadingScreen"> Optional parameter to show <see cref="loadingScreen"/> while the new scene loads (default is true).</param>
        private void LoadScene(string sceneName, AppState end, bool showLoadingScreen = true)
        {
            StartCoroutine(LoadAsyncScene(sceneName, end, showLoadingScreen));
        }
        /// <summary>Loads a VR scene asynchronously.</summary>
        /// <para>Fades out the current scene, (optionally) shows <see cref="loadingScreen"/> and fades in the new scene.</para>
        /// <para>Calls <see cref="LoadAsyncVRScene"/> in order to transition from 2D to VR state</para>
        /// <param name="sceneName">The name of the scene to load.</param>
        public void LoadVRScene(string sceneName)
        {
            StartCoroutine(LoadAsyncVRScene(sceneName));
        }
        /// <summary>Asynchronously loads <see cref="menuScene"/>.</summary>
        /// <exception cref="UnityEngine.MissingComponentException">No <see cref="menuScene"/> attached to script.</exception>
        public void LoadMenu()
        {
            if (!string.IsNullOrWhiteSpace(menuScene))
            {
                LoadScene(menuScene, AppState.Menu);
            }
            else
            {
                throw new MissingComponentException("AppManager requires a Menu Scene.");
            }

        }
        /// <summary>Loads a VR scene asynchronously.</summary>
        /// <para>Fades out the current scene, (optionally) shows <see cref="loadingScreen"/> and fades in the new scene.</para>
        /// <para>Calls <see cref="EnableVR"/> in order to transition from 2D to VR state</para>
        /// <param name="sceneName">The name of the scene to load.</param>
        private IEnumerator LoadAsyncVRScene(string sceneName)
        {
            Coroutine coroutine = StartCoroutine(LoadAsyncScene(sceneName, AppState.VR));
            yield return coroutine;
            coroutine = StartCoroutine(EnableVR());
            yield return coroutine;
        }
        /// <summary>Enabled VR mode and loads <see cref="VRDevice"/> if present. </summary>
        private IEnumerator EnableVR()
        {
            if (string.Compare(XRSettings.loadedDeviceName, VRDevice, true) != 0)
            {
                XRSettings.LoadDeviceByName(VRDevice);
                yield return null;
            }
            XRSettings.enabled = true;
        }
        /// <summary> Unloads the current <see cref="VRDevice"/> and resets all cameras. </summary>
        private IEnumerator Enable2D()
        {
            XRSettings.LoadDeviceByName("");
            yield return null;
            ResetCameras();
        }
        /// <summary> Resets all camera positions to <see cref="UnityEngine.Vector3.zero"/> and 
        /// all camera rotations to <see cref="UnityEngine.Quaternion.identity"/>.
        /// <para> Intended to be called after exiting a VR scene.</para>
        /// </summary>
        private void ResetCameras()
        {
            for (int i = 0; i < Camera.allCameras.Length; i++)
            {
                Camera cam = Camera.allCameras[i];
                if (cam.enabled && cam.stereoTargetEye != StereoTargetEyeMask.None)
                {
                    cam.transform.localPosition = Vector3.zero;
                    cam.transform.localRotation = Quaternion.identity;
                }
            }
        }
        /// <summary>Asynchronously executes a fullscreen fade effect using <see cref="FadeController"/></summary>
        /// <param name="fadeIn">Set to fade in or fade out.</param>
        /// <param name="updateSortOrder">Optionally update the sort order of both <see cref="FadeController"/> and <see cref="loadingScreen"/>
        /// to ensure the fade will be seen. Default is false, but should be set to true if fading into a scene that has at least one Canvas whose 
        /// sorting order is higher than 0.</param>
        /// <returns> A <see cref="UnityEngine.Coroutine"/> that can optionally be waited upon in another Coroutine.</returns>
        private IEnumerator FadeAsync(bool fadeIn, bool updateSortOrder = false)
        {
            if (updateSortOrder)
            {
                UpdateSortingOrder();
            }

            if (fadeIn)
            {
                Task t = FadeController.FadeInAsync();
                while (!t.IsCompleted)
                {
                    yield return null;
                }
            }
            else
            {
                Task t = FadeController.FadeOutAsync();
                while (!t.IsCompleted)
                {
                    yield return null;
                }
            }
        }
        /// <summary>Updates the sorting order of <see cref="loadingScreenCanvases"/> and <see cref="FadeController"/> 
        /// so that they will be rendered after any other canvases in the scene. 
        /// </summary>
        private void UpdateSortingOrder()
        {
            int sortingOrder = FadeController.UpdateSortingOrder();
            if (loadingScreen)
            {
                foreach (Canvas canvas in loadingScreenCanvases)
                {
                    canvas.sortingOrder = sortingOrder - 1;
                }
            }
        }
        /// <summary>Initializes <see cref="AppManager"/> and ensures only one instance is present in the scene.</summary>
        /// <para> Also responsible for setting the <see cref="FirebaseSdkDir" /> which will default to <see cref="UnityEngine.Application.dataPath"/>
        /// if not set in the editor.</para>
        /// <para>Note: if another <see cref="AppManager"/> is already present in the scene, a warning will be logged and 
        /// the GameObject this instance is attached to will be destroyed. </para>
        public void Awake()
        {
            if (string.IsNullOrWhiteSpace(FirebaseSdkDir))
            {
                FirebaseSdkDir = string.Format("{0}/Firebase/", Application.dataPath);
            }
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Another instance of AppManager was found in the scene. Destroying the current instance.");
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        /// <summary>Responsible for checking input changes that take place in <see cref="Update"/></summary>
        private void HandleInput()
        {
            if (CurrentAppState == AppState.Landing)
            {
                if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
                {
                    bool showLoading = string.IsNullOrWhiteSpace(landingScene) ? true : false;
                    LoadScene(menuScene, AppState.Menu, showLoading);
                }
            }

            if (CurrentAppState == AppState.Default || CurrentAppState == AppState.VR)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    LoadScene(menuScene, AppState.Menu);
                }
            }
        }

        public void Update()
        {
            HandleInput();
        }

        /********** FIREBASE METHODS **********/

        /// <summary>Checks the Firebase SDK version based on maven-metadata.xml found in the package. </summary>
        /// <para>Sets <see cref="AppManager.FirebaseSdkVersion"/> if found. If not, the version is set to 0.0.0</para> 	
        private void CheckFirebaseSDKVersion()
        {
            string sdkVersion = _sdkNotFoundVersion;
            if (System.IO.Directory.Exists(FirebaseSdkDir))
            {
                try
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.Load(string.Format("{0}m2repository/com/google/firebase/firebase-app-unity/maven-metadata.xml", FirebaseSdkDir));
                    System.Xml.XmlNodeList versions = doc.SelectNodes("metadata/versioning/release");
                    if (versions.Count > 0) sdkVersion = versions[0].InnerText;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex);
                }
            }
            FirebaseSdkVersion = new Version(sdkVersion);
        }

        /// <summary>Initializes the Firebase app, checks for dependencies.</summary>
        /// <para>Responsible for setting StudioX.AppManager.firebaseApp and StudioX.AppManager.firebaseReady if <see cref="Firebase.DependencyStatus"/> is set to Available.</para>
        /// <para>See <see cref="Firebase.FirebaseApp.CheckAndFixDependenciesAsync"/> for more details</para> 	
        public void InitFirebase()
        {

#if UNITY_EDITOR
            CheckFirebaseSDKVersion();
            if (FirebaseSdkVersion.Equals(new Version(_sdkNotFoundVersion)))
            {
                Debug.LogError("No Firebase SDK found. Are you sure you imported the packages?");
            }
            else if (!FirebaseSdkVersion.Equals(MinFirebaseSdkVersion))
            {
                Debug.LogWarning(string.Format("Firebase SDK Version ({0}) is different than the one used for this project ({1}). This may result in errors.", FirebaseSdkVersion, MinFirebaseSdkVersion));
            }
#endif

            // From https://firebase.google.com/docs/cloud-messaging/unity/client
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    firebaseReady = true;
                    firebaseApp = Firebase.FirebaseApp.DefaultInstance;
                    InitMessaging();
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format("Could not resolve all Firebase dependenceis: {0}", dependencyStatus));
                }
            });
        }

        /// <summary>Subscribes StudioX.AppManager.OnTokenReceived and StudioX.AppManager.OnMessageReceived to 
        ///  Firebase messaging events.</summary>
        /// <para>See <see cref="Firebase.Messaging.FirebaseMessaging.TokenReceived"/> and 
        /// <see cref="Firebase.Messaging.FirebaseMessaging.MessageReceived"/> for more details.</para> 	
        ///
        private void InitMessaging()
        {
            if (firebaseReady)
            {
                Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
                if (!string.IsNullOrWhiteSpace(firebaseMessagingTopic))
                {
                    string fbTopic = firebaseMessagingTopic.ToLower().Replace(" ", "-");
                    Debug.Log(string.Format("Subscribing to topic {0}", fbTopic));
                    Firebase.Messaging.FirebaseMessaging.SubscribeAsync(fbTopic).ContinueWith(task =>
                    {
                        LogTaskStatus(task, "SubscribeAsync");
                    });
                }

            }
            else
            {
                Debug.Log("Could not initialize Firebase event handlers");
            }
        }

        /// <summary>Logs the status of a <see cref="System.Threading.Tasks.Task"/>.</summary>
        /// <para>Responsible for setting StudioX.AppManager.firebaseApp and StudioX.AppManager.firebaseReady if <see cref="Firebase.DependencyStatus"/> is set to Available.</para>
        /// <para>See <see cref="Firebase.FirebaseApp.CheckAndFixDependenciesAsync"/> for more details.</para>
        /// <param name="task">The task.</param>
        /// <param name="operation">The name of the operation (for logging purposes only).</param> 	
        private bool LogTaskStatus(Task task, string operation)
        {
            bool status = false;
            if (task.IsCanceled)
            {
                Debug.Log(operation + " task was cancelled");
            }
            else if (task.IsFaulted)
            {
                Debug.Log(operation + " encountered an error");
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    string errorCode = "";
                    Firebase.FirebaseException fbException = exception as Firebase.FirebaseException;
                    if (fbException != null)
                    {
                        errorCode = String.Format("Error.{0}: ", ((Firebase.Messaging.Error)fbException.ErrorCode).ToString());
                    }
                    Debug.Log(errorCode + exception.ToString());
                }
            }
            else if (task.IsCompleted)
            {
                Debug.Log(operation + " completed");
                status = true;
            }
            return status;
        }

        /// <summary>Event handler for <see cref="Firebase.Messaging.FirebaseMessaging.TokenReceived"/>.</summary>
        /// <para> Used for debugging - token can be used to send test messages to a specific device via FCM.</para>
        /// <param name="sender">The source of the event.</param>
        /// <param name="token">The event args containing the token string.</param>
        public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
            Debug.Log("Registration Token Received: " + token.Token);
        }
        /// <summary>Event handler for <see cref="Firebase.Messaging.FirebaseMessaging.MessageReceived"/>.</summary>
        /// <para> Used for debugging - can check to see if messages are received from FCM.</para>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event args containing the <see cref="Firebase.Messaging.FirebaseMessage"/>.</param>
        public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        {
            Debug.Log("Received a new message from: " + e.Message.From);
        }
    }
}


