﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp
{
    /// <summary>
    /// The class that represents a translated iterator method.
    /// </summary>
    internal sealed class IteratorStateMachine : StateMachineTypeSymbol
    {
        private readonly MethodSymbol constructor;
        private readonly ImmutableArray<NamedTypeSymbol> interfaces;

        internal readonly TypeSymbol ElementType;

        public IteratorStateMachine(VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, MethodSymbol iteratorMethod, int iteratorMethodOrdinal, bool isEnumerable, TypeSymbol elementType)
            : base(slotAllocatorOpt, compilationState, iteratorMethod, iteratorMethodOrdinal)
        {
            this.ElementType = TypeMap.SubstituteType(elementType);

            var interfaces = ArrayBuilder<NamedTypeSymbol>.GetInstance();
            if (isEnumerable)
            {
                interfaces.Add(ContainingAssembly.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T).Construct(ElementType));
                interfaces.Add(ContainingAssembly.GetSpecialType(SpecialType.System_Collections_IEnumerable));
            }

            interfaces.Add(ContainingAssembly.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerator_T).Construct(ElementType));
            interfaces.Add(ContainingAssembly.GetSpecialType(SpecialType.System_IDisposable));
            interfaces.Add(ContainingAssembly.GetSpecialType(SpecialType.System_Collections_IEnumerator));
            this.interfaces = interfaces.ToImmutableAndFree();

            this.constructor = new IteratorConstructor(this);
        }

        public override TypeKind TypeKind
        {
            get { return TypeKind.Class; }
        }

        internal override MethodSymbol Constructor
        {
            get { return constructor; }
        }

        internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved)
        {
            return interfaces;
        }

        internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics
        {
            get { return ContainingAssembly.GetSpecialType(SpecialType.System_Object); }
        }
    }
}
