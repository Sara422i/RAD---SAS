using System;
using System.Collections.Generic;
using System.Linq;

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

//Test opgave 3 
Console.WriteLine("\nTest opgave 3");

var testStream = new List<Tuple<ulong, int>>
{
    Tuple.Create(7UL, 20),
    Tuple.Create(3UL, -5),
    Tuple.Create(7UL, -3),
    Tuple.Create(9UL, 100)
};

int lExact = 4;
var hExact = HashFunctions.MultiplyShiftHashFunction(lExact);

ExactSecondMoment exact = new ExactSecondMoment();
long S = exact.ComputeS(testStream, hExact, lExact);

Console.WriteLine("S, forventet 10314: " + S);