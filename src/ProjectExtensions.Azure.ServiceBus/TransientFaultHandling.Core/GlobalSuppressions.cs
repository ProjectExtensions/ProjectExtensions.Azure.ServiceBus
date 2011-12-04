//===============================================================================
// Microsoft patterns & practices Enterprise Library
// Transient Fault Handling Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Scope = "member", Target = "Microsoft.Practices.TransientFaultHandling.ShouldRetry.#Invoke(System.Int32,System.Exception,System.TimeSpan&)", Justification = "As designed")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Backoff", Scope = "member", Target = "Microsoft.Practices.TransientFaultHandling.RetryPolicy`1.#.ctor(System.Int32,System.TimeSpan,System.TimeSpan,System.TimeSpan)", Justification = "As designed")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Backoff", Scope = "member", Target = "Microsoft.Practices.TransientFaultHandling.RetryPolicy.#.ctor(Microsoft.Practices.TransientFaultHandling.ITransientErrorDetectionStrategy,System.Int32,System.TimeSpan,System.TimeSpan,System.TimeSpan)", Justification = "As designed")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Backoff", Scope = "member", Target = "Microsoft.Practices.TransientFaultHandling.ExponentialBackoff.#.ctor(System.String,System.Int32,System.TimeSpan,System.TimeSpan,System.TimeSpan,System.Boolean)", Justification = "As designed")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Backoff", Scope = "member", Target = "Microsoft.Practices.TransientFaultHandling.ExponentialBackoff.#.ctor(System.String,System.Int32,System.TimeSpan,System.TimeSpan,System.TimeSpan)", Justification = "As designed")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Backoff", Scope = "member", Target = "Microsoft.Practices.TransientFaultHandling.ExponentialBackoff.#.ctor(System.Int32,System.TimeSpan,System.TimeSpan,System.TimeSpan)", Justification = "As designed")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Backoff", Scope = "type", Target = "Microsoft.Practices.TransientFaultHandling.ExponentialBackoff", Justification = "As designed")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Backoff", Scope = "member", Target = "Microsoft.Practices.TransientFaultHandling.RetryStrategy.#DefaultMinBackoff", Justification = "As designed")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Backoff", Scope = "member", Target = "Microsoft.Practices.TransientFaultHandling.RetryStrategy.#DefaultMaxBackoff", Justification = "As designed")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Backoff", Scope = "member", Target = "Microsoft.Practices.TransientFaultHandling.RetryStrategy.#DefaultClientBackoff", Justification = "As designed")]
