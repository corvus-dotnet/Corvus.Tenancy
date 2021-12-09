// <copyright file="GlobalSuppressions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1008:Opening parenthesis should be spaced correctly",
    Justification = "This looks like a 'StyleCop has yet to catch up with C# 8' type issue",
    Scope = "member",
    Target = "~M:Corvus.Tenancy.TenantExtensions.GetParentTree(System.String)~System.Collections.Generic.IEnumerable{System.String}")]
