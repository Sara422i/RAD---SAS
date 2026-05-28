using System;
using System.Numerics;
using System.Collections.Generic;

/*
Right-Shift: For at lave et right-shift af x med l bits bruges x>>l i C#.
Bitwise AND: For at lave det bitvise AND mellem to strenge x og y bruges x&y i C#.
Store multiplikationer: BigInteger i C#
*/

// Opgave 1. Implementering af hashfunktioner.

public static class HashFunctions {
    // a) Implementer multiply-shift hashing.
    public static Func<ulong, ulong> MultiplyShiftHashFunction(int l) {
        ulong a = Helpers.GenerateRandomOddULong();
        return (x) => (a * x) >> (64 - l);
    }

    // b) Implementer multiply-mod-prime hashing.
    public static Func<ulong, ulong> MultiplyModPrimeHashFunction(int l) {   
        int q = 89;
        BigInteger p = (BigInteger.One << q) -1;
        BigInteger a = Helpers.GenerateRandomBigInteger(p);
        BigInteger b = Helpers.GenerateRandomBigInteger(p);
        return (x) => {
            BigInteger ax = a * x; // Dette er x1*x2 hvor begge er i [p]
            BigInteger axb = Helpers.ModP(ax, p, q) + b;
            BigInteger result = Helpers.ModP(axb, p, q);
            return (ulong)(result % (BigInteger)(1UL << l));
        };
    }
}

// Opgave 2. Implementering af hashtabel med chaining.
public class ChainedHashTable {
    readonly List<Entry>[] table;
    readonly Func<ulong, ulong> h;

    class Entry {
        public ulong Key;
        public long Value;

        public Entry(ulong key, long value) {
            Key = key;
            Value = value;
        }
    }

    public ChainedHashTable(Func<ulong, ulong> hashFunction, int l) {
        h = hashFunction;
        int size = 1 << l;
        table = new List<Entry>[size];
        for (int i = 0; i < size; i++)
            table[i] = new List<Entry>();
    }

    // a) get(x): Returnerer værdien for x, eller 0 hvis x ikke findes.
    public long Get(ulong x) {
        int index = (int)h(x);
        foreach (Entry entry in table[index])
            if (entry.Key == x)
                return entry.Value;
        return 0;
    }

    // b) set(x, v): Sætter x til værdien v.
    public void Set(ulong x, long v) {
        int index = (int)h(x);
        foreach (Entry entry in table[index])
            if (entry.Key == x) {
                entry.Value = v;
                return;
            }
        table[index].Add(new Entry(x, v));
    }

    // c) increment(x, d): Lægger d til værdien for x.
    public void Increment(ulong x, long d) {
        int index = (int)h(x);
        foreach (Entry entry in table[index])
            if (entry.Key == x) {
                entry.Value += d;
                return;
            }
        table[index].Add(new Entry(x, d));
    }

    // Opgave 3. Udregning af kvadratsummer.
    public class ExactSecondMoment {
        public long ComputeS(IEnumerable<Tuple<ulong, int>> stream, Func<ulong, ulong> h, int l) {
            ChainedHashTable table = new(h, l);

            foreach (var pair in stream) {
                ulong x = pair.Item1;
                int d = pair.Item2;

                table.Increment(x, d);
            }

            return table.SumOfSquares();
        }
    }
}