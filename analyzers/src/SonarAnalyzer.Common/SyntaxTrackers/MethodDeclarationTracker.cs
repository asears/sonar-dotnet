﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public abstract class MethodDeclarationTracker : TrackerBase<MethodDeclarationContext>
    {
        public abstract Condition ParameterAtIndexIsUsed(int index);
        protected abstract SyntaxToken? GetMethodIdentifier(SyntaxNode methodDeclaration);

        protected MethodDeclarationTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) : base(analyzerConfiguration, rule) { }

        public void Track(SonarAnalysisContext context, params Condition[] conditions)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (IsEnabled(c.Options))
                    {
                        c.RegisterSymbolAction(TrackMethodDeclaration, SymbolKind.Method);
                    }
                });

            void TrackMethodDeclaration(SymbolAnalysisContext c)
            {
                if (IsTrackedMethod((IMethodSymbol)c.Symbol, c.Compilation))
                {
                    foreach (var declaration in c.Symbol.DeclaringSyntaxReferences)
                    {
                        var methodIdentifier = GetMethodIdentifier(declaration.GetSyntax());
                        if (methodIdentifier.HasValue)
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, methodIdentifier.Value.GetLocation()));
                        }
                    }
                }
            }

            bool IsTrackedMethod(IMethodSymbol methodSymbol, Compilation compilation)
            {
                var conditionContext = new MethodDeclarationContext(methodSymbol, compilation);
                return conditions.All(c => c(conditionContext));
            }
        }

        public static Condition MatchMethodName(params string[] methodNames) =>
            context => methodNames.Contains(context.MethodSymbol.Name);

        public static Condition IsOrdinaryMethod() =>
            context => context.MethodSymbol.MethodKind == MethodKind.Ordinary;

        public static Condition IsMainMethod() =>
            context => context.MethodSymbol.IsMainMethod();

        internal static Condition AnyParameterIsOfType(params KnownType[] types)
        {
            var typesArray = types.ToImmutableArray();
            return context =>
                context.MethodSymbol.Parameters.Length > 0
                && context.MethodSymbol.Parameters.Any(parameter => parameter.Type.DerivesOrImplementsAny(typesArray));
        }

        internal static Condition DecoratedWithAnyAttribute(params KnownType[] attributeTypes) =>
            context => context.MethodSymbol.GetAttributes().Any(a => a.AttributeClass.IsAny(attributeTypes));
    }
}
