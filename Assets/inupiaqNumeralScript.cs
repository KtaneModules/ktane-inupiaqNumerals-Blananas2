using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class inupiaqNumeralScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable Submit;
    public KMSelectable[] DigitModifiers;
    public TextMesh Problem;
    public TextMesh Oper1; public TextMesh Oper2;
    public GameObject[] LEDs;
    public Material Lit;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    string numerals = ""; //used by the font
    string NforLog = "0123456789ABCDEFGHIJ";
    private List<string> operations = new List<string> { "+", "-", "×", " " };
    int stages = 0;
    int numA = 0; int numB = 0; int numC = 0;
    int answer = 0;
    bool thisAintGood = false;
    string textOnModule = "";

    private InupiaqNumeralsSettings Settings = new InupiaqNumeralsSettings();
    int numberOfStages = 4;

    void Awake () {
        moduleId = moduleIdCounter++;

        ModConfig<InupiaqNumeralsSettings> modConfig = new ModConfig<InupiaqNumeralsSettings>("InupiaqNumeralsSettings");
        //Read from the settings file, or create one if one doesn't exist
        Settings = modConfig.Settings;
        //Update the settings file incase there was an error during read
        modConfig.Settings = Settings;

        numberOfStages = Settings.TotalStages;
        if (numberOfStages <= 0) {
            Debug.LogFormat("<Iñupiaq Numerals #{0}> Do not make the number of stages less than 1.", moduleId);
            numberOfStages = 4;
        }
        Debug.LogFormat("<Iñupiaq Numerals #{0}> Number of stages: {1}", moduleId, numberOfStages);

        foreach (KMSelectable button in DigitModifiers) {
            button.OnInteract += delegate () { buttonPress(button); return false; };
        }

        Submit.OnInteract += delegate () { submitPress(); return false; };
    }

    // Use this for initialization
    void Start () {
        operations.Shuffle();
        GenerateStage();
    }

    // Update is called once per frame
    void Update () {
        Problem.text = textOnModule.Replace("$", numerals[answer/20].ToString()).Replace("&", numerals[answer%20].ToString());
    }

    void GenerateStage() {
        if (operations[stages%4] == " ") {
            Oper2.gameObject.SetActive(true);
			Oper1.text = "-";
        } else {
            Oper2.gameObject.SetActive(false);
			Oper1.text = operations[stages%4];
        }

        TryAgain:
        thisAintGood = false;
        numA = UnityEngine.Random.Range(0, 400);
        numB = UnityEngine.Random.Range(0, 400);

        switch (operations[stages%4]) {
            case "+":
                numC = numA + numB;
                if (numC > 399) {
                    thisAintGood = true;
                }
            break;
            case "-":
                numC = numA - numB;
                if (numC < 0) {
                    thisAintGood = true;
                }
            break;
            case "×":
                numC = numA * numB;
                if (numC > 399) {
                    thisAintGood = true;
                }
            break;
            case " ":
                if (numB == 0 || numA % numB != 0) {
                    thisAintGood = true;
                } else {
                    numC = numA / numB;
                }
            break;
            default: Debug.Log("Damnit."); break;
        }

        if (thisAintGood) {
            goto TryAgain;
        } else {
            textOnModule = String.Format("{0}{1}\n{2}{3}\n$&", numerals[numA/20], numerals[numA%20], numerals[numB/20], numerals[numB%20]);
            Debug.LogFormat("[Iñupiaq Numerals #{0}] Stage {1}: {2}{3} {4} {5}{6} = {7}{8}", moduleId, stages+1, NforLog[numA/20], NforLog[numA%20], operations[stages%4].Replace(" ", "÷"), NforLog[numB/20], NforLog[numB%20], NforLog[numC/20], NforLog[numC%20]);
        }
    }

    void buttonPress(KMSelectable button) {
        Audio.PlaySoundAtTransform("PressButton" + UnityEngine.Random.Range(1, 6), transform);
        for (int i = 0; i < 8; i++) {
            if (button == DigitModifiers[i]) {
                switch (i) {
                    case 0: answer = (answer+300)%400; break;
                    case 1: answer = (answer+100)%400; break;
                    case 2: answer = (answer+395)%400; break;
                    case 3: answer = (answer+  5)%400; break;
                    case 4: answer = (answer+380)%400; break;
                    case 5: answer = (answer+ 20)%400; break;
                    case 6: answer = (answer+399)%400; break;
                    case 7: answer = (answer+  1)%400; break;
                    default: Debug.Log("Damnit."); break;
                }
            }
        }
    }

    void submitPress() {
        Submit.AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (answer == numC) {
            Debug.LogFormat("[Iñupiaq Numerals #{0}] {1}{2} submitted for Stage {3}, that is correct.", moduleId, NforLog[answer/20], NforLog[answer%20], stages+1);
            stages++;
            float percentage = (float)stages / (float)numberOfStages;
                if (percentage >= 0.25f) {
                    LEDs[0].GetComponent<MeshRenderer>().material = Lit;
                }
                if (percentage >= 0.5f) {
                    LEDs[1].GetComponent<MeshRenderer>().material = Lit;
                }
                if (percentage >= 0.75f) {
                    LEDs[2].GetComponent<MeshRenderer>().material = Lit;
                }
            if (stages == numberOfStages) {
                LEDs[3].GetComponent<MeshRenderer>().material = Lit;
                Debug.LogFormat("[Iñupiaq Numerals #{0}] All {1} stages finished, module solved.", moduleId, numberOfStages);
                GetComponent<KMBombModule>().HandlePass();
                moduleSolved = true;
            } else {
                answer = 0;
                GenerateStage();
            }
        } else {
            Debug.LogFormat("[Iñupiaq Numerals #{0}] {1}{2} submitted for Stage {3}, that is incorrect. Strike!", moduleId, NforLog[answer/20], NforLog[answer%20], stages+1);
            GetComponent<KMBombModule>().HandleStrike();
        }
    }
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press [tll/trl/tlr/trr/bll/brl/blr/brr] [1-20] presses the given button by the given number (Example: !{0} press tlr 15. The command presses the top-left button on the right-most digit 15 times) | !{0} submit submits the answer";
    #pragma warning restore 414
	
	string[] ValidPress = {"tll", "trl", "tlr", "trr", "bll", "brl", "blr", "brr"};
	
    IEnumerator ProcessTwitchCommand(string command)
    {
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			yield return null;
			Submit.OnInteract();
		}
		
		if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			yield return null;
			if (parameters.Length != 3)
			{
				yield return "sendtochaterror Invalid parameter length. The command was not processed.";
				yield break;
			}
			
			if (new[] {parameters[1].ToLowerInvariant()}.Any(c => !ValidPress.Contains(c)))
			{
				yield return "sendtochaterror Button press in not valid. The command was not processed.";
				yield break;
			}
			
			int Out;
			if (!Int32.TryParse(parameters[2], out Out))
			{
				yield return "sendtochaterror Invalid number was sent. The command was not processed.";
				yield break;
			}
			
			if (Out < 1 || Out > 20)
			{
				yield return "sendtochaterror Number was not between 1-20. The command was not processed.";
				yield break;
			}

			for (int x = 0; x < Out; x++)
			{
				DigitModifiers[Array.IndexOf(ValidPress, parameters[1].ToLowerInvariant())].OnInteract();
				yield return new WaitForSecondsRealtime(0.1f);
			}
		}
	}

    class InupiaqNumeralsSettings
    {
        public int TotalStages = 4;
    }

    static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
        new Dictionary<string, object>
        {
            { "Filename", "InupiaqNumerals.json" }, //okay it's not like that in-game? idk what's up with that --me right after workshop push on the 29th
            { "Name", "Iñupiaq Numerals Settings" },
            { "Listing", new List<Dictionary<string, object>>{
                new Dictionary<string, object>
                {
                    { "Key", "TotalStages" },
                    { "Text", "Amount of stages the module will have." }
                }
            } }
        }
    };
}
