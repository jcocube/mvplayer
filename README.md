# smvplayer
smvplayer is a Super MultiView (SMV) player designed to play SMV content (video and static frames) with Head-Mounted Devices (HMD). Additionally, smvplayer can be also used without needing a HMD, simulating the HMD movement using the keyboard.

## Content

This project contains two main folders: 'build' and 'src'. 'Build' contains a build version of the Unity project ready to run on Windows, while 'src' contains the original Unity Project


## Requirements

* As just explained, the 'build' folder contains a .exe file with the player ready to run on Windows platforms. 
* The player has been tested,
  - in a computer with the following characteristics: Intel Core i7-4790 CPU 3.60GHz, RAM 32GB, 64bit OS, Windows 10 OS, NVIDIA GeForce GTX970.
  - using the Oculus Rift DK2 headset, but should also run in Oculus CV1.
  - with the support of Oculus Runtime software, that must be installed in the computer
  - and using Unity 3D 2017.3.1f1 software (only needed if you desire to modify the source code).

## BUILD

### The 'SMVPlayer_Data' folder inside this build contains the 'StreamingAssets' folder where the SMV videos or frames that want to be played must be located.
#### The SMV videos must be in the 'SMVPlayer_Data/StreamingAssets/videos' path in WEBM's format. All the videos of a same sequence must have the same prefix in the name, followed by a 4 digits number. For example: BBB_Flowers_cam0000,BBB_Flowers_cam0001,...,BBB_Flowers_cam0089,BBB_Flowers_cam0090. 
#### The SMV frames must be in the 'SMVPlayer_Data/StreamingAssets/frames' path in PNG's format. All the frames of a same content must have the same prefix in the name, followed by a 4 digits number. For example: frame0_champa0000,frame0_champa0001,...,frame0_champa0078,frame0_champa0079.


## SRC

### Additionally, the source code is also provided. It contains two scenes (each one with an associated script) one where the SMV content settings must be specified, and the main scene where the playback takes place.
### The project can be easily adapted to run with other HMD headset than Oculus and built to other platforms (Mac, Linux...).


## User manual

### Configuration/Settings screen:
#### Video name format: Prefix of the video files names common in all the files. For example, for the sequence of 91 videos with videos from BBB_Butterfly_cam0000 to BBB_Butterfly_cam0090, the video name format will be BBB_Butterfly_cam.
#### Number of cameras/videos: The total number of videos in the sequence.
#### Interocular distance (stereo baseline): distance between left and right views (measured in number of camera gaps).
#### Number of background views: number of views that are played simultaneously in the background, and prepared to be displayed on the screen. 
#### Video resolution: Resolution of the input videos/frames. We recommend the use of SD resolutions.
#### Switching distance: horizontal distance between two consecutive views, i.e. distance that the head needs to move to switch to the contiguous view.
####Type of content: 1 for SMV video, 0 for SMV frames.

### KeyMap
#### A: virtually move the camera to the left
#### D: virtually move the camera to the right
#### Esc, Enter or C: go to the player scene
#### M: increase the light intensity
#### N: decrese the light intensity
#### S: force re-synchronization
#### Up: moving the screens closer to the camera
#### Down: moving the screens further to the camera


## Notes

### The system is multiplaform but the provided ready-to-run application (build folder) has been built for Windows platform. To run it in other OS, please open the Unity 3D software (src folder) and build it for the desired platform.
### The system should also work for the consumer version CV1 of the Oculus HMD. To use with other HMDs, such as the HTC Vive, please open the Unity scene and change the main camera object.
### After running the application, the Oculus Runtime should automatically starts. If not, please do it manually.
