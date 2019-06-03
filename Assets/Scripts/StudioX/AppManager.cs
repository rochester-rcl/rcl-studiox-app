namespace StudioX {
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System;
	using UnityEngine;
	///<summary>Singleton class to be used to manage scene changes, Firebase support, and any other housekeeping</summary>
	public class AppManager : MonoBehaviour {
		private static AppManager _instance;
		private static readonly object appManagerLock = new object();
		private bool firebaseReady;
		private Firebase.FirebaseApp firebaseApp;
		private Version MinFirebaseSdkVersion = new Version("6.0.0");
		private const string _sdkNotFoundVersion = "0.0.0";
		public static string FirebaseSdkDir {get; set;}
		public Version FirebaseSdkVersion {get; set;}

		///<summary>The optional name of the Firebase Messaging Topic the app will subscribe to.false Defaults to an empty string.
		///<para><see cref="Firebase.Messaging.FirebaseMessaging.SubscribeAsync(string)"/> for more details</para> 	
		///</summary>
		[SerializeField]
		[Tooltip("Firebase Messaging Channel to Subscribe To")]
		public string topic;

		///<summary>Static thread-safe singleton instance of AppManager</summary>
		public static AppManager Instance {
			get {
				lock(appManagerLock) {
					return _instance; 
				} 	
			}
		}

		///<summary>Checks the Firebase SDK version based on maven-metadata.xml found in the package.
		///<para>Sets <see cref="AppManager.FirebaseSdkVersion)"/> if found. If not, the version is set to 0.0.0</para> 	
		///</summary>
		private void CheckFirebaseSDKVersion () {
			string sdkVersion = _sdkNotFoundVersion;
			if (System.IO.Directory.Exists(FirebaseSdkDir)) {
				try {
					System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
					doc.Load(string.Format("{0}m2repository/com/google/firebase/firebase-app-unity/maven-metadata.xml", FirebaseSdkDir));
					System.Xml.XmlNodeList versions = doc.SelectNodes("metadata/versioning/release");
					if (versions.Count > 0) sdkVersion = versions[0].InnerText;
				} catch (Exception ex) {
					Debug.LogWarning(ex);
				}
			}
			FirebaseSdkVersion = new Version(sdkVersion);
		}

		public void Start () {
			InitFirebase();
		}

		public void Awake() {
			if (string.IsNullOrEmpty(FirebaseSdkDir)) {
				FirebaseSdkDir = string.Format("{0}/Firebase/", Application.dataPath);
			}
			if (_instance != null && _instance != this) {
				Debug.LogWarning("Another instance of AppManager was found in the scene. Destroying the current instance.");
				Destroy(gameObject);
			} else {
				_instance = this;
				DontDestroyOnLoad(gameObject);
			}
		}
		
		public void Update () {

		}

		///<summary>Initializes the Firebase app, checks for dependencies. 
		///<para>Responsible for setting StudioX.AppManager.firebaseApp and StudioX.AppManager.firebaseReady if <see cref="Firebase.DependencyStatus"/> is set to Available.</para>
		///<para>See <see cref="Firebase.FirebaseApp.CheckAndFixDependenciesAsync"/> for more details</para> 	
		///</summary>
		public void InitFirebase() {

			# if UNITY_EDITOR
				CheckFirebaseSDKVersion();
				if (FirebaseSdkVersion.Equals(new Version(_sdkNotFoundVersion))) {
					Debug.LogError("No Firebase SDK found. Are you sure you imported the packages?");
				} else if (!FirebaseSdkVersion.Equals(MinFirebaseSdkVersion)) {
					Debug.LogWarning(string.Format("Firebase SDK Version ({0}) is different than the one used for this project ({1}). This may result in errors.", FirebaseSdkVersion, MinFirebaseSdkVersion));
				}
			# endif

			// From https://firebase.google.com/docs/cloud-messaging/unity/client
			Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
				var dependencyStatus = task.Result;
				if (dependencyStatus == Firebase.DependencyStatus.Available) {
					firebaseReady = true;
					firebaseApp = Firebase.FirebaseApp.DefaultInstance;
					InitMessaging();
				} else {
					UnityEngine.Debug.LogError(System.String.Format("Could not resolve all Firebase dependenceis: {0}", dependencyStatus));
				}
			});
		}

		///<summary>Subscribes StudioX.AppManager.OnTokenReceived and StudioX.AppManager.OnMessageReceived to 
		/// Firebase messaging events <see cref="Firebase.Messaging.FirebaseMessaging.TokenReceived"/> and 
		/// <see cref="Firebase.Messaging.FirebaseMessaging.MessageReceived"/>
		///<para>See <see cref="Firebase.Messaging.FirebaseMessaging"/> for more details</para> 	
		///</summary>
		private void InitMessaging() {
			if (firebaseReady) {
				Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
				Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
				if (!String.IsNullOrEmpty(topic)) {
					Firebase.Messaging.FirebaseMessaging.SubscribeAsync(topic).ContinueWith(task => {
						LogTaskStatus(task, "SubscribeAsync");
					});
				}
			} else {
				Debug.Log("Could not initialize Firebase event handlers");
			}
		}

		///<summary>Logs the status of a <see cref="System.Threading.Tasks.Task"/> 
		///<para>Responsible for setting StudioX.AppManager.firebaseApp and StudioX.AppManager.firebaseReady if <see cref="Firebase.DependencyStatus"/> is set to Available.</para>
		///<para>See <see cref="Firebase.FirebaseApp.CheckAndFixDependenciesAsync"/> for more details</para>
		///<param name="task">The task</param>
		///<param name="operation">The name of the operation (for logging purposes only)</param> 	
		///</summary>
		private bool LogTaskStatus(Task task, string operation) {
			bool status = false;
			if (task.IsCanceled) {
				Debug.Log(operation + " task was cancelled");
			} else if (task.IsFaulted) {
				Debug.Log(operation + " encountered an error");
				foreach (Exception exception in task.Exception.Flatten().InnerExceptions) {
					string errorCode = "";
					Firebase.FirebaseException fbException = exception as Firebase.FirebaseException;
					if (fbException != null) {
						errorCode = String.Format("Error.{0}: ", ((Firebase.Messaging.Error)fbException.ErrorCode).ToString());
					}
					Debug.Log(errorCode + exception.ToString());
				}
			} else if (task.IsCompleted) {
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
		public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
			Debug.Log("Registration Token Received: " + token.Token);
		}
		///<summary>Event handler for <see cref="Firebase.Messaging.FirebaseMessaging.MessageReceived"/>
		///<para> Used for debugging - can check to see if messages are received from FCM</para>
		///<param name="sender">The source of the event</param>
		///<param name="e">The event args containing the <see cref="Firebase.Messaging.FirebaseMessage"/></param>
		///</summary>
		public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
			Debug.Log("Received a new message from: " + e.Message.From);
		}
	}
}


