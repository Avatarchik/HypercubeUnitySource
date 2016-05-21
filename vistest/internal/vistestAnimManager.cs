using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 #if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif



public class vistestAnimManager : MonoBehaviour {

    public GameObject dropDownParent;
    public UnityEngine.UI.Dropdown animDropdown;
    public UnityEngine.UI.Toggle localOrAll;

    public AnimatorOverrideController overrider;

    public Animator anim; //can be set ahead of time, or during runtime with init()
   int resetHash;

    List<AnimationClip> clips = new List<AnimationClip>(); //all clips in the scene (and project, because we load every single one into the scene)

    bool isPlaying = true; //an abstraction of the state so we can choose the next step easily when stop/pause/playing

    void Start()
    {
        resetHash = Animator.StringToHash("reset");

        if (anim == null)
        {
            init( GameObject.FindObjectOfType<Animator>()); 
        }

        //get all the animations.and put them in the gui
        if (!Application.isEditor) //Unity can not find animations dynamically in the player.
        {
            //enabled = false;
            dropDownParent.SetActive(false);
            return;
        }

        if (!anim)
            dropDownParent.SetActive(false);
        else if (clips.Count > 0)
        {
            setAnimationTo(animDropdown.value); //probably anim 0
            play();
        }          

    }
	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isPlaying)
                pause();
            else
                play();
        }

        if (Input.GetKeyDown(KeyCode.Period) )
        {
            int v = animDropdown.value;
            v++;
            if (v >= animDropdown.options.Count)
                v = 0;

            animDropdown.value = v;
        }

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            int v = animDropdown.value;
            v--;
            if (v < 0)
                v = animDropdown.options.Count -1;

            animDropdown.value = v;
        }   
	}

    public void init(Animator _anim)   
    {
        if (!_anim)
        {
            dropDownParent.SetActive(false);
            return;
        }

        dropDownParent.SetActive(true);
        anim = _anim;
        anim.runtimeAnimatorController = overrider;

        populateAnimGUI(localOrAll.isOn);
    }


    public void play()
    {
        if (!anim)
            return;
        anim.enabled = true;
        anim.SetBool(resetHash, false); //reset anim?
        isPlaying = true;
    }
    public void pause()
    {
        if (!anim)
            return;
        anim.SetBool(resetHash, false); //reset anim?
        anim.enabled = false;
        isPlaying = false;
    }
    public void stop()
    {
        if (!anim)
            return;
        anim.enabled = true;
        anim.SetBool(resetHash, true); //reset anim?
        isPlaying = false;
    }


    public void populateAnimGUI(bool listAll = false)
    {
        if (!anim)
            listAll = true;

        clips.Clear();
        animDropdown.ClearOptions();
 #if UNITY_EDITOR

        //populate the gui. we are in the editor, so lets make a list of the anims
        //THIS LOADS EVERY ANIMATION IN THE ENTIRE PROJECT!
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
        foreach (string g in guids)
        {
            AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(g));
        }


        string assetPathOfAnimatedMesh = "";
        if (!listAll)
        {
            assetPathOfAnimatedMesh = AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(anim));
            if (assetPathOfAnimatedMesh.EndsWith(".prefab")) //the mesh that was dragged in is a prefab. The animationClip paths will not match, so find an asset that can be matched through the skinnedMeshRenderer.  If it is not found, it's not a problem since this code is only relevant to animation.
            {
                SkinnedMeshRenderer r = anim.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
                if (r && r.sharedMesh)
                    assetPathOfAnimatedMesh = AssetDatabase.GetAssetPath(r.sharedMesh);              
            }
            Debug.Log(assetPathOfAnimatedMesh);
        }

        //this will list all anims currently in the scene! - not those in the assets folders.  Hence we need the step above
        Object[] allClips = Resources.FindObjectsOfTypeAll(typeof(AnimationClip));
        List<string> names = new List<string>();
        foreach (Object o in allClips)
        {
            if (!o.name.StartsWith("__preview__")) //ignore preview anims
            {
                string clipAssetPath = AssetDatabase.GetAssetPath(o);
                //    Debug.Log(clipAssetPath);
                if (listAll || assetPathOfAnimatedMesh == clipAssetPath)
                {
                    names.Add(o.name);
                    clips.Add(o as AnimationClip);
                }
            }

        }

        animDropdown.AddOptions(names);
#endif

    }

    public void setAnimationTo(int index)
    {
        if (index >= clips.Count)
            return;

        overrider["None"] = clips[index];      //"None" is the name of the default blank animation. It is named None so that it will show nicely in the GUI as an option to shut off anims 
    }





    }




