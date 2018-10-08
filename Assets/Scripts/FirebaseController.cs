using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using ZXing;
using ZXing.QrCode;

public class FirebaseController : MonoBehaviour {

    public string loadLevel;
    public InputField roomInput;
    public Text errorMessage;
    public GameObject dialog;
    public string status;
    public bool waiting;
    public bool qrCodeScene;
    public bool questionScene;
    public bool studentNameScene;
    public GameObject questionButton;
    public static GameObject questionContent;
    public GameObject closeButton;

    public string questionIndex;
    public static string globalQuestionIndex;
    public string[] choices;
    public string answer;
    public string replyAnswer;
    public string questionStatus;
    public string selected;
    public static string[] choiceButtons = { "Choice A", "Choice B", "Choice C", "Choice D" };
    public static string[] choiceCodes = { "a", "b", "c", "d" };
    public GameObject[] choiceObjects;
    public GameObject[] replyObjects;
    public GameObject replyButton;

    public static Color colorFocus = new Color(254 / 255.0F, 126 / 255.0F, 123 / 255.0F, 1.0F);
    public static Color colorAnswer = new Color(209 / 255.0F, 209 / 255.0F, 209 / 255.0F, 1.0F);
    public static Color colorNone = new Color(0.0F, 0.0F, 0.0F, 0.0F);

    public static Color colorTextRed = new Color(254 / 255.0F, 126 / 255.0F, 123 / 255.0F, 1.0F);
    public static Color colorTextWhite = new Color(255.0F, 255.0F, 255.0F, 1.0F);

    public Sprite choiceDefault;
    public Sprite choiceSelected;
    public Sprite choiceCorrect;
    public Sprite choiceWrong;

    public DatabaseReference mDatabaseRef;
    public static DataSnapshot snapshot;
    public static string roomCode;
    public static string studentName;
    public static string mode;
    public static int score;

    public static WebCamTexture camTexture;
    private Rect qrCodeRect;
    public string loadLevelBack;
	private Quaternion baseRotation;

    private string timeDifferentURL = "https://us-central1-histolearn-unesco.cloudfunctions.net/accessRoom?code=";
    private string timeDifferent;
    private bool isRoomDone = false;

    // Use this for initialization
    void Start () {
 
        // Set this before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://histolearn-unesco.firebaseio.com/");

        if (questionScene)
            questionContent = GameObject.Find("Question Plane").transform.Find("Panel Question").transform.Find("Content").gameObject;

        // Get the root reference location of the database.
        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        if (errorMessage)
            errorMessage.enabled = false;

        if (questionScene)
        {
            LoadQuestions();

            timeDifferent = GetTimeDifferent(roomCode);
            if (timeDifferent == "done" || timeDifferent == null)
            {
                RoomDone();
            }
            else
            {
                Invoke("RoomDone", int.Parse(timeDifferent));
                Debug.Log("Time Different: " + timeDifferent + " seconds");
            }
        }
        else if (qrCodeScene)
            ReadQRCode();

        if (roomCode != null)
        {
            FirebaseDatabase.DefaultInstance
            .GetReference("rooms").Child(roomCode).Child("status")
            .ValueChanged += HandleValueChanged;
        }
    }
	
	// Update is called once per frame
	void Update () {
		if (qrCodeScene)
			transform.rotation = baseRotation * Quaternion.AngleAxis(camTexture.videoRotationAngle, Vector3.up);
	}

    void HandleValueChanged(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (dialog && args.Snapshot.Value.ToString() != status)
        {
            if (questionScene)
            {
                RoomDone();
            }
            else if(studentNameScene)
                dialog.SetActive(true);
        }

        if (waiting)
        {
            if (args.Snapshot.Value.ToString() == "active")
            {
                LoadScene.ChangeScene(loadLevel);
            }
        }
    }

    public void CheckRoomCodeFromInput() {
        string room = roomInput.text;
        CheckRoomCode(room);
        roomInput.text = "";
    }

