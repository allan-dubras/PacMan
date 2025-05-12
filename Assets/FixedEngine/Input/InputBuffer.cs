using FixedEngine;

public class InputBuffer<TFormat>
    where TFormat : struct, IFixedPointFormat
{
    private readonly FixedVector2<TFormat>[] buffer;
    private int head = 0;    // index où on écrit le prochain input
    private int count = 0;   // nb d’éléments effectivement en mémoire (≤ capacity)

    public int Capacity { get; }

    public InputBuffer(int capacity)
    {
        Capacity = capacity;
        buffer = new FixedVector2<TFormat>[capacity];
        // initialise à zero
        for (int i = 0; i < capacity; i++)
            buffer[i] = FixedVector2<TFormat>.Zero;
    }

    /// <summary>
    /// Enregistre l’input quantifié pour le prochain tick.
    /// </summary>
    public void Record(FixedVector2<TFormat> input)
    {
        buffer[head] = input;
        head = (head + 1) % Capacity;
        if (count < Capacity) count++;
    }

    /// <summary>
    /// Renvoie l’input enregistré 'back' ticks en arrière (0 = dernier, 1 = avant-dernier, etc.).
    /// Si back ≥ count, renvoie Zero.
    /// </summary>
    public FixedVector2<TFormat> Get(int back)
    {
        if (back < 0 || back >= count)
            return FixedVector2<TFormat>.Zero;

        int index = (head - 1 - back + Capacity) % Capacity;
        return buffer[index];
    }
}
