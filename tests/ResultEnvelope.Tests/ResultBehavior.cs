using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ResultEnvelope.Tests {
   
   [TestClass]
   public class ResultBehavior {

      [TestMethod]
      public void Adds_Message_To_ErrorBuilder() {

         var errors = new ErrorBuilder();

         var result = new Result(HttpStatusCode.BadRequest, "a");

         Assert.AreEqual(!errors.Assert(result), result.IsError);
         Assert.AreEqual(errors.GetErrors().ToString(), result.Value.ToString());
      }
   }
}
