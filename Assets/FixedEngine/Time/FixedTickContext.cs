// File: FixedTickContext.cs
namespace FixedEngine
{
    /// <summary>
    /// Fournit le contexte de tick logique courant, utilisé pour garantir un appel unique par cycle.
    /// </summary>
    public static class FixedTickContext
    {
        public static int CurrentTick { get; private set; } = -1;

        public static void AdvanceTick()
        {
            CurrentTick++;
        }
    }
}
