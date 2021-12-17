// <copyright file="GlobalSuppressions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Simplification",
    "RCS1021:Convert lambda expression body to expression-body.",
    Justification = "Not an improvement in readability in this case",
    Scope = "member",
    Target = "~M:Corvus.Tenancy.Specs.Bindings.TenancyCosmosContainerBindings.InitializeContainer")]
