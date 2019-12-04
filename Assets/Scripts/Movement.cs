//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.HelloAR
{
    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using UnityEngine;
    using UnityEngine.EventSystems;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    /// <summary>
    /// Controls the HelloAR example.
    /// </summary>
    public class Movement : MonoBehaviour
    {
        
        private bool m_IsQuitting = false;

        public Camera FirstPersonCamera;
        public GameObject CameraTarget;
        private Vector3 PrevARPosePosition;
        private bool Tracking = false;

        public void Start()
        {
            //set initial position
            PrevARPosePosition = Vector3.zero;
        }

        public void Update()
        {
            _UpdateApplicationLifecycle();

            //move the person indicator according to position
            Vector3 currentARPosition = Frame.Pose.position;
            if (!Tracking)
            {
                Tracking = true;
                PrevARPosePosition = Frame.Pose.position;
            }
            //Remember the previous position so we can apply deltas
            Vector3 deltaPosition = currentARPosition - PrevARPosePosition;
            PrevARPosePosition = currentARPosition;
            if (CameraTarget != null)
            {
                // The initial forward vector of the sphere must be aligned with the initial camera  direction in the XZ plane.
             // We apply translation only in the XZ plane.
             CameraTarget.transform.Translate(deltaPosition.x, 0.0f, deltaPosition.z);
                // Set the pose rotation to be used in the CameraFollow script
                FirstPersonCamera.GetComponent<ArrowDirection>().targetRot = Frame.Pose.rotation;
            }
        }

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void _UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to
            // appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage(
                    "ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject =
                        toastClass.CallStatic<AndroidJavaObject>(
                            "makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
