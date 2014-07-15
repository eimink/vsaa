using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace vsaa
{

	public enum messageType
	{
		SERVER_AUTH = -100,
		APPLICATION_EVENT_START = 0,
		APPLICATION_EVENT_QUIT,
		APPLICATION_EVENT_PAUSED,
		APPLICATION_EVENT_GENERIC
	}

	public class Communicator : MonoBehaviour {

		public string serverUrl;
		public string apiKey;
		public string apiSecret;
		public string platformStatus;

		private string appToken;
		private bool allowQuit = false;

		void Start () {
			Authenticate(); // Automatically tries to authenticate with server when the script is run.
		}

		/// <summary>
		/// Raises the application quit event. The quit will be delayed until the application quit event has been sent to the server.
		/// This is only supported by standalone players, so for webplayer build further trickery is needed.
		/// </summary>
		void OnApplicationQuit()
		{
			if (!allowQuit)
			{
				Application.CancelQuit();
			}
			StartCoroutine(SendRequest(messageType.APPLICATION_EVENT_QUIT,"",delegate(string retval){
				if (Application.isEditor)
					Debug.Log(retval);
				platformStatus = "OK!";
				allowQuit = true;
				Application.Quit();
			}));
		}
		/// <summary>
		/// Raises the application paused event. This is mostly used on iOS, since when game is sent to background this gets triggered.
		/// </summary>
		void OnApplicationPaused()
		{
			StartCoroutine(SendRequest(messageType.APPLICATION_EVENT_PAUSED,"",delegate(string retval){
				if (Application.isEditor)
					Debug.Log(retval);
				platformStatus = "OK!";
			}));
		}

		public void SendGenericEvent(string content)
		{
			StartCoroutine(SendRequest(messageType.APPLICATION_EVENT_GENERIC,content,delegate(string retval){
				if (Application.isEditor)
					Debug.Log(retval);
				platformStatus = "OK!";
			}));
		}

		private void Authenticate()
		{
			StartCoroutine(SendRequest(messageType.SERVER_AUTH,"",delegate(string rval){
				if (Application.isEditor)
					Debug.Log(rval);
				var dict = Json.Deserialize(rval) as Dictionary<string,object>;
				appToken = (string)dict["access_token"];
				StartCoroutine(SendRequest(messageType.APPLICATION_EVENT_START,"",delegate(string retval){
				if (Application.isEditor)
					Debug.Log(retval);
					platformStatus = "OK!";
				}));
			}));
		}

		private string CreateAuthString()
		{
			return System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(apiKey+":"+apiSecret));
		}

		IEnumerator SendRequest(messageType type, string content, Action<string> onRequestCompleted)
		{
			string url = serverUrl;
			WWW www;
			WWWForm form = new WWWForm();
			var headers = new Hashtable();
			switch (type)
			{
			case messageType.SERVER_AUTH:
				url += "login";
				form.AddField("grant_type","client_credentials");
				headers.Add("Authorization", "Basic "+CreateAuthString());
				www = new WWW(url, form.data, headers);
				break;
			case messageType.APPLICATION_EVENT_START:
				url += "event";
				headers.Add("Authorization", "Bearer "+appToken);
				form.AddField("Description","Application start");
				form.AddField("ApiKey",apiKey);
				form.AddField("DeviceId",SystemInfo.deviceUniqueIdentifier);
				www = new WWW(url, form.data, headers);
				break;
			case messageType.APPLICATION_EVENT_QUIT:
				url += "event";
				headers.Add("Authorization", "Bearer "+appToken);
				form.AddField("Description","Application quit");
				form.AddField("ApiKey",apiKey);
				form.AddField("DeviceId",SystemInfo.deviceUniqueIdentifier);
				www = new WWW(url, form.data, headers);
				break;
			case messageType.APPLICATION_EVENT_PAUSED:
				url += "event";
				headers.Add("Authorization", "Bearer "+appToken);
				form.AddField("Description","Application paused");
				form.AddField("ApiKey",apiKey);
				form.AddField("DeviceId",SystemInfo.deviceUniqueIdentifier);
				www = new WWW(url, form.data, headers);
				break;
			case messageType.APPLICATION_EVENT_GENERIC:
				url += "event";
				headers.Add("Authorization", "Bearer "+appToken);
				form.AddField("Description",content);
				form.AddField("ApiKey",apiKey);
				form.AddField("DeviceId",SystemInfo.deviceUniqueIdentifier);
				www = new WWW(url, form.data, headers);
				break;
			default:
				url += "token";
				form.AddField("grant_type","client_credentials");
				headers.Add("Authorization", "Basic "+CreateAuthString());
				www = new WWW(url, form.data, headers);
				break;
			}
			yield return www;
			if (string.IsNullOrEmpty(www.error))
				onRequestCompleted(www.text);
			else
			{
				Debug.Log(www.error);
				platformStatus = www.error;
			}
		}
	}

}