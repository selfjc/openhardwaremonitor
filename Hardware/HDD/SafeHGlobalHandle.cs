/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace OpenHardwareMonitor.Hardware.HDD {
  [SecurityCritical]
  [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
  internal sealed class SafeHGlobalHandle : SafeHandleZeroOrMinusOneIsInvalid {
    private int size;

    private SafeHGlobalHandle() : base(true) {
      size = 0;
    }

    [SecurityCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    public SafeHGlobalHandle(int size) : base(true) {
      SetHandle(Marshal.AllocHGlobal(size));
      this.size = size;
    }

    public T PtrToStructure<T>() where T : struct {
      return PtrToStructure<T>(0);
    }

    [SecurityCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    public T PtrToStructure<T>(int offset) where T : struct {
      if (checked(offset + Marshal.SizeOf<T>()) > size)
        throw new ArgumentOutOfRangeException();
      IntPtr ptr = IntPtr.Add(handle, offset);
      T result;
      bool flag = false;
      RuntimeHelpers.PrepareConstrainedRegions();
      try {
        DangerousAddRef(ref flag);
        result = Marshal.PtrToStructure<T>(ptr);
      }
      finally {
        if (flag) {
          DangerousRelease();
        }
      }
      return result;
    }

    public void StructureToPtr<T>(T structure) where T : struct {
      StructureToPtr<T>(structure, 0);
    }

    [SecurityCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    public void StructureToPtr<T>(T structure, int offset) where T : struct {
      if (checked(offset + Marshal.SizeOf<T>()) > size)
        throw new ArgumentOutOfRangeException();
      IntPtr ptr = IntPtr.Add(handle, offset);
      bool flag = false;
      RuntimeHelpers.PrepareConstrainedRegions();
      try {
        DangerousAddRef(ref flag);
        Marshal.StructureToPtr<T>(structure, ptr, false);
      }
      finally {
        if (flag) {
          DangerousRelease();
        }
      }
    }

    [SecurityCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    public string GetStringAnsi(int offset) {
      if (offset >= size)
        throw new ArgumentOutOfRangeException();
      IntPtr ptr = IntPtr.Add(handle, offset);
      string result;
      bool flag = false;
      RuntimeHelpers.PrepareConstrainedRegions();
      try {
        DangerousAddRef(ref flag);
        result = Marshal.PtrToStringAnsi(ptr);
      }
      finally {
        if (flag) {
          DangerousRelease();
        }
      }
      return result;
    }

    [SecurityCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    public string GetStringUni(int offset) {
      if (offset >= size)
        throw new ArgumentOutOfRangeException();
      IntPtr ptr = IntPtr.Add(handle, offset);
      string result;
      bool flag = false;
      RuntimeHelpers.PrepareConstrainedRegions();
      try {
        DangerousAddRef(ref flag);
        result = Marshal.PtrToStringUni(ptr);
      }
      finally {
        if (flag) {
          DangerousRelease();
        }
      }
      return result;
    }

    public byte[] ToArray() {
      return ToArray(0, size);
    }

    [SecurityCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    public byte[] ToArray(int offset, int length) {
      if (length == 0 || checked(offset + length) > size)
        throw new ArgumentOutOfRangeException();
      IntPtr ptr = IntPtr.Add(handle, offset);
      byte[] array = new byte[length];
      bool flag = false;
      RuntimeHelpers.PrepareConstrainedRegions();
      try {
        DangerousAddRef(ref flag);
        Marshal.Copy(ptr, array, 0, array.Length);
      }
      finally {
        if (flag) {
          DangerousRelease();
        }
      }
      return array;
    }

    [SecurityCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    protected override bool ReleaseHandle() {
      if (handle != IntPtr.Zero) {
        Marshal.FreeHGlobal(handle);
        handle = IntPtr.Zero;
        return true;
      }
      return false;
    }
  }
}
