using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;  
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.XR;

/**
 * SUPER MULTIVIEW PLAYER FOR HEAD-MOUNTED DEVICES
 * Main code
 * 
 * Javier Cubelos Ordás
 * 
 * Grupo de Tratamiento de Imágenes
 * Escuela Técnica Superior de Ingenieros de Telecomunicación (E.T.S.I. Telecomunicación)
 * Universidad Politécnica de Madrid (U.P.M.)
 * 2018
 * 
 * 	  This file is part of SMVPlayer.
 *
 *    SMVPlayer is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU Lesser General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    SMVPlayer is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU Lesser General Public License for more details.
 *
 *    You should have received a copy of the GNU Lesser General Public License
 *    along with SMVPlayer.  If not, see <http://www.gnu.org/licenses/>.
 **/

public class Director : MonoBehaviour 
{

	// Sample of screen that will be instantiated for each video
	public GameObject screen;

	//VR Camera for movement
	public Transform cameraVR;
	
	// VR separated cameras for stereo
	public Transform cameraVRhmdL;
	public Transform cameraVRhmdR;
	public Transform cameraVRhmdC;

	// List of video players
	private List<VideoPlayer> videoPlayerList;
	
	// Light
	public Transform light;

	// Position and rotation of the screens
	private Vector3 posPant;
	private Quaternion rotPant = new Quaternion(0,180,0,0);
	// Position of the HMD
	private int position = 0;
	// View indexes of both eyes
	private int view_left;
	private int view_right;
	
	// First update call flag
	private bool first = true;
	// Last update finshed flag
	private bool finished_lastupdate=true;
	// Name of the screen to enable/disable
	private String name_pos;
	// Finished loading videos flag
	private bool setup_finished=false;

	// Screen enables for each position
	private GameObject screen_selected;
	// Used to disable the rest of screens
	private GameObject screen_unselected;

	// Name of the sequence files (ex: BBB_Flowers_cam)
	private String seq_name;
	// Number of views in the sequence
	private int seq_videos;
	// Videos playing synchronously in background
	private int numVideos;
	// Width
	private int width;
	// Height
	private int height;
	// Interocular distance or baseline (in # camera gaps)
	private int inter_dist;
	// Switching distance (mm)
	private double switch_dist;
	// Number of videos played in each direction (half the total number of videos playing in background)
	private int left_videos;
	private int right_videos;

	// Last displayed camera in the left eye
	private int last_camera;
	// Maximum and minimum view indexes (e.g. not smaller than 0, not bigger than the total number of views)
	private int videoMin;
	private int videoMax;
	// View synchronization finished flag
	private bool finished_sync = true;
	// Light intensity in the screens
	private int light_intensity;
	// Is the content a video (1) or a frame (0)
	private int is_video;
	// Initial time to get the absolute time of each playback
	private float initTime;
	// First frame flag
	private bool flag_first_frame = true;

	// Texture for the left view when using frames
	Texture2D text_left = null;
	// Byte stream to load the frame textures for the left screen
    byte[] fileData_left;
    // Same for right screen
	Texture2D text_right = null;
    byte[] fileData_right;

	// Before executing anything
	void Awake()
	{
	// Getting the settings set in the configuration scene
	seq_name=PlayerPrefs.GetString("conf_seq_name");
	seq_videos=int.Parse (PlayerPrefs.GetString("conf_seq_videos"));
	inter_dist=int.Parse (PlayerPrefs.GetString("conf_inter_dist"));
	numVideos=int.Parse (PlayerPrefs.GetString("conf_numVideos"));
	width=int.Parse (PlayerPrefs.GetString("conf_width"));
	height=int.Parse (PlayerPrefs.GetString("conf_height"));
	switch_dist=(float.Parse (PlayerPrefs.GetString("conf_switch_dist")))/1000;
	is_video=int.Parse (PlayerPrefs.GetString("conf_is_video"));
	posPant= new Vector3(0,1,8*(float.Parse (PlayerPrefs.GetString("conf_h"))));
	left_videos = numVideos/2;
	right_videos = left_videos;
	// By default, the first displayed view will be the one in the center of the array
	last_camera = seq_videos/2;
	// Default intensity
	light_intensity = 1;

	// Recentering the cameras
	cameraVR.transform.position = new Vector3 (0,0,0);
	cameraVRhmdL.transform.position = new Vector3 (0,0,0);
	cameraVRhmdR.transform.position = new Vector3 (0,0,0);
	cameraVR.transform.localRotation = new Quaternion (0,0,0,0);
	cameraVRhmdL.transform.localRotation = new Quaternion (0,0,0,0);
	cameraVRhmdR.transform.localRotation = new Quaternion (0,0,0,0);
	}

