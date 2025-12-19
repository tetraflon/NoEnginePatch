// Decompiled with JetBrains decompiler
// Type: System.Runtime.CompilerServices.RefSafetyRulesAttribute
// Assembly: com.nikkorap.UnrestrictedWeapons, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 59A076EA-AFB7-41DA-960E-67733DB3299D
// Assembly location: C:\Users\fuwei\Downloads\com.nikkorap.UnrestrictedWeapons_1.3.0.dll

using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices;

#nullable disable
namespace System.Runtime.CompilerServices;

[CompilerGenerated]
[Embedded]
[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
internal sealed class RefSafetyRulesAttribute : Attribute
{
  public readonly int Version;

  public RefSafetyRulesAttribute([In] int obj0) => this.Version = obj0;
}
