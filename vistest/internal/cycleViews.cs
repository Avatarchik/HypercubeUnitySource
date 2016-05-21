using UnityEngine;
using System.Collections;

[System.Serializable]
public class cycleState
{
    public GameObject[] show;
    //  public GameObject[] hide;
}

public class cycleViews : MonoBehaviour {

    public KeyCode cycleKeyBackward = KeyCode.LeftBracket;
    public KeyCode cycleKeyForward = KeyCode.RightBracket;

    public cycleState[] states;
    int currentState = 0;

	void Start () 
    {
        setState(0);
	}
	
	// Update is called once per frame
	void Update () 
    {
        //cycle states
        if (Input.GetKeyDown(cycleKeyForward))
        {
            currentState++;
            if (currentState >= states.Length)
                currentState = 0;

            setState(currentState);
        }
        else if (Input.GetKeyDown(cycleKeyBackward))
        {
            currentState--;
            if (currentState < 0)
                currentState = states.Length - 1;

            setState(currentState);
        }
	}

    void setState(int s)
    {
        foreach (cycleState cs in states)
        {
            foreach (GameObject g in cs.show)
            {
                    g.SetActive(false);
            }
        }

        //show whats in the current State
        for (int i = 0; i < states[s].show.Length; i++)
        {
            states[s].show[i].SetActive(true);
        }
    }
}
