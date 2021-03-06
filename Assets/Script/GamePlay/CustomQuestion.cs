using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FullSerializer;
using Proyecto26;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace Assets
{
    [Serializable]
    public class CustomQuestion : MonoBehaviour
    {
        private string databaseURL = "https://fir-auth-9c8cd-default-rtdb.firebaseio.com/CustomLobbyQuestions/";
        private string AuthKey = "AIzaSyCp3-tVb1biSiZ4fASGQ_gUit-IZhko5mM";
        private string userPassword = "password123";
        private string userEmail = "teacher123@gmail.com";

        public static fsSerializer serializer = new fsSerializer();

        private string idToken;
        public static string localId;

        public InputField getLobbyName;
        public InputField getQuestion;
        public InputField getOptions;
        public InputField getAnswer;
        public Dropdown questionTypeSelection;
        public Text quizCounterDisplay;
        public Text questionCounterDisplay;
        public Text warningDisplay;
        public Dropdown selectQuestion;
        public Dropdown selectQuiz;
        public GameObject continueButton;
        public GameObject classAssign;

        public int quizCounters = 1;
        public int questionCounters = 1;
        private int questionTypeCounter = 0;
        private string questionType = null;
        private string newQuestionType = null;
        private int setType = -1;
        private int checkType = -1;
        private int quizNo = -1;
        private int questionNo = -1;
        int quizCounterHolder = -1;
        int questionCounterHolder = -1;
        bool allQuestionsCreated = false;
        private string assignmentID;
        private GameObject teacherObject;
        private bool isTeacher;
        MCQData questionData = new MCQData();
        public Text assignmenText;
        private void Awake()
        {
            
        }

        private void Start()
        {
            teacherObject = GameObject.Find("TeacherObject");
            if(teacherObject!=null)
                isTeacher = teacherObject.GetComponent<isTeacherObject>().isTeacher;

            if (isTeacher)
            {
                classAssign.SetActive(true);
                getLobbyName.interactable = false;
                assignmentID = FirebaseManager.getAssignmentKey();
            }  

            
            PhotonNetwork.ConnectUsingSettings("0.2");
            string userData = "{\"email\":\"" + userEmail + "\",\"password\":\"" + userPassword + "\",\"returnSecureToken\":true}";
            RestClient.Post<SignResponse>("https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key=" + AuthKey, userData).Then(
                response =>
                {
                    idToken = response.idToken;
                    localId = response.localId;
                }).Catch(error =>
                {
                    Debug.Log(error);
                });
            Debug.Log("test2");

        }
        private void Update()
        {
            //if 1 question created, continue button set active (for testing only)
            /*if (quizCounters == 1 && questionCounters == 2)
                allQuestionsCreated = true;*/
            allQuestionsCreated = true;
            if (allQuestionsCreated)
                continueButton.SetActive(true);
            if (string.Compare(questionTypeSelection.options[questionTypeSelection.value].text, "Short Answer") == 0)
            {
                getOptions.interactable = false;
            }
            else
            {
                getOptions.interactable = true;
            }
        }

        /**
         * Goes to next page depending on type of account.
         */
        public async void continueBut()
        {
            Debug.Log("cont");
            if (isTeacher)
            {
                Dropdown classDrop = classAssign.GetComponent<Dropdown>();
                string classAssigned = classDrop.options[classDrop.value].text;
                Debug.Log("class assigned = " + classAssigned);
                //string classAssigned = classAssign.GetComponent<InputField>().text;
                Debug.Log($"class assigned is{ classAssigned}");
                await FirebaseManager.addToAllSubscribedStudents(classAssigned, assignmentID);
                Debug.Log("All students' assignmentlist updated");
                assignmenText.text = "Assignment ID: " + assignmentID;

            }
            else
            {
                PhotonNetwork.CreateRoom(getLobbyName.text, new RoomOptions() { maxPlayers = 2 }, null);
                PhotonNetwork.LoadLevel("Lobby");
            }

        }

        /**
         * Post questions to database when button is clicked.
         */
        public void OnSubmit()
        {
            PostToDatabase();
            questionTypeChecker();
            getAnswer.text = "";
            getQuestion.text = "";
            getOptions.text = "";

        }

        /**
         * To edit question in database.
         */
        public void submitQuestionChange()
        {
            retrieveCustomInfo();
        }

        /**
         * Returns to previous page.
         * Teacher menu if acc type is teacher.
         * Main menu is acc type is student
         */
        public void backButton()
        {
            if (isTeacher)
            {
                Destroy(GameObject.Find("TeacherObject"));
                PhotonNetwork.LoadLevel("Teacher Menu");
            }
            else
            {
                Destroy(GameObject.Find("modeObject"));
                PhotonNetwork.LoadLevel("Main Menu");
            }


        }

        /**
         * Post questions to database.
         */
        private void PostToDatabase()
        {

            MCQData mcqData = new MCQData();

            mcqData.Answer = getAnswer.text;
            mcqData.Question = getQuestion.text;
            mcqData.Options = getOptions.text;

            Debug.Log(quizCounters + " " + questionCounters);


            questionCounterHolder = questionCounters + 1;
            quizCounterHolder = quizCounters;
            if (questionCounters == 3 && quizCounters == 5)
            {
                quizCounterHolder = 1;
                questionCounterHolder = 1;
                quizCounterDisplay.text = "Quiz : " + quizCounterHolder.ToString() + " / 5 Quizzes";
                questionCounterDisplay.text = "Question: " + questionCounterHolder.ToString() + " / 3 Questions";
            }
            else if (questionCounters == 3)
            {
                questionCounterHolder = 1;
                quizCounterHolder = quizCounters + 1;
                quizCounterDisplay.text = "Quiz : " + quizCounterHolder.ToString() + " / 5 Quizzes";
                questionCounterDisplay.text = "Question: " + questionCounterHolder.ToString() + " / 3 Questions";
            }
            else
            {
                quizCounterDisplay.text = "Quiz : " + quizCounterHolder.ToString() + " / 5 Quizzes";
                questionCounterDisplay.text = "Question: " + questionCounterHolder.ToString() + " / 3 Questions";
            }

            if (isTeacher)
            {
                //RestClient.Put("https://fir-auth-9c8cd-default-rtdb.firebaseio.com/Assignments/" + assignmentID + "/" + "quiz_" + quizCounters.ToString() + "/" + questionCounters.ToString() + "/" + localId + ".json?auth=" + idToken, mcqData);
                //Insert add assignment to student lsit
                RestClient.Put("https://fir-auth-9c8cd-default-rtdb.firebaseio.com/Assignments/" + assignmentID + "/" + "quiz_" + quizCounters.ToString() + "/" + questionCounters.ToString() + ".json?auth=" + idToken, mcqData);
                

            }
            else
            {
                //RestClient.Put(databaseURL + getLobbyName.text + "/" + "quiz_" + quizCounters.ToString() + "/" + questionCounters.ToString() + "/" + localId + ".json?auth=" + idToken, mcqData);
                RestClient.Put(databaseURL + getLobbyName.text + "/" + "quiz_" + quizCounters.ToString() + "/" + questionCounters.ToString() + ".json?auth=" + idToken, mcqData);
                Debug.Log(databaseURL + getLobbyName.text + "/" + "quiz_" + quizCounters.ToString() + "/" + questionCounters.ToString() + ".json?auth=" + idToken);
            }

            if (questionCounters == 3)
            {
                quizCounters = quizCounters + 1;
            }
            questionCounters = questionCounters + 1;

            if (quizCounters == 6 && questionCounters == 4)
            {
                Debug.Log("Reached 15 Questions");
                quizCounters = 1;
                questionCounters = 1;
                allQuestionsCreated = true;
            }
            else if (questionCounters == 4 && quizCounters <= 5)
            {

                questionCounters = 1;

            }
        }

        /**
         * Checks question type and sets the type.
         */
        private void questionTypeChecker()
        {

            if (questionTypeCounter > 2)
            {
                questionTypeCounter = 0;
            }
            if (questionTypeCounter == 0)
            {
                questionType = getOptions.text;

                if (String.IsNullOrEmpty(questionType))
                {
                    setType = 0;
                }
                else
                {
                    setType = 1;
                }

            }
            newQuestionType = getOptions.text;
            if (String.IsNullOrEmpty(newQuestionType) && questionTypeCounter != 0)
            {
                checkType = 0;
            }
            else
            {
                checkType = 1;
            }


            if (setType != checkType && questionTypeCounter != 0)
            {
                Debug.Log("Error question type not the same");
                warningDisplay.text = "Error for previous question, please go back and re enter question.";
            }
            else
            {
                warningDisplay.text = "";
            }
            questionTypeCounter += 1;

        }

        /**
         * Get custom questions.
         */
        private void retrieveCustomInfo()
        {
            Debug.Log("tsad");
            questionNo = selectQuestion.value + 1;
            quizNo = selectQuiz.value + 1;

            if (isTeacher)
            {
                RestClient.Get<MCQData>("https://fir-auth-9c8cd-default-rtdb.firebaseio.com/Assignments/" + assignmentID + "/" + "quiz_" + quizNo.ToString() + "/" + questionNo.ToString() + "/" + ".json?auth=" + idToken).Then(response =>
                {
                    Debug.Log("hello");
                    questionData = response;
                    reInsertQuestion();

                });
            }
            else
            {
                RestClient.Get<MCQData>(databaseURL + getLobbyName.text + "/" + "quiz_" + quizNo.ToString() + "/" + questionNo.ToString() + "/" +  ".json?auth=" + idToken).Then(response =>
                {
                    Debug.Log("hello");
                    questionData = response;
                    reInsertQuestion();

                });
            }



        }

        /**
         * Edits question when user submits question changes.
         */
        private void reInsertQuestion()
        {
            int quizSelect = selectQuiz.value + 1;
            int questionSelect = selectQuestion.value + 1;

            quizCounters = quizSelect;
            questionCounters = questionSelect;

            Debug.Log(questionData.Options);
            getQuestion.text = questionData.Question;
            getAnswer.text = questionData.Answer;
            getOptions.text = questionData.Options;

            Debug.Log(quizCounters + " " + questionCounters);

        }
    }
}

