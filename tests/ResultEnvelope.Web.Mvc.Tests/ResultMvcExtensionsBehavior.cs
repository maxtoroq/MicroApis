using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ResultEnvelope.Web.Mvc.Tests {
   
   [TestClass]
   public class ResultMvcExtensionsBehavior {

      [TestMethod]
      public void Add_Errors_From_List_To_ModelState() {

         var errors = new ErrorBuilder()
            .Add("a", null, "a")
            .Add("b", null, "b")
            .GetErrors();

         var result = new Result(HttpStatusCode.BadRequest, errors);

         var modelState = new ModelStateDictionary();
         modelState.AddModelErrors(result);

         Assert.AreEqual(errors.Count, modelState.Sum(m => m.Value.Errors.Count));
      }

      [TestMethod]
      public void Add_Errors_From_Builder_To_ModelState() {

         var errors = new ErrorBuilder()
            .Add("a", null, "a")
            .Add("b", null, "b");

         var result = new Result(HttpStatusCode.BadRequest, errors);

         var modelState = new ModelStateDictionary();
         modelState.AddModelErrors(result);

         Assert.AreEqual(errors.Count, modelState.Sum(m => m.Value.Errors.Count));
      }

      [TestMethod]
      public void Add_Value_String() {

         var result = new Result(HttpStatusCode.BadRequest, "foo");

         var modelState = new ModelStateDictionary();
         modelState.AddModelErrors(result);

         Assert.AreEqual(1, modelState.Count);
         Assert.IsNotNull(modelState[""]);
         Assert.AreEqual(result.Value.ToString(), modelState[""].Errors[0].ErrorMessage);
      }

      [TestMethod]
      public void Add_Even_If_Not_Error_Result() {

         var modelState = new ModelStateDictionary();

         var result = new Result(HttpStatusCode.OK, "foo");
         modelState.AddModelErrors(result);

         Assert.AreEqual(1, modelState.Count);

         modelState.Clear();

         var errors = new ErrorBuilder()
            .Add("a", null, "a")
            .Add("b", null, "b")
            .GetErrors();

         result = new Result(HttpStatusCode.OK, errors);
         modelState.AddModelErrors(result);

         Assert.AreEqual(errors.Count, modelState.Sum(m => m.Value.Errors.Count));
      }

      [TestMethod]
      public void Dont_Add_Message_Twice_When_Taken_From_Errors_Collection() {

         var errors = new ErrorBuilder()
            .Add("a");

         var modelState = new ModelStateDictionary();
         modelState.AddModelErrors((Result)errors);

         Assert.AreEqual(1, modelState.Sum(m => m.Value.Errors.Count));
      }
   }
}
