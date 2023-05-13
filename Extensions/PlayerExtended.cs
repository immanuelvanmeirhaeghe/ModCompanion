using ModCompanion.Data;
using ModCompanion.Managers;
using UnityEngine;

namespace ModCompanion.Extensions
{
    class PlayerExtended : Player
    {
        protected override void Start()
        {
            base.Start();
            new GameObject($"__{nameof(ModCompanion)}__").AddComponent<ModCompanion>();
            new GameObject($"__{nameof(Instruction)}__").AddComponent<Instruction>();
            new GameObject($"__{nameof(InstructionsManager)}__").AddComponent<InstructionsManager>();
            new GameObject($"__{nameof(InstructionsManagerHelpers)}__").AddComponent<InstructionsManagerHelpers>();
        }
    }
}
