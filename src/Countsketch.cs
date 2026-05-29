using System;
using System.Numerics;
using System.Collections.Generic;

public class CountSketch {
    private const int Q = 89;
    private static readonly BigInteger P = (BigInteger.One << Q) - 1;

    // Opgave 4. Implementering af 4-universal hashfunktion.
    public static Func<ulong, BigInteger> FourUniversalHashFunction() {
        BigInteger a0 = Helpers.GenerateRandomBigInteger(P);
        BigInteger a1 = Helpers.GenerateRandomBigInteger(P);
        BigInteger a2 = Helpers.GenerateRandomBigInteger(P);
        BigInteger a3 = Helpers.GenerateRandomBigInteger(P);

        return (x) => {
            BigInteger bx = new BigInteger(x);
            BigInteger y = a3;
            y = Helpers.ModP(y * bx + a2, P, Q);
            y = Helpers.ModP(y * bx + a1, P, Q);
            y = Helpers.ModP(y * bx + a0, P, Q);
            return y;
        };
    }

    // Opgave 5. Implementering af hashfunktioner til Count-Sketch.
    // Returnerer (h, s) som et par af funktioner.
    // h(x) = g(x) mod m, hvor m = 2^t  (de t mindst betydende bits)
    // s(x) = 1 - 2 * floor(g(x) / 2^(b-1)), dvs. baseret på bit 88
    public static (Func<ulong, ulong>, Func<ulong, int>) CountSketchHashFunctions(
        int t, Func<ulong, BigInteger> g)
    {
        BigInteger mask = (BigInteger.One << t) - 1; // maske for de t laveste bits

        Func<ulong, ulong> h = (x) => {
            BigInteger gx = g(x);
            return (ulong)(gx & mask); // g(x) mod 2^t
        };

        Func<ulong, int> s = (x) => {
            BigInteger gx = g(x);
            // floor(g(x) / 2^(b-1)) = floor(g(x) / 2^88)
            // Da g(x) ∈ [0, p-1] og p = 2^89 - 1, er dette enten 0 eller 1
            BigInteger msb = gx >> (Q - 1); // right-shift 88 bits
            return 1 - 2 * (int)msb;
        };

        return (h, s);
    }

    // Opgave 6. Implementering af Count-Sketch.
    private long[] C;
    private Func<ulong, ulong> hFunc;
    private Func<ulong, int> sFunc;
    private int m;

    public CountSketch(int t, Func<ulong, BigInteger> g) {
        m = 1 << t;
        C = new long[m];
        (hFunc, sFunc) = CountSketchHashFunctions(t, g);
    }

    // Processer ét element (x, d) fra strømmen
    public void Process(ulong x, int d) {
        ulong bucket = hFunc(x);
        int sign = sFunc(x);
        C[bucket] += sign * d;
    }

    // Processer en hel strøm
    public void ProcessStream(IEnumerable<Tuple<ulong, int>> stream) {
        foreach (var pair in stream) {
            Process(pair.Item1, pair.Item2);
        }
    }

    // Beregn estimatet X = Σ C[y]^2
    public long Estimate() {
        long X = 0;
        for (int i = 0; i < m; i++) {
            X += C[i] * C[i];
        }
        return X;
    }
}