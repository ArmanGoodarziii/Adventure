using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class UsernameManager : MonoBehaviour
{
    public InputField usernameInput;
    public GameObject usernamePanel;
    public Text myUsername;
    void Start()
    {
        if(PlayerPrefs.GetString("Username") == "" || PlayerPrefs.GetString("Username") == null)
        {
            usernamePanel.SetActive(true);
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("Username");

            myUsername.text = "Username" + PlayerPrefs.GetString("Username");

            usernamePanel.SetActive(false);
        }
    }
    public void SaveUsername()
    {
        PhotonNetwork.NickName = usernameInput.text;

        PlayerPrefs.SetString("Username" , usernameInput.text);

        myUsername.text = "Username:" + usernameInput.text;

        usernamePanel.SetActive(false);
    }
}
