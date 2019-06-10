using Project1.Models.Helpers;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Project1.Filters
{
    public class StaticFilesVersionAttribute : ActionFilterAttribute
    {
        private HtmlTextWriter _textWriter;
        private StringWriter _stringWriter;
        private StringBuilder _stringBuilder;
        private HttpWriter _httpWriter;
        private string[] excludedScriptsForPostfix = new string[]
        {
            "modernizr"
        };
        private string[] excludedCssPostfix = new string[]
        {
            "bootstrap"
        };

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _stringBuilder = new StringBuilder();
            _stringWriter = new StringWriter(_stringBuilder);
            _textWriter = new HtmlTextWriter(_stringWriter);
            _httpWriter = (HttpWriter)filterContext.RequestContext.HttpContext.Response.Output;
            filterContext.RequestContext.HttpContext.Response.Output = _textWriter;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            string response = _stringBuilder.ToString();
            string version = ConfigurationManager.AppSettings.Get(Constants.STATIC_FILE_VERSION_KEY);
            string postfix = ConfigurationManager.AppSettings.Get(Constants.STATIC_FILE_POSTFIX_KEY);

            var updateModel = new UpdateModel()
            {
                InputString = response,
                FileExtension = ".js",
                TagAttribute = "src",
                StartPath = "/Scripts"
            };
            updateModel.InputString = UpdatePostfix(updateModel, postfix, excludedScriptsForPostfix);

            updateModel.InputString = UpdateVersion(updateModel, version);

            updateModel.FileExtension = ".css";
            updateModel.TagAttribute = "href";
            updateModel.StartPath = "/Content";
           
            updateModel.InputString = UpdatePostfix(updateModel, postfix, excludedCssPostfix);            
            response = UpdateVersion(updateModel, version);

            _httpWriter.Write(response);
        }

        private static string UpdatePostfix(UpdateModel model, string postfix, string[] excludedFileNames = null)
        {
            string res = string.Empty;
            if (!string.IsNullOrWhiteSpace(postfix) && !string.IsNullOrWhiteSpace(model?.InputString))
            {
                res = model.InputString;
                string pattern = $"{model.TagAttribute}=\"{model.StartPath}[^\"]+\"";
                var matches = Regex.Matches(model.InputString, pattern, RegexOptions.Singleline)
                    .Cast<Match>()
                    .Select(x => x.Groups[0].Value)
                    .ToList();

                foreach (var item in matches.Where(m => !string.IsNullOrWhiteSpace(m)))
                {
                    if ((excludedFileNames?.Count(e => item.IndexOf(e, StringComparison.InvariantCultureIgnoreCase) > -1) ?? 0) == 0)
                    {
                        int index = item.IndexOf(model.FileExtension);
                        if (index > -1)
                        {
                            string newValue = item.Insert(index, postfix);
                            res = res.Replace(item, newValue);
                        }
                    }
                }
            }
            return res;
        }

        private static string UpdateVersion(UpdateModel model, string version, string[] excludedFileNames = null)
        {
            string res = string.Empty;
            if (!string.IsNullOrWhiteSpace(version) && !string.IsNullOrWhiteSpace(model?.InputString))
            {
                res = model.InputString;
                string pattern = $"{model.TagAttribute}=\"{model.StartPath}[^\"]+" + @"\?v(\d+\.?)+" + "\"";
                var matches = Regex.Matches(model.InputString, pattern, RegexOptions.Singleline)
                    .Cast<Match>()
                    .Select(x => x.Groups[0].Value)
                    .ToList();

                foreach (var item in matches.Where(m => !string.IsNullOrWhiteSpace(m)))
                {
                    if ((excludedFileNames?.Count(e => item.IndexOf(e, StringComparison.InvariantCultureIgnoreCase) > -1) ?? 0) == 0)
                    {
                        int index = item.IndexOf("?v");
                        if (index > -1)
                        {
                            string newValue = item.Substring(0, index + 1) + version + "\"";
                            res = res.Replace(item, newValue);
                        }
                    }
                }
            }
            return res;
        }
    }

    public class UpdateModel
    {
        public string InputString { get; set; }
        public string TagAttribute { get; set; }
        public string FileExtension { get; set; }
        public string StartPath { get; set; }
    }
}