	// After creating the scene, before the first update
	void Start () 
	{
		Application.runInBackground = true;
		// If video content
		if(is_video==1)
		{
			// Loading videos and playing selected ones
			LoadVideos();
		//If static content (frames)
		}
		else if(is_video==0)
		{
		// Creating instance of the screen for the left view
		GameObject pantalla=Instantiate(screen,posPant,rotPant);
		pantalla.transform.localScale = new Vector3(pantalla.transform.localScale.y*width/height,
		                                                      pantalla.transform.localScale.y,
		                                                      pantalla.transform.localScale.z);
		pantalla.transform.Rotate(180,0,0);
		// Putting the view index into the desired format for Unity's scene (2 digits)
		pantalla.name="view"+last_camera.ToString("00");
		name_pos = "view"+last_camera.ToString("00");
		// Finding the screen object
		screen_selected=GameObject.Find(name_pos);
		// Putting it to the left eye mask
		screen_selected.layer = LayerMask.NameToLayer("left_eye");
		// Enabling its renderer (making the screen visible)

		// Same for the right view screen
		screen_selected.GetComponent<Renderer>().enabled=true;
		GameObject pantalla2=Instantiate(screen,posPant,rotPant);
		pantalla2.transform.localScale = new Vector3(pantalla2.transform.localScale.y*width/height,
		                                                      pantalla2.transform.localScale.y,
		                                                      pantalla2.transform.localScale.z);
		pantalla2.transform.Rotate(180,0,0);
		pantalla2.name="view"+(last_camera+1).ToString("00");
		name_pos = "view"+(last_camera+1).ToString("00");
		screen_unselected=GameObject.Find(name_pos);
		screen_unselected.layer = LayerMask.NameToLayer("right_eye");
		screen_unselected.GetComponent<Renderer>().enabled=true;
		}

		// Getting the initial time
		initTime=Time.time;
	}

	void LoadVideos() 
	{
		// Initializing lists
		videoPlayerList = new List<VideoPlayer>(seq_videos);

		// For each of the video clips
		for(int i=0; i<seq_videos; i++)
		{
			// Creating a new instance of the screen
			GameObject pantalla=Instantiate(screen,posPant,rotPant);
			pantalla.transform.localScale = new Vector3(pantalla.transform.localScale.y*width/height,
		                                                      pantalla.transform.localScale.y,
		                                                      pantalla.transform.localScale.z);
			pantalla.transform.Rotate(180,0,0);
			pantalla.name="view"+(i).ToString("00");
        	pantalla.GetComponent<Renderer>().material = new Material(Shader.Find("UI/Default"));

			// Add VideoPlayer to the GameObject
			VideoPlayer videoPlayer = pantalla.AddComponent<VideoPlayer>();
			videoPlayerList.Add(videoPlayer);
			
			// VideoPlayer settings
			videoPlayerList[i].isLooping = true;
			videoPlayerList[i].playOnAwake = true;
			videoPlayerList[i].source = VideoSource.VideoClip;
			videoPlayerList[i].renderMode = VideoRenderMode.MaterialOverride;
			videoPlayerList[i].audioOutputMode = VideoAudioOutputMode.None;

			// We want to play from video clip not from url
			String url = Application.streamingAssetsPath + "/videos/" + seq_name+(i).ToString("0000")+".webm";			
			videoPlayerList[i].url = url;

			// Prepare the clip							
			videoPlayerList[i].Prepare();
			
			// Texture each of the videos to each of the scene objects
			pantalla.GetComponent<Renderer>().material.mainTexture =videoPlayerList[i].texture;
			pantalla.GetComponent<Renderer>().enabled=false;		
		}

		// For all the views comprised in the background playback range of views
		for(int i=(last_camera-left_videos); i<(last_camera+right_videos+inter_dist); i++)
		{
			// We play all the videos
			videoPlayerList[i].Play();

			// If they aren't synchronized (more than 3 frames of difference with the reference), we sync them
			if(i>(last_camera-left_videos) && Math.Abs(videoPlayerList[i].frame-videoPlayerList[i-1].frame)>3)
			{
				if(videoPlayerList[i].isPlaying && videoPlayerList[i-1].isPlaying)
				{
					videoPlayerList[i].frame = videoPlayerList[i-1].frame;		
				}
			}
		}

		// Setting the left view to the default (last_camera)
		view_left= last_camera;
		name_pos = "view"+view_left.ToString("00");
		// Getting the screen object, putting it to the left eye mask and enabling its renderer
	    screen_selected=GameObject.Find(name_pos);
	    screen_selected.layer = LayerMask.NameToLayer("left_eye");
	    screen_selected.GetComponent<Renderer>().enabled=true;

	    // Same for the right eye view (left + baseline)
	    view_right=view_left+inter_dist;
	   	name_pos = "view"+view_right.ToString("00");
	    screen_selected=GameObject.Find(name_pos);
	    screen_selected.layer = LayerMask.NameToLayer("right_eye");
	    screen_selected.GetComponent<Renderer>().enabled=true;

		// Flag, the setup is done
		setup_finished=true;
	}

