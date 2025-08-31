// This is a custom memory reading library I built for reading data from CS2.
// I added caching to improve performance.
// Uses Kernel32 for process memory access. No internet or external deps here.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

public class Memory
{
    private Process proc;  // The target process (CS2)
    private readonly Dictionary<IntPtr, IntPtr> pointerCache = new Dictionary<IntPtr, IntPtr>();  // Cache for pointers to speed up repeated reads
    private readonly TimeSpan cacheDuration = TimeSpan.FromMilliseconds(100.0);  // How long to keep cached values
    private readonly Dictionary<IntPtr, DateTime> cacheTimestamps = new Dictionary<IntPtr, DateTime>();  // Timestamps for cache invalidation

    // DLL imports for memory reading
    [DllImport("Kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, IntPtr lpNumberOfBytesRead);
    [DllImport("kernel32.dll")]
    private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hObject);

    public Memory()  // Constructor - sets up the process
    {
        proc = SetProcess();
    }

    public Process GetProcess()  // Get the current process
    {
        return proc;
    }

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
                {
                    return module.BaseAddress;
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine("[i]: CS2 is not running. Please start the game first!");
            Thread.Sleep(3000);
            Environment.Exit(0);
        }
        return IntPtr.Zero;
    }

    public IntPtr ReadPointer(IntPtr addy)  // Read a pointer from memory
    {
        byte[] array = new byte[8];
        if (ReadProcessMemory(proc.Handle, addy, array, array.Length, IntPtr.Zero))
        {
            return (IntPtr)BitConverter.ToInt64(array, 0);
        }
        return IntPtr.Zero;
    }

    public IntPtr ReadPointerCached(IntPtr addy)  // Cached version for performance
    {
        lock (pointerCache)
        {
            if (pointerCache.TryGetValue(addy, out var value) && cacheTimestamps.TryGetValue(addy, out var value2) && DateTime.Now - value2 < cacheDuration)
            {
                return value;
            }
            IntPtr intPtr = ReadPointer(addy);
            pointerCache[addy] = intPtr;
            cacheTimestamps[addy] = DateTime.Now;
            return intPtr;
        }
    }

    public IntPtr ReadPointer(IntPtr addy, int offset)  // Read pointer with single offset
    {
        byte[] array = new byte[8];
        if (ReadProcessMemory(proc.Handle, addy + offset, array, array.Length, IntPtr.Zero))
        {
            return (IntPtr)BitConverter.ToInt64(array, 0);
        }
        return IntPtr.Zero;
    }

    public IntPtr ReadPointerCached(IntPtr addy, int offset)  // Cached with offset
    {
        IntPtr key = (IntPtr)((long)addy + offset);
        lock (pointerCache)
        {
            if (pointerCache.TryGetValue(key, out var value) && cacheTimestamps.TryGetValue(key, out var value2) && DateTime.Now - value2 < cacheDuration)
            {
                return value;
            }
            IntPtr intPtr = ReadPointer(addy, offset);
            pointerCache[key] = intPtr;
            cacheTimestamps[key] = DateTime.Now;
            return intPtr;
        }
    }

