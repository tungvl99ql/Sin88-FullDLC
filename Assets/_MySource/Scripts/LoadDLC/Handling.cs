using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Core.Server.Api
{
    public class Handling : MonoBehaviour
	{

		public string[] tempListReadInFile;

		private string path = "";
		void Start()
		{
		}
		public void OnDownLoadTxt(string tempUrl , Action callback = null)
        {
			StartCoroutine(DownloadTxt(tempUrl , callback ));
		}

		private IEnumerator DownloadTxt(string urlTxt , Action callback = null)
		{
			string url = urlTxt;
			path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                + "/Text/lang_" + App.languageCode.ToLower() + ".txt";
			Debug.Log(url + " URL  text");
			Debug.Log(path + " Path  text");
			//if (File.Exists(path))
			//{
			//	string allText = File.ReadAllText(path);
			//	tempListReadInFile = allText.Split('\n');
			//	SetDataToList();
			//}
			//else
   //         {
				Debug.Log(url+"TXT");
				WWW www = new WWW(url);
				yield return www;

				Directory.CreateDirectory(((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
					+ "/Text/");
				File.WriteAllBytes(path, www.bytes);
				string allText = File.ReadAllText(path);
				//Debug.Log("ALL Text : " + allText);
				tempListReadInFile = allText.Split('\n');
				SetDataToList();
				callback.Invoke();
            //}
        }

		public void SetDataToList()
		{
			foreach (var item in tempListReadInFile)
			{
				if (item.Contains("::"))
				{
					string[] KeyValue = item.Split(new string[] { "::" }, StringSplitOptions.None);

					App.listKeyText.Add(KeyValue[0], KeyValue[1]);

					//PlayerPrefs.SetString(KeyValue[0], KeyValue[1]);
				}
			}
			//if (File.Exists(path))
			//{
			//	File.Delete(path);
			//}

			string tempdefaultStatus = App.listKeyText["DEFAULT_STATUS"];

			CPlayer.defaultStatus = tempdefaultStatus.Split('<');
			taiDLC.instance.GetQuickChat();
		}
	}
}
