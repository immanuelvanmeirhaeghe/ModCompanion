using ModCompanion.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModCompanion.Managers
{
    /// <summary>
    /// Manager for GPT system -, user - and other messages stored as text files.
    /// </summary>
    public class InstructionsManager : MonoBehaviour
    {
        private static InstructionsManager Instance;
        private static readonly string ModuleName = nameof(InstructionsManager);

        public readonly string NpcInitUrl = "https://localhost:7230/npc/init?npcName=";
        public readonly string NpcInstructionUrl = "https://localhost:7230/npc/instruction?npcName=";
        public readonly string NpcPromptUrl = "https://localhost:7230/npc/prompt?scribe=true&gpt=true&question=";


        public string SystemInstructions { get; set; } = string.Empty;
        public string UserInstructions { get; set; } = string.Empty;

        public InstructionsManager() 
        {
            Instance = this;
        }

        public static InstructionsManager Get() => Instance;

        /// <summary>
        /// Give instruction for a System message for an NPC bot.
        /// If not given, uses defaults.
        /// </summary>
        /// <param name="npcName">Name of the npc</param>
        /// <param name="npcNameParameter">placeholder for npc name</param>
        /// <param name="gameDataPath">Optional folder path where instructions are located</param>
        /// <param name="gameDataPathParameter">placeholder for game data path</param>
        /// <param name="systemInstructionsFileName">Optional file with instructions</param>
        /// <returns>System instructions</returns>
        public string GetSystemInstructions(
                                                                    string npcName = null,
                                                                    string npcNameParameter = null,
                                                                    string gameDataPath = null,
                                                                    string gameDataPathParameter = null,
                                                                    string systemInstructionsFileName = null)
        {
            string name = npcName ?? InstructionsManagerHelpers.DefaultNpcName;
            string nameParameter = npcNameParameter ?? InstructionsManagerHelpers.DefaultNpcNameParameter;
            string dataPath = gameDataPath ?? InstructionsManagerHelpers.DefaultGameDataPath;
            string pathParameter = gameDataPathParameter ?? InstructionsManagerHelpers.DefaultGameDataPathParameter;
            string fileName = systemInstructionsFileName ?? InstructionsManagerHelpers.DefaultSystemInstructionsFileName;
            string path = Path.Combine(InstructionsManagerHelpers.DefaultPath, dataPath, fileName);
            SystemInstructions = File.ReadAllText(path).Replace(pathParameter, dataPath).Replace(nameParameter, name);
            return SystemInstructions;
        }

        /// <summary>
        /// Give instruction for a User message for an NPC bot.
        /// If not given, uses defaults.
        /// </summary>
        /// <param name="npcName">Name of the npc</param>
        /// <param name="npcNameParameter">placeholder for npc name</param>
        /// <param name="gameDataPath">Optional folder path where instructions are located</param>
        /// <param name="gameDataPathParameter">placeholder for game data path</param>
        /// <param name="systemInstructionsFileName">Optional file with instructions</param>
        /// <returns>User instructions</returns>
        public string GetUserInstructions(
                                                 string npcName = null,
                                                 string npcNameParameter = null,
                                                 string gameDataPath = null,
                                                 string gameDataPathParameter = null,
                                                 string userInstructionsFileName = null)
        {
            string name = npcName ?? InstructionsManagerHelpers.DefaultNpcName;
            string nameParameter = npcNameParameter ?? InstructionsManagerHelpers.DefaultNpcNameParameter;
            string dataPath = gameDataPath ?? InstructionsManagerHelpers.DefaultGameDataPath;
            string pathParameter = gameDataPathParameter ?? InstructionsManagerHelpers.DefaultGameDataPathParameter;
            string fileName = userInstructionsFileName ?? InstructionsManagerHelpers.DefaultUserInstructionsFileName;
            UserInstructions = File.ReadAllText(Path.Combine(InstructionsManagerHelpers.DefaultPath, dataPath, fileName)).Replace(pathParameter, dataPath).Replace(nameParameter, name);
            return UserInstructions;
        }

        /// <summary>
        /// Get system - and user instructions currently set for an NPC.
        /// </summary>
        /// <returns><see cref="Instruction"/></returns>
        public Instruction GetInstruction(string npcName)
        {
            Instruction instruction = new Instruction();
            try
            {
                string name = npcName ?? InstructionsManagerHelpers.DefaultNpcName;
                StringBuilder instructionsBuilder = new StringBuilder();

                string systemInstructions = GetSystemInstructions(name);
                instructionsBuilder.AppendLine(systemInstructions);
                instruction.FromSystem = systemInstructions;

                string userInstructions = GetUserInstructions(name);
                instructionsBuilder.AppendLine(userInstructions);
                instruction.FromUser = userInstructions;

                return instruction;
            }
            catch (Exception)
            {
                return instruction;
            }
        }
    }
}
