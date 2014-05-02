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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;

namespace ResultEnvelope {

   /// <summary>
   /// Represents the outcome of an operation, which includes the result value,
   /// status code and optionally other metadata.
   /// </summary>
   [DebuggerDisplay("{StatusCode}")]
#if ResultEnvelope_Public 
   public 
#endif
   class Result {

      readonly HttpStatusCode _StatusCode;
      readonly object _Value;

      /// <summary>
      /// The HTTP status code of the result.
      /// </summary>
      public HttpStatusCode StatusCode { get { return _StatusCode; } }

      /// <summary>
      /// The result value of the operation.
      /// </summary>
      public object Value { get { return _Value; } }

      /// <summary>
      /// A URL for the Location HTTP header.
      /// </summary>
      public string Location { get; set; }

      /// <summary>
      /// A URL for the Content-Location HTTP header.
      /// </summary>
      public string ContentLocation { get; set; }

      /// <summary>
      /// true if the outcome of the operation was not successful; otherwise false.
      /// This is determined by the <see cref="StatusCode"/> property.
      /// </summary>
      public bool IsError {
         get { return (int)StatusCode >= 400; }
      }

      /// <summary>
      /// true if the <see cref="StatusCode"/> is a redirect status (300-399 range).
      /// </summary>
      public bool IsRedirect {
         get { return (int)StatusCode >= 300 && !IsError; }
      }

      /// <summary>
      /// Creates a new <see cref="Result"/> object using the provided status code.
      /// </summary>
      /// <param name="statusCode">The status code of the result.</param>
      /// <returns>
      /// A new <see cref="Result"/> object whose <see cref="StatusCode"/> property
      /// is initialized with the <paramref name="statusCode"/> parameter.
      /// </returns>
      public static implicit operator Result(HttpStatusCode statusCode) {
         return new Result(statusCode);
      }

