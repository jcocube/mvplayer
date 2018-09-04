using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**
 * SUPER MULTIVIEW PLAYER FOR HEAD-MOUNTED DEVICES
 * Setting the player
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

public class Configuration : MonoBehaviour
{
	// Name of the video files of the selected sequence (ex: champa for files champa0000 to champa0079)
	public InputField seq_name;
	// Total number of video files of the selected sequence (ex: 80)
	public InputField seq_videos;
	// Baseline or interocular distance (in camera gaps)
	public InputField inter_dist;
	// Number of videos playing in background (background playback range)
	public InputField numVideos;
	// Video width
	public InputField width;
	// Video height
	public InputField height;
	// Switching distance: distance that the user has to move the head to change to the next/previous view (mm)
	public InputField switch_dist;
	// Video/Frame flag (video=1, frame=0)
	public InputField is_video;
	
	// We put an example value to the input fields
	public void Start()
	{
		seq_name.text = PlayerPrefs.GetString("conf_seq_name", "BBB_Flowers_cam");
		seq_videos.text = PlayerPrefs.GetString("conf_seq_videos", "91");
		inter_dist.text = PlayerPrefs.GetString("conf_inter_dist", "1");
		numVideos.text = PlayerPrefs.GetString("conf_numVideos", "40");
		width.text = PlayerPrefs.GetString("conf_width", "640");
		height.text = PlayerPrefs.GetString("conf_height", "384");
		switch_dist.text = PlayerPrefs.GetString("conf_switch_dist", "7");
		is_video.text = PlayerPrefs.GetString("conf_is_video", "1");
	}
	
	// When the "Continue" button is pressed (or keypad enter or esc key), set the new values for each field
	public void Continuar()
	{
		PlayerPrefs.SetString("conf_seq_name", seq_name.text);
		PlayerPrefs.SetString("conf_seq_videos", seq_videos.text);
		PlayerPrefs.SetString("conf_inter_dist", inter_dist.text);
		PlayerPrefs.SetString("conf_numVideos", numVideos.text);
		PlayerPrefs.SetString("conf_width", width.text);
		PlayerPrefs.SetString("conf_height", height.text);
		PlayerPrefs.SetString("conf_switch_dist", switch_dist.text);
		PlayerPrefs.SetString("conf_session","");
		PlayerPrefs.SetString("conf_is_video",is_video.text);
		// By default, the screens are located at 3*height
		PlayerPrefs.SetString("conf_h","3");
		// Load the main scene
		SceneManager.LoadScene(1);
	}

	public void Update()
	{
		// If Esc or Enter key pressed, execute "Continuar" function
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Continuar();
	}
	
}