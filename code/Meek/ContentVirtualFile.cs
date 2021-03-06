﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Meek.Configuration;
using Meek.Storage;

namespace Meek
{
    public class ContentVirtualFile : VirtualFile
    {
        readonly string _pathKey;
        readonly Repository _repository;
        readonly Authorization _auth;

        public ContentVirtualFile(Repository repository, string requestedPath, string pathKey, Authorization auth)
            : base(requestedPath)
        {
            _pathKey = pathKey.Replace(".cshtml", string.Empty);
            _repository = repository;
            _auth = auth;
        }

        public override Stream Open()
        {
            var content = _repository.Get(_pathKey);
            var contentMarkup = content.Contents;

            var httpContext = HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current);

            if (_auth.IsContentAdmin(httpContext))
                contentMarkup = AddEditLinkMarkup(contentMarkup, content.Partial);

            var constructedContent = string.Format("@{{ {0} ViewBag.Title = \"{1}\";}} {2}"
                                                   , content.Partial ? " Layout = null;" : null
                                                   , content.Title
                                                   , contentMarkup);
            
            return new MemoryStream(Encoding.UTF8.GetBytes(constructedContent));
        }

        private string AddEditLinkMarkup(string content, bool partial)
        {
            if (partial || content.IndexOf(@"</html>") == -1)
            {
                content +=
                    string.Format(
                        "<div class=\"MeekEditLink\"><a href=\"/Meek/Manage?aspxerrorpath={0}\">Edit Content</a></div>",
                        _pathKey);
            }
            else
            {
                content = content.Insert(content.IndexOf(@"</html>"),
                                         string.Format(
                                             "<div class=\"MeekEditLink\"><a href=\"/Meek/Manage?aspxerrorpath={0}\">Edit Content</a></div>",
                                             _pathKey));
            }

            return content;

        }

    }
}
