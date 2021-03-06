using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

namespace Assets
{
    /**
     * CharacterSelection handles user's selection of character for a game.
     */
    public class CharacterSelection : MonoBehaviour
    {
        public SelectedCharacter sel;
        private Hashtable playerProperties = new Hashtable();
        public GameObject[] playerText;
        public GameObject[] readyText;
        public GameObject startButton;
        public GameObject readyButton;
        private bool readyState = false;
        private GameObject modeObject;
        private int mode;
        public GameObject displayText;

        private string player1_id;
        private string player2_id;

        private void Awake()
        {
            //find do not destroy object and get values
            modeObject = GameObject.Find("modeObject");
            mode = modeObject.GetComponent<mode>().modeType;

            if (mode == 1 || mode == 2)//if multiplayer or custom mode
            {
                //set all player's playerReady property to false
                for (int i = 0; i < PhotonNetworkMngr.checkPlayerListLength(); i++)
                {
                    playerProperties["PlayerReady"] = false;
                    PhotonNetworkMngr.setPlayerPropertiesForCurrentPlayer(playerProperties);
                }
            }


        }

        /**
         * Start() is called before the first frame update
         * Scenes will sync for all photon players
         */
        public void Start()
        {

            //scenes will sync for all photon players
            PhotonNetworkMngr.setAutomaticallySyncScene(true);

            //playerProperties.Add("PlayerReady", readyState);
            //check add UserId to the player
            string userid = FirebaseManager.auth.CurrentUser.UserId;
            UnityEngine.Debug.Log(userid);
            PhotonNetworkMngr.setUserId(userid);

            string username = FirebaseManager.auth.CurrentUser.DisplayName;
            UnityEngine.Debug.Log(username);
            //Photon Netwrok is a static class
            //Set player name
            PhotonNetworkMngr.setNickName(username);
        }

        /**
         * Function to update UI when user inputs.
         */
        public void Update()
        {
            checkInputs();
        }

        /**
         * Function to check inputs from user.
         * Displays name of chosen character.
         * Ready button is shown after player has chosen a character.
         * When player(single player) or players(multiplayer) are ready, a Start button is shown.
         */
        public void checkInputs()
        {
            //ready button is hidden until player chooses a character
            if (sel.selection != "")
                readyButton.SetActive(true);

            if (mode == 1 || mode == 2) // if multiplayer or custom game
            {
                //if all players are ready and player is master client, show start button
                if (allPlayersReady() && PhotonNetworkMngr.checkIsMasterClient())
                {
                    startButton.SetActive(true);
                }
                //check when a player is ready and display ready text under name
                for (int i = 0; i < PhotonNetworkMngr.checkPlayerListLength(); i++)
                {
                    playerText[i].SetActive(true);
                    if ((bool)PhotonNetworkMngr.getPlayerPropertyForSpecificPlayer(PhotonNetworkMngr.getPlayerFromPlayerlist(i), "PlayerReady"))
                    {
                        readyText[PhotonNetworkMngr.getPlayerFromPlayerlist(i).ID - 1].SetActive(true);
                    }
                    else
                    {
                        readyText[PhotonNetworkMngr.getPlayerFromPlayerlist(i).ID - 1].SetActive(false);
                    }
                }
            }
            //display character selection choice on screen
            switch (sel.selection)
            {
                case "alexis":
                    displayText.GetComponent<UnityEngine.UI.Text>().text = "Alexis";

                    break;

                case "chubs":
                    displayText.GetComponent<UnityEngine.UI.Text>().text = "Chubs";

                    break;
                case "john":
                    displayText.GetComponent<UnityEngine.UI.Text>().text = "John Cena";

                    break;
            }
        }

        /**
         * Function to save character selection when player clicks button corresponding to the character
         */
        public void aPress()
        {
            sel.selection = "alexis";
        }
        public void cPress()
        {
            sel.selection = "chubs";
        }
        public void jPress()
        {
            sel.selection = "john";
        }

        /**
         * Function to start game.
         * Create and join room for single player.
         * Load level-select scene if multiplayer or custom lobby.
         */
        public void startGame()
        {
            if (mode == 0 || mode == 3)// if single player
            {
                //create and join room for single player
                PhotonNetwork.ConnectUsingSettings("0.2");
                RoomOptions roomOptions = new RoomOptions();
                PhotonNetworkMngr.createRoom(null, roomOptions, TypedLobby.Default);
                PhotonNetworkMngr.loadLevel("Level_select");
            }
            else //if multiplayer or custom game
            {
                //load Level_select scene
                if (PhotonNetworkMngr.checkIsMasterClient())
                {
                    Debug.Log("load level");
                    PhotonNetworkMngr.loadLevel("Level_select");
                }

            }

        }

        /**
         * Function to update player properties when ready button is clicked.
         */
        public void readyClick()
        {
            if (mode == 1 || mode == 2)// if multiplayer or custom
            {
                //set player properties "playerReady" to true
                playerProperties["PlayerReady"] = true;
                PhotonNetworkMngr.setPlayerPropertiesForCurrentPlayer(playerProperties);
            }
            else
                startButton.SetActive(true);

        }

        /**
         * Function to check if all players have their player properties "player ready" to be true.
         */
        private bool allPlayersReady()
        {
            //check if all players have their player properties "player ready" to be true
            {
                foreach (var photonPlayer in PhotonNetwork.playerList)
                {

                    //if not all players ready
                    if (!(bool)PhotonNetworkMngr.getPlayerPropertyForSpecificPlayer(photonPlayer, "PlayerReady"))
                        return false;
                }

                return true;
            }
        }

        /** 
         *  Brings user back to main menu.
         *  Leaves photon network room if multiplayer or custom lobby.
         */
        public void backButton()
        {
            if (mode == 1 || mode == 2)
                PhotonNetworkMngr.leaveRoom();
            Destroy(GameObject.Find("modeObject"));
            PhotonNetworkMngr.loadLevel("Main Menu");
        }


    }

}
