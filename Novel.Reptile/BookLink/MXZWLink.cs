using System;
using System.Collections.Generic;
using System.Text;
using AngleSharp.Parser.Html;
using Novel.Reptile.Entities;

namespace Novel.Reptile.BookLink
{
    public class MXZWLink : LinkBase
    {
        public MXZWLink(BookLinks bookLinks) : base(bookLinks)
        {
        }

        public override void ReadLink()
        {
            using (BookContext bookContext = new BookContext())
            {
                var html = HttpHelper.HttpGet(Links.Url, "text/html", charset: "utf-8");
                var parser = new HtmlParser();
                var document = parser.Parse(html);
                var metas = document.QuerySelectorAll("meta");
                Task(bookContext, metas, syncType: 3);
                bookContext.BookLinks.Remove(Links);
                bookContext.SaveChanges();
            }
        }
    }
}