      /// <summary>
      /// Creates a new <see cref="Result"/> object using the <see cref="HttpStatusCode.BadRequest"/>
      /// status code and the provided value.
      /// </summary>
      /// <param name="value">The result value of the operation.</param>
      /// <returns>
      /// A new <see cref="Result"/> object whose <see cref="StatusCode"/> property
      /// is set to <see cref="HttpStatusCode.BadRequest"/>, and whose <see cref="Value"/> property
      /// is initialized with the <paramref name="value"/> parameter.
      /// </returns>
      public static implicit operator Result(ErrorBuilder value) {
         return new Result(HttpStatusCode.BadRequest, (value != null) ? value.GetErrors() : null);
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="Result"/> class, using the provided status code.
      /// </summary>
      /// <param name="statusCode">The status code of the result.</param>
      public Result(HttpStatusCode statusCode) {
         this._StatusCode = statusCode;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="Result"/> class, using the provided status code
      /// and value.
      /// </summary>
      /// <param name="statusCode">The status code of the result.</param>
      /// <param name="value">The result value.</param>
      public Result(HttpStatusCode statusCode, object value)
         : this(statusCode) {

         this._Value = value;
      }
   }

   /// <summary>
   /// An <see cref="Result"/> with a fixed successful result type.
   /// </summary>
   /// <typeparam name="TSuccess">The result type when the operation outcome is successful.</typeparam>
#if ResultEnvelope_Public 
   public 
#endif
   class Result<TSuccess> : Result {

      /// <summary>
      /// The result value when the operation outcome is successful.
      /// </summary>
      public TSuccess ValueAsSuccess {
         get { return (TSuccess)Value; }
      }

      /// <summary>
      /// Creates a new <see cref="Result&lt;TSuccess>"/> object using the provided status code.
      /// </summary>
      /// <param name="statusCode">The status code of the result.</param>
      /// <returns>
      /// A new <see cref="Result&lt;TSuccess>"/> object whose <see cref="Result.StatusCode"/> property
      /// is initialized with the <paramref name="statusCode"/> parameter.
      /// </returns>
      public static implicit operator Result<TSuccess>(HttpStatusCode statusCode) {
         return new Result<TSuccess>(statusCode);
      }

      /// <summary>
      /// Creates a new <see cref="Result&lt;TSuccess>"/> object using the <see cref="HttpStatusCode.OK"/>
      /// status code and the provided value.
      /// </summary>
      /// <param name="value">The result value of the operation.</param>
      /// <returns>
      /// A new <see cref="Result&lt;TSuccess>"/> object whose <see cref="Result.StatusCode"/> property
      /// is set to <see cref="HttpStatusCode.OK"/>, and whose <see cref="Result.Value"/> property
      /// is initialized with the <paramref name="value"/> parameter.
      /// </returns>
      public static implicit operator Result<TSuccess>(TSuccess value) {
         return new Result<TSuccess>(HttpStatusCode.OK, value);
      }

      /// <summary>
      /// Creates a new <see cref="Result&lt;TSuccess>"/> object using the <see cref="HttpStatusCode.BadRequest"/>
      /// status code and the provided value.
      /// </summary>
      /// <param name="value">The result value of the operation.</param>
      /// <returns>
      /// A new <see cref="Result&lt;TSuccess>"/> object whose <see cref="Result.StatusCode"/> property
      /// is set to <see cref="HttpStatusCode.BadRequest"/>, and whose <see cref="Result.Value"/> property
      /// is initialized with the <paramref name="value"/> parameter.
      /// </returns>
      public static implicit operator Result<TSuccess>(ErrorBuilder value) {
         return new Result<TSuccess>(HttpStatusCode.BadRequest, (value != null) ? value.GetErrors() : null);
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="Result&lt;TSuccess>"/> class, using the
      /// provided status code.
      /// </summary>
      /// <param name="statusCode">The status code of the result.</param>
      public Result(HttpStatusCode statusCode)
         : base(statusCode) { }

      /// <summary>
      /// Initializes a new instance of the <see cref="Result&lt;TSuccess>"/> class, using the
      /// provided status code and value.
      /// </summary>
      /// <param name="statusCode">The status code of the result.</param>
      /// <param name="value">The result value.</param>
      public Result(HttpStatusCode statusCode, object value)
         : base(statusCode, value) { }
   }

   /// <summary>
   /// An <see cref="Result&lt;TSuccess>"/> with a fixed error result type.
   /// </summary>
   /// <typeparam name="TSuccess">The result type when the operation outcome is successful.</typeparam>
   /// <typeparam name="TError">The result type when the operation outcome is not successful.</typeparam>
#if ResultEnvelope_Public 
   public 
#endif
   class Result<TSuccess, TError> : Result<TSuccess> {

      /// <summary>
      /// The result value when the operation outcome is not successful.
      /// </summary>
      public TError ValueAsError {
         get { return (TError)Value; }
      }

      /// <summary>
      /// Creates a new <see cref="Result&lt;TSuccess, TError>"/> object using the provided status code.
      /// </summary>
      /// <param name="statusCode">The status code of the result.</param>
      /// <returns>
      /// A new <see cref="Result&lt;TSuccess, TError>"/> object whose <see cref="Result.StatusCode"/> property
      /// is initialized with the <paramref name="statusCode"/> parameter.
      /// </returns>
      public static implicit operator Result<TSuccess, TError>(HttpStatusCode statusCode) {
         return new Result<TSuccess, TError>(statusCode);
      }

      /// <summary>
      /// Creates a new <see cref="Result&lt;TSuccess, TError>"/> object using the <see cref="HttpStatusCode.OK"/>
      /// status code and the provided value.
      /// </summary>
      /// <param name="value">The result value of the operation.</param>
      /// <returns>
      /// A new <see cref="Result&lt;TSuccess, TError>"/> object whose <see cref="Result.StatusCode"/> property
      /// is set to <see cref="HttpStatusCode.OK"/>, and whose <see cref="Result.Value"/> property
      /// is initialized with the <paramref name="value"/> parameter.
      /// </returns>
      public static implicit operator Result<TSuccess, TError>(TSuccess value) {
         return new Result<TSuccess, TError>(HttpStatusCode.OK, value);
      }

      /// <summary>
      /// Creates a new <see cref="Result&lt;TSuccess, TError>"/> object using the <see cref="HttpStatusCode.BadRequest"/>
      /// status code and the provided value.
      /// </summary>
      /// <param name="value">The result value of the operation.</param>
      /// <returns>
      /// A new <see cref="Result&lt;TSuccess, TError>"/> object whose <see cref="Result.StatusCode"/> property
      /// is set to <see cref="HttpStatusCode.BadRequest"/>, and whose <see cref="Result.Value"/> property
      /// is initialized with the <paramref name="value"/> parameter.
      /// </returns>
      public static implicit operator Result<TSuccess, TError>(TError value) {
         return new Result<TSuccess, TError>(HttpStatusCode.BadRequest, value);
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="Result&lt;TSuccess, TError>"/> class, using the
      /// provided status code.
      /// </summary>
      /// <param name="statusCode">The status code of the result.</param>
      public Result(HttpStatusCode statusCode)
         : base(statusCode) { }

      /// <summary>
      /// Initializes a new instance of the <see cref="Result&lt;TSuccess, TError>"/> class, using the
      /// provided status code and value.
      /// </summary>
      /// <param name="statusCode">The status code of the result.</param>
      /// <param name="value">The result value.</param>
      public Result(HttpStatusCode statusCode, object value)
         : base(statusCode, value) { }
   }

   partial class ErrorBuilder {

      /// <summary>
      /// Adds <see cref="Result.Value"/> from <paramref name="result"/> if
      /// <see cref="Result.IsError"/> on <paramref name="result"/> is true.
      /// </summary>
      /// <param name="result">A result.</param>
      /// <returns>The negated value of <see cref="Result.IsError"/> on <paramref name="result"/>.</returns>
      public bool Assert(Result result) {

         if (result == null) throw new ArgumentNullException("result");

         string message = (result.Value != null) ?
            result.Value.ToString() 
            : null;

         return Assert(!result.IsError, message);
      }

      /// <summary>
      /// Adds <see cref="Result.Value"/> from <paramref name="result"/> if
      /// <see cref="Result.IsError"/> on <paramref name="result"/> is true.
      /// </summary>
      /// <typeparam name="TMember">The type of the tested object.</typeparam>
      /// <param name="result">A result.</param>
      /// <param name="valueSelector">A lambda expression that returns the tested object.</param>
      /// <returns>The negated value of <see cref="Result.IsError"/> on <paramref name="result"/>.</returns>
      public bool Assert<TMember>(Result result, Expression<Func<TMember>> valueSelector) {

         if (result == null) throw new ArgumentNullException("result");

         string message = (result.Value != null) ?
            result.Value.ToString() 
            : null;

         return Assert<TMember>(!result.IsError, message, valueSelector);
      }

      /// <summary>
      /// Adds <see cref="Result.Value"/> from <paramref name="result"/> if
      /// <see cref="Result.IsError"/> on <paramref name="result"/> is true.
      /// </summary>
      /// <param name="result">A result.</param>
      /// <returns>The value of <see cref="Result.IsError"/> on <paramref name="result"/>.</returns>
      public bool Not(Result result) {
         return !Assert(result);
      }

      /// <summary>
      /// Adds <see cref="Result.Value"/> from <paramref name="result"/> if
      /// <see cref="Result.IsError"/> on <paramref name="result"/> is true.
      /// </summary>
      /// <typeparam name="TMember">The type of the tested object.</typeparam>
      /// <param name="result">A result.</param>
      /// <param name="valueSelector">A lambda expression that returns the tested object.</param>
      /// <returns>The value of <see cref="Result.IsError"/> on <paramref name="result"/>.</returns>
      public bool Not<TMember>(Result result, Expression<Func<TMember>> valueSelector) {
         return !Assert<TMember>(result, valueSelector);
      }
   }
}
