using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static int GetPowerOfNumber(int n)
    {
        if (n <= 1)
            return 0;
        int power = 0;
        while (n > 1) {
            n = n / 2;
            power++;
        }
        return power;
    }

    public static int GetNumberOfPower(int power)
    {
        return (int)Mathf.Pow(2, power);
    }
    
    public static Color GetColorByPower(int power)
    {
        return Database.GameConfiguration.NumberColors[power-1];
    }
    
    public static Color GetColorByNumber(int n)
    {
        return  GetColorByPower(GetPowerOfNumber(n));
    }
    
    public static int GetRandomPower(int minPower,int maxPower)
    {
        int range = maxPower - minPower + 1;
        int sumOfShares = range * (range + 1) / 2;
        int[] chanceIndex = new int[range];
        int chance = 0;
        for(int i = 0; i < range; i++)
        {
            chance += range - i;
            chanceIndex[i] = chance;
        }
        int chooseNumber = Random.Range(1, sumOfShares + 1);
        for (int i = 0; i < chanceIndex.Length; i++)
            if (chooseNumber <= chanceIndex[i])
                return i + minPower;

        return minPower;
    }
}
