﻿using AIs;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using ModManager.Data.Enums;
using ModCompanion.Managers;
using ModCompanion.Data;
using System.Web;
using System.Text.Json;

namespace ModCompanion
{

    public class ModCompanion : MonoBehaviour
    {
        private const string ErrorMessage = "Something went wrong, sorry ;p";
        private static ModCompanion Instance;
        private static readonly string ModName = nameof(ModCompanion);
        private static readonly string RuntimeConfiguration = Path.Combine(Application.dataPath.Replace("GH_Data", "Mods"), $"{nameof(RuntimeConfiguration)}.xml");

        private static InstructionsManager LocalInstructionsManager;
        private static HUDManager LocalHUDManager;
        private static CursorManager LocalCursorManager;
        private static Player LocalPlayer;       
        private static AIManager LocalAIManager;

        public Instruction LocalInstruction { get; set; } = null;

        private static float ModCompanionScreenTotalWidth { get; set; } = 500f;
        private static float ModCompanionScreenTotalHeight { get; set; } = 150f;
        private static float ModCompanionScreenMinWidth { get; set; } = 500f;
        private static float ModCompanionScreenMaxWidth { get; set; } = Screen.width;
        private static float ModCompanionScreenMinHeight { get; set; } = 50f;
        private static float ModCompanionScreenMaxHeight { get; set; } = Screen.height;
        private static float ModCompanionScreenStartPositionX { get; set; } = Screen.width / 2f;
        private static float ModCompanionScreenStartPositionY { get; set; } = Screen.height / 2f;
        private bool IsModCompanionScreenMinimized { get; set; } = false;
        private static int ModCompanionScreenId { get; set; }
        private static Rect ModCompanionScreen = new Rect(ModCompanionScreenStartPositionX, ModCompanionScreenStartPositionY, ModCompanionScreenTotalWidth, ModCompanionScreenTotalHeight);
        private bool ShowModCompanionScreen { get; set; } = false;
        private Vector3 Previous;
        private Vector3 Target;
        private Vector3 OriginalPosition;
        public bool IsModActiveForMultiplayer { get; private set; } = false;
        public bool IsModActiveForSingleplayer => ReplTools.AmIMaster();

        public KeyCode ShortcutKey { get; set; } = KeyCode.KeypadMultiply;
        public AI ParentAi { get; set; } = null;
        public GameObject ParentObject { get; set; } = null;
        public string NpcName { get; set; } = string.Empty;
        public bool IsNpcInitialized { get; set; } = false;
        public bool IsCompanionInitialized { get; set; } = false;
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string PostData { get; set; } = string.Empty;
        public Vector2 AISelectionScrollViewPosition { get; private set; }
        public int SelectedAiIndex { get; private set; }
        public string SelectedAiName { get; private set; }

        public Vector3 BoundingVolume = new Vector3(3f, 1f, 3f);
        public float Speed = 10f;
        public float RotateSpeed = 5f;

        private string OnlyForSinglePlayerOrHostMessage()
                    => "Only available for single player or when host. Host can activate using ModManager.";
        private string PermissionChangedMessage(string permission, string reason)
            => $"Permission to use mods and cheats in multiplayer was {permission} because {reason}.";
        private string HUDBigInfoMessage(string message, MessageType messageType, Color? headcolor = null)
            => $"<color=#{(headcolor != null ? ColorUtility.ToHtmlStringRGBA(headcolor.Value) : ColorUtility.ToHtmlStringRGBA(Color.red))}>{messageType}</color>\n{message}";

        protected virtual void OnlyForSingleplayerOrWhenHostBox()
        {
            using (new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label(OnlyForSinglePlayerOrHostMessage(), GUI.skin.label);
            }
        }

        protected virtual void  ModManager_onPermissionValueChanged(bool optionValue)
        {
            string reason = optionValue ? "the game host allowed usage" : "the game host did not allow usage";
            IsModActiveForMultiplayer = optionValue;

            ShowHUDBigInfo(
                          (optionValue ?
                            HUDBigInfoMessage(PermissionChangedMessage($"granted", $"{reason}"), MessageType.Info, Color.green)
                            : HUDBigInfoMessage(PermissionChangedMessage($"revoked", $"{reason}"), MessageType.Info, Color.yellow))
                            );
        }

