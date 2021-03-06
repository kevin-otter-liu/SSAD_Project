using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace Assets
{
    /**
     * SummaryReportManager handles Summary Report List for students in a class.
     */
    public class SummaryReportManager : MonoBehaviour
    {
        [Header("SummaryReport")]
        public GameObject classElement;
        public GameObject studentNameElement;
        public Transform classContent;
        public Transform studentNameContent;

        public static SummaryReportManager instance;

        /** 
         * Start() is called before the first frame update.
         * Loads class list.
         */
        void Start()
        {
            if (instance == null)
            {
                instance = this;
                Debug.Log("SummaryReportManager instantiated");
                Debug.Log("Loading Class Lists in Summary Report UI");
                LoadClassList();
                Debug.Log("Preloading Class Lists in Summary Report UI Done");

            }
        }

        /**
         * Loads class list with students names.
         */
        public void LoadClassList()
        {
            List<string> classList = new List<string>() {
            "FS6","FS7","FS8","FS9","TestClass"
        };
            ClassElement element = new ClassElement();
            foreach (Transform child in this.classContent.transform)
            {
                UnityEngine.Debug.Log("reached before transform loop");
                Destroy(child.gameObject);
                UnityEngine.Debug.Log("reached after transform loop");
            }
            foreach (string className in classList)
            {
                //get instantiated gameobject
                GameObject classBoardElement = Instantiate(classElement, this.classContent);
                //add and get script to gameobject
                element = classBoardElement.AddComponent<ClassElement>();
                //assign script's text to gameobject's child's text
                element.TextName = classBoardElement.transform.GetChild(0).GetComponent<Text>();
                //edit text
                element.NewElement(className);
                //add onclick function to gameobject which will load student names
                classBoardElement.GetComponent<Button>().onClick.AddListener(async delegate
                {
                    await LoadStudentNamesAsync(className);

                });
            }
        }

        /**
         * Button that Loads studentNames for the summary report.
         * Loads all student names in class.
         * @param  className contains class name to load student data from that class.
         */

        async public Task LoadStudentNamesAsync(string className)
        {
            UnityEngine.Debug.Log("reached inside LoadstudentNames function");
            Dictionary<string, string> StudentNames;
            var StudentNamesTask = FirebaseManager.LoadStudentNamesAsync(className);
            StudentNames = await StudentNamesTask;

            foreach (Transform child in studentNameContent.transform)
            {
                Destroy(child.gameObject);
                UnityEngine.Debug.Log("Destroyed a child");
            }
            //Need to instantiate prefab dynamically
            foreach (var item in StudentNames)
            {
                string username = item.Value;
                string userid = item.Key;
                UnityEngine.Debug.Log($"LoadStudentNamesAsync() in SummaryReport: Successfully loaded  with username:{username} and \n userid:{userid}");
                GameObject nameBoardElement = Instantiate(studentNameElement, studentNameContent);
                nameBoardElement.GetComponent<StudentNameElement>().NewStudentNameElement(username, userid);
                UnityEngine.Debug.Log($"Successfully instantiated classelement on Summary Report with for username: {username}");
            }
        }

        /**
         * Button to load class list
         */
        public void summaryReportButton()
        {
            LoadClassList();
            TeacherMenuUIManager.instance.summaryReport();
        }


        // Update is called once per frame
        void Update()
        {

        }
    }

}
