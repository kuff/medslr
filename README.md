# htlab
This project is inteded as a template for working with OVR hand tracking in Unity. The `TrackingScene` contains a minimal scene with everything needed to get started. In addition, the project comes with:
* The [Oculus Integration SDK](https://developer.oculus.com/downloads/package/unity-integration/) pre-installed and configured, along with XR Plugin Management.
* The [recommended projects settings](https://developer.oculus.com/documentation/unity/unity-conf-settings/) by Oculus already configured.
* A minimal [Assembly Definition](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html) structure to speed up compile times.
* [Git Large File Storage](https://git-lfs.github.com/) pre-installed and `.gitattributes` file pre-configured for Unity.

## Where to put your code
To speed up compile times, the project splits up your code from third party code to avoid recompiling files which have not changed. This means that your code should live in the `Assets/Project/Runtime` directory. This also means that you must update the `Project.Runtime.asmdef` Assembly Definition file when importing third party libraries. In some IDEs such as Rider 2022 this can be done for you automatically.

## How to launch the project
Due to a bug in the OVR hand tracking implementation of OpenXR, you must use different backends depending on how you launch the project. By default, the project uses the `Legacy LibOVR+VRAPI` backend, enabling you to run hand tracking in Unity PlayMode over Oculus Link. However, when you wish to build the project into an Android apk-file, you must change the backend to `OpenXR`. This is done through `Oculus > Tools > OVR Ultilities PLugin > Set OVRPlugin to OpenXR`. If you wish keep working in the editor afterwards you simply select `Set OVRPlugin to Legacy LibOVR+VRAPI` in the same menu. Remember to switch your platform to Android before building. You do not have to switch back afterwards.
