﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;

public class SnapshotCamera : MonoBehaviour {

	public int imageCount = 0;
	private TextMesh overlayTextTop;
	private TextMesh overlayTextBottom;
	private string userName;


	// Use this for initialization
	void Start ()
	{
		overlayTextTop = gameObject.transform.Find("OverlayTextTop").GetComponent<TextMesh>();
		overlayTextBottom = gameObject.transform.Find("OverlayTextBottom").GetComponent<TextMesh>();

		userName = Environment.UserName;
		//userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
		userName = userName.Replace(@"\", "_");
		//
		// NOTE: in networked multiuser environment may be necessary to use:
		// System.Security.Principal.WindowsIdentity.GetCurrent().Name;
		// https://stackoverflow.com/questions/1240373/how-do-i-get-the-current-username-in-net-using-c
		//
		Debug.Log(userName);

	}

	public void CamCapture()
	{
		Camera Cam = GetComponent<Camera>();

		RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = Cam.targetTexture;

		Cam.Render();

		Texture2D Image = new Texture2D(Cam.targetTexture.width, Cam.targetTexture.height);
		Image.ReadPixels(new Rect(0, 0, Cam.targetTexture.width, Cam.targetTexture.height), 0, 0);
		Image.Apply();
		RenderTexture.active = currentRT;

		var Bytes = Image.EncodeToPNG();
		Destroy(Image);

		string directoryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/PeppySnapshots";

		//check if directory doesn't exit
		if (!Directory.Exists(directoryPath))
		{
			//if it doesn't, create it
			Directory.CreateDirectory(directoryPath);

		}
	
		//File.WriteAllBytes(Application.dataPath + "/Snapshots/" + FileCounter + ".png", Bytes);
		File.WriteAllBytes(directoryPath + "/" + userName + "_PeppySnapshot_" + imageCount + ".png", Bytes);

		imageCount++;

		
	}

	private void UpdateOverlay()
	{
		overlayTextTop.text = "Snapshot: " + imageCount.ToString();
		overlayTextBottom.text = userName + " " + DateTime.Now.ToString();



		


		//ToShortTimeString();
	}

	// Update is called once per frame
	void Update ()
	{
		UpdateOverlay();	
	}
}

// https://forum.unity.com/threads/how-to-save-manually-save-a-png-of-a-camera-view.506269/
