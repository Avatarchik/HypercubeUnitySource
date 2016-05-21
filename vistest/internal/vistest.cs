using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;




public class vistest : MonoBehaviour {

    
    public string loadPath = "LOAD/"; //this should be a path outside of Assets/ so that the tool can load things dynamically through the player. Also it will create this path if it doesn't exist
    public float cameraSpeedMod = 3f;

    public UnityEngine.UI.Dropdown targetDropdown;
    public UnityEngine.UI.Dropdown targetTypeDropdown;
    public UnityEngine.UI.Dropdown modelDropdown;
    public UnityEngine.UI.Dropdown textureDropdown;
    public UnityEngine.UI.Toggle clearSceneToggle;
    public UnityEngine.UI.Slider toneSlider;
    public UnityEngine.UI.Text toneText;
    public UnityEngine.UI.Slider overlapSlider;
    public UnityEngine.UI.Text overlapText;
    public UnityEngine.UI.InputField yOffset;
    public UnityEngine.UI.InputField sliceWidth;
    public UnityEngine.UI.InputField sliceHeight;

    public UnityEngine.UI.Dropdown culling;

    public Shader defaultShader;
    public GameObject hypercube;
    public GameObject cameras;
    public hypercubeCanvas canvas;
    public hypercubeFpsControl cubeControl;
    public hypercubeFpsControl cameraControl;

    vistestAnimManager animMgr;

    List<GameObject> loadedMeshes = new List<GameObject>();

    

    private bool mustResetCameras = true;

	// Use this for initialization
	void Start () 
    {
        //if default load path doesn't exist, create it
        if (!Directory.Exists(loadPath))
            Directory.CreateDirectory(loadPath);

        animMgr = GetComponent<vistestAnimManager>();

        //list the monitors
        List<string> monitorOptions = new List<string>();
        monitorOptions.Add("off");
        for (int i = 0; i < Display.displays.Length; i++)
        {
            monitorOptions.Add(i.ToString());
        }
        targetDropdown.AddOptions(monitorOptions);

        resetGUI();
        setSliceChanges();

        //init the scene with something in it, if it has anything non vistest related in it... including anims
        GameObject[] sceneObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject s in sceneObjects)
        { 
            if (s.tag != "VISTEST") //ignore anything related to the vistest itself
            {
                loadedMeshes.Add(s);
                animMgr.init(s.GetComponent<Animator>()); //this can handle null
            }
        }

        if (loadedMeshes.Count == 0) //nothing was in the scene so automatically load the first obj we find
            loadModelFromGUI();
        else
        {

        }

        mustResetCameras = true;
	}

    //set the player to a particular monitor
    public void setDisplay()
    {
        if (targetDropdown.value == 0)
            return;
        canvas.setToDisplay(targetDropdown.value - 1); //the -1 is remember that the first option is 'off'
    }

	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.F))
            resetCameras();

   //     if (Input.GetKeyDown(KeyCode.Return)) //this is a bad idea unless you have focus because it will also be triggered whenever the inputFields are used
   //         loadModel();

        if (Input.GetKey("escape"))
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
         Application.OpenURL(webplayerQuitURL);
#else
         Application.Quit();
