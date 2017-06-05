/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.IO;

namespace OpenHardwareMonitor.Common {
  internal class ShuffleStream : MemoryStream {
    private readonly int n;

    public ShuffleStream(int n) : base(0) {
      if(n < 2)
        throw new ArgumentOutOfRangeException("n");
      this.n = n;
    }

    public override byte[] ToArray() {
      byte[] buffer = GetBuffer();
      byte[] result = new byte[Length];
      int i = 0;
      for (int j = 0; j < n; j++) {
        for (int k = j; k < Length; k += n)
          result[i++] = buffer[k];
      }
      return result;
    }
  }
}
