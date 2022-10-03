using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WindowSettings : MonoBehaviour
{
    [SerializeField] private AudioSettings[] _audioSettings;
    [SerializeField] private LanguagesSelection _languagesSelection;
    [SerializeField] private TMP_InputField _playerIDField;

    public void LoadAudioPrefs()
    {
        foreach (var audio in _audioSettings) audio.Init();
    }

    public void LoadLanguages()
    {
        StartCoroutine(_languagesSelection.Init());
    }

    public void SetId(string id)
    {
        _playerIDField.text = id;
    }
}