        public virtual void ShowHUDBigInfo(string text, float duration = 3f)
        {
            string header = $"{ModName} Info";
            string textureName = HUDInfoLogTextureType.PveDialog.ToString();
            HUDBigInfo obj = (HUDBigInfo)LocalHUDManager.GetHUD(typeof(HUDBigInfo));
            HUDBigInfoData.s_Duration = duration;
            HUDBigInfoData data = new HUDBigInfoData
            {
                m_Header = header,
                m_Text = text,
                m_TextureName = textureName,
                m_ShowTime = Time.time
            };
            obj.AddInfo(data);
            obj.Show(show: true);
        }

        protected virtual void ToggleShowUI(int controlId)
        {
            switch (controlId)
            {
                case 0:
                    ShowModCompanionScreen = !ShowModCompanionScreen;
                    return;              
                default:
                    ShowModCompanionScreen = !ShowModCompanionScreen;
                    return;
            }
        }

        protected virtual void OnGUI()
        {
            if (ShowModCompanionScreen)
            {
                InitData();
                InitSkinUI();
                ShowModCompanionWindow();
            }
        }

        protected virtual void ShowModCompanionWindow()
        {
            if (ModCompanionScreenId <= 0)
            {
                ModCompanionScreenId = GetHashCode();
            }
            string ModCompanionScreenTitle = $"{ModName} created by [Dragon Legion] Immaanuel#4300";
            ModCompanionScreen = GUILayout.Window(ModCompanionScreenId, ModCompanionScreen, InitModCompanionScreen, ModCompanionScreenTitle,
                                           GUI.skin.window,
                                           GUILayout.ExpandWidth(true),
                                           GUILayout.MinWidth(ModCompanionScreenMinWidth),
                                           GUILayout.MaxWidth(ModCompanionScreenMaxWidth),
                                           GUILayout.ExpandHeight(true),
                                           GUILayout.MinHeight(ModCompanionScreenMinHeight),
                                           GUILayout.MaxHeight(ModCompanionScreenMaxHeight));
        }

        protected virtual void ScreenMenuBox()
        {
            string CollapseButtonText = IsModCompanionScreenMinimized ? "O" : "-";
            if (GUI.Button(new Rect(ModCompanionScreen.width - 40f, 0f, 20f, 20f), CollapseButtonText, GUI.skin.button))
            {
                CollapseWindow();
            }

            if (GUI.Button(new Rect(ModCompanionScreen.width - 20f, 0f, 20f, 20f), "X", GUI.skin.button))
            {
                CloseWindow();
            }
        }

        protected virtual void CollapseWindow()
        {
            if (!IsModCompanionScreenMinimized)
            {
                ModCompanionScreen = new Rect(ModCompanionScreenStartPositionX, ModCompanionScreenStartPositionY, ModCompanionScreenTotalWidth, ModCompanionScreenMinHeight);
                IsModCompanionScreenMinimized = true;
            }
            else
            {
                ModCompanionScreen = new Rect(ModCompanionScreenStartPositionX, ModCompanionScreenStartPositionY, ModCompanionScreenTotalWidth, ModCompanionScreenTotalHeight);
                IsModCompanionScreenMinimized = false;
            }
            ShowModCompanionWindow();
        }

        protected virtual void CloseWindow()
        {
            ShowModCompanionScreen = false;
            EnableCursor(false);
        }

        protected virtual void InitModCompanionScreen(int windowId)
        {
            ModCompanionScreenStartPositionX = ModCompanionScreen.x;
            ModCompanionScreenStartPositionY = ModCompanionScreen.y;

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                ScreenMenuBox();

                if (!IsModCompanionScreenMinimized)
                {
                    MultiplayerOptionBox();
                    ModCompanionManagerBox();
                }
            }
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }

