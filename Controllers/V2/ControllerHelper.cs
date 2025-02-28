using Microsoft.AspNetCore.Mvc;
namespace WebApi.Controllers.V2;

public class ControllerHelper: ControllerBase {

   // 400 Bad Request
   public ActionResult<T> DetailsBadRequest<T>(string detail) {
      return BadRequest(new ProblemDetails {
         Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
         Status = StatusCodes.Status400BadRequest,
         Title = "Bad request",
         Detail = detail
      });
   }

   // 401 Unauthorized
   public ActionResult<T> DetailsUnauthorized<T>(string detail) {
      return Unauthorized(new ProblemDetails {
         Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
         Status = StatusCodes.Status401Unauthorized,
         Title = "Unauthorized",
         Detail = detail
      });
   }
   
   // 403 Forbidden
   public ActionResult<T> DetailsForbidden<T>(string detail) {
      var problemDetails = new ProblemDetails {
         Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
         Status = StatusCodes.Status403Forbidden,
         Title = "Forbidden",
         Detail = detail
      };
      Response.StatusCode = StatusCodes.Status403Forbidden;
      return new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status403Forbidden };
   }
   
   // 404 Not Found
   public ActionResult<T> DetailsNotFound<T>(string detail) {
      return BadRequest(new ProblemDetails {
         Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
         Status = StatusCodes.Status404NotFound,
         Title = "Not found",
         Detail = detail
      });
   }
   
   // 405 Method Not Allowed
   public ActionResult<T> DetailsMethodNotAllowed<T>(string detail) {
      return BadRequest(new ProblemDetails {
         Type = "https://tools.ietf.org/html/rfc7231#section-6.5.5",
         Status = StatusCodes.Status405MethodNotAllowed,
         Title = "Method not allowed",
         Detail = detail
      });
   }
   
   // 406 Not Acceptable
   public ActionResult<T> DetailsNotAcceptable<T>(string detail) {
      return BadRequest(new ProblemDetails {
         Type = "https://tools.ietf.org/html/rfc7231#section-6.5.6",
         Status = StatusCodes.Status406NotAcceptable,
         Title = "Not acceptable",
         Detail = detail
      });
   }
   
   // 409 Conflict
   public ActionResult<T> DetailsConflict<T>(string detail) {
      return Conflict(new ProblemDetails {
         Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
         Status = StatusCodes.Status409Conflict,
         Title = "Conflict",
         Detail = detail
      });
   }
   
}