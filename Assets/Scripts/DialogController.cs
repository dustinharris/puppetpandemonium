using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DialogController : MonoBehaviour
{


    [System.Serializable]
    public class DialogTemplate
    {
        public string Identifier;
        public Button Button;
        public GameObject SpeakerPortrait;
        public GameObject SpeakerText;
        public GameObject Background;
        public GameObject SpeechBubble;
        public AudioSource Audio;
        private bool AudioPlayed = false;

        public bool AudioHasPlayed()
        {
            return AudioPlayed;
        }

        public void SetAudioPlayed(bool played)
        {
            AudioPlayed = played;
        }
    }

    public enum Button
    {
        RedPuppet, BluePuppet, Either
    }

    [SerializeField]
    private string FilePath;
    [SerializeField]
    private AudioSource BGMusic;
    [SerializeField]
    private DialogTemplate[] Templates;

    private Dictionary<string, DialogTemplate> Dict;
    private SceneSwitcher Switcher;

    private const string RED_BUTTON = "RedPuppet";
    private const string BLUE_BUTTON = "BluePuppet";

    void Start()
    {

        if (Templates == null || FilePath == null || FilePath == "")
        {
            throw new UnityException("Missing parameter(s)");
        }
        // Get scene switcher from previous scene if it wasn't destroyed on load
        GameObject prevSwitchObj = GameObject.Find("SceneSwitcher");
        if (prevSwitchObj != null)
        {
            SceneSwitcher prevSwitch = prevSwitchObj.GetComponent<SceneSwitcher>();
            if (prevSwitch != null)
            {
                if (prevSwitch.result == SceneSwitcher.Result.Win)
                {
                    FilePath = "Win" + FilePath;
                }
                else if (prevSwitch.result == SceneSwitcher.Result.Lose)
                {
                    FilePath = "Lose" + FilePath;
                }
            }
            Destroy(prevSwitchObj);
        }

        Dict = new Dictionary<string, DialogTemplate>();

        for (int i = 0; i < Templates.Length; i++)
        {
            Dict.Add(Templates[i].Identifier, Templates[i]);
        }

        Switcher = GetComponent<SceneSwitcher>();

        StartCoroutine(ReadFile());
    }

    public IEnumerator ReadFile()
    {
        var path = Path.Combine(Application.streamingAssetsPath, FilePath);
        using (StreamReader reader = new StreamReader(path))
        {
            Debug.Log("Using stream reader");

            DialogTemplate template = null;
            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                int index = line.IndexOf("]");
                if (index != -1)
                {
                    string iden = line.Substring(1, index - 1);
                    Debug.Log("Identifier: " + iden);
                    Dict.TryGetValue(iden, out template);
                }
                string message = line.Substring(index + 1);

                Debug.Log("Message: " + message);

                if (template != null)
                {
                    FillInfo(template, message);
                    yield return new WaitForSeconds(1f);
                    yield return WaitForInput(template.Button);
                }
            }
        }

        Switcher.SwitchScenes();
    }

    private void FillInfo(DialogTemplate template, string message)
    {
        template.SpeakerPortrait.SetActive(true);
        template.SpeakerText.SetActive(true);
        template.Background.SetActive(true);
        template.SpeechBubble.SetActive(true);
        if (template.Audio != null && !template.AudioHasPlayed())
        {
            template.Audio.Play();
            template.SetAudioPlayed(true);

            if (BGMusic != null && BGMusic.isPlaying)
            {
                BGMusic.Pause();
            }
        } else if (BGMusic != null)
        {
            if (!BGMusic.isPlaying)
            {
                BGMusic.UnPause();
            }
        }

        // Hide other templates
        foreach (DialogTemplate t in Dict.Values)
        {
            if (!t.Identifier.Equals(template.Identifier))
            {
                t.SpeakerPortrait.SetActive(false);
                t.SpeakerText.SetActive(false);
                t.Background.SetActive(false);
                t.SpeechBubble.SetActive(false);
                if (t.Audio != null)
                {
                    t.Audio.Stop();
                }
            }
        }
        Text text = template.SpeakerText.GetComponent<Text>();
        if (text != null)
        {
            text.text = message;
        }
    }

    private IEnumerator WaitForInput(Button button)
    {
        if (button == Button.Either)
        {
            while (!Input.GetButtonDown(RED_BUTTON) && !Input.GetButtonDown(BLUE_BUTTON))
            {
                yield return null;
            }
        }
        else
        {
            while (!Input.GetButtonDown(GetButtonString(button)))
            {
                yield return null;
            }
        }
    }

    private string GetButtonString(Button button)
    {
        switch (button)
        {
            case Button.RedPuppet:
                return RED_BUTTON;
            case Button.BluePuppet:
                return BLUE_BUTTON;
            default:
                return "";
        }
    }
}