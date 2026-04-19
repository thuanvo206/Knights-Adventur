using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningManager : MonoBehaviour
{
    AudioSource audioSource;

    public GameObject infoGame;
    public GameObject langGame;
    public GameObject keyBindings;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Scenes/GameScene");
        Time.timeScale = 1.0f;
    }

    public void QuitGame()
    {
        Debug.Log("Quit Button Worked");
        Application.Quit();
    }

    // BUG FIX: Logic cũ toggle isInfoButtonOn 2 lần trong cùng 1 hàm
    // → biến luôn trở về giá trị ban đầu → SetActive(false) không bao giờ được gọi
    // → nhấn nút lần 2 trở đi không đóng được panel
    public void InfoGame()
    {
        if (infoGame == null) return;
        infoGame.SetActive(!infoGame.activeSelf);
    }

    public void LanguageGame()
    {
        if (langGame == null) return;
        langGame.SetActive(!langGame.activeSelf);
    }

    public void KeyBindings()
    {
        if (keyBindings == null) return;
        keyBindings.SetActive(!keyBindings.activeSelf);
    }

    public void OkayButton(GameObject obj)
    {
        if (obj != null) obj.SetActive(false);
    }

    public void SocialButton()
    {
        Application.OpenURL("https://github.com/batuhandemiray");
    }
}