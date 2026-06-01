using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

int n = 10000000;
int l = 10;

// Test opgave 1a
var h1 = HashFunctions.MultiplyShiftHashFunction(l);
Console.WriteLine("Multiply-shift: " + h1(123456789UL));

// Test opgave 1b
var h2 = HashFunctions.MultiplyModPrimeHashFunction(l);
Console.WriteLine("Multiply-mod-prime: " + h2(123456789UL));

// Test opgave 1c
var stream = Helpers.CreateStream(n, l).ToList();

var watch1 = System.Diagnostics.Stopwatch.StartNew();
ulong sum1 = 0;
var hShift = HashFunctions.MultiplyShiftHashFunction(l);
foreach (var (x, d) in stream)
    sum1 += hShift(x);
watch1.Stop();
Console.WriteLine($"Multiply-shift: Sum = {sum1}, Time = {watch1.ElapsedMilliseconds} ms");

var watch2 = System.Diagnostics.Stopwatch.StartNew();
ulong sum2 = 0;
var hPrime = HashFunctions.MultiplyModPrimeHashFunction(l);
foreach (var (x, d) in stream)
    sum2 += hPrime(x);
watch2.Stop();
Console.WriteLine($"Multiply-mod-prime: Sum = {sum2}, Time = {watch2.ElapsedMilliseconds} ms");

// Test opgave 2: ChainedHashTable
Console.WriteLine("\nTest opgave 2");

int lTable = 2; // 4 buckets
Func<ulong, ulong> testHash = x => x % (1UL << lTable);

var table = new ChainedHashTable(testHash, lTable);

// Test 2a  
Console.WriteLine("2(a) Get(99), forventet 0: " + table.Get(99UL));

// Test 2b
table.Set(10UL, 5);
table.Set(14UL, 8); // 10 og 14 lander i samme bucket, da begge giver 2 mod 4

Console.WriteLine("2(b) Get(10), forventet 5: " + table.Get(10UL));
Console.WriteLine("2(b) Get(14), forventet 8: " + table.Get(14UL));

// Test 2c
table.Increment(10UL, 3);
table.Increment(14UL, -2);
table.Increment(20UL, 4); // ny nøgle

Console.WriteLine("2(c) Get(10), forventet 8: " + table.Get(10UL));
Console.WriteLine("2(c) Get(14), forventet 6: " + table.Get(14UL));
Console.WriteLine("2(c) Get(20), forventet 4: " + table.Get(20UL));
// ===================== Opgave 3 =====================
Console.WriteLine("\n=== Opgave 3: Kvadratsummer med stigende l ===");

int nOpg3 = 10_000_000; // n fast, 2^l <= n, dvs. l <= 23

Console.WriteLine($"{"l",4} {"2^l",10} {"S (shift)",18} {"ms (shift)",12} {"S (prime)",18} {"ms (prime)",12}");
Console.WriteLine(new string('-', 80));

for (int lVal = 2; lVal <= 6; lVal += 2) {
    long twoToL = 1L << lVal;
    if (twoToL > nOpg3) {
        Console.WriteLine($"l={lVal}: 2^l={twoToL} > n={nOpg3}, stopper.");
        break;
    }

    // Generer strømmen ÉN gang og gem den (bruges til begge hashfunktioner)
    var streamOpg3 = Helpers.CreateStream(nOpg3, lVal).ToList();

    // --- Multiply-shift ---
    var hShiftOpg3 = HashFunctions.MultiplyShiftHashFunction(lVal);
    var swShift = System.Diagnostics.Stopwatch.StartNew();
    var exactShift = new ExactSecondMoment();
    long sShift = exactShift.ComputeS(streamOpg3, hShiftOpg3, lVal);
    swShift.Stop();

    // --- Multiply-mod-prime ---
    var hPrimeOpg3 = HashFunctions.MultiplyModPrimeHashFunction(lVal);
    var swPrime = System.Diagnostics.Stopwatch.StartNew();
    var exactPrime = new ExactSecondMoment();
    long sPrime = exactPrime.ComputeS(streamOpg3, hPrimeOpg3, lVal);
    swPrime.Stop();

    Console.WriteLine($"{lVal,4} {twoToL,10} {sShift,18} {swShift.ElapsedMilliseconds,10} ms {sPrime,18} {swPrime.ElapsedMilliseconds,10} ms");
}

Console.WriteLine("\n=== Opgave 7: Count-Sketch eksperiment ===");

int n7 = 10_000_000;

// Vælg l lige under den grænse hvor opgave 3 blev for tung.
// Hvis I fx kunne klare l=22, men ikke l=24, så brug l=22.
int l7 = 22;

// m = 2^t counters i Count-Sketch
int t = 12;
int m = 1 << t;

var sigma = Helpers.CreateStream(n7, l7).ToList();

Console.WriteLine("Beregner eksakt S med chaining...");

var exactHash = HashFunctions.MultiplyShiftHashFunction(l7);
var exact = new ExactSecondMoment();

var swExact = System.Diagnostics.Stopwatch.StartNew();
long S = exact.ComputeS(sigma, exactHash, l7);
swExact.Stop();

Console.WriteLine($"Eksakt S = {S}");
Console.WriteLine($"Tid for chaining = {swExact.ElapsedMilliseconds} ms");

List<long> estimates = new();

Console.WriteLine("Kører Count-Sketch 100 gange...");

for (int i = 0; i < 100; i++)
{
    // VIGTIGT: ny g hver gang
    var g = CountSketch.FourUniversalHashFunction();

    var sketch = new CountSketch(t, g);
    sketch.ProcessStream(sigma);

    long X = sketch.Estimate();
    estimates.Add(X);

    Console.WriteLine($"Run {i + 1}: X = {X}");
}

// MSE
double mse = 0;

foreach (long X in estimates)
{
    double diff = X - S;
    mse += diff * diff;
}

mse /= 100.0;

double theoreticalVariance = 2.0 * S * S / m;

Console.WriteLine($"\nMSE = {mse}");
Console.WriteLine($"Teoretisk Var[X] ≈ 2S^2/m = {theoreticalVariance}");

// Sorterede estimater til første plot
var sortedEstimates = estimates.OrderBy(x => x).ToList();

using (StreamWriter writer = new StreamWriter("opgave7_sorted_estimates.csv"))
{
    writer.WriteLine("i,X,S");

    for (int i = 0; i < sortedEstimates.Count; i++)
    {
        writer.WriteLine($"{i + 1},{sortedEstimates[i]},{S}");
    }
}

// Median-trick
List<long> medians = new();

for (int group = 0; group < 9; group++)
{
    var G = estimates
        .Skip(group * 11)
        .Take(11)
        .OrderBy(x => x)
        .ToList();

    long median = G[5]; // midterste af 11 tal
    medians.Add(median);
}

medians.Sort();

using (StreamWriter writer = new StreamWriter("opgave7_medians.csv"))
{
    writer.WriteLine("i,M,S");

    for (int i = 0; i < medians.Count; i++)
    {
        writer.WriteLine($"{i + 1},{medians[i]},{S}");
    }
}

Console.WriteLine("CSV-filer lavet:");
Console.WriteLine("opgave7_sorted_estimates.csv");
Console.WriteLine("opgave7_medians.csv");
