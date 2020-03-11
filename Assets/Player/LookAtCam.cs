﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCam : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }
	
    private void Update()
    {
        Vector3 toCam = cam.transform.position - transform.position;
        toCam.y = 0;
        //transform.rotation = Quaternion.LookRotation(-toCam, Vector3.up);
    }

	private void LateUpdate()
	{
		transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
			cam.transform.rotation * Vector3.up);
	}
}
