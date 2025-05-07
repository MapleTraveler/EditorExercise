using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    /// <summary>
    /// 检查整数的第 n 位是否为 1。
    /// </summary>
    /// <param name="num">要检查的整数。</param>
    /// <param name="n">要检查的位的位置（从右到左，从 0 开始）。</param>
    /// <returns>如果第 n 位为 1，则返回 true；否则返回 false。</returns>
    public static bool IsBitSet(int num, int n)
    {
        if (n < 0 || n >= 32)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "位位置必须在 0 到 31 之间。");
        }
        
        int mask = 1 << n;    // 创建掩码
        return (num & mask) != 0;  // 使用按位与运算检查特定位
    }

    /// <summary>
    /// 获取整数的第 n 位的值。
    /// </summary>
    /// <param name="num">要检查的整数。</param>
    /// <param name="n">要检查的位的位置（从右到左，从 0 开始）。</param>
    /// <returns>返回第 n 位的值（0 或 1）。</returns>
    public static int GetBitValue(int num, int n)
    {
        if (n < 0 || n >= 32)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "位位置必须在 0 到 31 之间。");
        }

        return (num >> n) & 1; // 右移 n 位并检查最低位
    }

    /// <summary>
    /// 设置整数的第 n 位为 1。
    /// </summary>
    /// <param name="num">要设置位的整数。</param>
    /// <param name="n">要设置的位的位置（从右到左，从 0 开始）。</param>
    /// <returns>返回设置了第 n 位后的新整数。</returns>
    public static int SetBit(int num, int n)
    {
        if (n < 0 || n >= 32)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "位位置必须在 0 到 31 之间。");
        }

        int mask = 1 << n;    // 创建掩码
        return num | mask;    // 使用按位或运算设置特定位
    }

    /// <summary>
    /// 清除整数的第 n 位（将其设置为 0）。
    /// </summary>
    /// <param name="num">要清除位的整数。</param>
    /// <param name="n">要清除的位的位置（从右到左，从 0 开始）。</param>
    /// <returns>返回清除第 n 位后的新整数。</returns>
    public static int ClearBit(int num, int n)
    {
        if (n < 0 || n >= 32)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "位位置必须在 0 到 31 之间。");
        }

        int mask = ~(1 << n); // 创建掩码并取反
        return num & mask;    // 使用按位与运算清除特定位
    }

    /// <summary>
    /// 切换整数的第 n 位（如果为 0，则设置为 1；如果为 1，则设置为 0）。
    /// </summary>
    /// <param name="num">要切换位的整数。</param>
    /// <param name="n">要切换的位的位置（从右到左，从 0 开始）。</param>
    /// <returns>返回切换第 n 位后的新整数。</returns>
    public static int ToggleBit(int num, int n)
    {
        if (n < 0 || n >= 32)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "位位置必须在 0 到 31 之间。");
        }

        int mask = 1 << n;    // 创建掩码
        return num ^ mask;    // 使用按位异或运算切换特定位
    }
}
