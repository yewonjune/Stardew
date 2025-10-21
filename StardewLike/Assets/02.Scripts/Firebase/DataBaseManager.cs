using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;

public class DataBaseManager : MonoBehaviour
{
    public Text IDText;
    public Text NameText;
    public Text FramNameText;
    public Text GoldText;
    public Text LevelText;

    public Button Save_Btn;
    public Button Load_Btn;

    float ShowMsTimer = 0.0f;

    private DatabaseReference DBReference;
    private FirebaseAuth auth;

    public class Data
    {
        public string Name;
        public string FramName;
        public int Gold;
        //public List<string> Items;

        public Data()
        {

        }

        public Data(string name, string framName, int gold)
        {
            Name = name;
            FramName = framName;
            Gold = gold;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        DBReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
