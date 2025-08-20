using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class script : MonoBehaviour
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
        return (num - (int)num != 0) ? 1 : 0 + (int)num;
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
}
