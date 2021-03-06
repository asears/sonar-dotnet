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

using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ControllingPermissionsBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4834";
        protected const string MessageFormat = "Make sure controlling this permission is safe here.";

        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }

        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }

        protected PropertyAccessTracker<TSyntaxKind> PropertyAccessTracker { get; set; }

        protected MethodDeclarationTracker MethodDeclarationTracker { get; set; }

        protected BaseTypeTracker<TSyntaxKind> BaseTypeTracker { get; set; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(
                    KnownType.System_Security_Permissions_PrincipalPermission));

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.WhenDerivesOrImplementsAny(
                    KnownType.System_Security_Principal_IIdentity,
                    KnownType.System_Security_Principal_IPrincipal));

            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(
                    new MemberDescriptor(KnownType.System_Security_Principal_WindowsIdentity, "GetCurrent"),
                    new MemberDescriptor(KnownType.System_IdentityModel_Tokens_SecurityTokenHandler, "ValidateToken"),
                    new MemberDescriptor(KnownType.System_AppDomain, "SetPrincipalPolicy"),
                    new MemberDescriptor(KnownType.System_AppDomain, "SetThreadPrincipal")));

            PropertyAccessTracker.Track(context,
                PropertyAccessTracker.MatchProperty(
                    new MemberDescriptor(KnownType.System_Web_HttpContext, "User"),
                    new MemberDescriptor(KnownType.System_Threading_Thread, "CurrentPrincipal")));

            MethodDeclarationTracker.Track(context,
                MethodDeclarationTracker.AnyParameterIsOfType(
                    KnownType.System_Security_Principal_IIdentity,
                    KnownType.System_Security_Principal_IPrincipal),
                MethodDeclarationTracker.IsOrdinaryMethod());

            MethodDeclarationTracker.Track(context,
                MethodDeclarationTracker.DecoratedWithAnyAttribute(
                    KnownType.System_Security_Permissions_PrincipalPermissionAttribute));

            BaseTypeTracker.Track(context,
                BaseTypeTracker.MatchSubclassesOf(
                    KnownType.System_Security_Principal_IIdentity,
                    KnownType.System_Security_Principal_IPrincipal));
        }
    }
}