        protected virtual void ModCompanionManagerBox()
        {
            try
            {
                if (IsModActiveForSingleplayer || IsModActiveForMultiplayer)
                {
                    using (new GUILayout.VerticalScope(GUI.skin.box))
                    {
                        GUILayout.Label($"{ModName} Manager", GUI.skin.label);
                        GUILayout.Label($"{ModName} Options", GUI.skin.label);

                        GUILayout.Label($"Start with choosing and creating your companion.", GUI.skin.label);
                        AISelectionScrollViewBox();
                        if (GUILayout.Button($"Create Companion", GUI.skin.button))
                        {
                            InitCompanion();
                            InitNpc();
                        }

                        GUILayout.Label($"These are the instructions used to initialize your companion: ", GUI.skin.label);
                        if (GUILayout.Button($"Instructions", GUI.skin.button))
                        {
                             GetInstructions();
                            if (LocalInstruction != null)
                            {
                                using (new GUILayout.VerticalScope(GUI.skin.box))
                                {
                                    GUILayout.Label($"{nameof(LocalInstruction.FromSystem)}: ", GUI.skin.label);
                                    GUILayout.Label($"{LocalInstruction.FromSystem}", GUI.skin.label);                                    
                                }
                                using (new GUILayout.VerticalScope(GUI.skin.box))
                                {
                                    GUILayout.Label($"{nameof(LocalInstruction.FromUser)}: ", GUI.skin.label);
                                    GUILayout.Label($"{LocalInstruction.FromUser}", GUI.skin.label);
                                }
                            }
                            else
                            {
                                GUILayout.Label($"Instructions could not be retrieved. {ErrorMessage} ", GUI.skin.label);
                            }
                        }

                        if (IsCompanionInitialized && IsNpcInitialized)
                        { 
                            GUILayout.Label($"Here you can type in any question you would like to ask your companion.", GUI.skin.label);
                            using (new GUILayout.HorizontalScope(GUI.skin.box))
                            {
                                GUILayout.Label($"Question: ", GUI.skin.label);
                                Question = GUILayout.TextField(Question, GUI.skin.textField);
                            }
                            if (GUILayout.Button($"Send", GUI.skin.button))
                            {
                                AskQuestion();
                            }
                            if (!string.IsNullOrEmpty(Answer))
                            {
                                GUILayout.Label($"Answer: {Answer}", GUI.skin.label);
                            }
                            else
                            {
                                GUILayout.Label($"First, ask your companion a question. If you did and this is still visible: {ErrorMessage}", GUI.skin.label);
                            }
                        }
                        else
                        {
                            GUILayout.Label($"First create your companion. If you did and this still is visible: {ErrorMessage}", GUI.skin.label);
                            GUILayout.Label($"{nameof(IsCompanionInitialized)}: {IsCompanionInitialized}", GUI.skin.label);
                            GUILayout.Label($"{nameof(IsNpcInitialized)}: {IsNpcInitialized}", GUI.skin.label);
                        }                      
                    }
                }
                else
                {
                    OnlyForSingleplayerOrWhenHostBox();
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(ModCompanionManagerBox));
            }
        }

