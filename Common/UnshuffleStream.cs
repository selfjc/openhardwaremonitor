/*

  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.

  Copyright (C) 2017 Alexander Thulcke <alexth4ef9@gmail.com>

*/

using System;
using System.IO;

namespace OpenHardwareMonitor.Common {
  internal class UnshuffleStream : MemoryStream {
    public UnshuffleStream(byte[] buffer, int n) : base(Unshuffle(buffer, 0, buffer.Length, n), false) {
    }

    public UnshuffleStream(byte[] buffer, int index, int count, int n) : base(Unshuffle(buffer, index, count, n), false) {
    }

    private static byte[] Unshuffle(byte[] buffer, int index, int count, int n) {
      if (buffer == null)
        throw new ArgumentNullException("buffer");
      if (index < 0)
        throw new ArgumentOutOfRangeException("index");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count");
      if (buffer.Length - index < count)
        throw new ArgumentException();
      if (n < 1)
        throw new ArgumentOutOfRangeException("n");
      byte[] result = new byte[count];
      int i = index;
      for (int j = 0; j < n; j++) {
        for (int k = j; k < count; k += n)
          result[k] = buffer[i++];
      }
      return result;
    }
  }
}
