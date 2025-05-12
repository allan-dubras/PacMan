using System;
using System.Runtime.CompilerServices;

using FixedEngine;

namespace FixedEngine
{

    /// <summary>
    /// Représente un nombre à virgule fixe au format paramétré par <typeparamref name="TFormat"/>.
    /// Ce type permet des calculs déterministes sans utiliser de float, idéal pour les moteurs de jeu rétro ou réseau.
    /// </summary>
    /// <typeparam name="TFormat">
    /// Un format de type `struct` définissant le nombre de bits entiers et fractionnaires,
    /// ainsi que le signe (ex : Q8_4, Q8_4U).
    /// Doit implémenter les propriétés statiques : IntegerBits, FractionBits, IsSigned.
    /// </typeparam>
    /// <remarks>
    /// Utilise une représentation brute `int raw`, manipulée via masquage et extension du signe.
    /// Ce type est compatible avec Unity, y compris en build, et peut encapsuler l'usage de `float` uniquement côté développement.
    /// </remarks>
    /// <example>
    /// Exemple d'utilisation :
    /// <code>
    /// FixedPoint<Q8_4> a = FixedPoint<Q8_4>.FromFloat(2.5f);
    /// FixedPoint<Q8_4> b = 1.25f;
    /// float f = a.ToFloat(); // 2.5
    /// </code>
    /// </example>


    public partial struct FixedPoint<TFormat> where TFormat : struct, IFixedPointFormat
    {
        public readonly int Raw;

        public FixedPoint(int raw) => this.Raw = WrapRawValue(raw);

        /// <summary>
        /// Convertit une valeur flottante en un nombre à virgule fixe de type <see cref="FixedPoint{TFormat}"/>.
        /// Le comportement dépend du contexte de compilation :
        /// en mode développement ou éditeur Unity, un contrôle est effectué ; en build final, seul un arrondi déterministe est appliqué.
        /// </summary>
        /// <param name="f">La valeur en virgule flottante à convertir.</param>
        /// <returns>Une instance de <see cref="FixedPoint{TFormat}"/> représentant la valeur arrondie.</returns>
        /// <remarks>
        /// En build final, la conversion se fait par quantification via <c>MathF.Floor(f * scale) / scale</c>,
        /// ce qui garantit un résultat déterministe.
        /// </remarks>

        public static FixedPoint<TFormat> FromFloat(float f)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return CheckedFromFloat(f);
#else
            int scale = 1 << GetFractionBits();
            float quantized = MathF.Floor(f * scale) / scale;
            int raw = (int)(quantized * scale);
            return new FixedPoint<TFormat>(raw);
#endif
        }

        /// <summary>
        /// Convertit une valeur flottante en virgule fixe avec vérification des dépassements de capacité (overflow).
        /// Utilisée principalement en mode développement pour détecter les erreurs de format ou de dépassement de plage.
        /// </summary>
        /// <param name="value">La valeur flottante à convertir.</param>
        /// <returns>
        /// Une instance de <see cref="FixedPoint{TFormat}"/> représentant la valeur, avec mise en garde éventuelle en cas de dépassement.
        /// </returns>
        /// <remarks>
        /// Si la valeur dépasse les limites imposées par le format <typeparamref name="TFormat"/>,
        /// elle est automatiquement repliée (wrap) via <c>WrapRawValue</c>, et un avertissement est émis dans la console en mode Unity Editor.
        /// </remarks>

        public static FixedPoint<TFormat> CheckedFromFloat(float value)
        {
            int scale = 1 << GetFractionBits();
            float quantized = MathF.Floor(value * scale) / scale;
            int raw = (int)(quantized * scale);
            int wrapped = WrapRawValue(raw);

            /*#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        if (wrapped != raw)
                            Debug.LogWarning($"[FixedPoint<{typeof(TFormat).Name}>] Value {value} overflowed and wrapped to {new FixedPoint<TFormat>(wrapped).ToFloat()}");
            #endif*/

            return new FixedPoint<TFormat>(wrapped);
        }

