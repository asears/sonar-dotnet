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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class StringLiteralShouldNotBeDuplicated : StringLiteralShouldNotBeDuplicatedBase<SyntaxKind, LiteralExpressionSyntax>
    {
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } = CSharpGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration
        };

        public StringLiteralShouldNotBeDuplicated() : base(RspecStrings.ResourceManager) { }

        protected override bool IsMatchingMethodParameterName(LiteralExpressionSyntax literalExpression) =>
            literalExpression.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>()
                ?.ParameterList
                ?.Parameters
                .Any(p => p.Identifier.ValueText == literalExpression.Token.ValueText)
                ?? false;

        protected override bool IsInnerInstance(SyntaxNodeAnalysisContext context) =>
            context.Node.Ancestors().Any(x=> x is ClassDeclarationSyntax || x is StructDeclarationSyntax);

        protected override IEnumerable<LiteralExpressionSyntax> RetrieveLiteralExpressions(SyntaxNode node) =>
            node.DescendantNodes(n => !n.IsKind(SyntaxKind.AttributeList))
                .Where(les => les.IsKind(SyntaxKind.StringLiteralExpression))
                .Cast<LiteralExpressionSyntax>();

        protected override string GetLiteralValue(LiteralExpressionSyntax literal) =>
            literal.Token.Text;
    }
}
