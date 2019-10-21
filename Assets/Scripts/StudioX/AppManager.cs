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
    using RemoteAssetBundleTools;
    ///<summary>Singleton class to be used to manage scene changes, Firebase support, and any other housekeeping</summary>
    public class AppManager : MonoBehaviour
    {
        private static AppManager _instance;
        private static readonly object appManagerLock = new object();
        private bool firebaseReady;
        private Firebase.FirebaseApp firebaseApp;
        private Version MinFirebaseSdkVersion = new Version("6.3.0");
        private const string _sdkNotFoundVersion = "0.0.0";
        private FullscreenFade FadeController { get; set; }
        private enum AppState { Loading, Landing, Menu, VR, Default };
        private AppState CurrentAppState { get; set; }
        // TODO make this a property to set cardboard or daydream
        private const string VRDevice = "cardboard";
        private List<Canvas> loadingScreenCanvases;
        private RemoteAssetBundleMapper bundleMapper;
        public Dictionary<string, List<AssetBundle>> LoadedBundles { get; set; }
        public static string FirebaseSdkDir { get; set; }
        public Version FirebaseSdkVersion { get; set; }
        public string landingScene;
        public int landingDuration = 3;
        public GameObject loadingScreen;
        public string menuScene;
        public GameObject remoteAssetBundleMapper;

        ///<summary>The optional name of the Firebase Messaging Topic the app will subscribe to.false Defaults to an empty string.
        ///<para><see cref="Firebase.Messaging.FirebaseMessaging.SubscribeAsync(string)"/> for more details</para> 	
        ///</summary>
        public string firebaseMessagingTopic;

        ///<summary>Static thread-safe singleton instance of AppManager</summary>
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

        public static AppManager GetManager()
        {
            return GameObject.FindObjectOfType<AppManager>();
        }

        public void Start()
        {
            InitLoadingScreen();
            InitFirebase();
            StartCoroutine(CheckForRemoteContent());
            LoadedBundles = new Dictionary<string, List<AssetBundle>>();
            // DisplayLanding();
            // StartCoroutine(LoadLocalAssetBundles());
        }

        public void OnDestroy()
        {
            if (bundleMapper)
            {
                bundleMapper.UnloadAllBundles();
                bundleMapper.OnAllAssetBundlesLoaded -= HandleAllAssetBundlesLoaded;
                bundleMapper.OnManifestLoadingError -= HandleRemoteAssetBundleError;
                bundleMapper.OnAssetBundleLoadingError -= HandleRemoteAssetBundleError;
            }
        }

        private IEnumerator CheckForRemoteContent()
        {
            if (remoteAssetBundleMapper)
            {
                bundleMapper = remoteAssetBundleMapper.GetComponent<RemoteAssetBundleMapper>();
                bundleMapper.ToggleProgressBar(false);
                yield return FadeAsync(true);
                bundleMapper.ToggleProgressBar(true);
                bundleMapper.OnAllAssetBundlesLoaded += HandleAllAssetBundlesLoaded;
                bundleMapper.OnManifestLoadingError += HandleRemoteAssetBundleError;
                bundleMapper.OnAssetBundleLoadingError += HandleRemoteAssetBundleError;
                bundleMapper.GetUpdatedContent();
            }
        }

        private void HandleAllAssetBundlesLoaded()
        {
            foreach(RemoteAssetBundleMapper.RemoteAssetBundleMap map in bundleMapper.remoteAssetBundleMaps)
            {
                LoadedBundles[map.assetBundleKey] = map.Bundles;
            }
            StartCoroutine(TransitionToLanding());
        }

        private void HandleRemoteAssetBundleError(string message)
        {
            // SHOW BUTTONS TO RETRY OR SKIP HERE

        }

        private IEnumerator TransitionToLanding()
        {
            yield return FadeAsync(false);
            DisplayLanding();
        }

        public void InitLoadingScreen()
        {
            if (loadingScreen)
            {
                loadingScreenCanvases = new List<Canvas>();
                loadingScreen = Instantiate(loadingScreen, new Vector3(0, 0, 0), Quaternion.identity);
                foreach(Transform child in loadingScreen.transform)
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

        public IEnumerator HideLoadingScreen()
        {
            Coroutine fade = StartCoroutine(FadeAsync(false));
            yield return fade;
            loadingScreen.SetActive(false);
        }

        public IEnumerator ShowLoadingScreen()
        {
            Coroutine fade = StartCoroutine(FadeAsync(false, true));
            yield return fade;
            loadingScreen.SetActive(true);
            fade = StartCoroutine(FadeAsync(true));
            yield return fade;
        }

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

        public void ShowMenuScene()
        {
            if (!string.IsNullOrWhiteSpace(menuScene))
            {
                StartCoroutine(LoadAsyncScene(menuScene, AppState.Menu));
            }
        }

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

        /*private IEnumerator LoadLocalAssetBundles()
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
        }*/

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

        public void LoadScene(string sceneName, bool showLoadingScreen = true)
        {
            StartCoroutine(LoadAsyncScene(sceneName, showLoadingScreen));
        }

        private void LoadScene(string sceneName, AppState end, bool showLoadingScreen = true)
        {
            StartCoroutine(LoadAsyncScene(sceneName, end, showLoadingScreen));
        }

        public void LoadVRScene(string sceneName)
        {
            StartCoroutine(LoadAsyncVRScene(sceneName));
        }

        public void LoadMenu()
        {
            LoadScene(menuScene, AppState.Menu);
        }

        private IEnumerator LoadAsyncVRScene(string sceneName)
        {
            Coroutine coroutine = StartCoroutine(LoadAsyncScene(sceneName, AppState.VR));
            yield return coroutine;
            coroutine = StartCoroutine(EnableVR());
            yield return coroutine;
        }

        private IEnumerator EnableVR()
        {
            if (string.Compare(XRSettings.loadedDeviceName, VRDevice, true) != 0)
            {
                XRSettings.LoadDeviceByName(VRDevice);
                yield return null;
            }
            XRSettings.enabled = true;
        }

        private IEnumerator Enable2D()
        {
            XRSettings.LoadDeviceByName("");
            yield return null;
            ResetCameras();
        }

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

        ///<summary>Checks the Firebase SDK version based on maven-metadata.xml found in the package.
        ///<para>Sets <see cref="AppManager.FirebaseSdkVersion)"/> if found. If not, the version is set to 0.0.0</para> 	
        ///</summary>
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

        ///<summary>Initializes the Firebase app, checks for dependencies. 
        ///<para>Responsible for setting StudioX.AppManager.firebaseApp and StudioX.AppManager.firebaseReady if <see cref="Firebase.DependencyStatus"/> is set to Available.</para>
        ///<para>See <see cref="Firebase.FirebaseApp.CheckAndFixDependenciesAsync"/> for more details</para> 	
        ///</summary>
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

        ///<summary>Subscribes StudioX.AppManager.OnTokenReceived and StudioX.AppManager.OnMessageReceived to 
        /// Firebase messaging events <see cref="Firebase.Messaging.FirebaseMessaging.TokenReceived"/> and 
        /// <see cref="Firebase.Messaging.FirebaseMessaging.MessageReceived"/>
        ///<para>See <see cref="Firebase.Messaging.FirebaseMessaging"/> for more details</para> 	
        ///</summary>
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

        ///<summary>Logs the status of a <see cref="System.Threading.Tasks.Task"/> 
        ///<para>Responsible for setting StudioX.AppManager.firebaseApp and StudioX.AppManager.firebaseReady if <see cref="Firebase.DependencyStatus"/> is set to Available.</para>
        ///<para>See <see cref="Firebase.FirebaseApp.CheckAndFixDependenciesAsync"/> for more details</para>
        ///<param name="task">The task</param>
        ///<param name="operation">The name of the operation (for logging purposes only)</param> 	
        ///</summary>
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

        ///<summary>Event handler for <see cref="Firebase.Messaging.FirebaseMessaging.TokenReceived"/>
        ///<para> Used for debugging - token can be used to send test messages to a specific device via FCM </para>
        ///<param name="sender">The source of the event</param>
        ///<param name="token">The event args containing the token string</param>
        ///</summary>
        public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
            Debug.Log("Registration Token Received: " + token.Token);
        }
        ///<summary>Event handler for <see cref="Firebase.Messaging.FirebaseMessaging.MessageReceived"/>
        ///<para> Used for debugging - can check to see if messages are received from FCM</para>
        ///<param name="sender">The source of the event</param>
        ///<param name="e">The event args containing the <see cref="Firebase.Messaging.FirebaseMessage"/></param>
        ///</summary>
        public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        {
            Debug.Log("Received a new message from: " + e.Message.From);
        }
    }
}