        /// <summary>
        /// Crée un FixedPoint<TFormat> à partir d'un entier (1 → 1.0 fixe).
        /// </summary>
        public static FixedPoint<TFormat> FromInt(int value)
        {
            // rawPerUnit = 1.0 en raw (ex: 16 pour Q8_4U)
            int rawPerUnit = FixedMath.One<TFormat>().Raw;
            return new FixedPoint<TFormat>(value * rawPerUnit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> FromDouble(double v)
        {
            int frac = FixedFormatUtil<TFormat>.FractionBits;
            long raw = (long)Math.Round(v * (1 << frac));
            return new FixedPoint<TFormat>((int)raw);
        }

        /// <summary>
        /// Crée un FixedPoint<TFormat> directement à partir de son raw interne.
        /// </summary>
        public static FixedPoint<TFormat> FromRaw(int raw)
        {
            return new FixedPoint<TFormat>(raw);
        }

        /// <summary>
        /// Applique un masquage sur une valeur entière brute pour la forcer à respecter les contraintes du format fixé (<typeparamref name="TFormat"/>).
        /// Si le format est signé et que le bit de signe est actif, une extension du signe est effectuée.
        /// </summary>
        /// <param name="value">La valeur entière brute à normaliser.</param>
        /// <returns>
        /// Une valeur entière encodée correctement dans le nombre de bits autorisé (entiers + fractionnaires), avec gestion du signe si nécessaire.
        /// </returns>
        /// <remarks>
        /// Cette méthode garantit que la représentation binaire reste dans les bornes fixées par le format à virgule fixe,
        /// même en cas de dépassement ou d’écriture manuelle.
        /// </remarks>

        private static int WrapRawValue(int value)
        {
            int totalBits = GetIntegerBits() + GetFractionBits();
            int mask = (1 << totalBits) - 1;
            value &= mask;

            if (GetIsSigned())
            {
                int signBit = 1 << (totalBits - 1);
                if ((value & signBit) != 0)
                    value |= ~mask; // extension du signe
            }

            return value;
        }

        /// <summary>
        /// Convertit la valeur à virgule fixe en virgule flottante standard (`float`) pour l'affichage ou les calculs non déterministes.
        /// </summary>
        /// <returns>
        /// Une valeur `float` représentant la version réelle de ce nombre à virgule fixe,
        /// obtenue en divisant `raw` par <c>2^FractionBits</c>.
        /// </returns>
        /// <remarks>
        /// Cette méthode est utile pour le debug, l’affichage dans l’éditeur ou des calculs hors logique de jeu.
        /// </remarks>
        public float ToFloat() => Raw / (float)(1 << GetFractionBits());

        /// <summary>
        /// Convertit la valeur à virgule fixe en entier (`int`) en ignorant la partie fractionnaire.
        /// </summary>
        /// <returns>
        /// Un entier obtenu par décalage logique à droite (`raw >> FractionBits`),
        /// équivalent à une troncature.
        /// </returns>
        /// <remarks>
        /// Cette méthode n'arrondit pas : elle se contente d’ignorer les bits fractionnaires.
        /// </remarks>
        public int ToInt() => Raw >> GetFractionBits();

        /// <summary>
        /// Conversion implicite d’un `float` vers un <see cref="FixedPoint{TFormat}"/>.
        /// </summary>
        /// <param name="f">Valeur flottante à convertir.</param>
        /// <returns>
        /// Une instance de <see cref="FixedPoint{TFormat}"/> représentant la valeur convertie,
        /// en utilisant <see cref="FromFloat(float)"/>.
        /// </returns>
        /// <remarks>
        /// Permet d’écrire directement <c>FixedPoint&lt;Q8_4&gt; v = 1.5f;</c> sans appeler explicitement la méthode `FromFloat`.
        /// </remarks>

        public static implicit operator FixedPoint<TFormat>(float f) => FromFloat(f);

        /// <summary>
        /// Conversion implicite d’un <see cref="FixedPoint{TFormat}"/> vers un `float`.
        /// </summary>
        /// <param name="v">Valeur à virgule fixe à convertir.</param>
        /// <returns>
        /// La représentation flottante (`float`) de cette valeur, obtenue via <see cref="ToFloat()"/>.
        /// </returns>
        /// <remarks>
        /// Pratique pour les logs ou l’interfaçage avec des fonctions Unity ou .NET utilisant des `float`.
        /// </remarks>

        public static implicit operator float(FixedPoint<TFormat> v) => v.ToFloat();

        /// <summary>
        /// Récupère le nombre de bits fractionnaires définis par le format <typeparamref name="TFormat"/>.
        /// </summary>
        /// <returns>Le nombre de bits utilisés pour la partie décimale.</returns>
        /// <remarks>
        /// Cet accès indirect via <see cref="FixedFormatUtil{TFormat}"/> est nécessaire pour rester compatible avec WebGL et AOT,
        /// car C# 9/10 ne permet pas encore les struct `static` ou `interface static` génériques.
        /// Cette méthode permet donc une compatibilité maximale avec les plateformes IL2CPP.
        /// </remarks>

        private static int GetFractionBits() => FixedFormatUtil<TFormat>.FractionBits;

        /// <summary>
        /// Récupère le nombre de bits utilisés pour la partie entière.
        /// </summary>
        /// <returns>Le nombre de bits entiers alloués dans le format <typeparamref name="TFormat"/>.</returns>
        /// <remarks>
        /// Comme pour les bits fractionnaires, l’utilisation d’un helper centralisé permet une compatibilité parfaite avec WebGL,
        /// sans recourir aux réflexions ni aux fonctions virtuelles.
        /// </remarks>
        private static int GetIntegerBits() => FixedFormatUtil<TFormat>.IntegerBits;

        /// <summary>
        /// Indique si le format <typeparamref name="TFormat"/> est signé.
        /// </summary>
        /// <returns>`true` si le format est signé ; `false` sinon.</returns>
        /// <remarks>
        /// Permet à <see cref="FixedPoint{TFormat}"/> de gérer correctement les extensions de signe lors du wrap ou des calculs binaires.
        /// L’appel indirect permet une exécution optimale même en AOT ou WebGL.
        /// </remarks>
        private static bool GetIsSigned() => FixedFormatUtil<TFormat>.IsSigned;

        /// <summary>
        /// Retourne une représentation lisible de la valeur à virgule fixe, sous forme de chaîne de caractères en notation décimale.
        /// </summary>
        /// <returns>
        /// Une chaîne `string` représentant la valeur convertie en `float`, formatée à 4 décimales.
        /// </returns>
        /// <remarks>
        /// Utile pour le débogage ou l'affichage dans les interfaces de l’éditeur.
        /// Cette méthode ne doit pas être utilisée pour des comparaisons déterministes ou des sauvegardes binaires.
        /// </remarks>
        public override string ToString() => ToFloat().ToString("F4");


        /// <summary>
        /// Valeur nulle en virgule fixe pour le format TFormat.
        /// </summary>
        public static FixedPoint<TFormat> Zero
            => FixedMath<TFormat>.Zero;

        public static FixedPoint<TFormat> One
            => FixedMath.One<TFormat>();

        public static FixedPoint<TFormat> Half
            => FixedMath.Half<TFormat>();

        /// <summary>
        /// Additionne deux FixedPoint du même format.
        /// </summary>
        public static FixedPoint<TFormat> Add(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
            => FixedMath<TFormat>.Add(a, b);

        /// <summary>
        /// Soustrait un FixedPoint d’un autre.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Subtract(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
            => FixedMath<TFormat>.Subtract(a, b);

        /// <summary>
        /// Multiplie deux FixedPoint du même format.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Multiply(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
            => FixedMath<TFormat>.Multiply(a, b);

        /// <summary>
        /// Divise un FixedPoint par un autre.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Divide(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
            => FixedMath<TFormat>.Divide(a, b);


    }
}