    public void ReadQRCode() {
        qrCodeRect = new Rect(30, 0, Screen.width - 60, Screen.height - 60);
        camTexture = new WebCamTexture();
        camTexture.requestedHeight = (int) qrCodeRect.height;
        camTexture.requestedWidth = (int) qrCodeRect.width;

		//int rotAngle = -camTexture.videoRotationAngle;
		//while (rotAngle < 0) rotAngle += 360;
		//while (rotAngle > 360) rotAngle -= 360;

		//bool flipY;
		//if (Application.platform == RuntimePlatform.IPhonePlayer)
		//	flipY = !camTexture.videoVerticallyMirrored;

		Renderer renderer = GetComponent<Renderer>();
		renderer.material.mainTexture = camTexture;
		baseRotation = transform.rotation;

        if (camTexture != null) 
            camTexture.Play();
    }

    void OnGUI() {
        if (qrCodeScene && camTexture.isPlaying)
        {
            // drawing the camera on screen
            GUI.DrawTexture(qrCodeRect, camTexture, ScaleMode.ScaleToFit);
            // do the reading — you might want to attempt to read less often than you draw on the screen for performance sake
            try
            {
                IBarcodeReader barcodeReader = new BarcodeReader();
                // decode the current frame
                var result = barcodeReader.Decode(camTexture.GetPixels32(), camTexture.width, camTexture.height);
                if (result != null)
                {
                    Debug.Log("QR Code: " + result.Text);
                    CheckRoomCode(result.Text);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        }
    }

    private void CheckRoomCode (string room) {
        if(!Regex.IsMatch(room, @"^[a-zA-Z0-9]+$"))
        {
            errorMessage.text = "Wrong room code!";
            errorMessage.enabled = true;
        }
        else if (room != "")
        {
            FirebaseDatabase.DefaultInstance
            .GetReference("rooms").Child(room)
            .GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error");
                }
                else if (task.IsCompleted)
                {
                    snapshot = task.Result;
 
                    if (snapshot.Value != null)
                    {
                        string status = snapshot.Child("status").Value.ToString();

                        if (status == "waiting")
                        {
                            if (camTexture && camTexture.isPlaying)
                                camTexture.Stop();

                            roomCode = room;
                            mode = snapshot.Child("mode").Value.ToString();
                            errorMessage.enabled = false;
                            LoadScene.ChangeScene(loadLevel);
                        }
                        else if (status == "edit")
                        {
                            errorMessage.text = "Room is not ready!";
                            errorMessage.enabled = true;
                        }
                        else if (status == "done")
                        {
                            errorMessage.text = "Time-out!";
                            errorMessage.enabled = true;
                        }
                        else if (status == "active")
                        {
                            timeDifferent = GetTimeDifferent(room);

                            if (timeDifferent != null)
                            {
                                if (timeDifferent == "done")
                                {
                                    errorMessage.text = "Time-out!";
                                    errorMessage.enabled = true;
                                }
                                else
                                {
                                    errorMessage.text = "Room already started!";
                                    errorMessage.enabled = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        errorMessage.text = "Room not found!";
                        errorMessage.enabled = true;
                    }
                }
            });
        }
        else
        {
            errorMessage.text = "Please enter room code!";
            errorMessage.enabled = true;
        }
    }

    public void CheckStudentName()
    {
        if (roomInput.text != "")
        {
            string name = roomInput.text;

            if (!snapshot.HasChild("students"))
            {
                score = 0;
                mDatabaseRef.Child("rooms").Child(roomCode).Child("students").Child(name).Child("name").SetValueAsync(name);
                mDatabaseRef.Child("rooms").Child(roomCode).Child("students").Child(name).Child("score").SetValueAsync(score);
                studentName = name;

                errorMessage.enabled = false;
                LoadScene.ChangeScene(loadLevel);
            }
            else
            {
                foreach(DataSnapshot student in snapshot.Child("students").Children)
                {
                    if (name == student.Child("name").Value.ToString())
                    {
                        roomInput.text = "";
                        errorMessage.text = "Name already used!";
                        errorMessage.enabled = true;
                        return;
                    }
                }

                score = 0;
                mDatabaseRef.Child("rooms").Child(roomCode).Child("students").Child(name).Child("name").SetValueAsync(name);
                mDatabaseRef.Child("rooms").Child(roomCode).Child("students").Child(name).Child("score").SetValueAsync(score);
                studentName = name;

                errorMessage.enabled = false;
                LoadScene.ChangeScene(loadLevel);
            }
        }
        else
        {
            errorMessage.text = "Please enter your name!";
            errorMessage.enabled = true;
        }
    }

    public void LoadQuestions() {
        if (mode == "choice")
        {
            for (int i = 0; i < choiceObjects.Length; i++)
                choiceObjects[i].SetActive(true);
        }
        else if (mode == "reply")
        {
            for (int i = 0; i < replyObjects.Length; i++)
                replyObjects[i].SetActive(true);
        }

        FirebaseDatabase.DefaultInstance
        .GetReference("rooms").Child(roomCode).Child("questions")
        .GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error");
            }
            else if (task.IsCompleted)
            {
                snapshot = task.Result;
                GameObject newButton;
                float width = questionButton.GetComponent<RectTransform>().sizeDelta.x;
                float height = questionButton.GetComponent<RectTransform>().sizeDelta.y;
                string[] randomChoices = { "answer", "choice1", "choice2", "choice3" };

                questionContent.GetComponent<RectTransform>().sizeDelta = new Vector3(
                    width * snapshot.ChildrenCount, height, 0
                );

                for (int i = 0; i < snapshot.ChildrenCount; i++)
                {
                    newButton = Instantiate(questionButton, 
                        new Vector3(i * width, 0, 0), 
                        Quaternion.identity);
                    newButton.GetComponentInChildren<Text>().text = (i + 1).ToString();
                    newButton.transform.SetParent(questionContent.transform, false);
                    newButton.GetComponent<FirebaseController>().questionIndex = i.ToString();
                    newButton.GetComponent<FirebaseController>().choices = General.ShuffleStringList(randomChoices);
                    newButton.GetComponent<FirebaseController>().questionStatus = "none";
                }

                choices = questionContent.transform.GetChild(0).GetComponent<FirebaseController>().choices;
                UpdateQuestion("0", choices);
            }
        });
    }

    public void UpdateQuestion() {
        UpdateQuestion(questionIndex, choices);
    }

    public void UpdateQuestion(string questionIndex, string[] choices) {
        globalQuestionIndex = questionIndex;

        FirebaseDatabase.DefaultInstance
        .GetReference("rooms").Child(roomCode).Child("questions").Child(questionIndex)
        .GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error");
            }
            else if (task.IsCompleted)
            {
                snapshot = task.Result;

                GameObject.Find("Question Plane").transform.Find("Question Number").GetComponent<Text>().text = "Question " + (int.Parse(questionIndex) + 1).ToString();
                GameObject.Find("Question Plane").transform.Find("Question").GetComponent<Text>().text =  snapshot.Child("questionName").Value.ToString();
                string choiceName;

                if (mode == "choice")
                {

                    for (int i = 0; i < choiceButtons.Length; i++)
                    {
                        choiceName = snapshot.Child(choices[i]).Value.ToString();

                        GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).GetComponentInChildren<Text>().text = choiceCodes[i] + ". " + choiceName;
                        GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).GetComponent<FirebaseController>().answer = choices[i];
                        GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).GetComponent<FirebaseController>().questionIndex = questionIndex;
                    }
                }
                else if (mode == "reply")
                    GameObject.Find("Question Plane").transform.Find("Reply Button").GetComponent<FirebaseController>().questionIndex = questionIndex;

                ResetQuestionColor();
                questionContent.transform.GetChild(int.Parse(questionIndex)).Find("Panel").GetComponent<Image>().color = colorFocus;

                if (mode == "choice")
                    UpdateChoiceButton(questionIndex, questionContent.transform.GetChild(int.Parse(questionIndex)).GetComponent<FirebaseController>().selected);
                else if (mode == "reply")
                    UpdateReplyInput(questionIndex);
            }
        });
    }

    public void Answer() {
        if (questionContent.transform.GetChild(int.Parse(questionIndex)).GetComponent<FirebaseController>().questionStatus == "none")
        {
            if (mode == "choice")
            {
                mDatabaseRef.Child("rooms").Child(roomCode).Child("questions").Child(questionIndex).Child("replies")
                    .Child(studentName).SetValueAsync(answer);
                questionContent.transform.GetChild(int.Parse(questionIndex)).GetComponent<FirebaseController>().questionStatus = "answer";
                questionContent.transform.GetChild(int.Parse(questionIndex)).GetComponent<FirebaseController>().selected = answer;

                if (answer == "answer")
                {
                    score += 1;
                    mDatabaseRef.Child("rooms").Child(roomCode).Child("students").Child(studentName).Child("score").SetValueAsync(score);
                }
            }
            else if (mode == "reply")
            {
                string reply = GameObject.Find("Question Plane").transform.Find("Reply Input").GetComponent<InputField>().text;

                if (reply == "")
                    return;

                mDatabaseRef.Child("rooms").Child(roomCode).Child("questions").Child(questionIndex).Child("replies")
                    .Child(studentName).SetValueAsync(reply);
                questionContent.transform.GetChild(int.Parse(questionIndex)).GetComponent<FirebaseController>().questionStatus = "answer";
                questionContent.transform.GetChild(int.Parse(questionIndex)).GetComponent<FirebaseController>().replyAnswer = reply;
            }

            int i = (int.Parse(questionIndex) + 1) % questionContent.transform.childCount;
            while (i != int.Parse(questionIndex))
            {
                if (questionContent.transform.GetChild(i).GetComponent<FirebaseController>().questionStatus == "none")
                {
                    choices = questionContent.transform.GetChild(i).GetComponent<FirebaseController>().choices;
                    UpdateQuestion(i.ToString(), choices);
                    return;
                }

                i = (i + 1) % questionContent.transform.childCount;
            }

            // all questions is answered
            if (mode == "choice")
                UpdateChoiceButton(questionIndex, answer);
            else if (mode == "reply")
                UpdateReplyInput(questionIndex);
        }
    }

    public void ResetQuestionColor() {
        for (int i = 0; i < questionContent.transform.childCount; i++)
        {
            if (questionContent.transform.GetChild(i).GetComponent<FirebaseController>().questionStatus == "none")
                questionContent.transform.GetChild(i).Find("Panel").GetComponent<Image>().color = colorNone;
            else
                questionContent.transform.GetChild(i).Find("Panel").GetComponent<Image>().color = colorAnswer;
        }
    }

    public void UpdateChoiceButton(string questionIndex, string selected) {
        if (questionContent.transform.GetChild(int.Parse(questionIndex)).GetComponent<FirebaseController>().questionStatus == "answer")
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).GetComponent<FirebaseController>().answer == selected)
                {
                    GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).GetComponent<Image>().sprite = choiceSelected;
                    GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).Find("Text").GetComponent<Text>().color = colorTextWhite;
                }
                else
                {
                    GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).GetComponent<Image>().sprite = choiceDefault;
                    GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).Find("Text").GetComponent<Text>().color = colorTextRed;
                }
            }
        }
        else if (questionContent.transform.GetChild(int.Parse(questionIndex)).GetComponent<FirebaseController>().questionStatus == "solution")
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).GetComponent<FirebaseController>().answer == selected && selected != "answer")
                {
                    GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).GetComponent<Image>().sprite = choiceWrong;
                    GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).Find("Text").GetComponent<Text>().color = colorTextWhite;
                }
                else if (GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).GetComponent<FirebaseController>().answer == "answer")
                {
                    GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).GetComponent<Image>().sprite = choiceCorrect;
                    GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).Find("Text").GetComponent<Text>().color = colorTextWhite;
                }
                else
                {
                    GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).GetComponent<Image>().sprite = choiceDefault;
                    GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).Find("Text").GetComponent<Text>().color = colorTextRed;
                }
            }
        }
        else
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).GetComponent<Image>().sprite = choiceDefault;
                GameObject.Find("Question Plane").transform.Find(choiceButtons[i]).Find("Text").GetComponent<Text>().color = colorTextRed;
            }
        }
    }

    public void UpdateReplyInput(string questionIndex) {
        GameObject.Find("Question Plane").transform.Find("Reply Input").GetComponent<InputField>().text =
                questionContent.transform.GetChild(int.Parse(questionIndex)).GetComponent<FirebaseController>().replyAnswer;

        if (questionContent.transform.GetChild(int.Parse(questionIndex)).GetComponent<FirebaseController>().questionStatus == "none")
        {
            GameObject.Find("Question Plane").transform.Find("Reply Button").GetComponent<Image>().color = new Vector4(1.0F, 1.0F, 1.0F, 1.0F);
            GameObject.Find("Question Plane").transform.Find("Reply Input").GetComponent<InputField>().readOnly = false;
        }
        else if (questionContent.transform.GetChild(int.Parse(questionIndex)).GetComponent<FirebaseController>().questionStatus == "answer")
        {
            GameObject.Find("Question Plane").transform.Find("Reply Button").GetComponent<Image>().color = new Vector4(1.0F, 1.0F, 1.0F, 0.5F);
            GameObject.Find("Question Plane").transform.Find("Reply Input").GetComponent<InputField>().readOnly = true;
        }
    }

    public void GetSolution() {
        for (int i = 0; i < questionContent.transform.childCount; i++)
        {
            if (mode == "reply" && questionContent.transform.GetChild(i).GetComponent<FirebaseController>().questionStatus == "none")
                questionContent.transform.GetChild(i).GetComponent<FirebaseController>().replyAnswer = " ";
            questionContent.transform.GetChild(i).GetComponent<FirebaseController>().questionStatus = "solution";
        }

        if (mode == "reply")
        {
            GameObject.Find("Question Plane").transform.Find("Reply Input").GetComponent<InputField>().readOnly = true;
            replyButton.SetActive(false);
        }

        choices = questionContent.transform.GetChild(0).GetComponent<FirebaseController>().choices;
        UpdateQuestion("0", choices);

        dialog.SetActive(false);
    }

    public void UpdateReplyAnswer() {
        questionContent.transform.GetChild(int.Parse(globalQuestionIndex)).GetComponent<FirebaseController>().replyAnswer
            = GameObject.Find("Question Plane").transform.Find("Reply Input").GetComponent<InputField>().text;
    }

    public void OpenDialog() {
        dialog.SetActive(true);
    }

    public void ChangeSceneBack()
    {
        if (camTexture && camTexture.isPlaying)
            camTexture.Stop();

        LoadScene.ChangeScene(loadLevelBack);
    }

    private string GetTimeDifferent(string room)
    {
        WWW www = new WWW(timeDifferentURL + room);

        while (!www.isDone) ;

        if (www.error == null)
            return www.text;

        return null;
    }

    private void RoomDone()
    {
        if (!isRoomDone)
        {
            FirebaseDatabase.DefaultInstance
            .GetReference("rooms").Child(roomCode)
            .GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error");
                }
                else if (task.IsCompleted)
                {
                    snapshot = task.Result;

                    dialog.SetActive(true);
                    closeButton.SetActive(true);

                    dialog.transform.Find("Room Name").GetComponent<Text>().text = snapshot.Child("name").Value.ToString();
                    if (mode == "choice")
                    {
                        dialog.transform.Find("Score").GetComponent<Text>().text = "Score: " + score.ToString() + "/" + questionContent.transform.childCount.ToString();
                        dialog.transform.Find("Range").GetComponent<Text>().text = snapshot.Child("students").Child(studentName).Child("rank").Value.ToString();
                    }
                }
            });

            isRoomDone = true;
        }
    }
}