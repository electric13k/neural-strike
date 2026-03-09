using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CameraFeed : MonoBehaviour
{
    [Header("UI References")]
    public RawImage feedImage;
    public TextMeshProUGUI botNameText;
    public Camera feedCamera;
    
    private BotController bot;
    private RenderTexture renderTexture;
    
    public void Initialize(BotController botController)
    {
        bot = botController;
        
        if (botNameText != null)
            botNameText.text = bot.gameObject.name;
        
        SetupCamera();
    }
    
    private void SetupCamera()
    {
        if (bot == null) return;
        
        // Create render texture
        renderTexture = new RenderTexture(256, 144, 16);
        
        // Create camera
        GameObject camObj = new GameObject($"FeedCam_{bot.gameObject.name}");
        camObj.transform.SetParent(bot.transform);
        camObj.transform.localPosition = new Vector3(0, 2, -3);
        camObj.transform.localRotation = Quaternion.Euler(15, 0, 0);
        
        feedCamera = camObj.AddComponent<Camera>();
        feedCamera.targetTexture = renderTexture;
        feedCamera.depth = -10;
        feedCamera.clearFlags = CameraClearFlags.SolidColor;
        feedCamera.backgroundColor = Color.black;
        
        if (feedImage != null)
            feedImage.texture = renderTexture;
    }
    
    private void OnDestroy()
    {
        if (feedCamera != null)
            Destroy(feedCamera.gameObject);
        
        if (renderTexture != null)
            renderTexture.Release();
    }
    
    public BotController GetBot() { return bot; }
}