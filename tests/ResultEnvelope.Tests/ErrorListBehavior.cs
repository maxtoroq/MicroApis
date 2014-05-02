using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ResultEnvelope.Tests {
   
   [TestClass]
   public class ErrorListBehavior {

      [TestMethod]
      public void ToString_With_Message() {

         var errors = new ErrorBuilder()
            .Add("a")
            .GetErrors();

         Assert.AreEqual("a", errors.ToString());
      }

      [TestMethod]
      public void ToString_With_Empty_String_Member() {

         var errors = new ErrorBuilder()
            .Add("a", null, "x")
            .Add("b", null, "")
            .GetErrors();

         Assert.AreEqual("b", errors.ToString());
      }

      [TestMethod]
      public void ToString_With_Empty_Members() {

         var errors = new ErrorBuilder()
            .Add("a", null, "x")
            .Add("b")
            .GetErrors();

         Assert.AreEqual("b", errors.ToString());
      }
   }
}