    public IntPtr ReadPointer(IntPtr addy, int[] offsets)  // Read pointer with multiple offsets (chain)
    {
        IntPtr intPtr = addy;
        foreach (int offset in offsets)
        {
            intPtr = ReadPointerCached(intPtr, offset);
            if (intPtr == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
        }
        return intPtr;
    }

    // Overloads for convenience with different number of offsets
    public IntPtr ReadPointer(IntPtr addy, int offset1, int offset2)
    {
        return ReadPointer(addy, new int[2] { offset1, offset2 });
    }
    public IntPtr ReadPointer(IntPtr addy, int offset1, int offset2, int offset3)
    {
        return ReadPointer(addy, new int[3] { offset1, offset2, offset3 });
    }
    public IntPtr ReadPointer(IntPtr addy, int offset1, int offset2, int offset3, int offset4)
    {
        return ReadPointer(addy, new int[4] { offset1, offset2, offset3, offset4 });
    }
    public IntPtr ReadPointer(IntPtr addy, int offset1, int offset2, int offset3, int offset4, int offset5)
    {
        return ReadPointer(addy, new int[5] { offset1, offset2, offset3, offset4, offset5 });
    }
    public IntPtr ReadPointer(IntPtr addy, int offset1, int offset2, int offset3, int offset4, int offset5, int offset6)
    {
        return ReadPointer(addy, new int[6] { offset1, offset2, offset3, offset4, offset5, offset6 });
    }
    public IntPtr ReadPointer(IntPtr addy, int offset1, int offset2, int offset3, int offset4, int offset5, int offset6, int offset7)
    {
        return ReadPointer(addy, new int[7] { offset1, offset2, offset3, offset4, offset5, offset6, offset7 });
    }

    public byte[] ReadBytes(IntPtr addy, int bytes)  // Read raw bytes
    {
        byte[] array = new byte[bytes];
        ReadProcessMemory(proc.Handle, addy, array, array.Length, IntPtr.Zero);
        return array;
    }

    public byte[] ReadBytes(IntPtr addy, int offset, int bytes)  // Read bytes with offset
    {
        byte[] array = new byte[bytes];
        ReadProcessMemory(proc.Handle, addy + offset, array, array.Length, IntPtr.Zero);
        return array;
    }

    public int ReadInt(IntPtr address)  // Read integer
    {
        try
        {
            return BitConverter.ToInt32(ReadBytes(address, 4), 0);
        }
        catch
        {
            return 0;
        }
    }

    public int ReadInt(IntPtr address, int offset)  // Read int with offset
    {
        try
        {
            return BitConverter.ToInt32(ReadBytes(address + offset, 4), 0);
        }
        catch
        {
            return 0;
        }
    }

    public IntPtr ReadLong(IntPtr address)  // Read long (pointer-sized)
    {
        try
        {
            return (IntPtr)BitConverter.ToInt64(ReadBytes(address, 8), 0);
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    public IntPtr ReadLong(IntPtr address, int offset)  // Read long with offset
    {
        try
        {
            return (IntPtr)BitConverter.ToInt64(ReadBytes(address + offset, 8), 0);
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    public float ReadFloat(IntPtr address)  // Read float
    {
        try
        {
            return BitConverter.ToSingle(ReadBytes(address, 4), 0);
        }
        catch
        {
            return 0f;
        }
    }

    public float ReadFloat(IntPtr address, int offset)  // Read float with offset
    {
        try
        {
            return BitConverter.ToSingle(ReadBytes(address + offset, 4), 0);
        }
        catch
        {
            return 0f;
        }
    }

    public Vector3 ReadVec(IntPtr address)  // Read Vector3
    {
        try
        {
            byte[] value = ReadBytes(address, 12);
            return new Vector3(
                BitConverter.ToSingle(value, 0),
                BitConverter.ToSingle(value, 4),
                BitConverter.ToSingle(value, 8)
            );
        }
        catch
        {
            return new Vector3(float.NaN, float.NaN, float.NaN);
        }
    }

    public Vector3 ReadVec(IntPtr address, int offset)  // Read Vector3 with offset
    {
        try
        {
            byte[] value = ReadBytes(address + offset, 12);
            return new Vector3(
                BitConverter.ToSingle(value, 0),
                BitConverter.ToSingle(value, 4),
                BitConverter.ToSingle(value, 8)
            );
        }
        catch
        {
            return new Vector3(float.NaN, float.NaN, float.NaN);
        }
    }

    public float[] ReadMatrix(IntPtr address)  // Read 4x4 matrix (16 floats)
    {
        try
        {
            byte[] value = ReadBytes(address, 64);
            float[] array = new float[16];
            for (int i = 0; i < 16; i++)
            {
                array[i] = BitConverter.ToSingle(value, i * 4);
            }
            return array;
        }
        catch
        {
            return new float[16];
        }
    }

    public static float Clamp(float value, float min, float max)  // Utility to clamp values
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}