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
        public IEnumerator<string> GetSystemInstructionsAsync(
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
            string fromSystem = File.ReadAllText(path).Replace(pathParameter, dataPath).Replace(nameParameter, name);
            yield return fromSystem;
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
        public IEnumerator<string> GetUserInstructionsAsync(
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
            string fromUser = File.ReadAllText(Path.Combine(InstructionsManagerHelpers.DefaultPath, dataPath, fileName)).Replace(pathParameter, dataPath).Replace(nameParameter, name);
            yield return fromUser;
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

                IEnumerator<string> systemInstructions = GetSystemInstructionsAsync(name);
                instructionsBuilder.AppendLine(systemInstructions.Current);
                instruction.FromSystem = systemInstructions.Current;

                IEnumerator<string> userInstructions = GetUserInstructionsAsync(name);
                instructionsBuilder.AppendLine(userInstructions.Current);
                instruction.FromUser = userInstructions.Current;

                return instruction;
            }
            catch (Exception)
            {
                return instruction;
            }
        }
    }
}
