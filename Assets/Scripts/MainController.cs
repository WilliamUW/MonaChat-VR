using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics; // For BigInteger
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using TMPro;
using UnityEngine.Networking;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Required for UI Button interaction
using TMPro; // Required for TextMeshPro interaction
using System;
using static UnityEngine.Rendering.DebugUI;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Meta.Voice.Samples.Dictation;
using System.Reflection;
public class MainController : MonoBehaviour
{
    private Gemini gemini;
    public UnityEngine.UI.InputField textToSpeechInputTextField;
    public UnityEngine.UI.Button textToSpeechStartButton; // Reference to the UI Button

    public UnityEngine.UI.Button textToSpeechStopButton; // Reference to the UI Button


    public DictationActivation controller; // Assign this in the Inspector
    public UnityEngine.UI.Button clearButton;
    public TextMeshProUGUI transcriptionText; // Reference to the TextMeshPro UI component on the button
    MethodInfo onClickMethod = typeof(UnityEngine.UI.Button).GetMethod("Press", BindingFlags.NonPublic | BindingFlags.Instance);

    private bool isListening = false;
    void Start()
    {
        gemini = new Gemini("Pretend you are the Mona Lisa.", textToSpeechInputTextField, textToSpeechStartButton);
    }

    public async void AskQuestion(string question, bool announceQuestion = true)
    {
        if (gemini != null)
        {
            gemini.AskGemini(question, false, announceQuestion);
            // Debug.Log("Gemini Response: " + response);
        } else {
            Debug.Log("Gemini not initialized");
        }
    }

    public void palmUpEnter()
    {
        if (isListening) return;
        isListening = true;
        Debug.Log("Listen start");
        onClickMethod?.Invoke(clearButton, null);
        onClickMethod?.Invoke(textToSpeechStopButton, null);
        controller.ToggleActivation();
        Debug.Log("Listening...");
    }

    public void palmUpEnd()
    {
        if (!isListening) return;
        isListening = false;
        Debug.Log("Listen end");
        string user_input = transcriptionText.text;
        AskQuestion(user_input);
        Debug.Log("Asking ... " + user_input);
        controller.ToggleActivation();
    }

}

