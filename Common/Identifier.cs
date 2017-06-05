/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenHardwareMonitor.Common {
  public class Identifier : IComparable<Identifier>, IEquatable<Identifier>, IComparable<string>, IEquatable<string>, IComparable {
    private readonly string identifier;

    private const char Separator = '/';

    private static void CheckIdentifiers(IEnumerable<string> identifiers) {
      foreach (string s in identifiers)
        if (s.Contains(" ") || s.Contains(Separator.ToString()))
          throw new ArgumentException("Invalid identifier");
    }

    private Identifier(string identifier) {
      this.identifier = identifier;
    }

    public Identifier(params string[] identifiers) {
      CheckIdentifiers(identifiers);

      StringBuilder s = new StringBuilder();
      for (int i = 0; i < identifiers.Length; i++) {
        s.Append(Separator);
        s.Append(identifiers[i]);
      }
      this.identifier = s.ToString();
    }

    public Identifier(Identifier identifier, params string[] extensions) {
      CheckIdentifiers(extensions);

      StringBuilder s = new StringBuilder();
      s.Append(identifier.ToString());
      for (int i = 0; i < extensions.Length; i++) {
        s.Append(Separator);
        s.Append(extensions[i]);
      }
      this.identifier = s.ToString();
    }

    public static Identifier FromString(string identifier) {
      if (string.IsNullOrEmpty(identifier) || identifier.Contains(" ") || identifier[0] != Separator)
        throw new ArgumentException("Invalid identifier");
      return new Identifier(identifier);
    }

    public override string ToString() {
      return identifier;
    }

    public int CompareTo(Object other) {
      if (ReferenceEquals(other, null))
        return 1;
      if (other is Identifier)
        return CompareTo((Identifier)other);
      if (other is string)
        return CompareTo((string)other);
      return -1;
    }

    public override bool Equals(Object obj) {
      if (obj == null)
        return false;

      return Equals(obj as Identifier) || Equals(obj as string);
    }

    public override int GetHashCode() {
      return identifier.GetHashCode();
    }

    public bool Equals(Identifier other) {
      if (ReferenceEquals(other, null))
        return false;
      return (identifier == other.identifier);
    }

    public int CompareTo(Identifier other) {
      if (ReferenceEquals(other, null))
        return 1;
      else
        return string.Compare(this.identifier, other.identifier, StringComparison.Ordinal);
    }

    public static bool operator ==(Identifier id1, Identifier id2) {
      if (ReferenceEquals(id1, id2))
        return true;
      if (ReferenceEquals(id1, null) || ReferenceEquals(id2, null))
        return false;
      return id1.Equals(id2);
    }

    public static bool operator !=(Identifier id1, Identifier id2) {
      return !(id1 == id2);
    }

    public static bool operator <(Identifier id1, Identifier id2) {
      if (ReferenceEquals(id1, null))
        return !ReferenceEquals(id2, null);
      else
        return (id1.CompareTo(id2) < 0);
    }

    public static bool operator >(Identifier id1, Identifier id2) {
      if (ReferenceEquals(id1, null))
        return false;
      else
        return (id1.CompareTo(id2) > 0);
    }

    public int CompareTo(string other) {
      return string.Compare(this.identifier, other, StringComparison.Ordinal);
    }

    public bool Equals(string other) {
      return identifier == other;
    }

    public static bool operator ==(Identifier id1, string id2) {
      if (ReferenceEquals(id1, null))
        return ReferenceEquals(id2, null);
      else
        return id1.Equals(id2);
    }

    public static bool operator !=(Identifier id1, string id2) {
      return !(id1 == id2);
    }

    public static bool operator <(Identifier id1, string id2) {
      if (ReferenceEquals(id1, null))
        return !ReferenceEquals(id2, null);
      else
        return (id1.CompareTo(id2) < 0);
    }

    public static bool operator >(Identifier id1, string id2) {
      if (ReferenceEquals(id1, null))
        return false;
      else
        return (id1.CompareTo(id2) > 0);
    }

    public static Identifier operator +(Identifier id1, string id2) {
      return new Identifier(id1, id2);
    }
  }
}
