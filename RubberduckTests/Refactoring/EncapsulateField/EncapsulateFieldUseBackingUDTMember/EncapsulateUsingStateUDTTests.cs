﻿using NUnit.Framework;
using Rubberduck.Parsing.Rewriter;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Parsing.VBA;
using Rubberduck.Refactorings;
using Rubberduck.Refactorings.EncapsulateField;
using Rubberduck.VBEditor.SafeComWrappers;
using Rubberduck.VBEditor.Utility;
using RubberduckTests.Mocks;
using System;
using System.Linq;

namespace RubberduckTests.Refactoring.EncapsulateField
{
    [TestFixture]
    public class EncapsulateUsingStateUDTTests : EncapsulateFieldInteractiveRefactoringTest
    {
        private EncapsulateFieldTestSupport Support { get; } = new EncapsulateFieldTestSupport();

        [Test]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void EncapsulatePrivateFieldAsUDT()
        {
            var inputCode =
                @"|Private fizz As Integer";

            var presenterAction = Support.SetParametersForSingleTarget("fizz", "Name", asUDT: true);
            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);

            StringAssert.Contains("Name As Integer", actualCode);
            StringAssert.Contains($"this.Name = {Support.RHSIdentifier}", actualCode);
        }

        [TestCase("Public")]
        [TestCase("Private")]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void UserDefinedType_UserAcceptsDefaults_ConflictWithStateUDT(string accessibility)
        {
            var inputCode =
$@"
Private Type TBar
    First As String
    Second As Long
End Type

{accessibility} my|Bar As TBar

Private this As Long";

            var presenterAction = Support.UserAcceptsDefaults(convertFieldToUDTMember: true);
            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);
            StringAssert.Contains("Private this As Long", actualCode);
            StringAssert.Contains($"Private this1 As {Support.StateUDTDefaultTypeName}", actualCode);
        }

        [Test]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void UserDefinedTypeMembers_OnlyEncapsulateUDTMembers()
        {
            var inputCode =
$@"
Private Type TBar
    First As String
    Second As Long
End Type

Private my|Bar As TBar";

            var userInput = new UserInputDataObject()
                .UserSelectsField("myBar");

            userInput.EncapsulateUsingUDTField();

            var presenterAction = Support.SetParameters(userInput);

            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);
            StringAssert.Contains($"this.MyBar.First = {Support.RHSIdentifier}", actualCode);
            StringAssert.Contains($"First = this.MyBar.First", actualCode);
            StringAssert.Contains($"this.MyBar.Second = {Support.RHSIdentifier}", actualCode);
            StringAssert.Contains($"Second = this.MyBar.Second", actualCode);
            StringAssert.Contains($"MyBar As TBar", actualCode);
            StringAssert.Contains($"MyBar As TBar", actualCode);
        }

        [Test]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void LoadsExistingUDT()
        {
            var inputCode =
$@"
Private Type TBar
    First As String
    Second As Long
End Type

Private my|Bar As TBar

Public foo As Long
Public bar As String
Public foobar As Byte
";

            var userInput = new UserInputDataObject()
                .UserSelectsField("foo")
                .UserSelectsField("bar")
                .UserSelectsField("foobar");

            userInput.EncapsulateUsingUDTField("myBar");

            var presenterAction = Support.SetParameters(userInput);
            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);
            StringAssert.DoesNotContain($"Private this As {Support.StateUDTDefaultTypeName}", actualCode);
            StringAssert.Contains("Foo As Long", actualCode);
            StringAssert.DoesNotContain("Public foo As Long", actualCode);
            StringAssert.Contains("Bar As String", actualCode);
            StringAssert.DoesNotContain("Public bar As Long", actualCode);
            StringAssert.Contains("Foobar As Byte", actualCode);
            StringAssert.DoesNotContain("Public foobar As Long", actualCode);
            StringAssert.DoesNotContain("MyBar As TBar", actualCode);
            StringAssert.DoesNotContain("Private this As TBar", actualCode);
            StringAssert.Contains("First As String", actualCode);
            StringAssert.Contains("Second As Long", actualCode);
        }

        [Test]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void DoesNotChangeExistingUDTMembers()
        {
            var inputCode =
$@"
Private Type T{MockVbeBuilder.TestModuleName}
    Name As String
End Type

Private this As T{MockVbeBuilder.TestModuleName}

Public foo As Long
Public bar As String
Public foo|bar As Byte

Public Property Let Name(value As String)
    this.Name = value
End Property

Public Property Get Name() As String
    Name = this.Name
End Property
";

            var userInput = new UserInputDataObject()
                .UserSelectsField("foobar");

            userInput.EncapsulateUsingUDTField($"T{MockVbeBuilder.TestModuleName}");

            var presenterAction = Support.SetParameters(userInput);
            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);
            StringAssert.DoesNotContain($"Name_1 As String", actualCode);
            StringAssert.DoesNotContain($"ThisName As String", actualCode);
        }

        [Test]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void MultipleFields()
        {
            var inputCode =
$@"
Public fo|o As Long
Public bar As String
Public foobar As Byte
";

            var userInput = new UserInputDataObject()
                .UserSelectsField("foo")
                .UserSelectsField("bar")
                .UserSelectsField("foobar");

            userInput.EncapsulateUsingUDTField();

            var presenterAction = Support.SetParameters(userInput);
            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);
            StringAssert.Contains($"Private this As {Support.StateUDTDefaultTypeName}", actualCode);
            StringAssert.Contains($"Private Type {Support.StateUDTDefaultTypeName}", actualCode);
            StringAssert.Contains("Foo As Long", actualCode);
            StringAssert.Contains("Bar As String", actualCode);
            StringAssert.Contains("Foobar As Byte", actualCode);
        }

        [Test]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void UserDefinedType_MultipleFieldsWithUDT()
        {
            var inputCode =
$@"

Private Type TBar
    First As Long
    Second As String
End Type

Public fo|o As Long
Public myBar As TBar
";

            var userInput = new UserInputDataObject()
                .UserSelectsField("myBar");

            userInput.EncapsulateUsingUDTField();

            var presenterAction = Support.SetParameters(userInput);

            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);
            StringAssert.Contains($"this.MyBar.First = {Support.RHSIdentifier}", actualCode);
            StringAssert.Contains("First = this.MyBar.First", actualCode);
            StringAssert.Contains($"this.MyBar.Second = {Support.RHSIdentifier}", actualCode);
            StringAssert.Contains("Second = this.MyBar.Second", actualCode);
            var index = actualCode.IndexOf("Get Second", StringComparison.InvariantCultureIgnoreCase);
            var indexLast = actualCode.LastIndexOf("Get Second", StringComparison.InvariantCultureIgnoreCase);
            Assert.AreEqual(index, indexLast);
        }

        [Test]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void UserDefinedType_MultipleFieldsOfSameUDT()
        {
            var inputCode =
$@"

Private Type TBar
    First As Long
    Second As String
End Type

Public fooBar As TBar
Public myBar As TBar
";

            var userInput = new UserInputDataObject()
                .UserSelectsField("myBar");

            userInput.EncapsulateUsingUDTField();

            var presenterAction = Support.SetParameters(userInput);
            var model = Support.RetrieveUserModifiedModelPriorToRefactoring(inputCode, "myBar", DeclarationType.Variable, presenterAction);

            Assert.AreEqual(1, model.ObjectStateUDTCandidates.Count());
        }

        [Test]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void UserDefinedType_PrivateEnumField()
        {
            var inputCode =
@"
Private Enum NumberTypes 
    Whole = -1 
    Integral = 0 
    Rational = 1 
End Enum

Public numberT|ype As NumberTypes
";

            var userInput = new UserInputDataObject()
                .UserSelectsField("numberType");

            userInput.EncapsulateUsingUDTField();

            var presenterAction = Support.SetParameters(userInput);
            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);
            StringAssert.Contains("Property Get NumberType() As Long", actualCode);
            StringAssert.Contains("NumberType = this.NumberType", actualCode);
            StringAssert.Contains(" NumberType As NumberTypes", actualCode);
        }

        [TestCase("anArray", "5")]
        [TestCase("anArray", "1 To 100")]
        [TestCase("anArray", "")]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void UserDefinedType_BoundedArrayField(string arrayIdentifier, string dimensions)
        {
            var selectedInput = arrayIdentifier.Replace("n", "n|");
            var inputCode =
$@"
Public {selectedInput}({dimensions}) As String
";

            var userInput = new UserInputDataObject()
                .UserSelectsField(arrayIdentifier);

            userInput.EncapsulateUsingUDTField();

            var presenterAction = Support.SetParameters(userInput);
            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);
            StringAssert.Contains("Property Get AnArray() As Variant", actualCode);
            StringAssert.Contains("AnArray = this.AnArray", actualCode);
            StringAssert.Contains($" AnArray({dimensions}) As String", actualCode);
        }

        [Test]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void UserDefinedTypeDefaultNameHasConflict()
        {
            var expectedIdentifier = "TTestModule2";
            var inputCode =
$@"

Private Type TBar
    First As Long
    Second As String
End Type

Private Type TTestModule1
    Bar As Long
End Type

Public fo|o As Long
Public myBar As TBar
";

            var userInput = new UserInputDataObject()
                .UserSelectsField("foo")
                .UserSelectsField("myBar");

            userInput.EncapsulateUsingUDTField();

            var presenterAction = Support.SetParameters(userInput);
            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);
            StringAssert.Contains($"Private Type {expectedIdentifier}", actualCode);
        }

        [TestCase("Public", 1)]
        [TestCase("Private", 2)]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void ObjectStateUDTs(string udtFieldAccessibility, int expectedCount)
        {
            var inputCode =
$@"
Private Type TBar
    First As String
    Second As Long
End Type

Public mFoo As String
Public mBar As Long
Private mFizz

{udtFieldAccessibility} myBar As TBar";

            var userInput = new UserInputDataObject()
                .UserSelectsField("mFizz");

            userInput.EncapsulateUsingUDTField();

            var presenterAction = Support.SetParameters(userInput);

            var model = Support.RetrieveUserModifiedModelPriorToRefactoring(inputCode, "mFizz", DeclarationType.Variable, presenterAction);
            var test = model.ObjectStateUDTCandidates;

            Assert.AreEqual(expectedCount, model.ObjectStateUDTCandidates.Count());
        }

        [Test]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void UDTMemberIsPrivateUDT()
        {
            var inputCode =
$@"

Private Type TFoo
    Foo As Integer
    Bar As Byte
End Type

Private Type TBar
    FooBar As TFoo
End Type

Private my|Bar As TBar
";

            var userInput = new UserInputDataObject();

            userInput.EncapsulateUsingUDTField();

            var presenterAction = Support.SetParameters(userInput);

            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);

            StringAssert.Contains("Public Property Let Foo(", actualCode);
            StringAssert.Contains("Public Property Let Bar(", actualCode);
            StringAssert.Contains($"this.MyBar.FooBar.Foo = {Support.RHSIdentifier}", actualCode);
        }

        [Test]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void UDTMemberIsPublicUDT()
        {
            var inputCode =
$@"

Public Type TFoo
    Foo As Integer
    Bar As Byte
End Type

Private Type TBar
    FooBar As TFoo
End Type

Private my|Bar As TBar
";

            var userInput = new UserInputDataObject();

            userInput.EncapsulateUsingUDTField();

            var presenterAction = Support.SetParameters(userInput);

            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);
            StringAssert.Contains("Public Property Let FooBar(", actualCode);
            StringAssert.Contains($"this.MyBar.FooBar = {Support.RHSIdentifier}", actualCode);
        }

        [Test]
        [Category("Refactorings")]
        [Category("Encapsulate Field")]
        public void PrivateUDT_SelectedOtherThanObjectStateUDT()
        {
            var inputCode =
$@"

Private Type TTest
    TestValue As String
    TestNumber As Long
End Type

Private Type TTestModule1
    SomeValue As Long
End Type

Private mTest As TTest

Private this As TTestModule1

Private the|Target As Variant

Public Property Get SomeValue() As Long
    SomeValue = this.SomeValue
End Property

Public Property Let SomeValue(ByVal value As Long)
    this.SomeValue = value
End Property

Public Sub Foo(arg As Long)
    SomeValue = arg * 4
End Sub
";

            var userInput = new UserInputDataObject()
                .UserSelectsField("theTarget");

            userInput.EncapsulateUsingUDTField("mTest");

            var presenterAction = Support.SetParameters(userInput);

            var actualCode = Support.RefactoredCode(inputCode.ToCodeString(), presenterAction);

            StringAssert.DoesNotContain("TheTarget = this.TheTarget", actualCode);
            StringAssert.Contains("TheTarget = mTest.TheTarget", actualCode);
            StringAssert.Contains("TheTarget As Variant", actualCode);
        }

        protected override IRefactoring TestRefactoring(
            IRewritingManager rewritingManager,
            RubberduckParserState state,
            RefactoringUserInteraction<IEncapsulateFieldPresenter, EncapsulateFieldModel> userInteraction,
            ISelectionService selectionService)
        {
            return Support.SupportTestRefactoring(rewritingManager, state, userInteraction, selectionService);
        }
    }
}
