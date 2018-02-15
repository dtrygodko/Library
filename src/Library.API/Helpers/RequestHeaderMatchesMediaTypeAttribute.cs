using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Library.API.Helpers
{
    public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly string[] _mediaTypes;
        private readonly string _requestHeaderToMatch;

        public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch, string[] mediaTypes)
        {
            _requestHeaderToMatch = requestHeaderToMatch;
            _mediaTypes = mediaTypes;
        }

        public bool Accept(ActionConstraintContext context)
        {
            var requestHeaders = context.RouteContext.HttpContext.Request.Headers;

            if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
            {
                return false;
            }

            foreach (var mediaType in _mediaTypes)
            {
                var mediaTypeMatches = string.Equals(requestHeaders[_requestHeaderToMatch].ToString(), mediaType,
                    StringComparison.OrdinalIgnoreCase);

                if (mediaTypeMatches)
                {
                    return true;
                }
            }

            return false;
        }

        public int Order => 0;
    }
}