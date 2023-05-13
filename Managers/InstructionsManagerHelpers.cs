using System;
using UnityEngine;

namespace ModCompanion.Managers
{
    public class InstructionsManagerHelpers : MonoBehaviour
    {
        public static string DefaultPath { get; set; } = AppContext.BaseDirectory.ToString();
        public static string DefaultGameDataPath { get; set; } = "Data";
        public static string DefaultSystemInstructionsFileName { get; set; } = "SystemInstructions.txt";
        public static string DefaultUserInstructionsFileName { get; set; } = "UserInstructions.txt";
        public static string DefaultNpcNameParameter { get; set; } = "__NPC__";
        public static string DefaultGameDataPathParameter { get; set; } = "__DataPath__";
        public static string DefaultNpcName { get; set; } = "Capybara";
    }
}
