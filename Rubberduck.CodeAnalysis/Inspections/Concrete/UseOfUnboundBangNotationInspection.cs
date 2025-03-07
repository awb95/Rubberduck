﻿using Rubberduck.CodeAnalysis.Inspections.Abstract;
using Rubberduck.Parsing.Grammar;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Parsing.VBA;
using Rubberduck.Parsing.VBA.DeclarationCaching;
using Rubberduck.Resources.Inspections;
using Rubberduck.VBEditor;
using System.Collections.Generic;
using System.Globalization;

namespace Rubberduck.CodeAnalysis.Inspections.Concrete
{
    /// <summary>
    /// Identifies the use of bang notation, formally known as dictionary access expression, for which the default member is not known at compile time.
    /// </summary>
    /// <why>
    /// A dictionary access expression looks like a strongly typed call, but it actually is a stringly typed access to the parameterized default member of the object.
    /// This is especially misleading the default member cannot be determined at compile time.  
    /// </why>
    /// <example hasresult="true">
    /// <module name="MyModule" type="Standard Module">
    /// <![CDATA[
    /// Public Function MyName(ByVal rst As Object) As Variant
    ///     MyName = rst!Name.Value
    /// End Function
    /// ]]>
    /// </module>
    /// </example>
    /// <example hasresult="true">
    /// <module name="MyModule" type="Standard Module">
    /// <![CDATA[
    /// Public Function MyName(ByVal rst As Variant) As Variant
    ///     With rst
    ///         MyName = !Name.Value
    ///     End With
    /// End Function
    /// ]]>
    /// </module>
    /// </example>
    /// <example hasresult="false">
    /// <module name="MyModule" type="Standard Module">
    /// <![CDATA[
    /// Public Function MyName(ByVal rst As ADODB.Recordset) As Variant
    ///     MyName = rst!Name.Value
    /// End Function
    /// ]]>
    /// </module>
    /// </example>
    /// <example hasresult="false">
    /// <module name="MyModule" type="Standard Module">
    /// <![CDATA[
    /// Public Function MyName(ByVal rst As Object) As Variant
    ///     MyName = rst("Name").Value
    /// End Function
    /// ]]>
    /// </module>
    /// </example>
    /// <example hasresult="false">
    /// <module name="MyModule" type="Standard Module">
    /// <![CDATA[
    /// Public Function MyName(ByVal rst As Variant) As Variant
    ///     With rst
    ///         MyName = .Fields.Item("Name").Value
    ///     End With
    /// End Function
    /// ]]>
    /// </module>
    /// </example>
    internal sealed class UseOfUnboundBangNotationInspection : IdentifierReferenceInspectionBase
    {
        public UseOfUnboundBangNotationInspection(IDeclarationFinderProvider declarationFinderProvider)
            : base(declarationFinderProvider)
        {
            Severity = CodeInspectionSeverity.Warning;
        }

        protected override IEnumerable<IdentifierReference> ReferencesInModule(QualifiedModuleName module, DeclarationFinder finder)
        {
            return finder.UnboundDefaultMemberAccesses(module);
        }

        protected override bool IsResultReference(IdentifierReference reference, DeclarationFinder finder)
        {
            return reference.IsIndexedDefaultMemberAccess
                   && reference.Context is VBAParser.DictionaryAccessContext;
        }

        protected override string ResultDescription(IdentifierReference reference)
        {
            var expression = reference.IdentifierName;
            return string.Format(InspectionResults.ResourceManager.GetString(nameof(UseOfRecursiveBangNotationInspection), CultureInfo.CurrentUICulture), expression);
        }
    }
}