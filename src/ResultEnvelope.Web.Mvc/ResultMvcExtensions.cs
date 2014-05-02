// Copyright 2012 Max Toro Q.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ResultEnvelope.Web.Mvc {
   using ErrCollection = ErrorBuilder.ErrCollection;

   /// <summary>
   /// Defines extension methods that integrate <see cref="N:ResultEnvelope"/> with ASP.NET MVC.
   /// </summary>
#if ResultEnvelope_Public 
   public 
#endif
   static class ResultMvcExtensions {

      /// <summary>
      /// Adds the errors from the <paramref name="result"/> parameter to <paramref name="modelState"/>.
      /// </summary>
      /// <param name="modelState">The <see cref="ModelStateDictionary"/> instance.</param>
      /// <param name="result">The <see cref="Result"/> that contains error information.</param>
      public static void AddModelErrors(this ModelStateDictionary modelState, Result result) {

         if (modelState == null) throw new ArgumentNullException("modelState");
         if (result == null) throw new ArgumentNullException("result");

         object value = result.Value;

         if (value != null) {

            ErrCollection errors = value as ErrCollection;

            if (errors == null) {

               ErrorBuilder builder = value as ErrorBuilder;

               if (builder != null) {
                  errors = (ErrCollection)builder.GetErrors();
               }
            }

            if (errors != null) {
               AddModelErrors(modelState, errors);
            } else {
               modelState.AddModelError("", value.ToString());
            }
         }
      }

      static void AddModelErrors(ModelStateDictionary modelState, ErrCollection errors) {

         string message = errors.ToString();

         if (String.IsNullOrEmpty(message)) {
            message = null;
         }

         if (message != null) {
            modelState.AddModelError("", message);
         }

         foreach (var item in errors) {

            IList<string> memberNames = item.MemberNames.ToList();
            string itemMessage = item.ErrorMessage;

            if (memberNames.Count > 0) {

               for (int i = 0; i < memberNames.Count; i++) {
                  modelState.AddModelError(memberNames[i] ?? "", itemMessage);
               }

            } else {

               if (message != null
                  && itemMessage == message) {

                  continue;
               }

               modelState.AddModelError("", itemMessage);
            }
         }
      }

      /// <summary>
      /// Adds errors from <paramref name="result"/> to <see cref="ViewDataDictionary.ModelState"/> in
      /// <paramref name="viewResult"/> and sets status and header information to the current HTTP response.
      /// </summary>
      /// <param name="viewResult">The <see cref="ViewResultBase"/> instance.</param>
      /// <param name="result">The <see cref="Result"/> instance.</param>
      /// <returns>
      /// A new <see cref="ActionResult"/> object that wraps <paramref name="viewResult"/> and adds status and header
      /// information to the current HTTP response, when executed.
      /// </returns>
      /// <example>
      /// This extension method is usually used like this:
      /// <code>
      /// if (result.IsError) {
      ///     return View().WithErrors(result);
      /// }
      /// </code>
      /// </example>
      public static ActionResult WithErrors(this ViewResultBase viewResult, Result result) {

         if (viewResult == null) throw new ArgumentNullException("viewResult");

         AddModelErrors(viewResult.ViewData.ModelState, result);

         return new ActionResultWrapper(viewResult, result);
      }

      /// <summary>
      /// Sets status information to the current HTTP response.
      /// </summary>
      /// <param name="actionResult">The <see cref="ActionResult"/> instance.</param>
      /// <param name="statusCode">The status code.</param>
      /// <returns>
      /// A new <see cref="ActionResult"/> object that wraps <paramref name="actionResult"/> and adds status
      /// information to the current HTTP response, when executed.
      /// </returns>
      public static ActionResult WithStatus(this ActionResult actionResult, HttpStatusCode statusCode) {
         return new ActionResultWrapper(actionResult, statusCode);
      }

      /// <summary>
      /// Sets status and header information to the current HTTP response.
      /// </summary>
      /// <param name="actionResult">The <see cref="ActionResult"/> instance.</param>
      /// <param name="result">The <see cref="Result"/> instance.</param>
      /// <returns>
      /// A new <see cref="ActionResult"/> object that wraps <paramref name="actionResult"/> and adds status and header
      /// information to the current HTTP response, when executed.
      /// </returns>
      public static ActionResult WithStatus(this ActionResult actionResult, Result result) {
         return new ActionResultWrapper(actionResult, result);
      }
   }

   class ActionResultWrapper : ActionResult {

      readonly ActionResult originalResult;
      readonly Result operationResult;
      readonly HttpStatusCode statusCode;

      public ActionResultWrapper(ActionResult originalResult, Result operationResult)
         : this(originalResult) {

         this.operationResult = operationResult;
      }

      public ActionResultWrapper(ActionResult originalResult, HttpStatusCode statusCode)
         : this(originalResult) {

         this.statusCode = statusCode;
      }

      private ActionResultWrapper(ActionResult originalResult) {

         if (originalResult == null) throw new ArgumentNullException("originalResult");

         this.originalResult = originalResult;
      }

      public override void ExecuteResult(ControllerContext context) {

         HttpResponseBase response = context.HttpContext.Response;

         if (this.operationResult != null) {
            SetStatusAndHeaders(response, this.operationResult);

         } else {
            response.StatusCode = (int)this.statusCode;
         }

         this.originalResult.ExecuteResult(context);
      }

      static void SetStatusAndHeaders(HttpResponseBase response, Result result) {

         response.StatusCode = (int)result.StatusCode;

         if (!String.IsNullOrEmpty(result.Location)) {
            response.RedirectLocation = result.Location;
         }

         if (!String.IsNullOrEmpty(result.ContentLocation)) {
            response.AddHeader("Content-Location", result.ContentLocation);
         }
      }
   }
}