	// Synchronization function, used to synchronized the views playing in background in each update call
	IEnumerator synchr(int vMin, int vMax)
	{
		// Flag to not re-synchronize while one synchronization is in process
		finished_sync=false;

		// For all the views
		for(int i=0;i<seq_videos; i++)
			{
			// If they are in the background playback range
			if(i>=vMin && i<=vMax){
				// If its not already playinh, prepare it and play it
				if(i>=0 && videoPlayerList[i].isPlaying==false)
				{
					videoPlayerList[i].Prepare();
					videoPlayerList[i].Play();
					// For left-most view of the comprised range
					if(i==vMin)
					{
						// Synchronizing the left-most view with the last-known one (last_camera)
						if(Math.Abs(videoPlayerList[i].frame-videoPlayerList[last_camera].frame)>3)
						videoPlayerList[i].frame = videoPlayerList[last_camera].frame;		
					}
					// For the rest, we synchronize with the left-most
					else if(i!=vMin && videoPlayerList[last_camera].isPlaying && videoPlayerList[i].isPlaying)
					{
						if(Math.Abs(videoPlayerList[i].frame-videoPlayerList[i-1].frame)>3)
						videoPlayerList[i].frame = videoPlayerList[i-1].frame;		
					}
				}
				// If they are playing but aren't synchronized, we sync them following the same protocol
				else if(i>0 && i!=vMin && Math.Abs(videoPlayerList[i].frame-videoPlayerList[i-1].frame)>3)
				{
					if(videoPlayerList[i].isPlaying && videoPlayerList[i-1].isPlaying)
					{
						videoPlayerList[i].frame = videoPlayerList[i-1].frame;		
					}
				}
				else if(i>0 && i==vMin && Math.Abs(videoPlayerList[i].frame-videoPlayerList[last_camera].frame)>3)
				{
					if(videoPlayerList[i].isPlaying && videoPlayerList[last_camera].isPlaying)
					{
						videoPlayerList[i].frame = videoPlayerList[last_camera].frame;		
					}
				}
			// If outside the background playback range, stop playing and preparing them for future playback
			}else{
				videoPlayerList[i].Stop();
				videoPlayerList[i].Prepare();
			}
		}
		// end of synchronization function
		finished_sync=true;
		yield return new WaitForSeconds(0.0f);
	}

