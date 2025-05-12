namespace FixedEngine.Input
{
    /// <summary>
    /// Accès statique au provider de type TFormat.
    /// Permet de gérer plusieurs formats en parallèle (Q8_4, Q8_4U, etc.)
    /// </summary>
    public static class FixedInput<TFormat> where TFormat : struct, IFixedPointFormat
    {
        public static IInputProvider<TFormat> Current { get; set; }

        public static void Tick()
        {
            Current?.UpdateState();
        }
    }
}
