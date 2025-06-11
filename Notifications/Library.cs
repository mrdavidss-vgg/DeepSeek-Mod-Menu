using iiMenu.Classes;
using iiMenu.Menu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static iiMenu.Menu.Main;

namespace iiMenu.Notifications
{
    public class NotifiLib : MonoBehaviour
    {
        public static NotifiLib instance;
        private void Start()
        {
            instance = this;
            LogManager.Log("DeepSeek Notifications loaded");
        }

        private void Init()
        {
            MainCamera = GameObject.Find("Main Camera");
            HUDObj = new GameObject();
            HUDObj2 = new GameObject();
            HUDObj2.name = "DEEPSEEK_NOTIFICATION_HUD";
            HUDObj.name = "DEEPSEEK_NOTIFICATION_CANVAS";
            
            // Setup main canvas
            HUDObj.AddComponent<Canvas>();
            HUDObj.AddComponent<CanvasScaler>().dynamicPixelsPerUnit *= highQualityText ? 2.5f : 1.2f;
            HUDObj.AddComponent<GraphicRaycaster>();
            HUDObj.GetComponent<Canvas>().enabled = true;
            HUDObj.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            HUDObj.GetComponent<Canvas>().worldCamera = MainCamera.GetComponent<Camera>();
            HUDObj.GetComponent<RectTransform>().sizeDelta = new Vector2(5f, 5f);
            HUDObj.GetComponent<RectTransform>().position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z);
            
