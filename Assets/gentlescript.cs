using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gentlescript : MonoBehaviour
{
    public KMSelectable[] buttons = new KMSelectable[3];
    public MeshRenderer[] colors = new MeshRenderer[3];
    public TextMesh f12;
    public KMBombModule module;
	public AudioClip[] sound = new AudioClip[6];
	public KMAudio audio;

    private int[] down = new int[3];
    private int[] ans = new int[3];
    private Color[] buttonColors = new Color[6] { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta }; // 0, 1, 2, 3, 4, 5
    private Color[] textColors = new Color[4] { Color.red, Color.yellow, Color.green, Color.blue }; //+, -, *, /
    private Color generatedColor;
    private bool solve = false;
    private int stage = 0;
	private int left;
    private int right;
    private int up;
	
	int input = 0;
	

    int mod(int a, int b)
    {
        while (a >= b) a -= b;
        while (a < 0) a += b;
        return a;
    }

    int ceil(float num)
    {
        return (num - (int)num != 0 ? 1 : 0) + (int)num;
    }
	
    void generate()
    {
		left = Random.Range(0, 6);
        right = Random.Range(1, 6);
        up = Random.Range(0, 6);
		
		colors[0].material.color = buttonColors[left];
        colors[1].material.color = buttonColors[up];
        colors[2].material.color = buttonColors[right];
		
        for (int i = 0; i < 3; i++)
        {
            down[i] = Random.Range(0, 4);	
			
            switch (down[i])
            {
                case 0:
                    {
                        ans[i] = mod(left + right, 3);
                        break;
                    }
                case 1:
                    {
                        ans[i] = mod(left - right, 3);
                        break;
                    }
                case 2:
                    {
                        ans[i] = mod(left * right, 3);
                        break;
                    }
                case 3:
                    {
                        ans[i] = mod(ceil(left / (float)right), 3);
                        break;
                    }
                    break;
            }
			if ((up >= 0 && up < 2) || up == 5) 
            {
                ans[i] = 2 - ans[i];
            }
            Debug.LogFormat("Stage {0}: Left={1}, Right={2}, Operation={3}, Answer={4}, ", i + 1, left, right, down[i], ans[i]);
        }
    }

    void press(int index)
    {
        if (solve || stage >= 3) return;
		
		int pressedColor;
		if (index == 0) pressedColor = left;
		else if (index == 1) pressedColor = up;
		else pressedColor = right;
		audio.PlaySoundAtTransform(sound[pressedColor].name, buttons[index].transform);

        Debug.LogFormat("Pressed {0}, expected answer {1}", index, ans[stage]);
			StopAllCoroutines();
			f12.color = new Color(1, 1, 1);
        if (index == ans[input])
        {
			input++;

			if (input > stage) {
				
				Debug.Log("correct! :)");
				input = 0;
				stage++;
				StartCoroutine(BlinkText());
			}

			
            if (stage >= 3)
            {
                solve = true;
                module.HandlePass();
				StopAllCoroutines();
                Debug.Log("Solved!!! :D");
                f12.color = Color.green;
            }
        }
        else
        {
            Debug.Log("Wrong! >:C");
			input = 0;
            module.HandleStrike();
			StartCoroutine(BlinkText());
        }
    }


    void Start()
    {
        f12.color = new Color(1, 1, 1);
			buttons[0].OnInteract = delegate { press(0); return true; };
			buttons[1].OnInteract = delegate { press(1); return true; };
			buttons[2].OnInteract = delegate { press(2); return true; };
		generate();
        generatedColor = textColors[down[stage]];

        StartCoroutine(BlinkText());
    }


    IEnumerator BlinkText()
    {
		yield return new WaitForSeconds(1.5f);
        while (!solve)
        {
            for (int i = 0; i <= stage; i++)
            {
                f12.color = textColors[down[i]];
				if (stage > 0) audio.PlaySoundAtTransform(sound[down[i]].name, f12.transform);
                yield return new WaitForSeconds(0.75f);
				f12.color = new Color(1, 1, 1);
				yield return new WaitForSeconds(0.5f);
            }
            
            yield return new WaitForSeconds(1.5f);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    #region Twitch Plays
    //The message to send to players showing available commands
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <left/l/top/t/right/r> [Presses the specified button] | Presses can be chained with spaces";
    #pragma warning restore 414
    //Process commands sent from TP to the module
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' '); //Split the command by spaces
        if (parameters[0].ToLowerInvariant().Equals("press")) //Make sure the command starts with "press"
        {
            if (parameters.Length == 1) //If the command is just "press" then send an error message
                yield return "sendtochaterror Please specify a button to press!";
            else
            {
                for (int i = 1; i < parameters.Length; i++) //Check everything after "press" for valid buttons
                {
                    if (!parameters[i].ToLowerInvariant().EqualsAny("left", "l", "top", "t", "right", "r"))
                    {
                        yield return "sendtochaterror!f The specified button to press '" + parameters[i] + "' is invalid!"; //Output that a button is invalid, the !f is to ensure parameters[i] is ignored by auto formatting
                        yield break; //Stop execution here since an invalid button was detected
                    }
                }
                yield return null; //Tell TP that the command is valid and to focus on the module
                for (int i = 1; i < parameters.Length; i++) //Press all buttons that were specified
                {
                    switch (parameters[i].ToLowerInvariant())
                    {
                        case "left":
                        case "l":
                            buttons[0].OnInteract();
                            break;
                        case "top":
                        case "t":
                            buttons[1].OnInteract();
                            break;
                        default:
                            buttons[2].OnInteract();
                            break;
                    }
                    yield return new WaitForSeconds(.25f); //Add some delay between presses so the module's sounds dont overlap as much
                }
            }
        }
    }

    //Make the module solve itself if it is forcefully solved by TP
    IEnumerator TwitchHandleForcedSolve()
    {
        while (!solve)
        {
            buttons[ans[input]].OnInteract();
            yield return new WaitForSeconds(.25f);
        }
    }
    #endregion
}
