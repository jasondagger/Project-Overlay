
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

internal sealed class ServiceUnityWebRequests :
    MonoBehaviour
{
	internal delegate void WebRequestCallback(
		string responseText,
		long   responseCode,
		string error
	);

	internal enum WebRequestMethod :
	uint
	{
		Get = 0U,
		Post,
	}

	internal static bool IsResponseSuccessCode(
		long responseCode
	)
	{
		return
			responseCode >= 200U && 
			responseCode < 300U;
	}

	internal void SendWebRequest(
		string                     url,
		Dictionary<string, string> headers,
		WebRequestMethod           method,
		string                     body,
		WebRequestCallback         webRequestCallback
	)
	{
		var webRequestData = new WebRequestData(
			url:                url,
			headers:            headers,
			method:             method,
			body:               body,
			webRequestCallback: webRequestCallback
		);
		lock (this.m_lock)
		{
			this.m_webRequestDatas.Enqueue(
				item: webRequestData
			);
		}
	}

	private struct WebRequestData
	{
		internal WebRequestData(
			string                     url,
			Dictionary<string, string> headers,
			WebRequestMethod           method,
			string                     body,
			WebRequestCallback         webRequestCallback
		)
		{
			this.Url                = url;
			this.Headers            = headers;
			this.Method             = method;
			this.Body               = body;
			this.WebRequestCallback = webRequestCallback;
		}

		internal string                     Url                { get; set; }
		internal Dictionary<string, string> Headers            { get; set; }
		internal WebRequestMethod           Method             { get; set; }
		internal string                     Body               { get; set; }
		internal WebRequestCallback         WebRequestCallback { get; set; }
	}

	private readonly Queue<WebRequestData> m_webRequestDatas = new();
	private readonly object                m_lock            = new();

	private void Update()
	{
		this.ProcessWebRequests();
	}

	private void ProcessWebRequests()
	{
		WebRequestData webRequestData;
		lock (this.m_lock)
		{
			if (this.m_webRequestDatas.Count > 0U)
			{
				webRequestData = this.m_webRequestDatas.Dequeue();
			}
			else
			{
				return;
			}
		}

		switch (webRequestData.Method)
		{
			case WebRequestMethod.Get:
				this.StartCoroutine(
					this.SendWebRequestGet(
						webRequestData: webRequestData
					)
				);
				break;

			case WebRequestMethod.Post:
				this.StartCoroutine(
					this.SendWebRequestPost(
						webRequestData: webRequestData
					)
				);
				break;

			default:
				break;
		}
	}

	private System.Collections.IEnumerator SendWebRequestGet(
		WebRequestData webRequestData
	)
	{
		using var webRequest = UnityWebRequest.Get(
			uri: webRequestData.Url
		);

		yield return webRequest.SendWebRequest();

		webRequestData.WebRequestCallback?.Invoke(
			responseText: webRequest.downloadHandler.text,
			responseCode: webRequest.responseCode,
			error:        webRequest.error
		);
	}

	private System.Collections.IEnumerator SendWebRequestPost(
		WebRequestData webRequestData
	)
	{
		var bodyAsBytes = Encoding.UTF8.GetBytes(
			s: webRequestData.Body
		);

		using var webRequest = new UnityWebRequest(
			url:    webRequestData.Url,
			method: "POST"
		);

		webRequest.uploadHandler   = new UploadHandlerRaw(
			data: bodyAsBytes
		);
		webRequest.downloadHandler = new DownloadHandlerBuffer();

		foreach (var header in webRequestData.Headers)
		{
			webRequest.SetRequestHeader(
				name:  header.Key,
				value: header.Value
			);
		}

		yield return webRequest.SendWebRequest();

		webRequestData.WebRequestCallback?.Invoke(
			responseText: webRequest.downloadHandler.text,
			responseCode: webRequest.responseCode,
			error:        webRequest.error
		);
	}
}