        protected virtual void MultiplayerOptionBox()
        {
            try
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label("Multiplayer Options", GUI.skin.label);

                    string multiplayerOptionMessage = string.Empty;
                    if (IsModActiveForSingleplayer || IsModActiveForMultiplayer)
                    {
                        if (IsModActiveForSingleplayer)
                        {
                            multiplayerOptionMessage = $"you are the game host";
                        }
                        if (IsModActiveForMultiplayer)
                        {
                            multiplayerOptionMessage = $"the game host allowed usage";
                        }
                        GUILayout.Label(PermissionChangedMessage($"granted", multiplayerOptionMessage), GUI.skin.label);
                    }
                    else
                    {
                        if (!IsModActiveForSingleplayer)
                        {
                            multiplayerOptionMessage = $"you are not the game host";
                        }
                        if (!IsModActiveForMultiplayer)
                        {
                            multiplayerOptionMessage = $"the game host did not allow usage";
                        }
                        GUILayout.Label(PermissionChangedMessage($"revoked", $"{multiplayerOptionMessage}"), GUI.skin.label);
                    }
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(MultiplayerOptionBox));
            }
        }

        protected virtual void InitSkinUI()
        {
            GUI.skin = ModAPI.Interface.Skin;
        }

        protected virtual void EnableCursor(bool blockPlayer = false)
        {
            LocalCursorManager.ShowCursor(blockPlayer, false);

            if (blockPlayer)
            {
                LocalPlayer.BlockMoves();
                LocalPlayer.BlockRotation();
                LocalPlayer.BlockInspection();
            }
            else
            {
                LocalPlayer.UnblockMoves();
                LocalPlayer.UnblockRotation();
                LocalPlayer.UnblockInspection();
            }
        }

        protected virtual void Start()
        {          
            InitData();
            OriginalPosition = transform.position;
            Previous = transform.position;
            Target = transform.position;
        }

        public ModCompanion()
        {
            useGUILayout = true;
            Instance = this;
        }

        public static ModCompanion Get()
        {
            return Instance;
        }

        protected virtual void HandleException(Exception exc, string methodName)
        {
            string info = $"[{ModName}:{methodName}] throws exception -  {exc.TargetSite?.Name}:\n{exc.Message}\n{exc.InnerException}\n{exc.Source}\n{exc.StackTrace}";
            ModAPI.Log.Write(info);
            Debug.Log(info);
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(ShortcutKey))
            {
                if (!ShowModCompanionScreen)
                {
                    InitData();
                    EnableCursor(true);
                }
                ToggleShowUI(0);
                if (!ShowModCompanionScreen)
                {
                    EnableCursor(false);
                }
            }

            if (IsCompanionInitialized && IsNpcInitialized)
            {
                UpdateCompanion();
            }
        }

        protected virtual void UpdateCompanion()
        {
            ParentAi.m_EnemyModule.SetEnemy(LocalAIManager.m_EnemyAIs.FirstOrDefault());
            ParentAi.m_EnemyModule.Update();

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(LocalPlayer.transform.position - transform.position), RotateSpeed * Time.deltaTime);
            transform.position = Vector3.Slerp(Previous, Target, Time.deltaTime * Speed);
            Previous = transform.position;
            if (Vector3.Distance(Target, transform.position) < 0.1f)
            {
                Target = transform.position + UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(0.7f, 4f);
                Target.Set(
                    Mathf.Clamp(Target.x, OriginalPosition.x - BoundingVolume.x, OriginalPosition.x + BoundingVolume.x), 
                    Mathf.Clamp(Target.y, OriginalPosition.y - BoundingVolume.y, OriginalPosition.y + BoundingVolume.y), 
                    Mathf.Clamp(Target.z, OriginalPosition.z - BoundingVolume.z, OriginalPosition.z + BoundingVolume.z)
                );
            }
        }

        protected virtual void InitData()
        {
            LocalCursorManager = CursorManager.Get();
            LocalPlayer = Player.Get();
            LocalHUDManager = HUDManager.Get();
            LocalAIManager = AIManager.Get();
            LocalInstructionsManager = InstructionsManager.Get();
        }

        protected virtual void InitCompanion()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedAiName))
                {
                    return;
                }
                GameObject companionPrefab = GreenHellGame.Instance.GetPrefab(SelectedAiName);
                if (companionPrefab != null)
                {
                    Vector3 forward = Camera.main.transform.forward;
                    Vector3 position = LocalPlayer.GetHeadTransform().position + forward * 10f;
                    ParentAi = Instantiate(companionPrefab, position, Quaternion.LookRotation(-forward, Vector3.up)).GetComponent<AI>();
                    if (ParentAi == null)
                    {
                        IsCompanionInitialized = false;
                        ShowHUDBigInfo($"Error - could not initialize parent AI {SelectedAiName}!");
                    }
                    else
                    {
                        ParentAi.m_EnemyModule.m_Enemy = null;
                        NpcName = ParentAi.GetName().Replace("(Clone)", string.Empty);
                        ParentObject = ParentAi.gameObject;
                        IsCompanionInitialized = true;
                        ShowHUDBigInfo($"Companion {SelectedAiName} was created!");
                    }
                }
                else
                {
                    IsCompanionInitialized = false;
                    ShowHUDBigInfo($"Error - could not initialize {SelectedAiName}!");
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(InitCompanion));               
                IsCompanionInitialized = false;
                ShowHUDBigInfo(exc.Message);
            }
        }

        public virtual void InitNpc() 
        {
            if (!IsNpcInitialized)
            {
                NpcName = NpcName.Replace($"(Clone)", string.Empty);
                LocalInstructionsManager.NpcInitUrlToPost =$"{LocalInstructionsManager.NpcInitUrl}&gameName=GreenHell&npcName={HttpUtility.UrlEncode(NpcName)}";
                StartCoroutine(PostInitNpcMessage());
            }
        }

        protected virtual void AskQuestion()
        {
            StartCoroutine(GetAnswer());
        }

        protected virtual void GetInstructions()
        {
            StartCoroutine(RequestInstructions());
        }

        protected virtual IEnumerator RequestInstructions()
        {
            string url = $"{LocalInstructionsManager.NpcInstructionUrl}&npcName={HttpUtility.UrlEncode(NpcName)}";
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = $"Could not request any instruction!\nError for GET {url}\n{www.error}";
                ModAPI.Log.Write(errorMsg);
                ShowHUDBigInfo(errorMsg);
            }
            else
            {
                LocalInstruction = JsonSerializer.Deserialize<Instruction>(www.downloadHandler.text);
                if (LocalInstruction != null)
                {
                    ShowHUDBigInfo($"{nameof(LocalInstruction.FromSystem)}\n{LocalInstruction.FromSystem}");
                    ShowHUDBigInfo($"{nameof(LocalInstruction.FromUser)}\n{LocalInstruction.FromUser}");
                }
                else
                {
                    string errorMsg = $"NPC did not give any answer!\n{ErrorMessage}";
                    ModAPI.Log.Write(errorMsg);
                    ShowHUDBigInfo(errorMsg);
                }
            }
        }

        protected virtual IEnumerator PostInitNpcMessage()
        {       
            UnityWebRequest www = UnityWebRequest.Post(LocalInstructionsManager.NpcInitUrlToPost, PostData);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = $"Could not initialize NPC!\nError for POST {LocalInstructionsManager.NpcInitUrlToPost} with {PostData}\n{www.error}";
                ModAPI.Log.Write(errorMsg);
                IsNpcInitialized = false;
                ShowHUDBigInfo(errorMsg);
            }
            else
            {
                IsNpcInitialized = true;
                ShowHUDBigInfo($"NPC initialized!");
            }
        }

        protected virtual IEnumerator GetAnswer()
        {
            string url = $"{LocalInstructionsManager.NpcPromptUrl}&question={HttpUtility.UrlEncode(Question)}&npcName={HttpUtility.UrlEncode(NpcName)}";
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = $"Could not ask any question!\nError for GET {url}\n{www.error}";
                ModAPI.Log.Write(errorMsg);
                ShowHUDBigInfo(errorMsg);
            }
            else
            {
                Answer = www.downloadHandler.text;
                if (!string.IsNullOrEmpty(Answer))
                {
                    ShowHUDBigInfo(Answer);
                }
                else
                {
                    string errorMsg = $"NPC did not give any answer!\n{ErrorMessage}";
                    ModAPI.Log.Write(errorMsg);
                    ShowHUDBigInfo(errorMsg);
                }
            }
        }

        protected virtual void  AISelectionScrollViewBox()
        {
            try
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label("AI selection grid", GUI.skin.label);

                    AiSelectionScrollView();
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(AISelectionScrollViewBox));
            }
        }

        protected virtual void  AiSelectionScrollView()
        {
            try
            {
                AISelectionScrollViewPosition = GUILayout.BeginScrollView(AISelectionScrollViewPosition, GUI.skin.scrollView, GUILayout.MinHeight(300f));

                string[] aiNames = GetAINames();
                if (aiNames != null)
                {
                    int _selectedAiIndex = SelectedAiIndex;
                    SelectedAiIndex = GUILayout.SelectionGrid(SelectedAiIndex, aiNames, 3, GUI.skin.button);
                    SelectedAiName = aiNames[SelectedAiIndex];
                }

                GUILayout.EndScrollView();
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(AiSelectionScrollView));
            }
        }

        public virtual string[] GetAINames()
        {
            var aiNames = Enum.GetNames(typeof(AI.AIID));
            return aiNames;
        }
    }

}
