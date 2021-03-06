using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace Assets
{

    /**
     * LeaderBoardManager handles the Leaderboard page
     */
    public class LeaderBoardManager : MonoBehaviour
    {
        //LeaderBoard variables
        public static LeaderBoardManager instance;
        [Header("LeaderBoard")]
        public GameObject scoreElement;
        public Transform scoreboardCustomContent;
        public Transform scoreboardMultiContent;
        public Transform scoreboardSingleContent;

        /**
         * Start() is called before the first frame update. Loads all the leaderboards.
         */
        void Start()
        {
            if (instance == null)
            {
                instance = this;
                Debug.Log("leaderBoard Manager instantiated");
                //preloading all leaderboards at main menu
                loadAllLeaderBoards();
            }
        }

        /**
         * Loads leaderboards for Single Player, Multiplayer and Custom Lobbys.
         */
        async public void loadAllLeaderBoards()
        {
            var customPlayerTask = LoadLeaderBoardData("customPlayer", scoreboardCustomContent);
            var multiPlayerTask = LoadLeaderBoardData("multiPlayer", scoreboardMultiContent);
            var singlePlayerTask = LoadLeaderBoardData("singlePlayer", scoreboardSingleContent);

            await Task.WhenAll(customPlayerTask, multiPlayerTask, singlePlayerTask);
            Debug.Log("Preloading of leaderBoard is done");
        }

        /**
         * Gets all the users data from Firebase databse, ordered by score values.
         */
        async public Task LoadLeaderBoardData(string gameMode, Transform scoreboardContent)
        {
            List<InitUser> LeaderBoardUsers = new List<InitUser>();
            var LeaderBoardUsersTask = FirebaseManager.GetLeaderBoardFromDatabase(gameMode);
            LeaderBoardUsers = await LeaderBoardUsersTask;
            Debug.Log("successfully returned from GetLeaderBoardFromDatabase");

            foreach (Transform child in scoreboardContent.transform)
            {
                Destroy(child.gameObject);
                UnityEngine.Debug.Log($"Destroyed child in leaderboard {gameMode}");
            }
            //Loop through every users UID
            foreach (InitUser user in LeaderBoardUsers)
            {
                string username = user.username;
                float totalPoints;
                if (gameMode == "customPlayer")
                {
                    totalPoints = user.customPlayer.totalPoints;

                }
                else if (gameMode == "singlePlayer")
                {
                    totalPoints = user.singlePlayer.totalPoints;

                }
                else
                {
                    totalPoints = user.multiPlayer.totalPoints;
                }
                //Instantiate new scoreboard elements
                GameObject scoreBoardElement = Instantiate(scoreElement, scoreboardContent);
                scoreBoardElement.GetComponent<ScoreElement>().NewScoreElement(username, totalPoints);
                UnityEngine.Debug.Log($"instantiating {username} on {gameMode} leaderboard with {totalPoints}");
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
