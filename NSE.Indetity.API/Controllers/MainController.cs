using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NSE.Indetity.API.Controllers
{
    [ApiController]
    public abstract class MainController : Controller
    {
        public ICollection<string> Erros = new List<string>();
        protected ActionResult CustomResponse(object result = null)
        {
            if (OperationValid())
            {
                return Ok(result);
            }

            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]> 
            {
                { "Mensagens", Erros.ToArray() } 
            }));
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            var errors = modelState.Values.SelectMany(e => e.Errors);

            foreach (var error in errors)
            {
                AddErrorsProcess(error.ErrorMessage);
            }

            return CustomResponse();
        } // validação da view model

        protected bool OperationValid()
        {
            return !Erros.Any();
        }

        protected void AddErrorsProcess(string erro)
        {
            Erros.Add(erro);
        }

        protected void ClearErrorsProcess()
        {
            Erros.Clear();
        }
    }
}
