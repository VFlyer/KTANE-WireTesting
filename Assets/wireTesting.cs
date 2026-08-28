using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

//Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);

public class wireTesting : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
    public KMColorblindMode ColorblindHandler;
    public GameObject[] wireObjects;
    public KMSelectable[] wireSelectables;
    public Transform WireContainer;
    public Texture[] WireColors;
    public KMSelectable CONFIRM;
    public KMSelectable DENY;
    public MeshRenderer[] LEDS;
    public Material[] LEDMaterials;
    public Material[] StatusLightMaterials;
    public MeshRenderer StatusLightMesh;
    public MeshRenderer StaticCube;
    public Texture[] Static;
    public Material SolveMaterial;
    public TextMesh[] LeftColorblindTexts;
    public TextMesh[] RightColorblindTexts;

    string ModuleName;
    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;
    GameObject wire;
    Wires[] Wires = new Wires[5];
    int stage = 1;
    string[,] validTable = new string[,] {
        { "UwU", "X", "Three", "D", "Tree", "Red", "UwU", "Tree", "Red", "Tree", "Tree", "D", "UwU", "Tree", "one" },
        { "Three", "UwU", "Two", "Two", "HELP", "Tree", "Tree", "UwU", "Tree", "X", "Two", "X", "D", "UwU", "HELP" },
        { "X", "HELP", "UwU", "HELP", "X", "one", "A", "Red", "UwU", "Four", "A", "Four", "HELP", "X", "UwU" },
        { "D", "one", "HELP", "UwU", "Four", "HELP", "Three", "HELP", "HELP", "UwU", "A", "A", "one", "X", "Red" },
        { "D", "D", "Four", "Tree", "UwU", "X", "Three", "A", "Red", "HELP", "UwU", "Four", "one", "X", "A" },
        { "Four", "Three", "A", "Three", "Red", "UwU", "D", "A", "A", "X", "A", "UwU", "A", "Three", "Two" }
    };
    Vector3 wireSize = new Vector3(32.21814f, 44.89068f, 401.3983f);
    
    void Awake () { //Shit that happens before Start
        ModuleName = Module.ModuleDisplayName;
        ModuleId = ModuleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += Activate;
        
        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
        }
        */

        //button.OnInteract += delegate () { buttonPress(); return false; };

        //keypadPress() and buttonPress() you have to make yourself and should just be what happens when you press a button. (deaf goes through it probably)
    }

    void Activate () { //Shit that should happen when the bomb arrives (factory)/Lights turn on

    }

    void Start () { //Shit
        Log("Stage 1:");
        for(int i=0; i<5; i++) {
            wire = Instantiate(wireObjects[Rnd.Range(0, wireObjects.Length)]);
            wire.transform.parent = WireContainer.Find($"{i}wire").transform;
            wire.transform.localScale = wireSize;
            wire.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            Wires[i] = new Wires(this, wire, i);
            Wires[i].Startup();
            int dummy = i;
            wireSelectables[dummy].OnInteract = delegate () { Wires[dummy].wirecut(); return false; };
            Log($"Wire {i+1} is colored {Wires[i].Color.name.Replace("blacK", "Black")} and will {Wires[i].Action} when cut.");
        }
        DENY.OnInteract = delegate () { SolCheck(false); return false; };
        CONFIRM.OnInteract = delegate () { SolCheck(true); return false; };
    }

    void SolCheck(bool type) {
        if(ModuleSolved) { return; }
        Log($"{boolToPressed(type)} was pressed at {Bomb.GetStrikes()} strike(s).");
        int count;
        int count2 = 0;
        bool actualPress = false;
        foreach(Wires Wire in Wires) {
            switch(validTable[Wire.ActionNumber, Wire.ColorNumber]) {
                case "Red":
                    count = 0;
                    foreach(Wires i in Wires) {
                        if(i.Color.name.Contains("Red")) {
                            count++;
                        }
                    }
                    if(count>=3) {
                        Wire.Valid = true;
                    }
                    break;
                case "Two":
                    if(!(stage == 2 || Wire.number+1 == 2)) {
                        Wire.Valid = true;
                    }
                    break;
                case "HELP":
                    foreach(char i in "PLEASEHELPME") {
                        if(Bomb.GetSerialNumber().Contains(i)) {
                            Wire.Valid = true;
                        }
                    }
                    break;
                case "UwU":
                    Wire.Valid = true;
                    break;
                case "Three":
                    if(!Wires[(Wire.number+1)%5].Color.name.Contains("Red")) {
                        Wire.Valid = true;
                    }
                    break;
                case "Tree":
                    if(Wire.number+1 == 3 || Bomb.GetSerialNumber().Contains((Wire.number+1).ToString())) {
                        Wire.Valid = true;
                    }
                    break;
                case "A":
                    count = 0;
                    foreach(Wires i in Wires) {
                        if(i.Action == "Be Pressed") {
                            count++;
                        }
                    }
                    if(!(count >= 2)) {
                        Wire.Valid = true;
                    }
                    break;
                case "Four":
                    if(char.IsNumber(Bomb.GetSerialNumber()[1])) {
                        foreach(char i in "GHIJKLMNOPQRSTUVWXYZ") {
                            if(Bomb.GetSerialNumber().Contains(i)) {
                                Wire.Valid = true;
                                break;
                            }
                        }
                    } else {
                        if(!Bomb.GetSerialNumber().Contains('4')) {
                            Wire.Valid = true;
                        }
                    }
                    break;
                case "X":
                    if(Bomb.GetStrikes() < 3) {
                        Wire.Valid = true;
                    }
                    break;
                case "D":
                    if(Bomb.IsPortPresent(KModkit.Port.DVI)) {
                        Wire.Valid = true;
                    }
                    break;
                case "one":
                    break;
            }
            count2++;
            Log($"Wire {count2} is {boolToValid(Wire.Valid)}");
        }
        count = 0;
        foreach(Wires Wire in Wires) {
            if(Wire.Valid) { count++; }
        }
        if(count >= 4) { actualPress = true; }
        if(actualPress == type) { Log($"Correct! Stage {stage} complete."); StageUp(); } else { Strike(); }
    }

    void StageUp() {
        if(stage==3) {
            LEDS[stage - 1].material = LEDMaterials[1];
            Solve();
            return;
        }
        if(_staticingCoroutine != null)
            StopCoroutine(_staticingCoroutine);
        _staticingCoroutine = StaticingCube();
        StartCoroutine(_staticingCoroutine);
        LEDS[stage-1].material = LEDMaterials[1];
        stage++;
        Log($"Stage {stage.ToString()}:");
        for(int i=0; i<5; i++) {
            Wires[i].Startup();
            Log($"Wire {i+1} is colored {Wires[i].Color.name} and will {Wires[i].Action} when cut.");
        }
    }

    IEnumerator _staticingCoroutine;

    IEnumerator StaticingCube() {
        StaticCube.enabled = true;
        float value = 0f;
        for(float timer = 0; timer<2; timer+=Time.deltaTime) {
            if(timer >= value+0.1f) {
                StaticCube.material.mainTexture = Static[Rnd.Range(0, Static.Length)];
                value = timer;
            }
            yield return null;
        }
        StaticCube.enabled = false;
    }

    IEnumerator SolvingCube() {
        StaticCube.enabled = true;
        float value = 0f;
        for(float timer = 0; timer < 2; timer += Time.deltaTime) {
            if(timer >= value + 0.1f) {
                StaticCube.material = SolveMaterial;
                StaticCube.material.mainTexture = null;
                value = timer;
            }
            yield return null;
        }
        StaticCube.enabled = true;
    }

    void Solve () {
        ModuleSolved = true;
        Module.HandlePass();
        Log("Module Solved.");
        if(_staticingCoroutine != null)
            StopCoroutine(_staticingCoroutine);
        _staticingCoroutine = SolvingCube();
        StartCoroutine(_staticingCoroutine);
    }

    void Strike () {
        StartCoroutine(StaticingCube());
        foreach(MeshRenderer LED in LEDS) {
            LED.material = LEDMaterials[0];
        }
        Module.HandleStrike();
        Log("Incorrect! Strike Issued.");
        FakeStrike(false);
        stage = 1;
        Log($"Stage 1:");
        for(int i = 0; i < 5; i++) {
            Wires[i].Startup();
            Log($"Wire {i} is colored {Wires[i].Color.name} and will {Wires[i].Action} when cut.");
        }
    }

    IEnumerator _blinkingCoroutine;

    public void FakeStrike (bool playSound = true) {
        if(_blinkingCoroutine != null)
            StopCoroutine(_blinkingCoroutine);
        _blinkingCoroutine = StrikeBlink(playSound);
        StartCoroutine(_blinkingCoroutine);
    }

    public void bip() {
        if(_blinkingCoroutine != null)
            StopCoroutine(_blinkingCoroutine);
        _blinkingCoroutine = bipBlink();
        StartCoroutine(_blinkingCoroutine);
    }

    public void FakeSolve () {
        if(_blinkingCoroutine != null)
            StopCoroutine(_blinkingCoroutine);
        _blinkingCoroutine = SolveBlink();
        StartCoroutine(_blinkingCoroutine);
    }

    IEnumerator StrikeBlink(bool playStrikeSound = true) {
        if (playStrikeSound)
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
        StatusLightMesh.material = StatusLightMaterials[1];
        yield return new WaitForSeconds(1f);
        StatusLightMesh.material = StatusLightMaterials[0];
    }

    IEnumerator bipBlink() {
        StatusLightMesh.material = StatusLightMaterials[2];
        yield return new WaitForSeconds(1f);
        StatusLightMesh.material = StatusLightMaterials[0];
    }

    IEnumerator SolveBlink() {
        StatusLightMesh.material = StatusLightMaterials[3];
        yield return new WaitForSeconds(1f);
        StatusLightMesh.material = StatusLightMaterials[0];
    }

    public void Log (string message) { 
        Debug.Log($"[{ModuleName} #{ModuleId}] {message}");
    }
    
    public void WirePress(GameObject self) {
        StartCoroutine(PressMovement(self));
    }

    IEnumerator PressMovement(GameObject self) {
        for(float timer = 0; timer < 1; timer += Time.deltaTime) {
            self.transform.localPosition += new Vector3(0, 0, 0.1f * Time.deltaTime);
            yield return null;
        }
    }

    string boolToValid(bool i) {
        if(i) {
            return "Valid";
        }
        return "Invalid";
    }

    string boolToPressed(bool i) {
        if(i) {
            return "CONFIRM";
        }
        return "DENY";
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} cut # To cut a wire. Use !{0} cutall to cut every wire in order with a delay. Use !{0} CONFIRM to press CONFIRM. Use !{0} DENY to press DENY.";
#pragma warning restore 414

    // Twitch Plays (TP) documentation: https://github.com/samfundev/KtaneTwitchPlays/wiki/External-Mod-Module-Support

    IEnumerator ProcessTwitchCommand (string Command) {
        string COMMAND = Command.ToUpper().Trim();
        Match m = Regex.Match(COMMAND, "(CUT [1-5]|CUTALL|CONFIRM|DENY)$");
        if(!m.Success) {
            yield break;
        }
        string command = m.Groups[1].Value;
        switch(command) {
            case "CUTALL":
                yield return null;
                for(int i = 0; i<5; i++) {
                    yield return wireSelectables[i];
                    yield return new WaitForSeconds(1.5f);
                }
                break;
            case "CONFIRM":
                yield return null;
                yield return CONFIRM;
                break;
            case "DENY":
                yield return null;
                yield return DENY;
                break;
            default:
                yield return null;
                yield return wireSelectables[int.Parse(command[command.Length - 1].ToString())-1];
                break;
        }
    }

    void TwitchHandleForcedSolve () {
        Solve();
        return;
    }
}
