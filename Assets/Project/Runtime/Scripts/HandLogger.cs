using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

/*
 * Gather data on player hands/head and save to disc
 */
public class HandLogger : MonoBehaviour
{
    private const bool DO_LOGGING = true;
    private const int LOGGING_FREQUENCY = 2;

    public enum LogType
    {
        FPS,
        LeftHand,
        RightHand,
        Head,
        Focus,
        Memory,
        Quit,
        Time,
        WizardDecision,
        StateChange,
        HandEvent,
        HandCapture,
        SentenceTarget,
        GestureResult,
        InferenceResult,
        FusionResult
    }

    private OVRPlugin.HandState _hsLeft = new OVRPlugin.HandState();
    private OVRPlugin.HandState _hsRight = new OVRPlugin.HandState();
    private Transform _mainCameraTransform;
    
    private static readonly List<IEnumerable> _logQueue = new List<IEnumerable>();
#pragma warning disable CS0414
    private static bool _fileSystemOperationInProgress = false;
    private static readonly string _logFileName = $"/LOG_MOVE_ME_{System.DateTime.Now:HHmmss-ffff}.txt";
    private static float _pointOfLastWrite = -1f;
    private static int _frameCounter = 0;
    private static float _frameCountTimestamp = -1f;
    private static int _invokesSinceLastWrite = LOGGING_FREQUENCY - 1;
    private static readonly IList[] _previousLogs = new IList[8];
#pragma warning restore CS0414

    protected void OnEnable()
    {
        Application.lowMemory += LogLowMemory;
        Application.quitting += OnApplicationQuit;
    }

    protected void Start()
    {
        var cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        _mainCameraTransform = cameraObject!.transform;
        
        Log(LogType.Time);
    }

    protected void Update()
    {
#pragma warning disable CS0162
        if (!DO_LOGGING) return;
#pragma warning restore CS0162
        
        // // Update OVR Hand States
        // OVRPlugin.GetHandState(OVRPlugin.Step.Render, OVRPlugin.Hand.HandLeft,  ref _hsLeft);
        // OVRPlugin.GetHandState(OVRPlugin.Step.Render, OVRPlugin.Hand.HandRight, ref _hsRight);
        //
        // // Log player hands- and head data
        // Log(LogType.LeftHand,   listData: GetSortedValues(_hsLeft));
        // Log(LogType.RightHand,  listData: GetSortedValues(_hsRight));
        // Log(LogType.Head,       listData: GetSortedValues(_mainCameraTransform));
        
        // Determine if FPS is to be logged in this frame
        _frameCounter++;
        _frameCountTimestamp += Time.deltaTime;
        if (_frameCountTimestamp is -1 or > 1)
        {
            // Log FPS count and reset
            Log(LogType.FPS, listData: new List<int> { _frameCounter }, ignorePrevious:true);
            
            _frameCounter = 0;
            _frameCountTimestamp = 0;
            
            // Write data to disc after x seconds
            _invokesSinceLastWrite++;
            if (_invokesSinceLastWrite >= LOGGING_FREQUENCY && !_fileSystemOperationInProgress)
            {
                WriteToDisc();
                _invokesSinceLastWrite = 0;
            }
        }
    }

    private static IEnumerable<object> GetSortedValues(OVRPlugin.HandState inputHandState)
    {
        // Hardcoded value return for HandState objects
        var result = new List<object>
        {
            inputHandState.Status,
            inputHandState.HandConfidence,
            inputHandState.FingerConfidences,
            inputHandState.RootPose,
            inputHandState.PointerPose,
            inputHandState.BoneRotations,
            inputHandState.HandScale,
            inputHandState.Pinches,
            inputHandState.PinchStrength,
            inputHandState.RequestedTimeStamp,
            inputHandState.SampleTimeStamp
        };
        
        return result;
    }
    
    private static IEnumerable<object> GetSortedValues(Transform inputTransform)
    {
        // Hardcoded value return for Transform objects
        var result = new List<object>
        {
            inputTransform.position, 
            inputTransform.rotation, 
            inputTransform.forward
        };
        return result;
    }

    private static void LogLowMemory()
    {
        Log(LogType.Memory);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        Log(LogType.Focus, listData: new List<bool> { hasFocus });
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Provide end-of-file signifier and log the remaining data from memory before quitting
        // NOTE: We do this in the pause event because the quit event on Android is unreliable
        Log(LogType.Quit, ignorePrevious:true);
        WriteToDisc();
    }

    private void OnApplicationQuit()
    {
        OnApplicationFocus(true);
    }

    public static void LogWizardDecision(int wasSuccessful)
    {
        Log(LogType.WizardDecision, new int[] { wasSuccessful }, true);
    }
    
    public static void LogStateChange(int stateIndex)
    {
        Log(LogType.StateChange, new int[] { stateIndex }, true);
    }
    
