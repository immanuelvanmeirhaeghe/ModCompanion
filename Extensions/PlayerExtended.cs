using UnityEngine;

namespace ModCompanion.Extensions
{
    class PlayerExtended : Player
    {
        protected override void Start()
        {
            base.Start();
            new GameObject($"__{nameof(ModCompanion)}__").AddComponent<ModCompanion>();
        }
    }
}