#endif
        }
	}

    void LateUpdate()
    {
        if ( mustResetCameras)
            resetCameras();
    }



    public void resetGUI()
    {
        modelDropdown.ClearOptions();
        textureDropdown.ClearOptions();

        List<string> modelOptions = new List<string>();
        List<string> textureOptions = new List<string>();

        var info = new DirectoryInfo(loadPath);
        FileInfo[] fileInfo = info.GetFiles();
        foreach (FileInfo file in fileInfo) 
        {
            if (file.Name.EndsWith(".obj"))
                modelOptions.Add(file.Name);

            else if (file.Name.EndsWith(".png") ||
                        file.Name.EndsWith(".jpg") ||
                        file.Name.EndsWith(".jpeg") ||
                       // file.Name.EndsWith(".psd") ||
                        file.Name.EndsWith(".bmp") 
                )
                textureOptions.Add(file.Name);
        }

        modelDropdown.AddOptions(modelOptions);
        textureDropdown.AddOptions(textureOptions);

    }


    public void resetCameras()
    {
        //first, get the bounding ranges of any items not related to the vistest
        Renderer[] sceneObjects = GameObject.FindObjectsOfType<Renderer>();

        Bounds mainBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool firstObject = true;
        foreach (Renderer s in sceneObjects)
        {
            if (s.tag != "VISTEST") //ignore anything related to the vistest itself
            {
                if (firstObject)
                {
                    mainBounds.center = s.transform.position;
                    firstObject = false;
                }

                mainBounds.Encapsulate(s.bounds); 
            }
        }

        float diffX = Mathf.Abs(mainBounds.max.x - mainBounds.min.x);
        float diffY = Mathf.Abs(mainBounds.max.y - mainBounds.min.y);
        float diffZ = Mathf.Abs(mainBounds.max.z - mainBounds.min.z);
        Vector3 centerPoint = new Vector3((mainBounds.max.x + mainBounds.min.x) / 2, (mainBounds.max.y + mainBounds.min.y) / 2, (mainBounds.max.z + mainBounds.min.z) / 2);

        float biggestDiff = diffX;
        if (diffY > biggestDiff)
            biggestDiff = diffY;
        if (diffZ > biggestDiff)
            biggestDiff = diffZ;

        hypercube.transform.rotation = Quaternion.identity;
        hypercube.transform.position = centerPoint;
        hypercube.transform.localScale = new Vector3(biggestDiff, biggestDiff, biggestDiff);
        cubeControl.reset();
        cubeControl.moveSpeed = biggestDiff / cameraSpeedMod;  //the bigger the scene, the faster the camera moves

        cameras.transform.position = new Vector3(centerPoint.x, centerPoint.y, mainBounds.min.z);
        //cameras.transform.LookAt(centerPoint);
        cameras.transform.rotation = Quaternion.identity;
        cameras.transform.localScale.Set(1f, 1f, 1f);
        cameraControl.reset();
        cameraControl.moveSpeed = biggestDiff / cameraSpeedMod;  //the bigger the scene, the faster the camera moves

        mustResetCameras = false;  
    }

    public void loadModelFromGUI()
    {
        if (clearSceneToggle.isOn)
            clearScene();

        loadModel();

        if (clearSceneToggle.isOn)
            mustResetCameras = true; //this must be delayed a frame so that the bounding boxes have time to update
    }
    public GameObject loadModel()
    {
        if (modelDropdown.options.Count < 1) //nothing to load!
            return null;

        GameObject newOBJ = loadOBJ(loadPath + modelDropdown.options[modelDropdown.value].text);
        loadedMeshes.Add(newOBJ);

        //apply a sane material at least to the first one.
        MeshRenderer r = newOBJ.GetComponent<MeshRenderer>();
        for (int i = 0; i < r.materials.Length; i++)
        {
            r.materials[i].shader = defaultShader;
        }

        loadTexture(loadPath + textureDropdown.options[textureDropdown.value].text, newOBJ);
        resetFaceCulling();

        animMgr.init(null); //this is an obj, it wont have an animator

        return newOBJ;
    }

    public void clearScene()
    {
        foreach (GameObject o in loadedMeshes)
            Destroy(o);

        loadedMeshes.Clear();
    }

    public void setTone()
    {
        hypercubeCanvas c = hypercube.GetComponent<hypercubeCamera>().localCanvas;
        if (c)
            c.setTone(toneSlider.value);
    }

    public void setOverlap()
    {
        hypercube.GetComponent<hypercubeCamera>().overlap = overlapSlider.value;
        overlapText.text = System.Math.Round(overlapSlider.value, 1).ToString();
    }

    public static GameObject loadOBJ(string filePath, bool useRightHandCoordinates = false)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("The file does not exist.");
        }

        string readText = File.ReadAllText(filePath);
        //Console.WriteLine(readText);

        return ObjImporter.Import(readText, Quaternion.identity, Vector3.one, Vector3.zero, null, null, false, false, useRightHandCoordinates);

    }

    //this handles updates to the options of the slice offsets and dims
    public void setSliceChanges()
    {
        canvas.sliceOffsetY = stringToInt(yOffset.text, 0);
        canvas.sliceWidth = stringToInt(sliceWidth.text, 600);
        canvas.sliceHeight = stringToInt(sliceHeight.text, 53);
    }
    static int stringToInt(string strVal, int defaultVal)
    {
        int output;
        if (System.Int32.TryParse(strVal, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out output))
            return output;

        return defaultVal;
    }

    public void textureOptionSelectionChanged()
    {
        if (loadedMeshes.Count == 0 || loadedMeshes[0] == null)
            return;

        loadTexture(loadPath + textureDropdown.options[textureDropdown.value].text, loadedMeshes);
    }


    public static bool loadTexture(string filePath, GameObject applyToMe)
    {
        List<GameObject> g = new List<GameObject>();
        g.Add(applyToMe);
        return loadTexture(filePath, g);
    }
    public static bool loadTexture(string filePath, List<GameObject> applyToMe)
    {
        if (applyToMe.Count == 0)
            return false;

        Texture2D tex = new Texture2D(2, 2);

        byte[] pngBytes = File.ReadAllBytes(filePath);
        // Load data into the texture.
        if (!tex.LoadImage(pngBytes))
            return false;

        // Assign texture to renderer's material.
        foreach (GameObject g in applyToMe)
        {
            Component[] renderers =  g.GetComponentsInChildren(typeof(Renderer));
            if (renderers.Length == 0)
                continue;

            foreach (Component c in renderers)
            {
                Renderer r = (Renderer)c;
                Material[] mats = r.materials;

                if (mats.Length == 0)
                    continue;

                mats[0].SetTexture("_MainTex", tex);
            }
          
        }

        //foreach (Material m in mats)
        //{
        //m.shader = Shader.Find("Unlit/Hypercube Shader");
        //m.mainTexture = tex;
        //}

        return true;
    }

    public void resetFaceCulling()
    {
        foreach (GameObject o in loadedMeshes)
        {
            Material[] mats = o.GetComponent<Renderer>().materials;
            foreach (Material m in mats)
            {
                m.SetInt("_Cull", culling.value);
            }
        }       
    }


}