    public static void LogHandEvent(int eventIndex)
    {
        Log(LogType.HandEvent, new int[] { eventIndex }, true);
    }

    public static void Log(LogType type, IEnumerable listData = default, bool ignorePrevious = false)
    {
        var data = listData?.Cast<object>().ToList();  // Cast input data to list
        
        // Check to make sure provided data is unique from previous log
        if (!ignorePrevious && _previousLogs[(int)type] is not null && data is not null && data.Count == _previousLogs[(int)type].Count)
        {
            try
            {
                if (type is LogType.LeftHand or LogType.RightHand)
                {
                    // Handle hand comparisons differently from other data by looking at orientation specifically
                    var compDataNew = (OVRPlugin.Posef)data[3];
                    var compDataOld = (OVRPlugin.Posef)_previousLogs[(int)type][3];
                    var orientation1 = compDataNew.Orientation;
                    var orientation2 = compDataOld.Orientation;
                    if (orientation1.Equals(orientation2)) return;
                }
                else
                {
                    // Compare other objects by turning them to json and comparing the strings
                    // TODO: Come up with a more performant approach to this
                    var obj1 = JsonUtility.ToJson(data);
                    var obj2 = JsonUtility.ToJson(_previousLogs[(int)type]);
                    if (obj1 == obj2) return;
                }
            }
            catch
            {
                // Ignored...
            }
        }
        
        // Define the beginning of the log string with LogType and timestamp
        var baseString = "" + (int) (Time.realtimeSinceStartup * 10000) + " " + (int)type + " ";

        // Parse log command
        switch (type) 
        {
            case LogType.FPS:
                _logQueue.Add(baseString + data![0]);
                break;
            case LogType.LeftHand:
            case LogType.RightHand:
            case LogType.Head:
                _logQueue.Add(baseString + BuildRecursively(data));  // Parse complex objects recursively
                break;
            case LogType.Focus:
                _logQueue.Add(baseString + ((bool)data![0] ? 1 : 0));
                break;
            case LogType.Memory:
                _logQueue.Add(baseString);
                break;
            case LogType.Quit:
                _logQueue.Add(baseString);
                break;
            case LogType.Time:
                _logQueue.Add(baseString + DateTime.Now);
                break;
            case LogType.WizardDecision:
            case LogType.StateChange:
            case LogType.HandEvent:
                _logQueue.Add(baseString + (int)data![0]);
                break;
            case LogType.HandCapture:
            case LogType.SentenceTarget:
            case LogType.GestureResult:
            case LogType.InferenceResult:
            case LogType.FusionResult:
                _logQueue.Add(baseString + string.Concat(data));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        string BuildRecursively(IEnumerable input)
        {
            var result = "";  // What will become the end result string
            var e = input.GetEnumerator();
            while (e.MoveNext())
            {
                var elem = e.Current;
                
                // Handle predictable values of elem
                switch (elem)
                {
                    case OVRPlugin.HandStatus:
                        elem = "" + (int)elem;
                        break;
                    case Quaternion:
                    case Vector3:
                    case OVRPlugin.Vector3f:
                    case OVRPlugin.Quatf:
                    case OVRPlugin.Posef:
                        // Extract object values through Reflection
                        var elemType = elem.GetType();
                        var elemProperties = elemType.GetFields
                            (BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
                        var elemSelection = elemProperties.Select(property => 
                            property.GetValue(elem)).ToList();
                        elem = elemSelection;
                        break;
                    case null:
                        elem = "n";
                        break;
                }
                
                // Parse and save elem
                if (elem is ICollection or IList) result += "< " + BuildRecursively((IEnumerable)elem) + "> ";
                else result += "" + elem + " ";
            }

            return result;
        }
    }

    private static bool WriteToDisc()
    {
        _fileSystemOperationInProgress = true;
#if UNITY_EDITOR
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + _logFileName;
#else  // NOTE: ASSUMING RELEASE BUILDS RUN ON DEVICE
        var path = Application.persistentDataPath + _logFileName;
#endif
        try
        {
            // Open log file or create one and write log entries as lines
            using (var sw = File.AppendText(path))
            {
                foreach (var list in _logQueue)
                    sw.WriteLine(list.ToString());
            }

            // Clear log queue and return
            _logQueue.Clear();
            _fileSystemOperationInProgress = false;
            _pointOfLastWrite = Time.realtimeSinceStartup;
            return true;
        }
        // NOTE: Currently errors are not handled beyond this
        catch (InvalidDataException e)
        {
            Debug.LogError("Target log path exists but is read-only\n" + e);
        }
        catch (PathTooLongException e)
        {
            Debug.LogError("Target log path name may be too long\n" + e);
        }
        catch (IOException e)
        {
            Debug.LogError("The disk may be full\n" + e);
        }

        // TODO: revert log file if write operations fail...

        _fileSystemOperationInProgress = false;
        _pointOfLastWrite = Time.realtimeSinceStartup;
        return false;
    }
}