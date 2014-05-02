using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ResultEnvelope.Tests {
   
   [TestClass]
   public class ErrorBuilderBehavior {

      [TestMethod]
      public void Adds_Message() {

         var errors = new ErrorBuilder();
         errors.Add("a");

         Assert.AreEqual("a", errors.GetErrors()[0].ErrorMessage);
      }

      [TestMethod]
      public void Adds_And_Formats_Message() {

         var errors = new ErrorBuilder();
         errors.Add("a {0}", "b");
         errors.Add("a {0} {1}", "b", "c");

         var err = errors.GetErrors();

         Assert.AreEqual("a b", err[0].ErrorMessage);
         Assert.AreEqual("a b c", err[1].ErrorMessage);
      }

      [TestMethod]
      public void Adds_And_Formats_Message_Using_Lambda_Expression() {

         string a = "ddf";

         var errors = new ErrorBuilder();
         errors.Add("{1} = {0}", () => a.Length);

         var err = errors.GetErrors();

         Assert.AreEqual("Length = 3", err[0].ErrorMessage);
         Assert.AreEqual("Length", err[0].MemberNames.First());

         errors.Clear();
         errors.IncludeValueSelectorFirstKeySegment = true;
         
         errors.Add("{1} = {0}", () => a.Length);

         err = errors.GetErrors();

         Assert.AreEqual("Length = 3", err[0].ErrorMessage);
         Assert.AreEqual("a.Length", err[0].MemberNames.First());
      }

      [TestMethod]
      public void Formats_Message_With_DisplayName() {

         var obj = new TestModel();

         var errors = new ErrorBuilder();
         errors.Add("Bad {1}", () => obj.SomeProperty);

         var err = errors.GetErrors();

         Assert.AreEqual("Bad Some Property", err[0].ErrorMessage);
      }

      [TestMethod]
      public void Null_Key_Is_Not_Added_To_Member_Names() {

         var errors = new ErrorBuilder();
         errors.Add("a", null, null);

         var err = errors.GetErrors();

         Assert.AreEqual(0, err[0].MemberNames.Count());
      }

      [TestMethod]
      public void GetErrors_Always_Returns_New_Instance() {

         var errors = new ErrorBuilder();

         var err1 = errors.GetErrors();

         Assert.IsNotNull(err1);

         var err2 = errors.GetErrors();

         Assert.IsNotNull(err2);

         Assert.IsFalse(Object.ReferenceEquals(err1, err2));
      }

      [TestMethod]
      public void ToString_With_Message() {

         var errors = new ErrorBuilder()
            .Add("a");

         Assert.AreEqual("a", errors.ToString());
      }

      [TestMethod]
      public void ToString_With_Empty_String_Member() {

         var errors = new ErrorBuilder()
            .Add("a", null, "x")
            .Add("b", null, "");

         Assert.AreEqual("b", errors.ToString());
      }

      [TestMethod]
      public void ToString_With_Empty_Members() {

         var errors = new ErrorBuilder()
            .Add("a", null, "x")
            .Add("b");

         Assert.AreEqual("b", errors.ToString());
      }

      class TestModel {

         [Display(Name = "Some Property")]
         public int SomeProperty { get; set; }
      }
   }
}