	// In each update call (Unity frame refresh)
	void Update()
	{
		// If we are dealing with video content
		if(is_video==1)
		{
			// Getting the left-most and right-most views of the background playback range
			videoMin = Mathf.Clamp(last_camera - left_videos, 0, seq_videos - (inter_dist + 1));
	        videoMax = Mathf.Clamp(last_camera + right_videos, 0, seq_videos - 1);
	  
	  		// Synchronizing them
	        if(finished_sync)
			synchr(videoMin, videoMax);

			// If the setup hasn't finished, ignore the update call
			if(!setup_finished)
			{
				return;
			}

			// If the last update call has finished
			if(finished_lastupdate)
			{
				finished_lastupdate=false;

				// Keyboard options
				// D: virtually move the camera to the right
				// A: virtually move the camera to the left
				// Esc, Enter or C: go to the player scene
				// M: increase the light intensity
				// N: decrese the light intensity
				// S: force re-synchronization
				// Up: moving the screens further to the camera
				// Down: moving the screens closer to the camera

				if (Input.GetKey(KeyCode.D))
		        {
		        	cameraVR.transform.Translate(Vector3.right/2 * Time.deltaTime);
		        }

		        if (Input.GetKey(KeyCode.A))
		        {
		        	cameraVR.transform.Translate(Vector3.left/2 * Time.deltaTime);
		        }

                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.C))
                {
                    SceneManager.LoadScene(0);
                }

				if (Input.GetKeyDown(KeyCode.M))
		        {
		        	light.GetComponent<Light>().intensity = light_intensity + 1;
		        	light_intensity = light_intensity + 1;
		        }

		       	if (Input.GetKeyDown(KeyCode.N))
		        {
		        	light.GetComponent<Light>().intensity = light_intensity - 1;
		        	light_intensity = light_intensity - 1;
		        }
		        if (Input.GetKeyDown(KeyCode.S))
	        	{
					synchr(videoMin, videoMax);    	
	        	}

		        if (Input.GetKey("up"))
				{
					cameraVR.transform.Translate(Vector3.forward * Time.deltaTime);
				}
			
				if (Input.GetKey("down"))
				{
					cameraVR.transform.Translate(Vector3.back * Time.deltaTime);
				}
		        
		        // Getting the actual position of the virtual camera 
		        position = Mathf.Clamp((int)Math.Round (cameraVRhmdC.position.x/switch_dist + (seq_videos/2)),0,seq_videos-1);
		        // Getting the corresponding view index (left view)
		        view_left= Mathf.Clamp(position, 0, seq_videos - (inter_dist + 1));
		        // Getting the right view index from the left one (+baseline)
		        view_right= view_left+inter_dist;
		       
		        // If the obtained view index is different than the last one obtained
		        if(last_camera != position && !first)
		        {
		        	// We look for the screen corresponding to the actual view indexes and enable their renderer
			        for(int i=0;i<seq_videos;i++)
			        {
			        	if(i == view_left && i!= view_right)
			        	{
			        		name_pos = "view"+i.ToString("00");
				        	screen_selected=GameObject.Find(name_pos);
				        	screen_selected.layer = LayerMask.NameToLayer("left_eye");
				        	screen_selected.GetComponent<Renderer>().enabled=true;
			        	}
			        	else if(i == view_right && i!= view_left)
			        	{
			        		name_pos = "view"+i.ToString("00");
				        	screen_selected=GameObject.Find(name_pos);
				        	screen_selected.layer = LayerMask.NameToLayer("right_eye");
				        	screen_selected.GetComponent<Renderer>().enabled=true;
			           	}
			           	// If both view indexes are the same --> mono
			           	else if(i == view_left && i== view_right)
			           	{
			           		cameraVRhmdR.GetComponent<Camera>().cullingMask = -1;
			           		name_pos = "view"+i.ToString("00");
				        	screen_selected=GameObject.Find(name_pos);
				        	screen_selected.layer = LayerMask.NameToLayer("left_eye");
				        	screen_selected.GetComponent<Renderer>().enabled=true;
			           	}
			           	// We desable the renderers of the rest of views
			           	else
			           	{
							name_pos = "view"+i.ToString("00");
		        			screen_unselected=GameObject.Find(name_pos);
		        			screen_unselected.GetComponent<Renderer>().enabled=false;
			        	}		   
			        }
			        // Updating the last_camera index
			       	last_camera = view_left;
		        }
		        finished_lastupdate=true;
		        first=false;
		    }
		// If dealing with frames, same for frames
		}else if(is_video==0)
		{
				// Keyboard options
				// D: virtually move the camera to the right
				// A: virtually move the camera to the left
				// Esc, Enter or C: go to the player scene
				// M: increase the light intensity
				// N: decrese the light intensity
				// S: force re-synchronization
				// Up: moving the screens further to the camera
				// Down: moving the screens closer to the camera

				if (Input.GetKey(KeyCode.D))
		        {
		        	cameraVR.transform.Translate(Vector3.right/4 * Time.deltaTime);
		        }

		        if (Input.GetKey(KeyCode.A))
		        {
		        	cameraVR.transform.Translate(Vector3.left/4 * Time.deltaTime);
		        }

		        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.C))
	        	{
					SceneManager.LoadScene(0);  
	        	}

				if (Input.GetKeyDown(KeyCode.M))
		        {
		        	light.GetComponent<Light>().intensity = light_intensity + 1;
		        	light_intensity = light_intensity + 1;
		        }

		       	if (Input.GetKeyDown(KeyCode.N))
		        {
		        	light.GetComponent<Light>().intensity = light_intensity - 1;
		        	light_intensity = light_intensity - 1;
		        }	  

		        if (Input.GetKey("up"))
				{
					cameraVR.transform.Translate(Vector3.forward * Time.deltaTime);
				}
			
				if (Input.GetKey("down"))
				{
					cameraVR.transform.Translate(Vector3.back * Time.deltaTime);
				}

				position = Mathf.Clamp((int)Math.Round (cameraVRhmdC.position.x/switch_dist + (seq_videos/2)),0,seq_videos-1);
		        view_left= Mathf.Clamp(position, 0, seq_videos - (inter_dist + 1));
		        view_right= view_left+inter_dist;
		        
		        if(last_camera != position || flag_first_frame)
		        {
		        	// We load the corresponding left-view frame as a texture
		        	fileData_left = File.ReadAllBytes(Application.streamingAssetsPath + "/frames/" + seq_name+view_left.ToString("0000") +".png");
					text_left = new Texture2D(2, 2);
					text_left.LoadImage(fileData_left);

		        	//Texture2D text_left = Resources.Load(seq_name+view_left.ToString("0000")) as Texture2D;
		        	if(text_left==null)
		        	Debug.Log("error loading"+seq_name+view_left.ToString("0000"));
		        	

		        	// Same for the right-view frame
		        	fileData_right = File.ReadAllBytes(Application.streamingAssetsPath + "/frames/" + seq_name+view_right.ToString("0000") +".png");
					text_right = new Texture2D(2, 2);
					text_right.LoadImage(fileData_right);

		        	//Texture2D text_right = Resources.Load(seq_name+view_right.ToString("0000")) as Texture2D;
		        	screen_selected.GetComponent<Renderer>().material = new Material(Shader.Find("UI/Default"));
        			screen_unselected.GetComponent<Renderer>().material = new Material(Shader.Find("UI/Default"));

        			// Set both textures to both eyes
			        screen_selected.GetComponent<Renderer>().material.mainTexture = text_left;
			        screen_unselected.GetComponent<Renderer>().material.mainTexture = text_right;

			       	last_camera = view_left;
			       	flag_first_frame=false;
		        }
		}
	}

	// Print in the computer screen the horizontal position, the camera index of the left view, and the current time
	void OnGUI()
    {
		GUI.Label(new Rect(10, 20, 200, 50), "Position.x: " + cameraVRhmdC.position.x.ToString());
		GUI.Label(new Rect(10, 0, 200, 50), "Camera: " + last_camera.ToString());
		GUI.Label(new Rect(300, 0, 200, 50), "Time: " + ((Time.time)-initTime).ToString());
		GUI.Label(new Rect(300, 20, 200, 50), "Cam position: " + (((screen_selected.transform.position.z)-(cameraVR.transform.position.z))/screen_selected.transform.localScale.y) + " x height");
    }

}