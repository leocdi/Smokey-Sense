// This is a custom memory reading library I built for reading data from CS2.
// I added caching to improve performance.
// Uses Kernel32 for process memory access. No internet or external deps here.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

public class Memory
{
    private Process proc;
    private readonly ConcurrentDictionary<IntPtr, (IntPtr value, DateTime timestamp)> pointerCache = new ConcurrentDictionary<IntPtr, (IntPtr, DateTime)>();
    private readonly TimeSpan cacheDuration = TimeSpan.FromMilliseconds(100.0);
    private static readonly byte[] buffer8 = new byte[8];
    private static readonly byte[] buffer4 = new byte[4];
    private static readonly byte[] buffer12 = new byte[12];
    private int lpNumberOfBytesRead;

    // DLL imports for memory reading
    [DllImport("Kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, out int lpNumberOfBytesRead);

    public Memory()  // Constructor - sets up the process
    {
        proc = SetProcess();
    }

    public Process GetProcess() => proc;  // Get the current process

    public Process SetProcess()  // Find and set the CS2 process
    {
        proc = Process.GetProcessesByName("cs2").FirstOrDefault();
        if (proc == null)
        {
            Console.WriteLine("[i]: CS2 is not running. Please start the game first!");
            Thread.Sleep(3000);
            Environment.Exit(0);
        }
        return proc;
    }

    public IntPtr GetModuleBase()  // Get base address of client.dll module
    {
        if (proc == null)
        {
            Console.WriteLine("[i]: CS2 is not running. Please start the game first!");
            Thread.Sleep(3000);
            Environment.Exit(0);
        }
        try
        {
            foreach (ProcessModule module in proc.Modules)
            {
                if (module.ModuleName == "client.dll")
                    return module.BaseAddress;
            }
        }
        catch
        {
            Console.WriteLine("[i]: CS2 is not running. Please start the game first!");
            Thread.Sleep(3000);
            Environment.Exit(0);
        }
        return IntPtr.Zero;
    }

    public IntPtr ReadPointer(IntPtr addy)  // Read a pointer from memory
    {
        if (ReadProcessMemory(proc.Handle, addy, buffer8, 8, out lpNumberOfBytesRead))
            return (IntPtr)BitConverter.ToInt64(buffer8, 0);
        return IntPtr.Zero;
    }

    public IntPtr ReadPointerCached(IntPtr addy)  // Cached version for performance
    {
        var now = DateTime.Now;
        if (pointerCache.TryGetValue(addy, out var entry) && now - entry.timestamp < cacheDuration)
            return entry.value;

        var ptr = ReadPointer(addy);
        pointerCache[addy] = (ptr, now);
        return ptr;
    }

    public IntPtr ReadPointer(IntPtr addy, int offset)  // Read pointer with single offset
    {
        if (ReadProcessMemory(proc.Handle, addy + offset, buffer8, 8, out lpNumberOfBytesRead))
            return (IntPtr)BitConverter.ToInt64(buffer8, 0);
        return IntPtr.Zero;
    }

    public IntPtr ReadPointerCached(IntPtr addy, int offset)  // Cached with offset
    {
        IntPtr key = (IntPtr)((long)addy + offset);
        var now = DateTime.Now;
        if (pointerCache.TryGetValue(key, out var entry) && now - entry.timestamp < cacheDuration)
            return entry.value;

        var ptr = ReadPointer(addy, offset);
        pointerCache[key] = (ptr, now);
        return ptr;
    }

    public IntPtr ReadPointer(IntPtr addy, params int[] offsets)
    {
        IntPtr intPtr = addy;
        foreach (int offset in offsets)
        {
            intPtr = ReadPointerCached(intPtr, offset);
            if (intPtr == IntPtr.Zero)
                return IntPtr.Zero;
        }
        return intPtr;
    }

    public byte[] ReadBytes(IntPtr addy, int bytes)  // Read raw bytes
    {
        var array = new byte[bytes];
        ReadProcessMemory(proc.Handle, addy, array, array.Length, out lpNumberOfBytesRead);
        return array;
    }

    public byte[] ReadBytes(IntPtr addy, int offset, int bytes)  // Read bytes with offset
    {
        var array = new byte[bytes];
        ReadProcessMemory(proc.Handle, addy + offset, array, array.Length, out lpNumberOfBytesRead);
        return array;
    }

    public int ReadInt(IntPtr address)  // Read integer
    {
        if (ReadProcessMemory(proc.Handle, address, buffer4, 4, out lpNumberOfBytesRead))
            return BitConverter.ToInt32(buffer4, 0);
        return 0;
    }

    public int ReadInt(IntPtr address, int offset)  // Read int with offset
    {
        if (ReadProcessMemory(proc.Handle, address + offset, buffer4, 4, out lpNumberOfBytesRead))
            return BitConverter.ToInt32(buffer4, 0);
        return 0;
    }

    public IntPtr ReadLong(IntPtr address)  // Read long (pointer-sized)
    {
        if (ReadProcessMemory(proc.Handle, address, buffer8, 8, out lpNumberOfBytesRead))
            return (IntPtr)BitConverter.ToInt64(buffer8, 0);
        return IntPtr.Zero;
    }

    public IntPtr ReadLong(IntPtr address, int offset)  // Read long with offset
    {
        if (ReadProcessMemory(proc.Handle, address + offset, buffer8, 8, out lpNumberOfBytesRead))
            return (IntPtr)BitConverter.ToInt64(buffer8, 0);
        return IntPtr.Zero;
    }

    public float ReadFloat(IntPtr address)  // Read float
    {
        if (ReadProcessMemory(proc.Handle, address, buffer4, 4, out lpNumberOfBytesRead))
            return BitConverter.ToSingle(buffer4, 0);
        return 0f;
    }

    public float ReadFloat(IntPtr address, int offset)  // Read float with offset
    {
        if (ReadProcessMemory(proc.Handle, address + offset, buffer4, 4, out lpNumberOfBytesRead))
            return BitConverter.ToSingle(buffer4, 0);
        return 0f;
    }

    public Vector3 ReadVec(IntPtr address)  // Read Vector3
    {
        if (ReadProcessMemory(proc.Handle, address, buffer12, 12, out lpNumberOfBytesRead))
            return new Vector3(
                BitConverter.ToSingle(buffer12, 0),
                BitConverter.ToSingle(buffer12, 4),
                BitConverter.ToSingle(buffer12, 8)
            );
        return new Vector3(float.NaN, float.NaN, float.NaN);
    }

    public Vector3 ReadVec(IntPtr address, int offset)  // Read Vector3 with offset
    {
        if (ReadProcessMemory(proc.Handle, address + offset, buffer12, 12, out lpNumberOfBytesRead))
            return new Vector3(
                BitConverter.ToSingle(buffer12, 0),
                BitConverter.ToSingle(buffer12, 4),
                BitConverter.ToSingle(buffer12, 8)
            );
        return new Vector3(float.NaN, float.NaN, float.NaN);
    }

    public float[] ReadMatrix(IntPtr address)  // Read 4x4 matrix (16 floats)
    {
        var value = ReadBytes(address, 64);
        var array = new float[16];
        for (int i = 0; i < 16; i++)
            array[i] = BitConverter.ToSingle(value, i * 4);
        return array;
    }

    public static float Clamp(float value, float min, float max)  // Utility to clamp values
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}