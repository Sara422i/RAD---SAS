using System;
using System.Numerics;
using System.Collections.Generic;

public class CountSketch {
    private const int Q = 89;
    private static readonly BigInteger P = (BigInteger.One << Q) -1;
    
    // Opgave 4. Implementering af 4-universal hashfunktion.
    private static Func<ulong, BigInteger> FourUniversalHashFunction() {
        BigInteger a0 = Helpers.GenerateRandomBigInteger(P);
        BigInteger a1 = Helpers.GenerateRandomBigInteger(P);
        BigInteger a2 = Helpers.GenerateRandomBigInteger(P);
        BigInteger a3 = Helpers.GenerateRandomBigInteger(P);

        return (x) => {
            BigInteger bx = new BigInteger(x);
            // Horner's rule: g(x) = a0 + a1*x + a2*x^2 + a3*x^3 mod p
            // Beregnes bagfra: y = a3, y = y*x + a2, y = y*x + a1, y = y*x + a0
            BigInteger y = a3;                    // y = a3
            y = Helpers.ModP(y * bx + a2, P, Q);  // y = a3*x + a2
            y = Helpers.ModP(y * bx + a1, P, Q);  // y = a3*x^2 + a2*x + a1
            y = Helpers.ModP(y * bx + a0, P, Q);  // y = a3*x^3 + a2*x^2 + a1*x + a0
            return y;
        };
    }
}