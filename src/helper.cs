using System;
using System.Numerics;
using System.Collections.Generic;

public static class Helpers
{
    public static ulong GenerateRandomOddULong() {
        Random random = new Random();
        byte[] bytes = new byte[8];
        random.NextBytes(bytes);
        bytes[7] |= 1;
        return BitConverter.ToUInt64(bytes, 0);
    }
    public static BigInteger ModP(BigInteger x, BigInteger p, int q) {
        BigInteger y = (x & p) + (x >> q);
        if (y >= p) y -= p;
        return y;
    }
    public static BigInteger GenerateRandomBigInteger(BigInteger p) {
        Random random = new Random();
        byte[] bytes = new byte[12];
        BigInteger result;
        do
        {
            random.NextBytes(bytes);
            bytes[11] &= 0x1F;
            result = new BigInteger(bytes);
        } while (result >= p || result < 0);
        return result;
    }
    public static IEnumerable < Tuple < ulong , int > > CreateStream (int n , int l ) {
        // We generate a random uint64 number .
        Random rnd = new System . Random () ;
        ulong a = 0UL ;
        Byte [] b = new Byte [8];
        rnd . NextBytes ( b ) ;
        for( int i = 0; i < 8; ++ i ) {
            a = ( a << 8) + ( ulong ) b [ i ];
        }
        // We demand that our random number has 30 zeros on the
        // significant bits and then a one.
        a = ( a | ((1UL << 31) - 1UL ) ) ^ ((1UL << 30) - 1UL ) ;
    
        ulong x = 0UL ;
        for( int i = 0; i < n /3; ++ i ) {
            x = x + a ;
            yield return Tuple . Create ( x & (((1UL << l ) - 1UL ) <<
                30) , 1) ;
        }
    
        for( int i = 0; i < ( n + 1) /3; ++ i ) {
            x = x + a ;
            yield return Tuple . Create ( x & (((1UL << l ) - 1UL ) <<
                30) , -1) ;
        }
        for( int i = 0; i < ( n + 2) /3; ++ i ) {
            x = x + a ;
            yield return Tuple . Create ( x & (((1UL << l ) - 1UL ) <<
                30) , 1) ;
        }
    }

}