            // Position the notification display
            HUDObj2.transform.position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z - 4.6f);
            HUDObj.transform.parent = HUDObj2.transform;
            HUDObj.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 1.6f);
            
            // Rotation and scale
            Vector3 eulerAngles = HUDObj.GetComponent<RectTransform>().rotation.eulerAngles;
            eulerAngles.y = -270f;
            HUDObj.transform.localScale = new Vector3(1f, 1f, 1f);
            HUDObj.GetComponent<RectTransform>().rotation = Quaternion.Euler(eulerAngles);
            
            // Main notification text
            Testtext = new GameObject
            {
                transform = { parent = HUDObj.transform }
            }.AddComponent<Text>();
            Testtext.text = "";
            Testtext.fontSize = 32; // Slightly larger font
            Testtext.font = agency;
            Testtext.rectTransform.sizeDelta = new Vector2(500f, 230f); // Larger area
            Testtext.alignment = TextAnchor.LowerLeft;
            Testtext.verticalOverflow = VerticalWrapMode.Overflow;
            Testtext.rectTransform.localScale = new Vector3(0.0035f, 0.0035f, 0.35f); // Slightly larger
            Testtext.rectTransform.localPosition = new Vector3(-1.1f, -1.1f, -0.5f); // Adjusted position
            Testtext.material = new Material(Shader.Find("GUI/Text Shader")) { color = new Color(0.1f, 0.9f, 0.8f, 1f) }; // DeepSeek teal color
            NotifiText = Testtext;

            // Mod list text
            Text Text2 = new GameObject
            {
                transform = { parent = HUDObj.transform }
            }.AddComponent<Text>();
            Text2.text = "";
            Text2.fontSize = 22;
            Text2.font = agency;
            Text2.rectTransform.sizeDelta = new Vector2(500f, 1200f);
            Text2.alignment = TextAnchor.UpperLeft;
            Text2.rectTransform.localScale = new Vector3(0.0035f, 0.0035f, 0.35f);
            Text2.rectTransform.localPosition = new Vector3(-1.1f, -0.8f, -0.5f);
            Text2.material = new Material(Shader.Find("GUI/Text Shader")) { color = new Color(0.7f, 0.9f, 1f, 1f) }; // Light blue color
            ModText = Text2;
            
            // Add DeepSeek logo/watermark
            GameObject watermarkObj = new GameObject("DeepSeek_Watermark");
            watermarkObj.transform.parent = HUDObj.transform;
            Text watermarkText = watermarkObj.AddComponent<Text>();
            watermarkText.text = "<color=#1ae6d6>DEEPSEEK</color> <color=#aaaaaa>v1.0</color>";
            watermarkText.font = agency;
            watermarkText.fontSize = 18;
            watermarkText.rectTransform.sizeDelta = new Vector2(300f, 50f);
            watermarkText.alignment = TextAnchor.LowerRight;
            watermarkText.rectTransform.localScale = new Vector3(0.003f, 0.003f, 0.3f);
            watermarkText.rectTransform.localPosition = new Vector3(1.5f, -1.5f, -0.5f);
        }

        private void FixedUpdate()
        {
            try
            {
                if (!HasInit && GameObject.Find("Main Camera") != null)
                {
                    Init();
                    HasInit = true;
                }
                
                // Update position to follow camera
                HUDObj2.transform.position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z);
                HUDObj2.transform.rotation = MainCamera.transform.rotation;
                
                try
                {
                    // Update font settings
                    Testtext.font = activeFont;
                    ModText.font = activeFont;
                    Testtext.fontStyle = activeFontStyle;
                    ModText.fontStyle = activeFontStyle;

                    if (advancedArraylist)
                        ModText.fontStyle = (FontStyle)((int)activeFontStyle % 2);
                }
                catch { }
                
                // Update mod list position and alignment
                ModText.rectTransform.localPosition = new Vector3(-1.1f, -0.8f, flipArraylist ? 0.5f : -0.5f);
                ModText.alignment = flipArraylist ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
                
                if (showEnabledModsVR)
                {
                    string enabledModsText = "";
                    List<string> alphabetized = new List<string>();
                    int categoryIndex = 0;
                    
                    foreach (ButtonInfo[] buttonlist in Buttons.buttons)
                    {
                        foreach (ButtonInfo v in buttonlist)
                        {
                            try
                            {
                                if (v.enabled && (!hideSettings || (hideSettings && !Buttons.categoryNames[categoryIndex].Contains("Settings"))))
                                {
                                    string buttonText = (v.overlapText == null) ? v.buttonText : v.overlapText;
                                    if (translate)
                                        buttonText = TranslateText(buttonText);
                                    
                                    // Custom DeepSeek color scheme
                                    buttonText = buttonText.Replace(" <color=grey>[</color><color=green>", 
                                        " <color=#aaaaaa>[</color><color=#1ae6d6>"); // DeepSeek teal
                                    
                                    if (lowercaseMode)
                                        buttonText = buttonText.ToLower();
                                    
                                    alphabetized.Add(buttonText);
                                }
                            }
                            catch { }
                        }
                        categoryIndex++;
                    }

                    string[] sortedButtons = alphabetized
                        .OrderByDescending(s => UI.Main.ExternalCalcSize(new GUIContent(NoRichtextTags(s))).x)
                        .ToArray();

                    int index = 0;
                    foreach (string v in sortedButtons)
                    {
                        if (advancedArraylist)
                            enabledModsText += (flipArraylist ?
                                $"<color=#{ColorToHex(textColor)}>{v}</color><color=#{ColorToHex(GetDeepSeekBGColor(index * -0.1f))}> |</color>"
                              : $"<color=#{ColorToHex(GetDeepSeekBGColor(index * -0.1f))}>| </color><color=#{ColorToHex(textColor)}>{v}</color>") + "\n";
                        else
                            enabledModsText += v + "\n";

                        index++;
                    }
                    
                    ModText.text = enabledModsText;
                    ModText.color = GetIndex("Swap GUI Colors").enabled ? textColor : GetDeepSeekBGColor(0f);
                }
                else
                {
                    ModText.text = "";
                }
                
                if (lowercaseMode)
                {
                    ModText.text = ModText.text.ToLower();
                    NotifiText.text = NotifiText.text.ToLower();
                }
                
                HUDObj.layer = GetIndex("Hide Notifications on Camera").enabled ? 19 : 0;
            } 
            catch (Exception e) { LogManager.Log(e); }
        }

        private Color GetDeepSeekBGColor(float offset)
        {
            // Custom DeepSeek gradient colors
            float r = Mathf.Clamp01(0.1f + offset);
            float g = Mathf.Clamp01(0.9f + offset * 0.5f);
            float b = Mathf.Clamp01(0.8f + offset * 0.3f);
            return new Color(r, g, b, 0.7f);
        }

        public static void SendNotification(string NotificationText, int clearTime = -1)
        {
            if (clearTime < 0)
                clearTime = notificationDecayTime;
            
            if (!disableNotifications)
            {
                try
                {
                    if (PreviousNotifi != NotificationText)
                    {
                        if (translate)
                        {
                            if (translateCache.ContainsKey(NotificationText))
                                NotificationText = TranslateText(NotificationText);
                            else
                            {
                                TranslateText(NotificationText, delegate { SendNotification(NotificationText, clearTime); });
                                return;
                            }
                        }

                        // Play DeepSeek custom notification sound
                        if (notificationSoundIndex != 0 && (Time.time > (timeMenuStarted + 5f)))
                        {
                            string[] notificationServerNames = new string[]
                            {
                                "none",
                                "deepseek_pop",  // Custom DeepSeek sounds
                                "deepseek_ding",
                                "deepseek_alert",
                                "deepseek_success",
                                "deepseek_error",
                                "deepseek_chime",
                                "deepseek_notify"
                            };
                            Play2DAudio(LoadSoundFromURL("https://raw.githubusercontent.com/yourgithub/DeepSeek-ModMenu/main/sounds/" + notificationServerNames[notificationSoundIndex] + ".wav", 
                                notificationServerNames[notificationSoundIndex] + ".wav"), buttonClickVolume / 8f);
                        }

                        if (!NotificationText.Contains(Environment.NewLine))
                            NotificationText += Environment.NewLine;
                        
                        // Apply DeepSeek color scheme
                        NotificationText = "<color=#1ae6d6>[DeepSeek]</color> " + NotificationText;
                        NotificationText = NotificationText.Replace("<color=green>", "<color=#1ae6d6>"); // Replace green with DeepSeek teal

                        NotifiText.text = NotifiText.text + NotificationText;
                        if (lowercaseMode)
                            NotifiText.text = NotifiText.text.ToLower();

                        NotifiText.supportRichText = true;
                        PreviousNotifi = NotificationText;

                        try
                        {
                            CoroutineManager.RunCoroutine(ClearLast(clearTime / 1000f));
                        } 
                        catch { }

                        if (narrateNotifications)
                        {
                            try
                            {
                                CoroutineManager.RunCoroutine(NarrateText("[DeepSeek Notification] " + NoRichtextTags(NotificationText, "")));
                            }
                            catch { }
                        }
                    }
                }
                catch
                {
                    LogManager.LogError("DeepSeek Notification failed: " + NotificationText);
                }
            }
        }

        public static void ClearAllNotifications()
        {
            NotifiText.text = "";
        }

        public static void ClearPastNotifications(int amount)
        {
            string text = "";
            foreach (string text2 in Enumerable.ToArray<string>(Enumerable.Skip<string>(NotifiText.text.Split(Environment.NewLine.ToCharArray()), amount)))
            {
                if (text2 != "")
                    text = text + text2 + "\n";
            }
            NotifiText.text = text;
        }

        public static IEnumerator ClearLast(float time = 1f)
        {
            yield return new WaitForSeconds(time);
            ClearPastNotifications(1);
        }

        private GameObject HUDObj;
        private GameObject HUDObj2;
        private GameObject MainCamera;
        public static string PreviousNotifi;
        private static Text NotifiText;
        private static Text ModText;
        private Text Testtext;
        private bool HasInit;
    }
}